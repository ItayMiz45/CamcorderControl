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

        public static List<Gesture> GetGestures()
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var output = cnn.Query<Gesture>("SELECT * FROM Gestures;", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<Action> GetActions()
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var output = cnn.Query<Action>("SELECT * FROM Actions;", new DynamicParameters());
                return output.ToList();
            }
        }

        public static Action GetAction(Int64 actionId)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("ActionId", actionId);
                var output = cnn.Query<Action>("SELECT * FROM Actions WHERE ActionId=@ActionId;", parameters);
                return output.First();
            }
        }

        public static Connector GetConnector(Int64 connectorId)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("ConnectorId", connectorId);
                var output = cnn.Query<Connector>("SELECT * FROM Connectors WHERE ConnectorId=@ConnectorId;", parameters);
                return output.First();
            }
        }

        public static Connector GetConnectorByUserId(Int64 userId)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("UserId", userId);
                var output = cnn.Query<Connector>("SELECT * FROM Connectors WHERE UserId=@UserId;", parameters);
                return output.First();
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
