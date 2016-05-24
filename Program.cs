using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Configuration;


namespace LTS
{    // This class deals with database connection stuff in a pretty way
    class DB
    {
        public static MySqlConnection getconn()
        {
            // Establish connection to sever


            // Yes, I know I have my password avaliable here. Don't worry about it. 
            string connStr = "server=localhost;user=root;database=test;password=removekebab;";
            
            
            /*string.Format(connStr, ConfigurationManager.AppSettings["mysql_user"].ToString(), ConfigurationManager.AppSettings["mysql_db"], ConfigurationManager.AppSettings["mysql_pass"]);
            Console.WriteLine(connStr);
            Console.ReadKey();*/
            MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            if (conn != null)
            {
                Console.WriteLine(conn.ToString());
                return conn;
            }
            else
            {
                throw new SystemException();
            }
        }
    }

    /* // In this code snippet, I was trying to make a very reusable way to query the database.
       // TODO : Finish this
    class ClientMySQL
    {
        public string Select(string Column, string Where, string WhereData, string Table="test")
        {
            var conn = DB.getconn();

            string query = "SELECT @Column FROM @table WHERE @Where = @WhereData";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", this.ID);

            MySqlDataReader reader = cmd.ExecuteReader();
            string final = "Didnt work";
            while (reader.Read()) { final = reader.GetString(0); }
            conn.Close();
            return final;
        }
    } */

    // This is basically the "learner" object. Whenver I refer to a learner, I mean the LTS
    class LTS
    {
        public bool is_initialized;
        int ID;
        string _first_name;
        string _last_name;
        string _notes;
        int _location;
        string _rfid;

        public LTS(int id)
        {
            // Initalize a student using their id. All other info regarding the student
            // is pulled up via _attributes.
            this.ID = id;
            this.is_initialized = true;
        }


        // Pull up a learning object just by knowing their ID
        public static LTS learner_from_rfid(string rfid)
        {
            var conn = DB.getconn();
            string query = "SELECT learner_id FROM learners WHERE rfid = @rfid";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@rfid", rfid);

            MySqlDataReader reader = cmd.ExecuteReader();

            // Establishes default id as -1. If it is -1, an error should be thrown up
            Int64 id = -1;
            while (reader.Read()) { id = reader.GetInt64(0); }
            conn.Close();
            // Casts id (int64) to int. TODO : Do something smarter than this
            return new LTS((int)id);
        }

        public string first_name
        {
            get
            {
                MySqlConnection conn;
                string query;
                MySqlCommand cmd;
                string final;

                conn = DB.getconn();

                query = "SELECT first_name FROM learners WHERE learner_id = @id";
                cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                // This is the default value
                final = "Didnt work";
                while (reader.Read()) { final = reader.GetString(0); }
                conn.Close();
                return final;
            }
            set
            {
                // Run a SQL command to update name sql-side
                var conn = DB.getconn();

                string query = @"UPDATE learners
                                SET first_name = @new_name
                                WHERE learner_id = @id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);
                cmd.Parameters.AddWithValue("@new_name", value);
                int status = cmd.ExecuteNonQuery();

                // Run a SQL query to grab it from the server. To make sure that it
                // properly updated sql-side
                query = "SELECT first_name FROM learners WHERE learner_id = @id";
                cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);
                MySqlDataReader reader = cmd.ExecuteReader();
                string final = "Didnt work";
                while (reader.Read()) { final = reader.GetString(0); }
                conn.Close();

                // Set the learner attribute to the value grabbed from the sql server.
                // This makes it completely certain that the attribute and server are 
                // always the same.
                this._first_name = final;
            }

        }
        public string last_name
        {
            get
            {
                var conn = DB.getconn();

                string query = "SELECT last_name FROM learners WHERE learner_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                string final = "Didnt work";
                while (reader.Read()) { final = reader.GetString(0); }
                conn.Close();
                return final;
            }
            set
            {
                var conn = DB.getconn();

                string query = @"UPDATE learners
                                SET last_name = @new_name
                                WHERE learner_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);
                cmd.Parameters.AddWithValue("@new_name", value);

                int status = cmd.ExecuteNonQuery();
                query = "SELECT last_name FROM learners WHERE learner_id = @id";
                cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                string final = "Didnt work";
                while (reader.Read()) { final = reader.GetString(0); }
                conn.Close();
                this._last_name = final;
            }

        }
        public string notes
        {
            get
            {
                var conn = DB.getconn();

                string query = "SELECT notes FROM learners WHERE learner_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                string final = "Didnt work";
                while (reader.Read())
                {
                    try
                    {
                        final = reader.GetString(0);
                    }
                    catch (System.Data.SqlTypes.SqlNullValueException e)
                    {
                        final = "";
                    }
                }
                conn.Close();
                return final;
            }
            set
            {
                var conn = DB.getconn();

                string query = @"UPDATE learners
                                SET notes = @new_notes
                                WHERE learner_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);
                cmd.Parameters.AddWithValue("new_notes", value);

                query = "SELECT notes FROM learners WHERE learner_id = @id";
                cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                string final = "Didnt work";
                while (reader.Read())
                {
                    try
                    {
                        final = reader.GetString(0);
                    }
                    catch (System.Data.SqlTypes.SqlNullValueException e)
                    {
                        final = "";
                    }
                }
                conn.Close();
                this._notes = final;
            }
        }
        public int location
        {
            get
            {
                var conn = DB.getconn();

                string query = "SELECT location FROM learners WHERE learner_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                Int64 final = -1;
                while (reader.Read()) { final = reader.GetInt64(0); }
                conn.Close();
                // I hope you dont overflow... That'd have to be a lot of locations tho
                return (int)final;
            }
            set
            {
                var conn = DB.getconn();

                string query = @"UPDATE learners
                                SET location = @new_location
                                WHERE learner_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);
                cmd.Parameters.AddWithValue("new_location", value);

                int status = cmd.ExecuteNonQuery();

                query = "SELECT location FROM learners WHERE learner_id = @id";
                cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                Int64 final = -1;
                while (reader.Read()) { final = reader.GetInt64(0); }
                conn.Close();
                // I hope you dont overflow... That'd have to be a lot of locations tho
                this._location = (int)final;
            }
        }
        public string rfid
        {
            get
            {
                var conn = DB.getconn();
                string query = "SELECT rfid FROM learners WHERE learner_id = @id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                string final = "Didnt work";
                while (reader.Read()) { final = reader.GetString(0); }
                conn.Close();
                return final;
            }
            set
            {
                var conn = DB.getconn();
                string query = @"UPDATE learners
                                SET rfid = @new_rfid
                                WHERE learner_id = @id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);
                cmd.Parameters.AddWithValue("new_rfid", value);

                int status = cmd.ExecuteNonQuery();
                query = "SELECT rfid FROM learners WHERE learner_id = @id";

                cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", this.ID);

                MySqlDataReader reader = cmd.ExecuteReader();
                string final = "Didnt work";
                while (reader.Read()) { final = reader.GetString(0); }
                conn.Close();
                this._rfid = final;
            }
        }
    }
}


namespace Server
{
    // This will allow the same server to process requests from a client (not a scanner). This will be
    // an administrative tool. 
    // TODO: Finish this
    class ClientRequest
    {
        // It's a bad idea to try to access these.
        public Int32 TimeCreated { get; } // Unix time when reqeust is created
        public string ClientType { get; } // Type of client that is sending request eg (WinClient,UnixClient,PythonClient, WebClient)
        public string RequestType { get; set; } // Type of request eg (SELECT,UPDATE,DELETE)
        public Dictionary<string, string> parameters { get; set; }
        public bool IsInitialized { get; }

        public ClientRequest(string RequestType, Dictionary<string, string> parameters)
        {
            this.TimeCreated = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            this.ClientType = GetClient();
            this.RequestType = RequestType;
            this.parameters = parameters;
            this.IsInitialized = true;
        }

        private string GetClient()
        {
            string ClientType;
            Type t = Type.GetType("Mono.Runtime");
            if (t != null) { ClientType = "UnixClient"; }
            else { ClientType = "WinClient"; }
            return ClientType;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }



        /* An example of the serialized object
        {  
           "TimeCreated":1439780326,
           "ClientType":"WinClient",
           "RequestType":"select",
           "IsInitialized":true,
           "parameters":{  
              "where":"id",
              "id":"3",
              "column":"first_name"
           }
        }



        */
    }

    class RequestHandeler
    {
        public static ClientRequest DeserializeRequest(string JSON)
        {
            return JsonConvert.DeserializeObject<ClientRequest>(JSON);
        }

        public void parse(ClientRequest Request)
        {
            switch (Request.RequestType)
            {
                case "select":

                    break;
            }
        }
    }

    class Program
    {
        public static void Logger(Exception error)
        {

            System.IO.StreamWriter file = new System.IO.StreamWriter("D:\\Users\\Visual Studio 2015\\Projects\\ConsoleApplication1\\logs\\logs.txt", true);
            string time = DateTime.Now.ToString("MM/dd/yyyy h:mm tt");
            string errorstring = error.ToString();
            string trace = error.StackTrace;
            string errorlog = time + "-" + errorstring + "-" + trace;
            file.WriteLine(errorlog);

            file.Close();
        }

        static void startTCP()
        {
            TcpListener server = new TcpListener(8080);
            TcpClient client = default(TcpClient);
            server.Start();
            Console.WriteLine("Server Started");
            while (true)
            {
                try
                {
                    client = server.AcceptTcpClient();
                    Console.WriteLine("\nAccepted Client Connection\n");
                    NetworkStream stream = client.GetStream();
                    byte[] bytesFrom = new byte[10025];
                    stream.Read(bytesFrom, 0, 32);
                    string FromClient = Encoding.ASCII.GetString(bytesFrom);
                    FromClient = FromClient.Substring(0, 32);
                    Console.WriteLine("\nGot data: " + FromClient + "\n");
                    Byte[] sendByte = Encoding.ASCII.GetBytes(received(FromClient));
                    stream.Write(sendByte, 0, sendByte.Length);
                    stream.Flush();
                    Console.WriteLine("\nSuccesfully sent state\n");
                    client.Close();
                    Console.WriteLine("\nClosing the Client\n");
                }
                catch (Exception error)
                {

                    Logger(error);
                    Console.WriteLine("\nThere was an error:\n" + error.ToString() + "\n");
                }
            }
        }

        static String received(String rec)
        {
            Console.WriteLine("\nSQL collection started\n");
            LTS.LTS learner = LTS.LTS.learner_from_rfid(rec);
            learner.location = 11;
            Console.WriteLine("\nSQL collection done\n");
            return "1"; //This is what to send back either "0" or "1" notpass/pass
        }

        static void Main(string[] args)
        {
            startTCP();
        }
    }
}
