using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

using System.Diagnostics;

namespace TroyPortMonitorService.PclParsing
{
    public class PortMonCustomException : System.ApplicationException
    {
        private bool logToErrorLog;
        public bool LogToErrorLog
        {
            get {return logToErrorLog;}
            set {logToErrorLog = value;}
        }

        private System.Diagnostics.EventLogEntryType eventType;
        public System.Diagnostics.EventLogEntryType EventType
        {
            get {return eventType;}
        }

        private bool fatalError;
        public bool FatalError
        {
            get {return fatalError;}
        }


        public PortMonCustomException(string message, bool logError)
        {
            logToErrorLog = logError;
        }

        public PortMonCustomException(string message, bool logError, System.Diagnostics.EventLogEntryType evType, bool fatal) :
            base(message)
        {
            logToErrorLog = logError;
            eventType = evType;
            fatalError = fatal;
        }

    
    }

    class PortMonLogging
    {

        private FileStream errorFileStream;
        private FileStream traceFileStream;

        public enum TraceLogLevels
        {
            ServiceLevel = 1,
            InitialReadLevel = 2,
            FileParseLevel = 4,
            FileWriteLevel = 8,
            FilePrintLevel = 16,
            ConfigurationDump = 32
        }

        public bool ErrorLogInitialized = false;
        public bool TraceLogInitialized = false;

        private string errorLogFileName;
        private string traceLogFileName;

        private string errorLogFilePath;
        public string ErrorLogFilePath
        {
            set {errorLogFilePath = value;}
        }


        private bool logToErrorFile;
        public bool LogToErrorFile
        {
            get {return logToErrorFile;}
            set {logToErrorFile = value;}
        }

        private bool useDateInErrorLogName;
        public bool UseDateInErrorLogName
        {
            get {return useDateInErrorLogName;}
            set {useDateInErrorLogName = value;}
        }

        private bool logErrorsToEventLog;
        public bool LogErrorsToEventLog
        {
            get {return logErrorsToEventLog;}
            set {logErrorsToEventLog = value;}
        }

        private bool logStatusToEventLog;
        public bool LogStatusToEventLog
        {
            get {return logStatusToEventLog;}
            set {logStatusToEventLog = value;}
        }

        private bool enableTraceLog;
        public bool EnableTraceLog
        {
            get {return enableTraceLog;}
            set {enableTraceLog = value;}
        }

        private string traceLogPath;
        public string TraceLogPath
        {
            get {return traceLogPath;}
            set {traceLogPath = value;}
        }

        private int traceLevel;
        public int TraceLevel
        {
            get {return traceLevel;}
            set {traceLevel = value;}
        }

        private int numberOfErrorLogDays;
        public int NumberOfErrorLogDays
        {
            get {return numberOfErrorLogDays;}
            set {numberOfErrorLogDays = value;}
        }

        private int numberOfTraceLogDays;
        public int NumberOfTraceLogDays
        {
            get {return numberOfTraceLogDays;}
            set {numberOfTraceLogDays = value;}
        }

        private bool enableMessageBoxes;
        public bool EnableMessageBoxes
        {
            get { return enableMessageBoxes; }
            set { enableMessageBoxes = value; }
        }


        public bool InitializeErrorLog()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(errorLogFilePath);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                System.DateTime dt = DateTime.Now;
                string dateString = dt.ToString("yyyyMMdd");
                errorLogFileName = errorLogFilePath + "TroyPortMonError_" + dateString.ToString() + ".txt";

                FileInfo fileCheck = new FileInfo(errorLogFileName);
                FileMode fileMode;
                if (!fileCheck.Exists)
                {
                    fileMode = FileMode.CreateNew;
                }
                else
                {
                    fileMode = FileMode.Append;
                }

                errorFileStream = new FileStream(errorLogFileName, fileMode, FileAccess.Write, FileShare.ReadWrite);
                ErrorLogInitialized = true;

                if (numberOfErrorLogDays > 0)
                {
                    int findLoc;
                    uint nowNumber = Convert.ToUInt32(dateString.Substring(0, 8));
                    uint fileNumber;
                    foreach (FileInfo fi in dirInfo.GetFiles())
                    {
                        findLoc = fi.Name.IndexOf("TroyPortMonError_", 0);
                        if (findLoc > -1)
                        {
                            fileNumber = Convert.ToUInt32(fi.Name.Substring(10, 8));
                            if ((nowNumber - fileNumber) > numberOfErrorLogDays)
                            {
                                fi.Delete();
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in InitializeErrorLog(). Error: " + ex.Message, "TROY Port Monitor Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
        }

        public bool InitializeTraceLog()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(traceLogPath);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                System.DateTime dt = DateTime.Now;
                string dateString = dt.ToString("yyyyMMdd");
                traceLogFileName = traceLogPath + "\\" + "TroyPortMonTrace_" + dateString.ToString() + ".txt";

                FileInfo fileCheck = new FileInfo(traceLogFileName);
                FileMode fileMode;
                if (!fileCheck.Exists)
                {
                    fileMode = FileMode.CreateNew;
                }
                else
                {
                    fileMode = FileMode.Append;
                }

                traceFileStream = new FileStream(traceLogFileName, fileMode, FileAccess.Write, FileShare.ReadWrite);
                TraceLogInitialized = true;

                if (numberOfTraceLogDays > 0)
                {
                    int findLoc;
                    uint nowNumber = Convert.ToUInt32(dateString.Substring(0, 8));
                    uint fileNumber;
                    foreach (FileInfo fi in dirInfo.GetFiles())
                    {
                        findLoc = fi.Name.IndexOf("TroyPortMonTrace_", 0);
                        if (findLoc > -1)
                        {
                            fileNumber = Convert.ToUInt32(fi.Name.Substring(10, 8));
                            if ((nowNumber - fileNumber) > numberOfTraceLogDays)
                            {
                                fi.Delete();
                            }
                        }
                    }
                }

                string outString = "INITIALIZING THE TRACE LOG @ " + dt.ToString("hh:mi:ss mm-dd-yyyy");
                byte[] info = new UTF8Encoding(true).GetBytes(outString);
                traceFileStream.Write(info, 0, info.Length);
                traceFileStream.Flush();


                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing the Trace Log. " + ex.Message,"TROY Port Monitor Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }


        }


        public bool LogError(string errorString, EventLogEntryType eventType, bool fatalError)
        {
            try
            {
                if (LogErrorsToEventLog)
                {
                    EventLog.WriteEntry("TROY SecurePort Monitor", errorString, eventType);
                }

                if (ErrorLogInitialized)
                {

                    if (LogToErrorFile)
                    {
                        System.DateTime dt = DateTime.Now;
                        string dateString = dt.ToString("MM-dd-yyyy hh:mm:ss");
                        string outString = dateString + " " + errorString + "\r\n";
                        byte[] info = new UTF8Encoding(true).GetBytes(outString);
                        errorFileStream.Write(info, 0, info.Length);
                        errorFileStream.Flush();
                    }
                }

                if (fatalError)
                {
                    //MessageBox.Show("Fatal Error!  " + errorString, "TROY Port Monitor Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                }


                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in LogError(). Error: " + ex.Message, "TROY Port Monitor Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
        }

        public bool LogToTraceLog(string logEntry, TraceLogLevels level)
        {
            try
            {
                System.DateTime dt = DateTime.Now;
                string dateString = dt.ToString("MMddyyyy hhmmss");
                string outString;
                switch (level)
                {
                    case TraceLogLevels.ServiceLevel:
                        outString = dateString + " SERVICE: " + logEntry;
                        break;
                    case TraceLogLevels.InitialReadLevel:
                        outString = dateString + " FILE READ: " + logEntry;
                        break;
                    case TraceLogLevels.FileParseLevel:
                        outString = dateString + " PARSE: " + logEntry;
                        break;
                    case TraceLogLevels.FileWriteLevel:
                        outString = dateString + " FILE WRITE: " + logEntry;
                        break;
                    case TraceLogLevels.FilePrintLevel:
                        outString = dateString + " FILE PRINT: " + logEntry;
                        break;
                    case TraceLogLevels.ConfigurationDump:
                        outString = dateString + " CONFIGURATION: " + logEntry;
                        break;
                    default:
                        outString = dateString + " " + logEntry;
                        break;
                }
                if (outString.Length > 0)
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(outString);
                    traceFileStream.Write(info, 0, info.Length);
                    traceFileStream.Flush();
                }
                return true;

            }
            catch (Exception ex)
            {
                if (ErrorLogInitialized)
                {
                    LogError("Error in LogToTraceLog().  Error: " + ex.Message, EventLogEntryType.Error, false);
                }
                return false;
            }
        }

        public bool CloseErrorLog()
        {
            return true;
        }

        public PortMonLogging()
        {
            logToErrorFile = false;
            useDateInErrorLogName = false;
            logErrorsToEventLog = false;
            logStatusToEventLog = false;
            numberOfErrorLogDays = 0;
            numberOfTraceLogDays = 0;
        }

    }
}
