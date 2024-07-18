using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.IO;
using System.Data.Common;

namespace Library_class_WCF
{

    public class Call_ICallMethod : ICallMethod
    {





        public string[] Read_Character_Names()
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                Console.WriteLine("BD connect");
                string sqlCommand = "SELECT name FROM character";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, dbConnection);
                SQLiteDataReader dataReader = dbCommand.ExecuteReader();
                List<string> result = new List<string>();
                while (dataReader.Read())
                {
                    result.Add(Convert.ToString(dataReader["name"]));
                }
                return result.ToArray();
            }
        }


        public void Enter_character(string name_Pers, int class_id, int speed, int hp)
        {
            using (var connect_BD = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                connect_BD.Open();
                Console.WriteLine("BD connect");

                string DB_Select = "SELECT * FROM character WHERE name = @name AND class_id = @class_id";
                SQLiteCommand data_db = new SQLiteCommand(DB_Select, connect_BD);
                data_db.Parameters.AddWithValue("@name", name_Pers);
                data_db.Parameters.AddWithValue("@class_id", class_id);

                SQLiteDataReader unload_db = data_db.ExecuteReader();
                List<string> result_db = new List<string>();

                while (unload_db.Read())
                {
                    result_db.Add(Convert.ToString(unload_db["name"]));
                }

                if (result_db.Contains(name_Pers))
                {
                    return;
                }
                else
                {
                    DB_Select = $"INSERT INTO character (name, class_id, speed, hp) VALUES (@name, @class_id, @speed, @hp)";
                    data_db = new SQLiteCommand(DB_Select, connect_BD);
                    data_db.Parameters.AddWithValue("@name", name_Pers);
                    data_db.Parameters.AddWithValue("@class_id", class_id);
                    data_db.Parameters.AddWithValue("@speed", speed);
                    data_db.Parameters.AddWithValue("@hp", hp);
                    data_db.ExecuteNonQuery();
                }
            }
        }


        public void Create_Class(string name_Class, string image_Path)
        {
            using (var connect_BD = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                connect_BD.Open();
                Console.WriteLine("DB connected");

                string checkClassQuery = "SELECT COUNT(*) FROM class WHERE name_class = @name_Class";
                SQLiteCommand checkClassCommand = new SQLiteCommand(checkClassQuery, connect_BD);
                checkClassCommand.Parameters.AddWithValue("@name_Class", name_Class);
                int classCount = Convert.ToInt32(checkClassCommand.ExecuteScalar());

                if (classCount > 0)
                {
                    Console.WriteLine("A class with that name already exists.");
                    return;
                }
                else
                {
                    // Добавляем класс в базу данных
                    string insertClassQuery = "INSERT INTO class (name_class, path_image) VALUES (@name_Class, @image_Path)";
                    SQLiteCommand insertClassCommand = new SQLiteCommand(insertClassQuery, connect_BD);
                    insertClassCommand.Parameters.AddWithValue("@name_Class", name_Class);
                    insertClassCommand.Parameters.AddWithValue("@image_Path", image_Path);
                    insertClassCommand.ExecuteNonQuery();
                    Console.WriteLine("The class has been successfully added to the database.");
                }
            }
        }


        public bool Delete_Class(string className)
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                string sqlCommand = "DELETE FROM class WHERE name_class = @name_class";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, dbConnection);
                dbCommand.Parameters.AddWithValue("@name_class", className);
                return dbCommand.ExecuteNonQuery() > 0;
            }
        }


        public void Edit_chara(string name_pers, string description, int speed, int hp)
        {
            using (var connect_BD = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                connect_BD.Open();
                string DB_Select;
                Console.WriteLine("BD connect");


                DB_Select = $"UPDATE character SET name = @description, speed = @speed, hp = @hp WHERE name = @name";
                SQLiteCommand data_db = new SQLiteCommand(DB_Select, connect_BD);
                data_db.Parameters.AddWithValue("@description", description);
                data_db.Parameters.AddWithValue("@name", name_pers);
                data_db.Parameters.AddWithValue("@speed", speed);
                data_db.Parameters.AddWithValue("@hp", hp);
                data_db.ExecuteNonQuery();


            }
        }


        public void Delete_pers(string name_pers)
        {
            using (var connect_BD = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                connect_BD.Open();
                Console.WriteLine("BD connect");
                string sqlCommand = $"DELETE FROM character WHERE name = @name";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, connect_BD);
                dbCommand.Parameters.AddWithValue("@name", name_pers);
                dbCommand.ExecuteNonQuery();
            }
        }

        public bool CheckUserLoginPassword(string Login, string hashed_User_Password)
        {
            using (var connect_BD = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                connect_BD.Open();
                Console.WriteLine("BD connect");
                string sqlCommand = $"SELECT login, password FROM user WHERE login = '{Login}' AND password = '{hashed_User_Password}'";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, connect_BD);
                SQLiteDataReader dataReader = dbCommand.ExecuteReader();
                List<string> result = new List<string>();
                while (dataReader.Read())
                {
                    result.Add(dataReader.GetString(0));
                    result.Add(dataReader.GetString(1));
                }

                if (result.Contains(Login) && result.Contains(hashed_User_Password))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Log_Pass(string Login, string Pass)
        {
            using (var connect_BD = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                connect_BD.Open();
                string DB_Select = "SELECT COUNT(*) FROM user WHERE login = @Login AND password = @Pass";
                SQLiteCommand data_db = new SQLiteCommand(DB_Select, connect_BD);
                data_db.Parameters.AddWithValue("@Login", Login);
                data_db.Parameters.AddWithValue("@Pass", Pass);

                int count = Convert.ToInt32(data_db.ExecuteScalar());
                return count > 0;
            }
        }

        public string HashPassword(string userPassword)
        {
            var SHA512_hash = SHA512.Create();

            var hashedUserPassword = SHA512_hash.ComputeHash(Encoding.UTF8.GetBytes(userPassword));
            string finalHashedUserPassword = Convert.ToBase64String(hashedUserPassword);

            return finalHashedUserPassword;
        }
        public bool RegisterUser(string userLogin, string userPassword)
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                Console.WriteLine("BD connect");

                string checkUserQuery = "SELECT COUNT(*) FROM user WHERE login = @userLogin";
                SQLiteCommand checkUserCommand = new SQLiteCommand(checkUserQuery, dbConnection);
                checkUserCommand.Parameters.AddWithValue("@userLogin", userLogin);
                int userCount = Convert.ToInt32(checkUserCommand.ExecuteScalar());

                if (userCount > 0)
                {
                    Console.WriteLine("Пользователь с таким логином уже существует.");
                    return false;
                }
                else
                {
                    // Хэшируем пароль и добавляем пользователя в базу данных
                    string hashedPassword = HashPassword(userPassword);
                    string insertUserQuery = "INSERT INTO user (login, password) VALUES (@userLogin, @hashedPassword)";
                    SQLiteCommand insertUserCommand = new SQLiteCommand(insertUserQuery, dbConnection);
                    insertUserCommand.Parameters.AddWithValue("@userLogin", userLogin);
                    insertUserCommand.Parameters.AddWithValue("@hashedPassword", hashedPassword);
                    insertUserCommand.ExecuteNonQuery();
                    Console.WriteLine("Пользователь успешно зарегистрирован.");
                    return true;
                }
            }
        }

        public string[] Read_Character_Details()
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                Console.WriteLine("BD connect");
                string sqlCommand = "SELECT ch.name, c.name_class, ch.speed, ch.hp FROM character ch INNER JOIN class c ON ch.class_id = c.id";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, dbConnection);
                SQLiteDataReader dataReader = dbCommand.ExecuteReader();
                List<string> result = new List<string>();
                while (dataReader.Read())
                {
                    string characterName = Convert.ToString(dataReader["name"]);
                    string className = Convert.ToString(dataReader["name_class"]);
                    string speed = Convert.ToString(dataReader["speed"]);
                    string hp = Convert.ToString(dataReader["hp"]);

                    result.Add($"{characterName} {className} {speed} {hp}");
                }
                return result.ToArray();
            }
        }



        public string[] GetClassesFromDatabase()
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                Console.WriteLine("BD connect");
                string sqlCommand = "SELECT name_class FROM class";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, dbConnection);
                SQLiteDataReader dataReader = dbCommand.ExecuteReader();
                List<string> result = new List<string>();
                while (dataReader.Read())
                {
                    string className = Convert.ToString(dataReader["name_class"]);
                    result.Add(className);
                }
                return result.ToArray();
            }
        }

        public string[] Read_User_Details()
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                Console.WriteLine("BD connect");
                string sqlCommand = "SELECT login, password FROM user";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, dbConnection);
                SQLiteDataReader dataReader = dbCommand.ExecuteReader();
                List<string> result = new List<string>();
                while (dataReader.Read())
                {
                    string login = Convert.ToString(dataReader["login"]);
                    string password = HashPassword(Convert.ToString(dataReader["password"]));
                    result.Add($"{login} {password}");
                }
                return result.ToArray();
            }
        }
        public void Delete_User(string login)
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                Console.WriteLine("BD connect");
                string sqlCommand = "DELETE FROM user WHERE login = @login";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, dbConnection);
                dbCommand.Parameters.AddWithValue("@login", login);
                dbCommand.ExecuteNonQuery();
            }
        }


        public string[] Read_Class_Details()
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                string sqlCommand = "SELECT name_class, path_image FROM class";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, dbConnection);
                SQLiteDataReader dataReader = dbCommand.ExecuteReader();
                List<string> result = new List<string>();
                while (dataReader.Read())
                {
                    string className = Convert.ToString(dataReader["name_class"]);
                    string pathImage = Convert.ToString(dataReader["path_image"]);
                    result.Add($"{className} {pathImage}");
                }
                return result.ToArray();
            }
        }

        public bool RegisterClass(string className, string pathImage)
        {
            using (var dbConnection = new SQLiteConnection("Data Source = A:\\Ado.net\\userdata.db"))
            {
                dbConnection.Open();
                string sqlCommand = "INSERT INTO class (name_class, path_image) VALUES (@name_class, @path_image)";
                SQLiteCommand dbCommand = new SQLiteCommand(sqlCommand, dbConnection);
                dbCommand.Parameters.AddWithValue("@name_class", className);
                dbCommand.Parameters.AddWithValue("@path_image", pathImage);
                return dbCommand.ExecuteNonQuery() > 0;
            }
        }

        public void SaveData(string characterName, int gold, int goldPerClick, int printingMachines)
        {
            try
            {
                using (var dbConnection = new SQLiteConnection("Data Source=A:\\Ado.net\\userdata.db"))
                {
                    dbConnection.Open();
                    string query = "INSERT INTO save_data (character_name, gold, gold_per_click, printing_machines) VALUES (@characterName, @gold, @goldPerClick, @printingMachines)";
                    SQLiteCommand command = new SQLiteCommand(query, dbConnection);
                    command.Parameters.AddWithValue("@characterName", characterName);
                    command.Parameters.AddWithValue("@gold", gold);
                    command.Parameters.AddWithValue("@goldPerClick", goldPerClick);
                    command.Parameters.AddWithValue("@printingMachines", printingMachines);
                    command.ExecuteNonQuery();
                    Console.WriteLine("Data saved successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
                throw; // Пробросим исключение выше для обработки в клиентском коде
            }
        }

        public (int gold, int goldPerClick, int printingMachines) LoadSavedData(string characterName)
        {
            try
            {
                using (var dbConnection = new SQLiteConnection("Data Source=A:\\Ado.net\\userdata.db"))
                {
                    dbConnection.Open();
                    string query = "SELECT gold, gold_per_click, printing_machines FROM save_data WHERE character_name = @characterName ORDER BY id DESC LIMIT 1";
                    SQLiteCommand command = new SQLiteCommand(query, dbConnection);
                    command.Parameters.AddWithValue("@characterName", characterName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int gold = reader.GetInt32(0);
                            int goldPerClick = reader.GetInt32(1);
                            int printingMachines = reader.GetInt32(2);
                            Console.WriteLine($"Data loaded. Gold: {gold}, Gold per click: {goldPerClick}, Printing Machines: {printingMachines}");
                            return (gold, goldPerClick, printingMachines);
                        }
                        else
                        {
                            return (0, 1, 0); // Если нет данных, возвращаем начальные значения
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                throw; // Пробросим исключение выше для обработки в клиентском коде
            }
        }

    }
        [ServiceContract]
        public interface ICallMethod
        {

            [OperationContract]
            void Delete_User(string login);
            [OperationContract]
            string[] GetClassesFromDatabase();

            [OperationContract]
            string[] Read_Character_Names();
            [OperationContract]
            void SaveData(string characterName, int gold, int goldPerClick, int printingMachines);

            [OperationContract]
            (int gold, int goldPerClick, int printingMachines) LoadSavedData(string characterName);
            [OperationContract]
            string[] Read_Character_Details();
            [OperationContract]
            string[] Read_User_Details();
            [OperationContract]
            string[] Read_Class_Details();
            [OperationContract]
            void Enter_character(string name_Pers, int class_id, int speed_Pers, int hp_Pers);

            [OperationContract]
            void Create_Class(string name_Class, string image_Path);

            [OperationContract]
            void Edit_chara(string name_pers, string description, int speed, int hp);

            [OperationContract]
            void Delete_pers(string name_Pers);
            [OperationContract]
            bool RegisterClass(string className, string pathImage);


            [OperationContract]
            bool CheckUserLoginPassword(string login, string password);

            [OperationContract]
            bool Log_Pass(string Login, string Pass);
            [OperationContract]
            bool Delete_Class(string className);
            [OperationContract]
            bool RegisterUser(string userLogin, string userPassword);
            [OperationContract]
            string HashPassword(string userPasswordHash);


        }
    
}
