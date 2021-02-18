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

        public static Int64 getActionID(string actionName)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                Int64 output = cnn.Execute($"SELECT actionID FROM Actions WHERE Command = {actionName};");
                return output;
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

        internal static object getActionID(object p)
        {
            throw new NotImplementedException();
        }

        public static Int64 GetUserId(string username)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("Username", username);
                var output = cnn.Query<Int64>("SELECT Userid FROM Users WHERE Username=@Username;", parameters);
                return output.ToList()[0];
            }
        }

        public static void AddUser(User user)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                cnn.Execute($"INSERT INTO Users(Username) VALUES(@Username)", user);
                SetDefaultConnector(GetUserId(user.Username));
            }
        }

        private static string GetConnectionString(string id="Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

        private static void SetDefaultGestures()
        {
            string insertations = "INSERT INTO gestures(gest_name, hand_side)VALUES ";
            string[] gest_names = { "'1'", "'2'", "'3'", "'5'" };
            string[] hand_sides = { "0", "1" };

            for(int i = 0; i < gest_names.Length; i++)
            {
                insertations += $"({gest_names[i]}, {hand_sides[0]}), ";
                insertations += $"({gest_names[i]}, {hand_sides[1]}), ";
            }

            insertations.Substring(0, insertations.Length - 2);
            insertations += ";";
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                cnn.Execute(insertations);
            }


        }
        
        private static List<Int64> GetAllGesturesIDs()
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                var gesturesFromTable= cnn.Query<Int64>("SELECT Gestureid FROM Gestures;");
                List<Int64> listOfGestures = gesturesFromTable.ToList();
                return listOfGestures;
            }
        }

        private static void SetDefaultConnector(Int64 userId)
        {
            List<Int64> IDs = GetAllGesturesIDs();
            const string defaultActionID = "1";
            string gesturesIDsForTable = "";
            string actionsIDSForTable = "";
            for(int i = 0; i < IDs.Count; i++) 
            {
                gesturesIDsForTable += IDs[i].ToString() ;
                actionsIDSForTable += defaultActionID;
                if (i < IDs.Count - 1) //if last item, dont add psikim
                {
                    gesturesIDsForTable += ",";
                    actionsIDSForTable += ",";
                }
            }
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                string insertations = $"INSERT INTO connectors(Userid, GesturesArray, ActionsArray) VALUES ({userId}, '{gesturesIDsForTable}', '{actionsIDSForTable}');";
                cnn.Execute(insertations);
            }
        }

        public static void ChangeConnectors(Int64 userID, string gesturesArray, string actionsArray)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                string updating = $"UPDATE connectors SET GesturesArray = {gesturesArray}, ActionsArray = {actionsArray} WHERE UserId = {userID};";
                cnn.Execute(updating);
            }
        }
        

        public static void ChangeUserName(Int64 userID, string newUserName)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                string updating = $"UPDATE Users SET Username = '{newUserName}' WHERE UserId = {userID};";
                cnn.Execute(updating);
            }
        }

        public static void DeleteUser(Int64 userID)
        {
            using (IDbConnection cnn = new SQLiteConnection(GetConnectionString()))
            {
                string deleting = $"DELETE FROM Users WHERE UserId = {userID};";
                cnn.Execute(deleting);
            }
        }
    }
}
