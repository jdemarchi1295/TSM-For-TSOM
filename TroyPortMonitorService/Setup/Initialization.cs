using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

//Added for Port Monitor
using System.IO;  //File Watcher
using System.Threading;  //Threading
using System.Windows.Forms; //MessageBox
using System.Xml;
using System.Xml.Serialization;
using Troy.PortMonitor.Core.XmlConfiguration;


namespace TroyPortMonitorService.Setup
{
    public static class Initialization
    {
        static PortMonitorConfigurationTsom currTpmc;


        //The path to the main PM config file
        static string serviceConfigFilePath;

        const string REGISTRY_PATH_LOCATION = "System\\CurrentControlSet\\Control\\Print\\Monitors\\TroySecurePortMonitor";
        const string REGISTRY_PORTS_LOCATION = REGISTRY_PATH_LOCATION + "\\Ports";

        static Dictionary<string, string> portsForFileWatcher = new Dictionary<string, string>();
        static Dictionary<string, PclParsing.PortMonLogging> portLogs = new Dictionary<string, PclParsing.PortMonLogging>();
        static Dictionary<string, PortMonitorConfigurationTsom> portConfigs = new Dictionary<string, PortMonitorConfigurationTsom>();
        static Dictionary<string, PclParsing.FontConfigSet> portFonts = new Dictionary<string, PclParsing.FontConfigSet>();

        static string printToFilePath;

        static bool InsertPjlHeader = false;

        /// <summary>
        /// Calls functions to read configuration and setup file watcher. 
        /// </summary>
        public static bool SetupPorts()
        {
            if (ReadMainConfiguration())
            {
                SetupFileWatchers();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool ReadMainConfiguration()
        {
            FileStream fs = null;
            try
            {
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey pmKey = registryKey.OpenSubKey(REGISTRY_PATH_LOCATION, false);

                serviceConfigFilePath = pmKey.GetValue("MainConfigurationPath").ToString();
                if ((serviceConfigFilePath.Length > 0) && (!serviceConfigFilePath.EndsWith("\\")))
                {
                    serviceConfigFilePath += "\\";
                }
                string configFile = serviceConfigFilePath + "TroyPMServiceConfiguration.xml";

                if (File.Exists(configFile))
                {
                    XmlSerializer dser = new XmlSerializer(typeof(PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration));
                    fs = new FileStream(configFile, FileMode.Open);
                    PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration tpmsc;
                    tpmsc = (PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration)dser.Deserialize(fs);

                    InsertPjlHeader = tpmsc.InsertPjlHeader;

                    string portMonName, configPath;
                    foreach (PortMonitorServiceConfigurationTsom.Port port in tpmsc.PortList)
                    {
                        if (port.MonitoredPort)
                        {
                            portMonName = port.PortMonitorName;
                            configPath = port.ConfigurationPath;
                            if ((configPath.ToUpper() != "DEFAULT") && (configPath.Length > 0) && (!configPath.EndsWith("\\")))
                            {
                                configPath += "\\";
                            }
                            if (portsForFileWatcher.ContainsKey(portMonName))
                            {
                                EventLog.WriteEntry("TROY SecurePort Monitor", "Error: Multiple Port Monitor Service defined for the same Port Monitor.  Port Monitor = " + portMonName, EventLogEntryType.Error);
                            }
                            else
                            {
                                portsForFileWatcher.Add(portMonName, configPath);
                            }
                        }
                    }
                }
                else
                {
                    EventLog.WriteEntry("TROY SecurePort Monitor", "Invalid File Path for Main Configuration File.  File: " + configFile, EventLogEntryType.Error);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("TROY SecurePort Monitor", "Fatal Error!  Error In ReadMainConfiguation. " + ex.Message.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        private static string GetPathFromRegistry(string portMonName)
        {
            string retStr;
            try
            {
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey pmKey = registryKey.OpenSubKey(REGISTRY_PORTS_LOCATION, false);

                string filePath = pmKey.GetValue(portMonName).ToString();

                retStr = filePath;
                if ((retStr.Length > 0) && (!retStr.EndsWith("\\")))
                {
                    retStr += "\\";
                }
                pmKey.Close();
                registryKey.Close();
                return retStr;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("TROY SecurePort Monitor", "Error in GetPathFromRegistry.  Error: " + ex.Message, EventLogEntryType.Error);
                return "";
            }
        }


        private static void SetupFileWatchers()
        {
            try
            {
                printToFilePath = "";
                string configPath;
                FileSystemWatcher fileWatcher;
                foreach (KeyValuePair<string, string> kvp in portsForFileWatcher)
                {
                    printToFilePath = GetPathFromRegistry(kvp.Key);
                    if (kvp.Value == "")
                    {
                        EventLog.WriteEntry("TROY SecurePort Monitor",
                            "Invalid Configuation Path for port name: " + kvp.Key, EventLogEntryType.Error);
                        configPath = "";
                    }
                        //Default is the print to file path + the config folder
                    else if (kvp.Value.ToUpper() == "DEFAULT")
                    {
                        configPath = printToFilePath + "Config\\";
                    }
                    else
                    {
                        configPath = kvp.Value.ToString();
                    }

                    string FileExt = "";
                    if (configPath != "")
                    {
                        if (!configPath.EndsWith("\\"))
                        {
                            configPath += "\\";
                        }

                        DirectoryInfo dirInfo = new DirectoryInfo(configPath);
                        if (!dirInfo.Exists)
                        {
                            EventLog.WriteEntry("TROY SecurePort Monitor",
                                "Configuration Path does not exist. Path: " + configPath, EventLogEntryType.Error);
                        }
                        else
                        {
                            string configFile = configPath + "PortMonitorConfigurationTsom.xml";
                            ReadPortMonitorConfiguration(configFile, printToFilePath, ref FileExt);

                        }
                    }

                    fileWatcher = new FileSystemWatcher();

                    DirectoryInfo dirCheck = new DirectoryInfo(printToFilePath);
                    if (!dirCheck.Exists)
                    {
                        dirCheck.Create();
                    }
                    else
                    {
                        foreach (FileInfo file in dirCheck.GetFiles("*." + FileExt))
                        {
                            file.Delete();
                        }
                        foreach (FileInfo file2 in dirCheck.GetFiles("*.bak"))
                        {
                            file2.Delete();
                        }
                    }

                    fileWatcher.InternalBufferSize = 12288;
                    fileWatcher.Path = printToFilePath;
                    fileWatcher.NotifyFilter = NotifyFilters.FileName;
                    fileWatcher.Filter = "*." + FileExt;
                    fileWatcher.IncludeSubdirectories = false;

                    //EVENTS HANDLERS (Note: Only one event handler is needed for both events)
                    fileWatcher.Changed += new FileSystemEventHandler(fileWatcherService_Changed);
                    fileWatcher.Created += new FileSystemEventHandler(fileWatcherService_Changed);


                    //ENABLE
                    fileWatcher.EnableRaisingEvents = true;
                    Globals.FileWatchers.fileWatchers.Add(fileWatcher);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("TROY SecurePort Monitor", "Error initializing FileWatchers.  Error: " + ex.Message,
                    EventLogEntryType.Error);
            }
            finally
            {
#if (DEBUG)
                Console.WriteLine("waiting...");
                Console.Read();
#endif
            }
        }



        private static bool ReadPortMonitorConfiguration(string configFileName, string fileWatcherPath, ref string FileExt)
        {
            try
            {
                XmlSerializer dser = new XmlSerializer(typeof(PortMonitorConfigurationTsom));
                FileStream fs = new FileStream(configFileName, FileMode.Open);
                currTpmc = (PortMonitorConfigurationTsom)dser.Deserialize(fs);
                fs.Close();

                //Backup
                if (currTpmc.DebugBackupFilesPath.ToUpper() == "DEFAULT")
                {
                    if (!Directory.Exists(fileWatcherPath + "Backup"))
                    {
                        Directory.CreateDirectory(fileWatcherPath + "Backup");
                    }
                    currTpmc.DebugBackupFilesPath = fileWatcherPath + "Backup";
                }
                if ((currTpmc.DebugBackupFilesPath != "") &&
                    (!currTpmc.DebugBackupFilesPath.EndsWith("\\")))
                {
                    currTpmc.DebugBackupFilesPath += "\\";
                }

                //Error Log
                if (currTpmc.ErrorLogPath.ToUpper() == "DEFAULT")
                {
                    if (!Directory.Exists(fileWatcherPath + "ErrorLog"))
                    {
                        Directory.CreateDirectory(fileWatcherPath + "ErrorLog");
                    }
                    currTpmc.ErrorLogPath = fileWatcherPath + "ErrorLog\\";
                }
                if ((currTpmc.ErrorLogPath != "") &&
                    (!currTpmc.ErrorLogPath.EndsWith("\\")))
                {
                    currTpmc.ErrorLogPath += "\\";
                }

                //Encryption Password
                int cdataLoc = currTpmc.EncryptPassword.IndexOf("[CDATA[");
                if (cdataLoc > -1)
                {
                    cdataLoc += 7;
                    currTpmc.EncryptPassword = currTpmc.EncryptPassword.Substring(cdataLoc, currTpmc.EncryptPassword.Length - cdataLoc - 2);
                }

                string glyphFile = currTpmc.TsomFontCharacterMapFile;
                if (!glyphFile.Contains("\\"))
                {
                    glyphFile = fileWatcherPath + "Config\\" + glyphFile;
                }
                ReadFontConfiguration(currTpmc.TsomSerializationDataFont, glyphFile, fileWatcherPath);

                portConfigs.Add(fileWatcherPath, currTpmc);

                PclParsing.PortMonLogging plogger = new PclParsing.PortMonLogging();
                plogger.EnableMessageBoxes = currTpmc.EnableErrorMessageBoxes;
                plogger.EnableTraceLog = false;
                if (currTpmc.ErrorLogPath != "")
                {
                    plogger.ErrorLogFilePath = currTpmc.ErrorLogPath;
                    plogger.LogToErrorFile = true;
                    plogger.InitializeErrorLog();
                }
                plogger.LogErrorsToEventLog = currTpmc.LogErrorsToEventLog;

                portLogs.Add(fileWatcherPath, plogger);

                FileExt = currTpmc.FileExtension;
                fs.Close();

                return true;

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("TROY SecurePort Monitor","Fatal Error!  Error In ReadPortMonitorConfiguration. " + ex.Message.ToString(),EventLogEntryType.Error);
                return false;
            }
        }

        private static void fileWatcherService_Changed(object source, FileSystemEventArgs e)
        {
            try
            {

                PclParsing.PrintJobThread printJob = new PclParsing.PrintJobThread();
                printJob.printFileName = e.FullPath;

                FileInfo getPath = new FileInfo(e.FullPath);

                string filePath = getPath.DirectoryName.ToString();
                if ((filePath.Length > 0) && (!filePath.EndsWith("\\")))
                {
                    filePath += "\\";
                }
                printJob.InsertPjlHeader = InsertPjlHeader;
                printJob.MonitorLocation = filePath;

                PortMonitorConfigurationTsom portMonConfig = portConfigs[filePath];
                if (portMonConfig != null)
                {
                    printJob.pmConfig = portMonConfig;
                }
                else
                {
                    Exception ex1 = new Exception("Error:  Could not find Configuration for file path " + filePath);
                    throw ex1;
                }

                PclParsing.PortMonLogging portMonLogging = portLogs[filePath];
                if (portLogs != null)
                {
                    printJob.pmLogging = portMonLogging;
                }

                PclParsing.FontConfigSet fontConfig = portFonts[filePath];
                if (fontConfig != null)
                {
                    printJob.fontConfigs = fontConfig;
                }

                Thread printJobThread = new Thread(new ThreadStart(printJob.PrintJobReceived));

                printJobThread.Name = e.Name + " thread";
                printJobThread.IsBackground = true;
                printJobThread.Priority = ThreadPriority.Normal;

                printJobThread.Start();

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("TROY SecurePort Monitor", "Error in File Watcher Handler.  Error: " + ex.Message, EventLogEntryType.Error);

            }

        }

        private static void ReadFontConfiguration(string fontName, string GlyphFileName, string fileWatcherPath)
        {
            try
            {
                string leadingPcl = "";
                string trailingPcl = "";
                string fontType = "SERIALIZATION";

                PclParsing.FontConfigSet fontConfigs = new PclParsing.FontConfigSet();
                fontConfigs.AddFont(fontName, leadingPcl, trailingPcl, GlyphFileName, fontType, false);

                portFonts.Add(fileWatcherPath, fontConfigs);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("TROY SecurePort Monitor", "Error in ReadFontConfiguration.  Error: " + ex.Message, EventLogEntryType.Error);
            }
        }
    }
}
