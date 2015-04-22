using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CustomInstallFunctions
{
    class Program
    {
        static void Main(string[] args)
        {
            const string REGISTRY_PATH_LOCATION = "System\\CurrentControlSet\\Control\\Print\\Monitors\\TroySecurePortMonitor";
            const string REGISTRY_PORTS_LOCATION = REGISTRY_PATH_LOCATION + "\\Ports";
            List<string> LogInfo = new List<string>();

            LogInfo.Add("--- Setting Registry Values  ---");
            Console.Write("Setting Registry Values...");

            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey troyRegPath = registryKey.OpenSubKey(REGISTRY_PATH_LOCATION, true);
            Microsoft.Win32.RegistryKey pmKey = registryKey.OpenSubKey(REGISTRY_PORTS_LOCATION, true);
            string progFiles = System.Windows.Forms.Application.StartupPath;
            LogInfo.Add("   Startup Path = " + progFiles);

            if (!progFiles.EndsWith("\\"))
            {
                progFiles += "\\";
            }

            troyRegPath.SetValue("MainConfigurationPath", progFiles);
            LogInfo.Add("   Set MainConfigurationPath = " + progFiles);

            Console.Write(progFiles + "....");
            LogInfo.Add("   Finding Each TROYPORT in Registry");
            foreach (string valnames in pmKey.GetValueNames())
            {
                if (valnames.Contains("TROYPORT"))
                {
                    string temp = valnames.Replace("TROYPORT", "");
                    temp = temp.Replace(":", "");
                    string printpath = progFiles + @"PrintPort" + temp + @"\";
                    pmKey.SetValue(valnames, printpath);
                    LogInfo.Add("      Setting " + valnames + " = " + printpath);
                    // Console.Write(temp);
                }
                else
                {
                    LogInfo.Add("      Non-TROYPORT value found. " + valnames);
                }
            }

            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Done");
            LogInfo.Add("--- Setting Registry Values COMPLETE ---");
            LogInfo.Add(" ");


            //LogInfo.Add("--- Copying Configuration and Data Files  ---");
            //if ((args.Length > 0) && (args[0] != ""))
            //{
            //    try
            //    {
            //        if (Directory.Exists(args[0] + "TspeFiles"))
            //        {
            //            Console.WriteLine("Copying TSPE File(s)....");
            //            LogInfo.Add("   Found TSPE Files folder.");
            //            DirectoryInfo dirInfo = new DirectoryInfo(args[0] + "TspeFiles");
            //            foreach (FileInfo fi in dirInfo.GetFiles())
            //            {
            //                string fn = Path.Combine(progFiles, fi.Name);
            //                fi.CopyTo(fn, true);
            //                LogInfo.Add("      Copied " + fi.Name + " to " + fn);
            //            }
            //            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            //            {
            //                if (!Directory.Exists(progFiles + @"Baseline\" + dir.Name))
            //                {
            //                    Directory.CreateDirectory(progFiles + @"Baseline\" + dir.Name);
            //                    LogInfo.Add("      Created directory " + progFiles + @"Baseline\" + dir.Name);
            //                }
            //                foreach (FileInfo fi in dir.GetFiles())
            //                {
            //                    string fn = Path.Combine(progFiles + @"Baseline\" + dir.Name, fi.Name);
            //                    fi.CopyTo(fn, true);
            //                    LogInfo.Add("      Copied " + fi.Name + " to " + fn);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            LogInfo.Add("   No Baseline Data Files to Copy.  " + args[0] + "TspeFiles folder not found (Note: Case Sensitive)");
            //            Console.WriteLine("No data files to copy. Directory: " + args[0] + "TspeFiles");
            //        }
                    
            //        if (Directory.Exists(args[0] + "Configuration"))
            //        {
            //            Console.WriteLine("Copying Configuration File(s)....");
            //            LogInfo.Add("   Found Configuration Files folder.");
            //            DirectoryInfo dirInfo = new DirectoryInfo(args[0] + "Configuration");
            //            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            //            {
            //                Console.WriteLine("   Found Config folder.");
            //                if (dir.Name.ToUpper() == "CONFIG")
            //                {
            //                    foreach (FileInfo fi in dir.GetFiles())
            //                    {
            //                        string fn = Path.Combine(progFiles + "Configuration", fi.Name);
            //                        fi.CopyTo(fn, true);
            //                        LogInfo.Add("      Copied " + fi.Name + " to " + fn);
            //                        fn = Path.Combine(progFiles + @"PrintPort1\Config", fi.Name);
            //                        fi.CopyTo(fn, true);
            //                        LogInfo.Add("      Copied " + fi.Name + " to " + fn);
            //                    }
            //                }
            //                else if (dir.Name.ToUpper() == "DATA")
            //                {
            //                    Console.WriteLine("   Found Data folder.");
            //                    foreach (FileInfo fi in dir.GetFiles())
            //                    {
            //                        string fn = Path.Combine(progFiles + "Data", fi.Name);
            //                        fi.CopyTo(fn, true);
            //                        LogInfo.Add("      Copied " + fi.Name + " to " + fn);
            //                        fn = Path.Combine(progFiles + @"PrintPort1\Data", fi.Name);
            //                        fi.CopyTo(fn, true);
            //                        LogInfo.Add("      Copied " + fi.Name + " to " + fn);
            //                    }
            //                }
            //            }

            //        }
            //        else
            //        {
            //            Console.WriteLine("No configuration files to copy." + args[0] + "Configuration");
            //            LogInfo.Add("   No Configuration Files to Copy.  " + args[0] + "Configuration folder not found (Note: Case Sensitive)");
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("Error.  " + ex.Message);
            //        LogInfo.Add("   ERROR! " + ex.Message);
            //    }
            //}
            //else
            //{
            //    Console.Write("No call arguments. ");
            //    LogInfo.Add("   No arguments found in call to CustomInstallations");
            //}
            //Console.WriteLine("Done");
            //LogInfo.Add("--- Copying Configuration and Data Files COMPLETE  ---");
            //LogInfo.Add(" ");

            LogInfo.Add("--- Restarting Services  ---");
            System.Threading.Thread.Sleep(500);
            Console.Write("Reseting the Windows Print Spooler....");
            System.ServiceProcess.ServiceController psService = new System.ServiceProcess.ServiceController("Print Spooler");
            if (psService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
            {
                psService.Stop();
            }
            System.Threading.Thread.Sleep(1000);
            psService.Start();
            Console.WriteLine("Done");
            LogInfo.Add("   Windows Print Spooler reset.");

            Console.Write("Starting the TROY SecurePort Monitor Service....");
            System.ServiceProcess.ServiceController pmService = new System.ServiceProcess.ServiceController("Troy Port Monitor Service");
            if (pmService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
            {
                pmService.Stop();
            }
            System.Threading.Thread.Sleep(1000);
            pmService.Start();
            Console.WriteLine("Done");
            LogInfo.Add("   Troy Port Monitor Service reset.");
            LogInfo.Add("--- Restarting Services COMPLETE ---");

            string filename = Path.Combine(progFiles, "CustomInstallLog.txt");
            try
            {
                if (File.Exists(filename))
                {
                    FileInfo saveFile = new FileInfo(filename);
                    StreamWriter saveWrite = new StreamWriter(saveFile.OpenWrite());
                    foreach (string listString in LogInfo)
                    {
                        saveWrite.WriteLine(listString);
                    }
                    saveWrite.Close();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to Log.  " + ex.Message);
            }
        }
    }
}
