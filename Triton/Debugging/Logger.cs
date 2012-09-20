using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Triton.Debugging
{
    public delegate void LogSignal(LogMessage m);

    public enum LoggingType
    {
        File = 1,
        Console = 2,
        Window = 4

    }

    public class LoggerConfiguration : UnitTestable
    {
        public string FileLocation;
        public LoggingType Type;
#if DEBUG
        internal override void _ExecuteTests()
        {
            LoggerConfiguration config = new LoggerConfiguration();
            config.Type = LoggingType.File;

            UnitTester.Test("Logtype set: FILE");
            UnitTester.Assert(config.IsLoggingFile);
            UnitTester.Assert(!config.IsLoggingConsole);
            UnitTester.Assert(!config.IsLoggingWindow);

            config.Type = LoggingType.Console;

            UnitTester.Test("Logtype set: CONSOLE");
            UnitTester.Assert(!config.IsLoggingFile);
            UnitTester.Assert(config.IsLoggingConsole);
            UnitTester.Assert(!config.IsLoggingWindow);

            config.Type = LoggingType.Window;

            UnitTester.Test("Logtype set: WINDOW");
            UnitTester.Assert(!config.IsLoggingFile);
            UnitTester.Assert(!config.IsLoggingConsole);
            UnitTester.Assert(config.IsLoggingWindow);
        }
#else
        internal override void _ExecuteTests()
        {


        }
#endif
        public bool IsLoggingFile
        {
            get
            {
                return ((Type & LoggingType.File) != 0) ? true : false;
            }
        }

        public bool IsLoggingConsole
        {
            get
            {
                return ((Type & LoggingType.Console) != 0) ? true : false;
            }
        }
        public bool IsLoggingWindow
        {
            get
            {
                return ((Type & LoggingType.Window) != 0) ? true : false;
            }
        }

        public LoggerConfiguration()
        {
            // Nothing to see here..
            base._Register(this);
        }

        internal void Notify(LogMessage m)
        {
            if (LogNotifiers != null)
                LogNotifiers(m);
        }

        public event LogSignal LogNotifiers;

    }

    public class LogMessage
    {
        internal int _Trace_FrameNo = 2;

        internal Assembly Assembly;
        public DateTime Time;
        public string Message;

        public string Trace_Method
        {
            get { return this._StackTrace.GetFrame(_Trace_FrameNo).GetMethod().Name; }
        }
        public string Trace_Class
        {
            get { return this._StackTrace.GetFrame(_Trace_FrameNo).GetMethod().ReflectedType.Name; }
        }
        public StackTrace StackTrace
        {
            get
            {
                return this._StackTrace;
            }
            internal set
            {
                this._StackTrace = value;
            }
        }
        private StackTrace _StackTrace = null;
        public object Sender;
        private Type _SenderType;
        public Type SenderType
        {
            get
            {
                if (this._SenderType != null) return this._SenderType;
                else if (Sender != null) return Sender.GetType();
                else return typeof(object);
            }
            set
            {
                this._SenderType = value;
            }
        }
        public Exception Exception;
    }

    public class LogInstance
    {
        public LoggerConfiguration Config;
        public string Application;
        public Assembly Assembly;
        private List<LogMessage> _Messages = new List<LogMessage>();

        public LogInstance()
        {

        }

        public void Add(LogMessage m)
        {
            _Messages.Add(m);

            Config.Notify(m);
        }

        public LogMessage[] Messages
        {
            get { return this._Messages.ToArray(); }

        }



    }


    public class Logger
    {
        private static List<LogInstance> Instances = new List<LogInstance>();

        private string _LogInstantation;

#if !DEBUG
        public static LogInstance GetOfTriton()
        {
            Logger.Log("Triton logger retrieved");
            return GetByAssembly(Assembly.GetAssembly(typeof(Logger)));
        }
#endif

        public event LogSignal LogNotifiers
        {
            add
            {
                GetByApplication(_LogInstantation).Config.LogNotifiers += value;
            }
            remove
            {
                GetByApplication(_LogInstantation).Config.LogNotifiers -= value;
            }
        }

        public static void Log(object sender, string message)
        {
            Assembly assem = Assembly.GetAssembly(sender.GetType());

            _StoreLog(assem, message, sender);

        }

        public static void Log(Type typ, string message)
        {
            Assembly assem = Assembly.GetAssembly(typ);

            _StoreLog(assem, message);
        }

        public static void Log(string message)
        {
            Assembly assem = Assembly.GetCallingAssembly();

            _StoreLog(assem, message);
        }

        private static void _StoreLog(Assembly assem, string message)
        {
            _StoreLog(assem, message, null);
        }

        private static void _StoreLog(Assembly assem, string message, object sender)
        {

            LogMessage m = new LogMessage();
            m.StackTrace = new StackTrace();
            m.Message = message;
            m.Time = DateTime.Now;
            m.Assembly = assem;
            m.Sender = sender;

            if (sender != null)
                m._Trace_FrameNo++;

            if (GetByAssembly(assem) == null)
                CreateInstance(assem, new LoggerConfiguration());

            GetByAssembly(assem).Add(m);

        }
        internal Logger(LogInstance logger)
        {
            Instances.Add(logger);

            _LogInstantation = logger.Application;
        }

        public static LoggerConfiguration GetConfiguration(LogMessage m)
        {
            LogInstance inst = GetByAssembly(m.Assembly);
            return inst.Config;

        }

        public static LogInstance GetInstance(LogMessage m)
        {
            return GetByAssembly(m.Assembly);

        }

        private static LogInstance GetByApplication(string Application)
        {
            LogInstance inst = Instances.Find(delegate(LogInstance i) { return i.Application == Application; });
            return inst;
        }

        private static LogInstance GetByAssembly(Assembly assem)
        {
            LogInstance inst = Instances.Find(delegate(LogInstance i) { return i.Assembly == assem; });
            return inst;
        }

        public static Logger CreateInstance(string Application, LoggerConfiguration Config)
        {
            LogInstance i = new LogInstance();
            i.Config = Config;
            i.Assembly = Assembly.GetCallingAssembly();
            i.Application = Application;

            Logger l = new Logger(i);

            return l;
        }

        internal static Logger CreateInstance(Assembly assem, LoggerConfiguration Config)
        {

            LogInstance i = new LogInstance();
            i.Config = Config;
            i.Assembly = assem;
            i.Application = assem.FullName;

            Logger l = new Logger(i);

            return l;
        }


        public static Logger CreateInstance(LoggerConfiguration Config)
        {
            Assembly assem = Assembly.GetCallingAssembly();

            LogInstance i = new LogInstance();
            i.Config = Config;
            i.Assembly = assem;
            i.Application = assem.FullName;

            Logger l = new Logger(i);

            return l;
        }

        public static Logger CreateInstance(object sender, LoggerConfiguration Config)
        {
            Assembly assem = Assembly.GetAssembly(sender.GetType());
            return CreateInstance(assem, Config);
        }

    }

}
