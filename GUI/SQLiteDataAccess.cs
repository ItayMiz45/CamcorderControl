using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public class SQLiteDataAccess
    {
        public enum SQLiteErrorCodes
        {
            UniqueError = 19,
        }

        public static List<User> GetUsers()
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var output = cnn.Query<User>("SELECT * FROM Users;", new DynamicParameters());
                return output.ToList();
            }
        }

        public static bool DoesUserExist(String Username)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("Username", Username);
                var output = cnn.Query<int>("SELECT COUNT(UserId) FROM Users WHERE Username=@Username;", parameters);
                return output.First() != 0; // return true if 1 and false if 0
            }
        }

        public static bool DoesUserExist(Int64 UserId)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("UserId", UserId);
                var output = cnn.Query<int>("SELECT COUNT(UserId) FROM Users WHERE UserId=@UserId;", parameters);
                return output.First() != 0; // return true if 1 and false if 0
            }
        }

        public static User GetUser(String Username)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("Username", Username);
                var output = cnn.Query<User>("SELECT * FROM Users WHERE Username=@Username; ", parameters);
                return output.First();
            }
        }

        public static User GetUser(Int64 UserId)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("UserId", UserId);
                var output = cnn.Query<User>("SELECT * FROM Users WHERE UserId=@UserId;", parameters);
                return output.ToList()[0];
            }
        }

        public static void AddUser(User user)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                cnn.Execute($"INSERT INTO Users(Username) VALUES(@Username)", user);
            }
        }

        private static string GetConnectionString(string id="Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
