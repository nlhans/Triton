using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Data;

using Triton.Debugging;

namespace Triton.Database
{
    public delegate IDbConnection ConnectionCreator();

    public class DatabaseConnectionPool
    {
        private static ConnectionCreator CreateConnection;

        private static int _MaxConnections = 16;
        private static int _ConnectionPeak = 0;
        private static Dictionary<int, IDbConnection> ConnectionPool = new Dictionary<int, IDbConnection>();

        private static bool _Configured = false;

        public static int MaxConnections { get { return _MaxConnections; } set { _MaxConnections = value; } }
        public static int ConnectionPeak { get { return _ConnectionPeak; } }

        public static void Boot(ConnectionCreator creator)
        {
            CreateConnection = creator;
            _Configured = true;

            System.Timers.Timer t = new System.Timers.Timer();
            t.Interval = 100;
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        private static void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            CleanUp();
        }

        private static int GetThreadIdentifier()
        {
            return System.AppDomain.GetCurrentThreadId();
            //return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        private static void CleanUp()
        {

            lock (ConnectionPool)
            {
                List<int> ids = new List<int>();
                foreach (KeyValuePair<int, IDbConnection> connect in ConnectionPool)
                {
                    if (connect.Value == null || connect.Key == 0)
                        ids.Add(connect.Key);
                }
                ids.ForEach(delegate(int k)
                {
                    if (ConnectionPool.ContainsKey(k))
                    {
                        if (ConnectionPool[k] != null)
                        {

                            try
                            {
                                ConnectionPool[k].Close();
                            }
                            catch (ThreadAbortException exas)
                            {
                            }
                            catch (Exception ex)
                            {
                                //
                                Logger.Log("Could not close connection: " + ex.Message);
                            }
                        }
                        ConnectionPool[k] = null;
                        ConnectionPool.Remove(k);

                    }
                });
            }
        }

        public static int Connections
        {
            get
            {
                int c = ConnectionPool.Count;
                if (c > ConnectionPeak)
                    _ConnectionPeak = c;
                return c;
            }
        }
        public static void CloseAll()
        {
            foreach (KeyValuePair<int, IDbConnection> connect in ConnectionPool)
            {
                connect.Value.Close();
            }
        }

        public static void Freeup(int ConnectionIdentifier)
        {
            if (!_Configured)
                throw new Exception("You need to configure this manager first with the Boot method");
            if (ConnectionIdentifier == 0)
                throw new Exception("You cannot use identifer 0");
            if (ConnectionPool.ContainsKey(ConnectionIdentifier))
            {
                try
                {
                    ConnectionPool[ConnectionIdentifier].Close();
                }
                catch (ThreadAbortException exas)
                {
                }
                catch (Exception ex)
                {
                    //
                    Logger.Log("Could not close connection: " + ex.Message);
                }

                lock (ConnectionPool)
                {
                    ConnectionPool[ConnectionIdentifier] = null;
                    ConnectionPool.Remove(ConnectionIdentifier);
                }
            }
        }

        public static void Freeup()
        {
            int ConnectionIdentifier = GetThreadIdentifier();
            Freeup(ConnectionIdentifier);
        }

        private static IDbConnection Create()
        {
            try
            {
                if (Connections >= MaxConnections)
                {
                    return null;
                }
                //IDbConnection sqlConnection = new IDbConnection(ConnectionString);
                IDbConnection sqlConnection = CreateConnection();

                return sqlConnection;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected static IDbConnection GetConnection()
        {
            // Use thread as a normal identifier
            int ConnectionIdentifier = GetThreadIdentifier();
            return GetConnection(ConnectionIdentifier);
        }

        protected static IDbConnection GetConnection(int ConnectionIdentifier)
        {
            if (!_Configured)
                throw new Exception("You need to configure this manager first with the Boot method");
            if (ConnectionIdentifier == 0)
                throw new Exception("You cannot use identifer 0");
            if (ConnectionPool.ContainsKey(ConnectionIdentifier) == false)
            {
                do
                {
                    while (Connections >= MaxConnections)
                    {
                        //Console.WriteLine("Max connections of " + MaxConnections + "reached.. waiting");
                        System.Threading.Thread.Sleep(100);
                    }

                    IDbConnection con;
                    try
                    {
                        lock (ConnectionPool)
                        {
                            con = Create();
                            if (con != null)
                            {
                                con.Open();
                                if (ConnectionPool.ContainsKey(ConnectionIdentifier) == false)
                                    ConnectionPool.Add(ConnectionIdentifier, con);
                                else
                                    ConnectionPool[ConnectionIdentifier] = con;
                            }
                        }

                    }
                    catch (ThreadAbortException exas)
                    {
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateInstance(new LoggerConfiguration());
                        System.Threading.Thread.Sleep(100);
                    }
                }
                while (ConnectionPool.ContainsKey(ConnectionIdentifier) == false);

            }

            return ConnectionPool[ConnectionIdentifier];
        }

    }
}
