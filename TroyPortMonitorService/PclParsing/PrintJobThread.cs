using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Troy.PortMonitor.Core.XmlConfiguration;

namespace TroyPortMonitorService.PclParsing
{



    class PrintJobThread
    {
        //Interface to service variables
        public string printFileName;
        public PortMonitorConfigurationTsom pmConfig;
        public PortMonLogging pmLogging;
        public string MonitorLocation = "";
        public FontConfigSet fontConfigs;
        public bool InsertPjlHeader = false;


        private class GlyphMapType
        {
            const int MaxNumberOfGlyphs = 255;
            public int[] GlyphMap = new int[MaxNumberOfGlyphs + 1];
            public void AddToGlyphMap(int fontCharId, int glyphId)
            {
                if (fontCharId <= MaxNumberOfGlyphs)
                {
                    GlyphMap[fontCharId] = glyphId;
                }
            }
            public int GetGlyphId(int fontCharId)
            {
                if (fontCharId <= MaxNumberOfGlyphs)
                {
                    return GlyphMap[fontCharId];
                }
                else
                {
                    return -1;
                }
            }
        }

        private const byte ESC = 0x1B;

        private byte[] DEFAULT_PAPER_TRAY_MAPPING_PCL = new byte[5] { ESC, 0x25, 0x69, 0x30, 0x54 }; //ESC%i0T
        private byte[] MICR_MODE_PCL = new byte[9] { ESC, 0x25, 0x2D, 0x31, 0x32, 0x34, 0x30, 0x30, 0x58 }; //ESC%-12400W
        private byte[] AUTO_PAGE_ROTATE = new byte[5] { ESC, 0x25, 0x6F, 0x31, 0x52 };   //ESC%o1R
        private byte[] LOGIN_COMMAND_PCL = new byte[5] { ESC, 0x25, 0x75, 0x31, 0x53 };  //ESC%u1S
        private byte[] ACCOUNT_PASSWORD_NONE_PCL = new byte[5] { ESC, 0x25, 0x70, 0x30, 0x57 }; //ESC%p0W

        private string HP_JOB_PASSWORD_PJL = "@PJL JOB NAME= \"TROY Job\" PASSWORD=";
        private string TROY_JOB_PIN_PJL_HEADER = "@PJL TROY JOB NAME ";
        private string TROY_JOB_PIN_PJL_TRAIL = " PIN=";
        private string TROY_MICR_PIN_PJL = "@PJL TROY MICR UNLOCK PIN=";
        private string TROY_PRINTER_PIN_PJL = "@PJL TROY PRINTER UNLOCK PIN=";

        private const int MAX_INSERT_PJL_BYTES_NEEDED = 150;


        //The File
        private FileInfo printJobFileInfo;

        //The Input Buffer
        private byte[] inputBuffer;

        //Constants
        private const int MAX_FONT_ID_DIGITS = 5;
        private const int MAX_FONT_DESC_DIGITS = 6;
        private const int FONT_NAME_LOC = 48;
        private const int MAX_POS_DIGITS = 4;
        private const int MAX_PAGES_PER_JOB = 1000;
        private const int MAX_MICR_LINES_PER_PAGE = 3;
        private const int MAX_DIG_SIGS_PER_PAGE = 10;

        private int LengthOfLeadingPcl = 64;
        private int LengthOfTrailingPcl = 64;

        private string printToPrinterName = "";

        private Dictionary<int, FontConfigOrig> fontConfigList = new Dictionary<int, FontConfigOrig>();
        private StringBuilder fontCharStr = new StringBuilder();
        private Dictionary<int, GlyphMapType> fontCharToGlyphMap = new Dictionary<int, GlyphMapType>();

        private int EndPjlLocation = -1;

        private int currentPageNum = 1;

        private string tempFileName = "";
        private string encFileName = "";
        private string printerFileName = "";

        private enum EventPointType
        {
            epInsert = 0,
            epRemove = 1,
            epSubstitute = 2,
            epInsertPoint = 3,
            epPageEnd = 4,
            epUELLocation = 5,
            epPrinterReset = 6,
            epPaperSource = 7
        }

        private struct EventPoints
        {
            public int PageNumber;
            public int Location;
            public EventPointType EventType;
            public int EventLength;
            public EventPoints(int pageNumber, int location, EventPointType eventType, int eventLength)
            {
                PageNumber = pageNumber;
                Location = location;
                EventType = eventType;
                EventLength = eventLength;
            }
        }
        private List<EventPoints> fileEventPoints;
        EventPoints newEventPoint;

        private const int pjlSetJobNameLength = 17;

        private bool JobPrinted = false;

        private bool fileReady = false;

        //Made these public so the Blob Viewer can use it
        public List<byte[]> EndOfPageStringList = new List<byte[]>();
        public byte[] InsertPointString;
        public int NumberOfPages = 0;
        private bool SaveDataForBlobViewer = false;

        //*p6400Y<FF>
        private byte[] DefaultEndOfPage = new byte[9] { 0x1B, 0x2A, 0x70, 0x36, 0x34, 0x30, 0x30, 0x59, 0x0C };
        //*p0x0Y
        private byte[] DefaultInsertPoint = new byte[7] { 0x1B, 0x2A, 0x70, 0x30, 0x78, 0x30, 0x59 };

        Blob.BlobProcessor blobProcessor = new Blob.BlobProcessor();
        private string SerializedData = "";
        
        //Use the with the Blob Viewer
        public Dictionary<int, string> BlobPerPage = new Dictionary<int, string>();
        public int EndOfPageUsedIndex = -1;

        private const int page2PclStartPageLength = 14;
        private byte[] page2PclStartPage = new byte[page2PclStartPageLength] { 0x1B, 0x26, 0x6C, 0x38, 0x63, 0x31, 0x45, 0x1B, 0x2A, 0x70, 0x30, 0x78, 0x30, 0x59 };

        private const int page2PclEndPageLength = 18;
        private byte[] page2PclEndPage = new byte[page2PclEndPageLength] { 0x1B, 0x2A, 0x62, 0x30, 0x4D, 0x1B, 0x2A, 0x72, 0x42, 0x1B, 0x2A, 0x70, 0x36, 0x34, 0x30, 0x30, 0x59, 0x0C };

        bool TSOMEnableDuplex = false;

        //********************************************************************************************
        // PRINT JOB RECEIVED
        //    The function that is called by the Windows Service.  The main function for the thread.
        //
        //********************************************************************************************
        public void PrintJobReceived()
        {
            FileStream mainFileStream = null;

            try
            {
                //Create an instance of the custom exception
                PortMonCustomException PortMonException;

                if (pmConfig.DefaultPrinter.Length < 1)
                {
                    if (pmConfig.FatalErrorIfNotDefaultPrinter)
                    {
                        //Default printer not defined. End the job.
                        PortMonException = new PortMonCustomException("A printer is not defined for this job.  Default printer not found. File: " + printFileName, true, EventLogEntryType.Error, true);
                        throw PortMonException;
                    }
                }
                else
                {
                    printToPrinterName = pmConfig.DefaultPrinter;
                }

                //Verify that the file triggering the event exists and then free the file info pointer
                printJobFileInfo = new FileInfo(printFileName);
                if (printJobFileInfo.Exists == false)
                {
                    PortMonException = new PortMonCustomException("Error opening the file from Port Monitor. " + printFileName, true, EventLogEntryType.Error, true);
                    throw PortMonException;
                }

                //Create a file name for the new temp file
                tempFileName = printJobFileInfo.FullName.Replace(printJobFileInfo.Extension, ".bak");

                int attemptCntr = 0;
                int maxAttempts = pmConfig.ReadAttempts;
                while (!fileReady)
                {
                    try
                    {
                        using (FileStream inputStream = File.Open(printFileName, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            fileReady = true;
                            inputStream.Close();
                        }
                    }
                    catch (IOException IOEx)
                    {
                        if (++attemptCntr >= maxAttempts)
                        {
                            throw IOEx;
                        }
                        Thread.Sleep(pmConfig.FileReadDelay_ms);
                    }
                }

                while (!(TopOfQueue(printJobFileInfo)))
                {
                    Thread.Sleep(pmConfig.QueueDelay_ms);
                }

                printJobFileInfo.Refresh();
                if (printJobFileInfo.Length == 0)
                {
                    JobPrinted = true;
                    return;
                }

                //Initialize the file input buffer
                mainFileStream = new FileStream(printFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                BinaryReader binReader = new BinaryReader(mainFileStream);
                inputBuffer = new Byte[mainFileStream.Length];
                inputBuffer = binReader.ReadBytes(Convert.ToInt32(mainFileStream.Length));

                fileEventPoints = new List<EventPoints>();
                //insertStringList = new List<InsertStrings>();

                //Find the end of the PJL and beginning of the PCL
                EndPjlLocation = FindPjlEnterPcl();
                if (EndPjlLocation < 0)
                {
                    PortMonException = new PortMonCustomException("File Does Not Contain PJL String Enter PCL. File: " + printFileName, true, EventLogEntryType.Error, true);
                    throw PortMonException;
                }

                if (pmConfig.EndOfPagePcl.Count > 0)
                {
                    foreach (string str in pmConfig.EndOfPagePcl)
                    {
                        string tmpstr;
                        tmpstr = str.Replace("/e", "\u001B");
                        tmpstr = tmpstr.Replace("/f", "\u000C");
                        byte[] temp = new UTF8Encoding(true).GetBytes(tmpstr);
                        EndOfPageStringList.Add(temp);
                    }
                }
                else
                {
                    EndOfPageStringList.Add(DefaultEndOfPage);
                }

                if (pmConfig.InsertPointPcl != "")
                {
                    string tmpstr = pmConfig.InsertPointPcl.Replace("/e", "\u001B");
                    InsertPointString = new UTF8Encoding(true).GetBytes(tmpstr);
                }
                else
                {
                    InsertPointString = DefaultInsertPoint;
                }


                //Read the file Input Buffer and find the beginning and end of pages and the fonts
                if (!ReadInputBuffer())
                {
                    PortMonException = new PortMonCustomException("Error in ReadInputBuffer()", false, EventLogEntryType.Error, true);
                    throw PortMonException;
                }


                //Write out the data to a new file
                if (!WriteOutPcl())
                {
                    //Errors will be logged in the function
                    PortMonException = new PortMonCustomException("Error in WriteOutPcl()", false, EventLogEntryType.Error, true);
                    throw PortMonException;
                }

                mainFileStream.Close();

                //Print the new file
                if (!PrintTheJob())
                {
                    //Errors will be logged in the function
                    PortMonException = new PortMonCustomException("Error in PrintTheJob()", false, EventLogEntryType.Error, true);
                    throw PortMonException;
                }
                JobPrinted = true;

            }
            //Catch custom errors
            catch (PortMonCustomException pme)
            {
                pmLogging.LogError(pme.Message.ToString(), pme.EventType, pme.FatalError);
            }
            //Catch other errors
            catch (Exception ex)
            {
                pmLogging.LogError("Error in PrintJobReceived(). File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
            }
            finally
            {
                int attemptCntr = 0;
                int maxAttempts = pmConfig.ReadAttempts;
                if (!JobPrinted)
                {
                    while (!fileReady)
                    {
                        try
                        {
                            using (FileStream inputStream = File.Open(printFileName, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                fileReady = true;
                                inputStream.Close();
                            }
                        }
                        catch (IOException IOEx)
                        {
                            if (++attemptCntr >= maxAttempts)
                            {
                                throw IOEx;
                            }
                            Thread.Sleep(pmConfig.FileReadDelay_ms);
                        }
                    }

                    if (fileReady)
                    {
                        if (printToPrinterName.Length > 1)
                        {
                            printerFileName = printFileName;
                            if (mainFileStream != null)
                            {
                                mainFileStream.Close();
                            }
                            PrintToSpooler.SendFileToPrinter(printToPrinterName, printerFileName, "TROY Port Monitor Pass Through");
                        }
                    }

                }
                if (mainFileStream != null)
                {
                    mainFileStream.Close();
                }
                FileCleanup();
            }

        }

        public bool CallFromBlobViewer()
        {
            try
            {
                SaveDataForBlobViewer = true;
                FileStream mainFileStream = null;

                //Initialize the file input buffer
                mainFileStream = new FileStream(printFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                BinaryReader binReader = new BinaryReader(mainFileStream);
                inputBuffer = new Byte[mainFileStream.Length];
                inputBuffer = binReader.ReadBytes(Convert.ToInt32(mainFileStream.Length));

                fileEventPoints = new List<EventPoints>();

                //Find the end of the PJL and beginning of the PCL
                EndPjlLocation = FindPjlEnterPcl();
                if (EndPjlLocation < 0)
                {
                }

                if (pmConfig.EndOfPagePcl.Count > 0)
                {
                    foreach (string str in pmConfig.EndOfPagePcl)
                    {
                        string tmpstr;
                        tmpstr = str.Replace("/e", "\u001B");
                        tmpstr = tmpstr.Replace("/f", "\u000C");
                        byte[] temp = new UTF8Encoding(true).GetBytes(tmpstr);
                        EndOfPageStringList.Add(temp);
                    }
                }
                else
                {
                    EndOfPageStringList.Add(DefaultEndOfPage);
                }

                if (pmConfig.InsertPointPcl != "")
                {
                    string tmpstr = pmConfig.InsertPointPcl.Replace("/e", "\u001B");
                    InsertPointString = new UTF8Encoding(true).GetBytes(tmpstr);
                }
                else
                {
                    InsertPointString = DefaultInsertPoint;
                }

                //Read the file Input Buffer and find the beginning and end of pages and the fonts
                if (!ReadInputBuffer())
                {
                    return false;
                }

                mainFileStream.Close();
                NumberOfPages = currentPageNum;
                
                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error CallFromBlobViewer. Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }
        }

        private bool CharIsNumeric(char inChar)
        {
            switch (inChar)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return true;
                default:
                    return false;
            }

        }

        private bool TopOfQueue(FileInfo currentFile)
        {
            try
            {
                string timestamp, compareTimestamp;
                bool returnVal = true;

                timestamp = currentFile.Name.Substring(pmConfig.FilePrefix.Length, 12);

                DirectoryInfo dirInfo = new DirectoryInfo(currentFile.Directory.ToString());
                foreach (FileInfo fi in dirInfo.GetFiles())
                {
                    if (fi.Name != currentFile.Name)
                    {
                        if (CheckValidName(fi))
                        {
                            compareTimestamp = fi.Name.Substring(pmConfig.FilePrefix.Length, 12);
                            if (Convert.ToInt64(timestamp) > Convert.ToInt64(compareTimestamp))
                            {
                                returnVal = false;
                            }
                        }
                    }
                }


                return returnVal;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in checking the queue for file: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                throw; // Goal is to rethrow the exception to get the thread to end.
                //return false;
            }

        }

        private bool CheckValidName(FileInfo checkFileInfo)
        {
            try
            {
                int baseLocation;

                baseLocation = checkFileInfo.FullName.IndexOf(MonitorLocation);
                if (baseLocation < 0)
                {
                    return false;
                }

                if (checkFileInfo.Extension.IndexOf(pmConfig.FileExtension) < 0)
                {
                    return false;
                }

                if (checkFileInfo.Name.IndexOf(pmConfig.FilePrefix) < 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in checking the file name format: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                throw; // Goal is to rethrow the exception to get the thread to end.
                //return false;
            }


        }


        private int FindPjlEnterPcl()
        {
            try
            {
                const int pjlEnterLangLength = 23;
                //@PJL ENTER LANGUAGE=PCL
                byte[] pjlEnterLang = new byte[pjlEnterLangLength] { 0x40, 0x50, 0x4A, 0x4C, 0x20, 0x45, 0x4E, 0x54, 0x45, 0x52, 0x20, 0x4C, 0x41, 0x4E, 0x47, 0x55, 0x41, 0x47, 0x45, 0x3D, 0x50, 0x43, 0x4C };

                bool continueLoop = true;
                int matchCntr = 0, cntr = 0;
                int bufferSize = inputBuffer.Length;

                int returnValue = -1;

                while ((cntr < bufferSize) && (continueLoop))
                {
                    if (inputBuffer[cntr] == pjlEnterLang[matchCntr])
                    {
                        matchCntr++;
                        if (matchCntr == pjlEnterLangLength)
                        {
                            continueLoop = false;
                            cntr = cntr + 4;
                            returnValue = cntr;
                        }
                    }
                    else
                    {
                        matchCntr = 0;
                    }
                    cntr++;
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in FindPjlEnterPcl(). File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                return -1;
            }
        }

        private bool ReadInputBuffer()
        {
            try
            {
                bool startFound = false, endFound = false;

                bool fontSelectedById = false;
                int selectedFont = 0;

                bool continueLoop = true;
                //int matchCntr = 0, 
                int cntr;
                int bufferSize = inputBuffer.Length;

                cntr = EndPjlLocation + 1;
                int EndLength = 0;

                while ((cntr < bufferSize) && (continueLoop))
                {
                    //Escape character
                    if (inputBuffer[cntr] == ESC)
                    {
                        if (CheckForInsertPoint(cntr))
                        {
                            //Add an entry in the file event list for a page insert location
                            if (!startFound)
                            {
                                newEventPoint = new EventPoints(currentPageNum, cntr, EventPointType.epInsertPoint, InsertPointString.Length);
                                fileEventPoints.Add(newEventPoint);
                                startFound = true;
                                endFound = false;
                            }
                            else
                            {
                                if (pmConfig.LogWarningMessages)
                                {
                                    pmLogging.LogError("Warning: Found the string *p0x0Y at position " + cntr.ToString() + " while looking for an end of job. File: " + printFileName, EventLogEntryType.Warning, false);
                                }
                            }
                            cntr += InsertPointString.Length;
                        }
                        //<ESC>E - Printer reset
                        else if ((cntr + 1 <= bufferSize) &&
                                 (inputBuffer[cntr + 1] == 0x45))
                        {
                            newEventPoint = new EventPoints(currentPageNum, cntr, EventPointType.epPrinterReset, 9);
                            fileEventPoints.Add(newEventPoint);
                            cntr += 1;
                        }
                        //UEL &-12345X
                        else if ((cntr + 8 <= bufferSize) &&
                                 ((inputBuffer[cntr + 1] == 0x25) && (inputBuffer[cntr + 2] == 0x2D) &&
                                  (inputBuffer[cntr + 3] == 0x31) && (inputBuffer[cntr + 4] == 0x32) &&
                                  (inputBuffer[cntr + 5] == 0x33) && (inputBuffer[cntr + 6] == 0x34) &&
                                  (inputBuffer[cntr + 7] == 0x35) && (inputBuffer[cntr + 8] == 0x58)))
                        {
                            if (endFound == false)
                            {
                                if ((cntr - 3 > 0) &&
                                    (inputBuffer[cntr - 1] == 0x45) && (inputBuffer[cntr - 2] == 0x1B) &&
                                    (inputBuffer[cntr - 3] == 0x0C))
                                {
                                    newEventPoint = new EventPoints(currentPageNum, cntr, EventPointType.epPageEnd, 9);
                                    fileEventPoints.Add(newEventPoint);
                                }
                            }
                            newEventPoint = new EventPoints(currentPageNum, cntr, EventPointType.epUELLocation, 9);
                            fileEventPoints.Add(newEventPoint);
                            cntr += 8;
                        }
                        //Paper Source &l#H where # is a number 1-9
                        else if ((cntr + 4 <= bufferSize) &&
                                 ((inputBuffer[cntr + 1] == 0x26) && (inputBuffer[cntr + 2] == 0x6C) &&
                                  (inputBuffer[cntr + 4] == 0x48)))
                        {
                            //newEventPoint = new EventPoints(currentPageNum, cntr, EventPointType.epPaperSource, 9);
                            //fileEventPoints.Add(newEventPoint);
                            cntr += 4;
                        }
                        //*c - begining of a font definition possibly
                        else if ((cntr + 2 <= bufferSize) &&
                                 ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x63)))
                        {
                            if ((inputBuffer[cntr + 3] == 0x30) && (inputBuffer[cntr + 4] == 0x74))
                            {
                                cntr += 5;
                            }
                            //Added for Yale-New Haven to remove the white rectangle.  *c1P is erase. 
                            //else if ((inputBuffer[cntr + 3] == 0x31) && (inputBuffer[cntr + 4] == 0x50))
                            //{
                            //    newEventPoint = new EventPoints(currentPageNum, cntr, EventPointType.epRemove, 5);
                            //    fileEventPoints.Add(newEventPoint);
                            //    cntr += 5;
                            //}
                            else
                            {
                                EvaluateFontDescription(ref cntr, ref selectedFont, ref fontSelectedById);
                            }
                        }
                        // ( - font call
                        else if ((cntr + 1 <= bufferSize) &&
                                 (inputBuffer[cntr + 1] == 0x28))
                        {
                            EvaluateSymbolSet(ref cntr, ref selectedFont, ref fontSelectedById);
                        }
                        //Start raster data *r1A
                        else if ((cntr < cntr + 5) &&
                                 ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x72) &&
                                  (inputBuffer[cntr + 3] == 0x31) && (inputBuffer[cntr + 4] == 0x41)))
                        {
                            cntr += 5;
                            bool foundEndRaster = false;
                            //loop until end of raster is found
                            while ((cntr < bufferSize) && (!foundEndRaster))
                            {
                                if (inputBuffer[cntr] == 0x1B)
                                {

                                    if ((cntr < cntr + 4) &&
                                        ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x72) &&
                                         (inputBuffer[cntr + 3] == 0x42)))
                                    {
                                        foundEndRaster = true;
                                        //IMPORTANT: NEEDS TO EXIT WITH CNTR POINTING TO ESCAPE SO DO NOT INCREMENT CNTR IN CASE THIS IS ALSO THE END OF PAGE
                                    }
                                    else if ((cntr < cntr + 4) &&
                                        ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x72) &&
                                         (inputBuffer[cntr + 3] == 0x43)))
                                    {
                                        foundEndRaster = true;
                                        //IMPORTANT: NEEDS TO EXIT WITH CNTR POINTING TO ESCAPE SO DO NOT INCREMENT CNTR IN CASE THIS IS ALSO THE END OF PAGE
                                    }
                                    else
                                    {
                                        cntr++;
                                    }
                                }
                                else
                                {
                                    cntr++;
                                }
                            }
                        }
                        //Look for ESC strings that have data (that end with W)
                        //)s,(s,(f,&n,*o,*b,*v,*m,*l,*i, *c
                        else if (((cntr + 2) < bufferSize) &&
                                 (((inputBuffer[cntr + 1] == 0x29) && (inputBuffer[cntr + 2] == 0x73)) ||
                                  ((inputBuffer[cntr + 1] == 0x28) && (inputBuffer[cntr + 2] == 0x73)) ||
                                  ((inputBuffer[cntr + 1] == 0x28) && (inputBuffer[cntr + 2] == 0x66)) ||
                                  ((inputBuffer[cntr + 1] == 0x26) && (inputBuffer[cntr + 2] == 0x6E)) ||
                                  ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x6F)) ||
                                  ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x62)) ||
                                  ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x63)) ||
                                  ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x76)) ||
                                  ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x6D)) ||
                                  ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x6C)) ||
                                  ((inputBuffer[cntr + 1] == 0x2A) && (inputBuffer[cntr + 2] == 0x69))))
                        {
                            int HoldStartPos = cntr + 2;
                            int LengthStartPos = HoldStartPos + 1;
                            bool inEscSeq = true;
                            cntr += 2;
                            while ((inEscSeq) && (cntr < bufferSize))
                            {
                                //Capital letter and @ mark the end of an Escape sequence
                                if ((inputBuffer[cntr] > 0x3F) && (inputBuffer[cntr] < 0x5B))
                                {
                                    inEscSeq = false;
                                    cntr++;
                                }
                                //check for a none numeric character (lower case letter)
                                else if (!((inputBuffer[cntr] > 0x29) && (inputBuffer[cntr] < 0x40)))
                                {
                                    cntr++;
                                    LengthStartPos = cntr;
                                }
                                else
                                {
                                    cntr++;
                                }
                            }

                            //W
                            if (inputBuffer[cntr - 1] == 0x57)
                            {
                                int JumpCntr = Convert.ToInt32(Encoding.ASCII.GetString(inputBuffer, LengthStartPos, cntr - LengthStartPos - 1));
                                cntr += JumpCntr;
                            }
                            //look for the *b ending with a V
                            else if ((inputBuffer[HoldStartPos - 1] == 0x2A) && (inputBuffer[HoldStartPos] == 0x62) && (inputBuffer[cntr - 1] == 0x56))
                            {
                                int JumpCntr = Convert.ToInt32(Encoding.ASCII.GetString(inputBuffer, LengthStartPos, cntr - LengthStartPos - 1));
                                cntr += JumpCntr;
                            }
                        }
                        //Another PCL string that could have data
                        else if (((cntr + 3) < bufferSize) &&
                                 ((inputBuffer[cntr + 1] == 0x26) && (inputBuffer[cntr + 2] == 0x70)))
                        {

                            int HoldStartPos = cntr + 2;
                            int LengthStartPos = HoldStartPos + 1;
                            bool inEscSeq = true;
                            cntr += 2;
                            while ((inEscSeq) && (cntr < bufferSize))
                            {
                                //Capital letter and @ mark the end of an Escape sequence
                                if ((inputBuffer[cntr] > 0x3F) && (inputBuffer[cntr] < 0x5B))
                                {
                                    inEscSeq = false;
                                    cntr++;
                                }
                                //check for a none numeric character (lower case letter)
                                else if (!((inputBuffer[cntr] > 0x29) && (inputBuffer[cntr] < 0x40)))
                                {
                                    cntr++;
                                    LengthStartPos = cntr;
                                }
                                else
                                {
                                    cntr++;
                                }
                            }

                            //X
                            if (inputBuffer[cntr - 1] == 0x58)
                            {
                                int JumpCntr = Convert.ToInt32(Encoding.ASCII.GetString(inputBuffer, LengthStartPos, cntr - LengthStartPos - 1));
                                cntr += JumpCntr;
                            }
                        }
                        //Look for the <ESC>9 string
                        else if (inputBuffer[cntr + 1] == 0x39)
                        {
                            cntr = cntr + 2;
                        }

                        else if (CheckForEndOfPage(cntr, ref EndLength))
                        {
                            //Add an entry in the file event list for a page end location
                            if (!endFound)
                            {
                                newEventPoint = new EventPoints(currentPageNum, cntr, EventPointType.epPageEnd, EndLength);
                                fileEventPoints.Add(newEventPoint);
                                if (SerializedData != "")
                                {
                                    if (SaveDataForBlobViewer)
                                    {
                                        BlobPerPage.Add(currentPageNum, SerializedData);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            blobProcessor.ParseBlob(SerializedData, currentPageNum, ref printToPrinterName, ref TSOMEnableDuplex);
                                        }
                                        catch (Exception ex)
                                        {
                                            pmLogging.LogError("Error Reading Blob.  Error:" + ex.Message, EventLogEntryType.Error, false);
                                        }
                                    }
                                }
                                else
                                {
                                    if (pmConfig.LogErrorSerializedDataNotFound)
                                    {
                                        pmLogging.LogError("Serialized Data not found on page " + currentPageNum.ToString(), EventLogEntryType.Error, false);
                                    }
                                }
                                SerializedData = "";
                                startFound = false;
                                endFound = true;
                                currentPageNum++;
                            }
                            else
                            {
                                if (pmConfig.LogWarningMessages)
                                {
                                    pmLogging.LogError("Warning: Found the string *p6400<FF> at position " + cntr.ToString() + " while looking for a start of job. File: " + printFileName, EventLogEntryType.Warning, false);
                                }
                            }
                            cntr += EndLength;
                        }
                        //*p - co-ordinate move that could precede a glyph id characters
                        else if ((cntr + 2 <= bufferSize) &&
                                 ((inputBuffer[cntr + 1] == 0x2a) && (inputBuffer[cntr + 2] == 0x70)))
                        {
                            if (fontSelectedById)
                            {
                                EvaluateChars(ref cntr, ref selectedFont, ref fontSelectedById);
                            }
                            else
                            {
                                cntr += 3;
                            }
                        }
                        //else jump to the next character
                        else
                        {
                            bool inEscSeq = true;
                            while ((inEscSeq) && (cntr < bufferSize))
                            {
                                //Capital letter and @ mark the end of an Escape sequence
                                if ((inputBuffer[cntr] > 0x3F) && (inputBuffer[cntr] < 0x5B))
                                {
                                    inEscSeq = false;
                                    cntr++;
                                }
                                else
                                {
                                    cntr++;
                                }
                            }
                        }
                    }

                    //Not Escape character
                    else
                    {
                        cntr++;
                    }
                    //DO NOT INCREMENT CNTR HERE!!!

                }
                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in ReadInputBuffer(). File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                return false;
            }
        }

        private bool CheckForEndOfPage(int cntr, ref int EndLength)
        {

            int lpcntr = 0;
            int indexcntr = -1;
            foreach (byte[] bytes in EndOfPageStringList)
            {
                indexcntr++;
                if (cntr + bytes.Length > inputBuffer.Length)
                {
                    //Move to next
                }
                else
                {
                    lpcntr = 0;

                }
                bool cont = true;
                while ((lpcntr + cntr < inputBuffer.Length) && (lpcntr < bytes.Length) && (cont))
                {
                    if (inputBuffer[cntr + lpcntr] != bytes[lpcntr])
                    {
                        cont = false;
                    }
                    lpcntr++;
                }
                if ((cont) && (lpcntr >= bytes.Length))
                {
                    //FOUND
                    EndLength = bytes.Length;
                    EndOfPageUsedIndex = indexcntr;
                    return true;
                }
            }

            return false;
        }

        private bool CheckForInsertPoint(int cntr)
        {
            if (cntr + InsertPointString.Length > inputBuffer.Length)
            {
                return false;
            }
            int lpcntr = 0;
            while ((lpcntr + cntr < inputBuffer.Length) && (lpcntr < InsertPointString.Length))
            {
                if (inputBuffer[cntr + lpcntr] != InsertPointString[lpcntr])
                {
                    return false;
                }
                lpcntr++;
            }

            return true;
        }



        private bool WriteOutPcl()
        {
            BinaryWriter outbuf = null;

            try
            {
                //Create an instance of the custom exception
                PortMonCustomException PortMonException;

                //                byte[] outBytes;

                StringBuilder tempString = new StringBuilder();



                //int byteCntr2 = 0;

                outbuf = new BinaryWriter(File.Open(tempFileName, FileMode.Create));

                if (EndPjlLocation > 0)
                {
                    if ((pmConfig.HpJobPassword.Length > 0) || (pmConfig.TroyJobPin.Length > 0) ||
                        (pmConfig.MicrPin.Length > 0) || (pmConfig.PrinterPin.Length > 0))
                    {
                        outbuf.Write(inputBuffer, 0, EndPjlLocation - 26); //Do not write the enter pcl yet
                        byte[] insertBytes = new byte[MAX_INSERT_PJL_BYTES_NEEDED];
                        int retCount = MAX_INSERT_PJL_BYTES_NEEDED;
                        if (WriteOutPjl(ref insertBytes, ref retCount))
                        {
                            outbuf.Write(insertBytes, 0, retCount);
                        }
                        outbuf.Write(inputBuffer, EndPjlLocation - 26, 27);
                    }
                    else
                    {
                        outbuf.Write(inputBuffer, 0, EndPjlLocation + 1);
                    }
                }
                else
                {
                    if (pmConfig.EncryptionType.ToUpper() != "NONE")
                    {
                        PortMonException = new PortMonCustomException("PJL not found.  Can not add the TroyMark configuration. File: " + printFileName, true, EventLogEntryType.Error, true);
                        throw PortMonException;
                    }
                }

                if (pmConfig.AccountUserName.Length > 0)
                {
                    byte[] accountInfo = new byte[13];
                    int accountInfoSize = 13;
                    GetUserPcl(pmConfig.AccountUserName, ref accountInfo, ref accountInfoSize);
                    outbuf.Write(accountInfo, 0, accountInfoSize);
                    if (pmConfig.AccountPassword.Length > 0)
                    {
                        accountInfoSize = 13;
                        GetPasswordPcl(pmConfig.AccountPassword, ref accountInfo, ref accountInfoSize);
                        outbuf.Write(accountInfo, 0, accountInfoSize);
                    }
                    else
                    {
                        outbuf.Write(ACCOUNT_PASSWORD_NONE_PCL, 0, ACCOUNT_PASSWORD_NONE_PCL.Length);
                    }
                    outbuf.Write(LOGIN_COMMAND_PCL, 0, LOGIN_COMMAND_PCL.Length);

                }

                if (pmConfig.EnableMicrMode)
                {
                    outbuf.Write(MICR_MODE_PCL, 0, MICR_MODE_PCL.Length);
                }

                if (pmConfig.AutoPageRotate)
                {
                    outbuf.Write(AUTO_PAGE_ROTATE, 0, AUTO_PAGE_ROTATE.Length);
                }

                if (pmConfig.DefaultPaperTrayMapping)
                {
                    outbuf.Write(DEFAULT_PAPER_TRAY_MAPPING_PCL, 0, DEFAULT_PAPER_TRAY_MAPPING_PCL.Length);
                }


                if (pmConfig.AlternateEscCharacter.Length > 0)
                {
                    byte[] altEscBuffer = new byte[25];
                    int altEscBufferLength = 25;
                    if (GetAltEscPcl(pmConfig.AlternateEscCharacter, ref altEscBuffer, ref altEscBufferLength))
                    {
                        outbuf.Write(altEscBuffer, 0, altEscBufferLength);
                    }
                }

                bool TSOMInsertPage = false;

                int currPage = 1;
                int currCntr = EndPjlLocation + 1;
                foreach (EventPoints evpt in fileEventPoints)
                {
                    if (evpt.PageNumber == currPage)
                    {

                        if (evpt.Location > currCntr)
                        {
                            outbuf.Write(inputBuffer, currCntr, evpt.Location - currCntr);
                            currCntr = evpt.Location;
                        }


                        switch (evpt.EventType)
                        {
                            case EventPointType.epInsertPoint:
                                foreach (Blob.BlobProcessor.InsertPclConfigurationArea ipca in blobProcessor.insertPclConfigurationArea)
                                {
                                    if (ipca.pageNumber == currPage)
                                    {
                                        outbuf.Write(ipca.pclByteArray, 0, ipca.pclByteArrayLength);
                                    }
                                }

                                outbuf.Write(inputBuffer, evpt.Location, evpt.EventLength);
                                currCntr += evpt.EventLength;
                                if ((currPage == 1) &&
                                    ((pmConfig.EnableDuplex != "") ||
                                     (TSOMEnableDuplex)))
                                {
                                    if ((TSOMEnableDuplex) && (pmConfig.EnableDuplex != "2"))
                                    {
                                        pmConfig.EnableDuplex = "1";
                                    }
                                    byte[] duplexInfo = new byte[15];
                                    int duplexInfoSize = 15;
                                    if ((pmConfig.EnableDuplex.Length > 0) && CharIsNumeric(pmConfig.EnableDuplex[0]))
                                    {
                                        GetEnableDuplex(Convert.ToInt32(pmConfig.EnableDuplex), ref duplexInfo, ref duplexInfoSize);
                                        outbuf.Write(duplexInfo, 0, duplexInfoSize);
                                    }
                                }
                                if (blobProcessor.insertPclPrintedArea.Count > 0)
                                {
                                    foreach (Blob.BlobProcessor.InsertPclPrintedArea ippa in blobProcessor.insertPclPrintedArea)
                                    {
                                        if ((ippa.pageNumber == currPage) && (ippa.pclByteArrayLength > 0))
                                        {
                                            outbuf.Write(ippa.pclByteArray, 0, ippa.pclByteArrayLength);
                                            TSOMInsertPage = ippa.insertNewPage;
                                        }
                                    }
                                }
                                break;
                            case EventPointType.epPageEnd:
                                outbuf.Write(inputBuffer, evpt.Location, evpt.EventLength);
                                currCntr += evpt.EventLength;
                                if (TSOMInsertPage)
                                {
                                    bool AddPage = false;
                                    if (blobProcessor.insertPclOnNewPage.Count > 0)
                                    {
                                        foreach (Blob.BlobProcessor.InsertPclPrintedArea ippa in blobProcessor.insertPclOnNewPage)
                                        {
                                            if (ippa.pageNumber == currPage)
                                            {
                                                AddPage = true;
                                                break;
                                            }
                                        }
                                        if (AddPage)
                                        {
                                            outbuf.Write(page2PclStartPage, 0, page2PclStartPageLength);
                                            foreach (Blob.BlobProcessor.InsertPclPrintedArea ippa in blobProcessor.insertPclOnNewPage)
                                            {
                                                if (ippa.pageNumber == currPage)
                                                {
                                                    outbuf.Write(ippa.pclByteArray, 0, ippa.pclByteArrayLength);
                                                }
                                            }
                                            outbuf.Write(page2PclEndPage, 0, page2PclEndPageLength);
                                        }
                                    }
                                }


                                int holdPage = currPage;
                                currPage++;
                                break;
                            case EventPointType.epSubstitute:
                                currCntr += evpt.EventLength;
                                //TBD
                                break;
                            case EventPointType.epRemove:
                                currCntr += evpt.EventLength;
                                break;
                            case EventPointType.epUELLocation:
                                outbuf.Write(inputBuffer, evpt.Location, evpt.EventLength);
                                currCntr += evpt.EventLength;
                                break;
                        }
                    }
                }

                outbuf.Flush();
                if (outbuf != null)
                {
                    outbuf.Close();
                }

                //Currently only triple DES is available
                if (pmConfig.EncryptionType == "TDES")
                {
                    Encryption encrypt = new Encryption();
                    encFileName = printJobFileInfo.FullName.Replace(printJobFileInfo.Extension, ".enc");
                    if (encrypt.EncryptData(tempFileName, encFileName, pmConfig.EncryptPassword, EndPjlLocation))
                    {
                        printerFileName = encFileName;
                    }
                    else
                    {
                        PortMonException = new PortMonCustomException("Encryption failed! Not printing.", true);
                        throw PortMonException;
                    }
                }
                else
                {
                    printerFileName = tempFileName;
                }

                return true;
            }
            catch (PortMonCustomException pme)
            {
                throw pme;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in WriteOutPcl(). File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                return false;
            }
            finally
            {
            }
        }


        private bool PrintTheJob()
        {
            try
            {
                if (printerFileName.Length > 0)
                {
                    PrintToSpooler.SendFileToPrinter(printToPrinterName, printerFileName, "Test From Port Monitor");
                }
                else
                {
                    PortMonCustomException pme = new PortMonCustomException("Job was not printer.  Job filename: " + printFileName, true);
                    throw pme;
                }
                return true;
            }
            catch (PortMonCustomException pe)
            {
                pmLogging.LogError("Job was not printed.  File: " + printFileName + " Error: " + pe.Message, EventLogEntryType.Error, true);
                return false;

            }
            catch (Exception ex)
            {
                pmLogging.LogError("Job was not printed.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                return false;
            }
        }

        private bool EvaluateFontDescription(ref int cntr, ref int selectedFont, ref bool fontSelectedById)
        {
            try
            {
                int subcntr = 1;
                bool continueSubLoop = true, fontDescFound = false, fontCharMapFound = false;
                bool skipGlyphDef = false;

                cntr += 2;
                //Look for a D or E (*c#D is font description, *c#E is a char definition)
                while ((subcntr <= MAX_FONT_ID_DIGITS + 1) && (continueSubLoop))
                {
                    //Look for the D
                    if (inputBuffer[cntr + subcntr] == 0x44)
                    {
                        continueSubLoop = false;
                        fontDescFound = true;
                    }
                    //Look for the E
                    else if (inputBuffer[cntr + subcntr] == 0x45)
                    {
                        continueSubLoop = false;
                        fontCharMapFound = true;
                    }
                    //Look for non-numeric
                    else if ((inputBuffer[cntr + subcntr] < 0x30) ||
                             (inputBuffer[cntr + subcntr] > 0x39))
                    {
                        continueSubLoop = false;
                    }
                    subcntr++;
                }

                //if D was found
                if (fontDescFound)
                {
                    int fontIdFromFile = Convert.ToInt32(Encoding.ASCII.GetString(inputBuffer, cntr + 1, subcntr - 2));
                    if (fontIdFromFile < 1)
                    {
                        pmLogging.LogError("Unexpected font id value " + fontIdFromFile + ".  File: " + printFileName, EventLogEntryType.Warning, false);
                    }
                    cntr += subcntr;

                    // not <ESC>)s,  this marks the beginning of the Font Descriptor
                    //  if not a font descriptor then its most likely a character definition coming up
                    if (!((inputBuffer[cntr] == 0x1B) && (inputBuffer[cntr + 1] == 0x29) &&
                          (inputBuffer[cntr + 2] == 0x73)))
                    {
                        if (fontConfigList.ContainsKey(fontIdFromFile))
                        {
                            fontSelectedById = true;
                            selectedFont = fontIdFromFile;
                        }
                        else
                        {
                            fontSelectedById = false;
                            selectedFont = 0;
                        }
                    }
                    //beginning of font descriptor found
                    else
                    {
                        cntr += 2;
                        subcntr = 1;
                        continueSubLoop = true;

                        //loop through until the W is found
                        while ((subcntr <= MAX_FONT_DESC_DIGITS + 1) && (continueSubLoop))
                        {
                            //Look for the W
                            if (inputBuffer[cntr + subcntr] == 0x57)
                            {
                                continueSubLoop = false;
                            }
                            subcntr++;
                        }

                        //if W was found
                        if ((!continueSubLoop) && (subcntr > 1))
                        {
                            int fontDescLength = Convert.ToInt32(Encoding.ASCII.GetString(inputBuffer, cntr + 1, subcntr - 2));
                            if (fontIdFromFile < 1)
                            {
                                pmLogging.LogError("Unexpected font id value " + fontIdFromFile + ".  File: " + printFileName, EventLogEntryType.Warning, false);
                            }
                            cntr += subcntr;

                            if (FONT_NAME_LOC > fontDescLength)
                            {
                                pmLogging.LogError("Unexpected font font description length " + fontDescLength + ".  File: " + printFileName, EventLogEntryType.Warning, false);
                            }

                            // Extract the font name
                            string fontName = Encoding.ASCII.GetString(inputBuffer, cntr + FONT_NAME_LOC, 15);
                            fontName = fontName.TrimEnd('\0');

                            cntr += fontDescLength;
                            //If the font name is in the configuration font list then add an entry to the array of fonts 
                            if (fontConfigs.FontInList(fontName))
                            {
                                if (fontConfigList.ContainsKey(fontIdFromFile))
                                {
                                    fontConfigList.Remove(fontIdFromFile);
                                }
                                fontConfigList.Add(fontIdFromFile, fontConfigs.GetFontConfig(fontName));
                            }
                        }
                    }
                }
                else if (fontCharMapFound)
                {
                    //If we are in a font description
                    if (fontSelectedById)
                    {
                        //Get the font char id (32 thru 254)
                        int fontCharId = Convert.ToInt32(Encoding.ASCII.GetString(inputBuffer, cntr + 1, subcntr - 2));
                        skipGlyphDef = false;
                        if ((fontCharId < 32) || (fontCharId > 254))
                        {
                            pmLogging.LogError("Unexpected font char id value " + fontCharId + ".  File: " + printFileName, EventLogEntryType.Warning, false);
                            skipGlyphDef = true;
                        }

                        cntr += subcntr;

                        //If we don't see an <ESC>(s then we have an unexpected sequence
                        if (!((inputBuffer[cntr] == 0x1b) && (inputBuffer[cntr + 1] == 0x28) &&
                              (inputBuffer[cntr + 2] == 0x73)))
                        {
                            pmLogging.LogError("Unexpected sequence in font char definition at location " + cntr + ".  File: " + printFileName, EventLogEntryType.Warning, false);
                        }

                        cntr += 3;
                        subcntr = 1;
                        continueSubLoop = true;
                        int HoldStartPos = cntr;

                        while ((subcntr <= MAX_FONT_DESC_DIGITS + 1) && (continueSubLoop))
                        {
                            //Look for the W
                            if (inputBuffer[cntr + subcntr] == 0x57)
                            {
                                continueSubLoop = false;
                            }
                            subcntr++;
                        }

                        //Added for 2.0.  Skip over the font glyph definition stuff
                        int JumpCntr = Convert.ToInt32(Encoding.ASCII.GetString(inputBuffer, HoldStartPos, subcntr - 1));

                        cntr += subcntr;

                        if ((!continueSubLoop) && (!skipGlyphDef))
                        {
                            int glyphId = Convert.ToInt32(inputBuffer[cntr + 7]);
                            if (!(fontCharToGlyphMap.ContainsKey(selectedFont)))
                            {
                                GlyphMapType gmt = new GlyphMapType();
                                gmt.AddToGlyphMap(fontCharId, glyphId);
                                fontCharToGlyphMap.Add(selectedFont, gmt);
                            }
                            else
                            {
                                GlyphMapType gmt = fontCharToGlyphMap[selectedFont];
                                gmt.AddToGlyphMap(fontCharId, glyphId);
                            }
                        }

                        //cntr += 7;
                        //Added for 2.0.  Skip over the font glyph definition stuff
                        cntr += JumpCntr;

                        //Added to 2.0.  Discovered that glyph chars can show up immediately after a glyph definition without a cursor movement.
                        if (inputBuffer[cntr] != 0x1B)
                        {
                            EvaluateChars(ref cntr, ref selectedFont, ref fontSelectedById);
                        }
                    }
                }
                else
                {
                    cntr += subcntr;
                }
                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in EvaluateFontDescription.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                return false;
            }
        }

        private bool EvaluateSymbolSet(ref int cntr, ref int selectedFont, ref bool fontSelectedById)
        {
            try
            {
                int subcntr = 1;
                bool continueSubLoop = true;

                cntr++;
                while ((subcntr <= MAX_FONT_ID_DIGITS + 1) && (continueSubLoop))
                {
                    //Look for the X
                    if (inputBuffer[cntr + subcntr] == 0x58)
                    {
                        continueSubLoop = false;
                    }
                    subcntr++;
                }

                //if X was found
                if ((!continueSubLoop) && (subcntr > 1))
                {
                    int fontIdFromFile = Convert.ToInt32(Encoding.ASCII.GetString(inputBuffer, cntr + 1, subcntr - 2));
                    if (fontIdFromFile < 1)
                    {
                        pmLogging.LogError("Unexpected font id value " + fontIdFromFile + ".  File: " + printFileName, EventLogEntryType.Warning, false);
                    }
                    cntr += subcntr;

                    if (fontConfigList.ContainsKey(fontIdFromFile))
                    {
                        fontSelectedById = true;
                        selectedFont = fontIdFromFile;
                    }
                    else
                    {
                        fontSelectedById = false;
                        selectedFont = 0;
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in EvaluateSymbolSet.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                return false;
            }
        }

        private bool EvaluateChars(ref int cntr, ref int selectedFont, ref bool fontSelectedById)
        {
            try
            {
                //bool deleteFont;
                //int finalLength;
                bool lookForString = false;
                string capturedStr;
                byte[] leadingPclInsert = new byte[LengthOfLeadingPcl];
                byte[] trailingPclInsert = new byte[LengthOfTrailingPcl];
                char transAsciiChar;

                //Create an instance of the custom exception
                PortMonCustomException PortMonException;

                int posCntr;
                int translatedChar;

                //Initialize variables
                int saveStartLoc = cntr;
                int saveWriteLength = 4;
                int subcntr = 1;
                bool continueSubLoop = true;


                fontCharStr.Remove(0, fontCharStr.Length);
                cntr += 3;

                while ((subcntr <= MAX_POS_DIGITS + 1) && (continueSubLoop))
                {
                    //Look for trailing X or Y
                    if ((inputBuffer[cntr + subcntr] == 0x58) || (inputBuffer[cntr + subcntr] == 0x59))
                    {
                        saveWriteLength += subcntr;
                        cntr += subcntr + 1;
                        //If the next character is not an Escape then it a glyph map char
                        if (!(inputBuffer[cntr] == 0x1B))
                        {
                            continueSubLoop = false;
                            lookForString = true;
                        }
                        //else if the next chars are <ESC>*p then another positioning string is next
                        else if ((inputBuffer[cntr + 1] == 0x2A) &&
                                 (inputBuffer[cntr + 2] == 0x70))
                        {
                            subcntr = 1;
                            saveWriteLength += 4;
                            cntr += 3;
                        }
                        //else another escape sequence follows, not a glyph char map
                        else
                        {
                            lookForString = false;
                            continueSubLoop = false;
                        }
                    }
                    else
                    {
                        subcntr++;
                    }
                }

                if (lookForString)
                {
                    int startOfGlyphChars = cntr;
                    int endOfGlyphChars = cntr;

                    continueSubLoop = true;
                    subcntr = 0;
                    while (continueSubLoop)
                    {
                        if (inputBuffer[cntr + subcntr] == 0x1B)
                        {
                            //if not *p then its the end of this string
                            //if not *p or *p6400Y<FF> then its the end of this string
                            if ((!((inputBuffer[cntr + subcntr + 1] == 0x2A) && (inputBuffer[cntr + subcntr + 2] == 0x70))) ||
                                ((inputBuffer[cntr + subcntr + 1] == 0x2A) && (inputBuffer[cntr + subcntr + 2] == 0x70) &&
                                  (inputBuffer[cntr + subcntr + 3] == 0x36) && (inputBuffer[cntr + subcntr + 4] == 0x34) &&
                                  (inputBuffer[cntr + subcntr + 5] == 0x30) && (inputBuffer[cntr + subcntr + 6] == 0x30) &&
                                  (inputBuffer[cntr + subcntr + 7] == 0x59) && (inputBuffer[cntr + subcntr + 8] == 0x0C)))
                            {
                                cntr += subcntr;
                                continueSubLoop = false;
                                capturedStr = fontCharStr.ToString();
                                if (capturedStr.Length < 1)
                                {
                                    //Nothing to do. Exit and do nothing
                                }
                                else
                                {
                                    if (fontConfigList.ContainsKey(selectedFont))
                                    {
                                        if (fontConfigList[selectedFont].FontType.ToUpper() == "SERIALIZATION")
                                        {
                                            SerializedData += capturedStr;
                                            if (endOfGlyphChars > startOfGlyphChars)
                                            {
                                                newEventPoint = new EventPoints(currentPageNum, startOfGlyphChars, EventPointType.epRemove, endOfGlyphChars - startOfGlyphChars + 1);
                                                fileEventPoints.Add(newEventPoint);
                                            }

                                        }
                                        else
                                        {
                                        }
                                    }
                                    else
                                    {
                                        PortMonException = new PortMonCustomException("Font Configuration entry not found for the font id " + selectedFont.ToString() + ".  File: " + printFileName, true, EventLogEntryType.Error, false);
                                        throw PortMonException;
                                    }
                                }
                            }
                            //else find the trailing X or Y then push the cntr ahead
                            else
                            {
                                subcntr += 3;
                                posCntr = 1;
                                while ((inputBuffer[cntr + subcntr] != 0x58) &&
                                       (inputBuffer[cntr + subcntr] != 0x59) &&
                                       (posCntr <= MAX_POS_DIGITS))
                                {
                                    posCntr++;
                                    subcntr++;
                                }
                                if (posCntr > MAX_POS_DIGITS + 1)
                                {
                                    PortMonException = new PortMonCustomException("Unexpected length of XY coordinate PCL and location " + cntr.ToString() + ". File: " + printFileName, true, EventLogEntryType.Error, false);
                                    throw PortMonException;
                                }
                            }
                        }
                        else
                        {
                            //Translate the glyph character to the equivalent ASCII, add to the string
                            if (fontCharToGlyphMap.ContainsKey(selectedFont))
                            {
                                GlyphMapType gmt = fontCharToGlyphMap[selectedFont];
                                translatedChar = gmt.GetGlyphId(Convert.ToInt32(inputBuffer[cntr + subcntr]));
                                if (fontConfigList.ContainsKey(selectedFont))
                                {
                                    FontConfigOrig fc2 = fontConfigList[selectedFont];
                                    transAsciiChar = fc2.GetCharFromGlyphId(translatedChar);
                                    if (transAsciiChar == '\0')
                                    {

                                    }
                                    else
                                    {
                                        fontCharStr.Append(transAsciiChar);
                                        endOfGlyphChars = cntr + subcntr;
                                    }
                                }
                            }

                            else
                            {
                                PortMonException = new PortMonCustomException("Font Configuration entry not found in glyph if map for the font id " + selectedFont.ToString() + ".  File: " + printFileName, true, EventLogEntryType.Error, false);
                                throw PortMonException;
                            }
                        }
                        subcntr++;
                    }
                }
                fontSelectedById = false;
                selectedFont = 0;


                return true;
            }
            catch (PortMonCustomException pme)
            {
                throw pme;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in EvaluateChars.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                return false;
            }

        }


        private bool GetEnableDuplex(int DuplexValue, ref byte[] pcl, ref int pclLength)
        {
            try
            {
                byte[] Duplex;
                Duplex = new UTF8Encoding(true).GetBytes(DuplexValue.ToString());
                int DuplexLength = Duplex.Length;

                if (pclLength < (4 + DuplexLength))
                {

                }
                else
                {
                    pcl[0] = ESC;
                    pcl[1] = 0x26; //&
                    pcl[2] = 0x6C; //l

                    int cntr;
                    for (cntr = 0; cntr < DuplexLength; cntr++)
                    {
                        pcl[3 + cntr] = Duplex[cntr];
                    }

                    pcl[3 + cntr] = 0x53; //S
                    pclLength = cntr + 4;


                }
                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetEnableDuplex.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }
        }


        private bool GetTroyPrinterPinPjl(string PrinterPinString, ref byte[] pjl, ref int pjlLength)
        {
            try
            {
                byte[] printerPinHdr = new UTF8Encoding(true).GetBytes(TROY_PRINTER_PIN_PJL);
                int printerPinHdrLength = printerPinHdr.Length;
                byte[] printerPin = new UTF8Encoding(true).GetBytes(PrinterPinString);
                int printerPinLength = printerPin.Length;

                if (pjlLength < printerPinHdrLength + printerPinLength + 2)
                {
                    Exception lengthEx = new Exception("Invalid buffer length.");
                    throw lengthEx;
                }
                else
                {
                    Array.Copy(printerPinHdr, pjl, printerPinHdrLength);
                    int cntr;
                    for (cntr = 0; cntr < printerPinLength; cntr++)
                    {
                        pjl[printerPinHdrLength + cntr] = printerPin[cntr];
                    }
                    pjl[printerPinHdrLength + cntr] = 0x0d;
                    pjl[printerPinHdrLength + cntr + 1] = 0x0a;

                    pjlLength = printerPinHdrLength + cntr + 2;
                }

                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetTroyPrinterPinPjl.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }

        }

        private bool GetTroyMicrPinPjl(string MicrPinString, ref byte[] pjl, ref int pjlLength)
        {
            try
            {
                byte[] micrPinHdr = new UTF8Encoding(true).GetBytes(TROY_MICR_PIN_PJL);
                int micrPinHdrLength = micrPinHdr.Length;
                byte[] micrPin = new UTF8Encoding(true).GetBytes(MicrPinString);
                int micrPinLength = micrPin.Length;

                if (pjlLength < micrPinHdrLength + micrPinLength + 2)
                {
                    Exception lengthEx = new Exception("Invalid buffer length.");
                    throw lengthEx;
                }
                else
                {
                    Array.Copy(micrPinHdr, pjl, micrPinHdrLength);
                    int cntr;
                    for (cntr = 0; cntr < micrPinLength; cntr++)
                    {
                        pjl[micrPinHdrLength + cntr] = micrPin[cntr];
                    }
                    pjl[micrPinHdrLength + cntr] = 0x0d;
                    pjl[micrPinHdrLength + cntr + 1] = 0x0a;
                    pjlLength = micrPinHdrLength + cntr + 2;

                }

                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetTroyMicrPinPjl.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }

        }

        private bool GetTroyJobPinPjl(string JobNameString, string JobPinString, ref byte[] pjl, ref int pjlLength)
        {
            try
            {
                byte[] jobPinHdr = new UTF8Encoding(true).GetBytes(TROY_JOB_PIN_PJL_HEADER);
                int jobPinHdrLength = jobPinHdr.Length;
                byte[] jobPinTrl = new UTF8Encoding(true).GetBytes(TROY_JOB_PIN_PJL_TRAIL);
                int jobPinTrlLength = jobPinTrl.Length;
                byte[] jobName = new UTF8Encoding(true).GetBytes(JobNameString);
                int jobNameLength = jobName.Length;
                byte[] jobPin = new UTF8Encoding(true).GetBytes(JobPinString);
                int jobPinLength = jobPin.Length;

                if (pjlLength < jobPinHdrLength + jobPinTrlLength + jobNameLength + jobPinLength + 2)
                {
                    Exception lengthEx = new Exception("Invalid buffer length.");
                    throw lengthEx;
                }
                else
                {
                    Array.Copy(jobPinHdr, pjl, jobPinHdrLength);
                    int cntr, pjlCntr;
                    pjlCntr = jobPinHdrLength;
                    for (cntr = 0; cntr < jobNameLength; cntr++)
                    {
                        pjl[pjlCntr] = jobName[cntr];
                        pjlCntr++;
                    }
                    for (cntr = 0; cntr < jobPinTrlLength; cntr++)
                    {
                        pjl[pjlCntr] = jobPinTrl[cntr];
                        pjlCntr++;
                    }
                    for (cntr = 0; cntr < jobPinLength; cntr++)
                    {
                        pjl[pjlCntr] = jobPin[cntr];
                        pjlCntr++;
                    }

                    pjl[pjlCntr] = 0x0d;
                    pjl[pjlCntr + 1] = 0x0a;

                    pjlLength = pjlCntr + 2;
                }

                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetTroyJobPinPjl.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }
        }

        private bool GetHpJobPinPjl(string HpPinString, ref byte[] pjl, ref int pjlLength)
        {
            try
            {
                byte[] hpPinHdr = new UTF8Encoding(true).GetBytes(HP_JOB_PASSWORD_PJL);
                int hpPinHdrLength = hpPinHdr.Length;
                byte[] hpPin = new UTF8Encoding(true).GetBytes(HpPinString);
                int hpPinLength = hpPin.Length;

                if (pjlLength < hpPinHdrLength + hpPinLength + 2)
                {
                    Exception lengthEx = new Exception("Invalid buffer length.");
                    throw lengthEx;
                }
                else
                {
                    Array.Copy(hpPinHdr, pjl, hpPinHdrLength);
                    int cntr;
                    for (cntr = 0; cntr < hpPinLength; cntr++)
                    {
                        pjl[hpPinHdrLength + cntr] = hpPin[cntr];
                    }
                    pjl[hpPinHdrLength + cntr] = 0x0d;
                    pjl[hpPinHdrLength + cntr + 1] = 0x0a;
                    pjlLength = hpPinHdrLength + cntr + 2;
                }



                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetHPPinPjl.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }

        }


        private bool GetUserPcl(string UserNameString, ref byte[] pcl, ref int pclLength)
        {
            try
            {
                byte[] UserName = new UTF8Encoding(true).GetBytes(UserNameString);
                int UserNameLength = UserName.Length;
                byte[] UserLength = new UTF8Encoding(true).GetBytes(UserNameLength.ToString());

                //8 is defined as the max in the documentation
                if ((UserNameLength > 8) || (pclLength < 5 + UserNameLength))
                {
                    Exception lengthEx = new Exception("Invalid buffer length.");
                    throw lengthEx;
                }
                else
                {
                    pcl[0] = ESC;
                    pcl[1] = 0x25; //%
                    pcl[2] = 0x75; //u
                    pcl[3] = UserLength[0];  //Length
                    pcl[4] = 0x57; //W

                    int cntr;
                    for (cntr = 0; cntr < UserNameLength; cntr++)
                    {
                        pcl[5 + cntr] = UserName[cntr];
                    }

                    pclLength = cntr + 5;

                }
                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetUserPcl.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }

        }

        private bool GetPasswordPcl(string PasswordString, ref byte[] pcl, ref int pclLength)
        {
            try
            {
                byte[] Password = new UTF8Encoding(true).GetBytes(PasswordString);
                int PasswordLength = Password.Length;
                byte[] PwLength = new UTF8Encoding(true).GetBytes(PasswordLength.ToString());

                //8 is defined as the max in the documentation
                if ((PasswordLength > 8) || (pclLength < 5 + PasswordLength))
                {
                    Exception lengthEx = new Exception("Invalid buffer length.");
                    throw lengthEx;
                }
                else
                {
                    pcl[0] = ESC;
                    pcl[1] = 0x25; //%
                    pcl[2] = 0x70; //p
                    pcl[3] = PwLength[0];  //Length
                    pcl[4] = 0x57; //W

                    int cntr;
                    for (cntr = 0; cntr < PasswordLength; cntr++)
                    {
                        pcl[5 + cntr] = Password[cntr];
                    }
                    pclLength = 5 + cntr;

                }
                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetPasswordPcl.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }

        }

        private bool GetAltEscPcl(string AltEscString, ref byte[] pcl, ref int pclLength)
        {
            try
            {
                byte[] AltEsc = new UTF8Encoding(true).GetBytes(AltEscString);
                int AltEscLength = AltEsc.Length;
                byte[] AeLength = new UTF8Encoding(true).GetBytes(AltEscLength.ToString());


                if ((pclLength < 5 + AltEscLength) || (AltEscLength > 1))
                {
                    Exception lengthEx = new Exception("Invalid buffer length.");
                    throw lengthEx;
                }
                else
                {
                    pcl[0] = ESC;
                    pcl[1] = 0x25; //%
                    pcl[2] = 0x65; //e
                    pcl[3] = AeLength[0];  //Length
                    pcl[4] = 0x57; //W
                    pcl[5] = AltEsc[0];

                    pclLength = 6;
                    //According to documentation, the firmware can only handle 1 char at this time
                    //for (int cntr = 0; cntr < AltEscLength; cntr++)
                    //{
                    //    pcl[5 + cntr] = AltEsc[cntr];
                    //}
                }
                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetAltEscPcl.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }
        }


        private bool WriteOutPjl(ref byte[] retBytes, ref int retBytesLength)
        {
            try
            {
                byte[] tempBytes = new byte[MAX_INSERT_PJL_BYTES_NEEDED];
                byte[] inputBytes = new byte[50];
                int currentSize = 0;
                int inputSize = 50;

                if (pmConfig.PrinterPin.Length > 0)
                {
                    GetTroyPrinterPinPjl(pmConfig.PrinterPin, ref inputBytes, ref inputSize);
                    Array.Copy(inputBytes, tempBytes, inputSize);
                    currentSize = inputSize;
                    inputSize = 50;
                }

                if (pmConfig.MicrPin.Length > 0)
                {
                    GetTroyMicrPinPjl(pmConfig.MicrPin, ref inputBytes, ref inputSize);
                    Array.ConstrainedCopy(inputBytes, 0, tempBytes, currentSize, inputSize);
                    currentSize += inputSize;
                    inputSize = 50;
                }

                if (pmConfig.TroyJobPin.Length > 0)
                {
                    GetTroyJobPinPjl(pmConfig.TroyJobName, pmConfig.TroyJobPin, ref inputBytes, ref inputSize);
                    Array.ConstrainedCopy(inputBytes, 0, tempBytes, currentSize, inputSize);
                    currentSize += inputSize;
                    inputSize = 50;
                }

                if (pmConfig.HpJobPassword.Length > 0)
                {
                    GetHpJobPinPjl(pmConfig.HpJobPassword, ref inputBytes, ref inputSize);
                    Array.ConstrainedCopy(inputBytes, 0, tempBytes, currentSize, inputSize);
                    currentSize += inputSize;
                    inputSize = 50;
                }

                Array.Copy(tempBytes, retBytes, currentSize);
                retBytesLength = currentSize;
                return true;

            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in WriteOutPjl.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }
        }

        private bool GetPjlTroyMarkData(ref byte[] troyMarkData, ref int troyMarkDataLength)
        {
            try
            {
                bool retVal = true;
                byte[] pjlJobName = new byte[pjlSetJobNameLength] { 0x40, 0x50, 0x4A, 0x4C, 0x20, 0x53, 0x45, 0x54, 0x20, 0x4A, 0x4F, 0x42, 0x4E, 0x41, 0x4D, 0x45, 0x3D };

                bool continueLoop = true;
                int matchCntr = 0, cntr = 0;
                int bufferSize = inputBuffer.Length;

                while ((cntr < bufferSize) && (continueLoop))
                {
                    if (inputBuffer[cntr] == pjlJobName[matchCntr])
                    {
                        matchCntr++;
                        if (matchCntr == pjlSetJobNameLength)
                        {
                            continueLoop = false;
                        }
                    }
                    else
                    {
                        matchCntr = 0;
                    }
                    cntr++;
                }

                if (continueLoop)
                {
                    pmLogging.LogError("Error in GetPjlTroyMarkData. Can not find PJL SET JOBNAME.", EventLogEntryType.Error, false);
                    retVal = false;
                }
                else
                {

                    int startPoint = cntr;
                    byte[] pdfExt = new byte[4] { 0x2E, 0x70, 0x64, 0x66 };
                    continueLoop = true;
                    matchCntr = 0;
                    while ((cntr < bufferSize) && (continueLoop))
                    {
                        if (inputBuffer[cntr] == pdfExt[matchCntr])
                        {
                            matchCntr++;
                            if (matchCntr == 4)
                            {
                                continueLoop = false;
                            }
                        }
                        else
                        {
                            matchCntr = 0;
                        }
                        cntr++;
                    }
                    if (continueLoop)
                    {
                        pmLogging.LogError("Error in GetPjlTroyMarkData. Can not find pdf extension in PJL.", EventLogEntryType.Error, false);
                        retVal = false;
                    }
                    else
                    {
                        Array.ConstrainedCopy(inputBuffer, startPoint + 1, troyMarkData, 0, cntr - startPoint - 1);
                        troyMarkDataLength = cntr - startPoint - 1;
                    }
                }

                return retVal;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetPjlTroyMarkData.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }
        }

        private bool GetPjlTroyMarkFields(byte[] inputBytes, int inputBytesLength,
                                      ref byte[] field1Data, ref int field1Length,
                                      ref byte[] field2Data, ref int field2Length,
                                      ref byte[] field3Data, ref int field3Length)
        {
            try
            {
                int cntr = 0;
                int fieldCntr = 1;
                int hold1;
                bool continueLoop = true;
                bool continueLoop1 = true;
                bool continueLoop2 = true;
                bool retVal = true;

                while ((cntr < inputBytesLength) && (continueLoop))
                {
                    if (inputBytes[cntr] == 0x5B)
                    {
                        if (fieldCntr == 1)
                        {
                            hold1 = cntr + 1;
                            while ((cntr < inputBytesLength) && (continueLoop1))
                            {
                                if (inputBytes[cntr] == 0x5D)
                                {
                                    continueLoop1 = false;
                                    if (field1Length < cntr - hold1)
                                    {
                                        pmLogging.LogError("Error in GetPjlTroyMarkFields. Invalid field 1 size.", EventLogEntryType.Error, false);
                                        continueLoop = false;
                                        retVal = false;
                                    }
                                    else
                                    {
                                        Array.ConstrainedCopy(inputBytes, hold1, field1Data, 0, cntr - hold1);
                                        field1Length = cntr - hold1;
                                    }
                                }
                                else
                                {
                                    cntr++;
                                }
                            }
                            if (continueLoop1)
                            {
                                pmLogging.LogError("Error in GetPjlTroyMarkFields.  First ] not found", EventLogEntryType.Error, false);
                                retVal = false;
                                continueLoop = false;
                            }
                            fieldCntr++;
                        }
                        else if (fieldCntr == 2)
                        {
                            hold1 = cntr + 1;
                            while ((cntr < inputBytesLength) && (continueLoop2))
                            {
                                if (inputBytes[cntr] == 0x5D)
                                {
                                    continueLoop2 = false;
                                    if (field1Length < cntr - hold1)
                                    {
                                        pmLogging.LogError("Error in GetPjlTroyMarkFields. Invalid field 2 size.", EventLogEntryType.Error, false);
                                        continueLoop = false;
                                        retVal = false;
                                    }
                                    else
                                    {
                                        Array.ConstrainedCopy(inputBytes, hold1, field2Data, 0, cntr - hold1);
                                        field2Length = cntr - hold1;
                                    }
                                    if (field3Length < inputBytesLength - cntr - 1 - 4) //do not include the .pdf
                                    {
                                        pmLogging.LogError("Error in GetPjlTroyMarkFields. Invalid field 3 size.", EventLogEntryType.Error, false);
                                        continueLoop = false;
                                        retVal = false;
                                    }
                                    else
                                    {
                                        Array.ConstrainedCopy(inputBytes, cntr + 1, field3Data, 0, inputBytesLength - cntr - 1 - 4);
                                        field3Length = inputBytesLength - cntr - 1 - 4;
                                    }
                                }
                                else
                                {
                                    cntr++;
                                }
                            }
                            continueLoop = false;
                            if (continueLoop2)
                            {
                                pmLogging.LogError("Error in GetPjlTroyMarkFields.  Second ] not found", EventLogEntryType.Error, false);
                                retVal = false;
                            }
                        }
                    }
                    cntr++;

                }
                if (continueLoop)
                {
                    pmLogging.LogError("Error in GetPjlTroyMarkFields(). Could not parse troy mark data from PJL string.", EventLogEntryType.Error, false);
                    retVal = false;
                }

                return retVal;

            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in GetPjlTroyMarkFields.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }
        }

        private void SubstituteSpaces(ref byte[] inputBytes, int inputBytesLength)
        {
            try
            {
                int cntr = 0;
                while (cntr < inputBytesLength)
                {
                    if (inputBytes[cntr] == 0x5F)
                    {
                        inputBytes[cntr] = 0x20;
                    }
                    cntr++;
                }
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in Substitute Spaces.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
            }
        }

        private bool CreatePantographRegion(byte[] inData, ref byte[] retData, ref int dataLength, byte trailingChar)
        {
            try
            {
                byte[] headerBytes = new byte[3] { 0x1B, 0x25, 0x70 };

                Array.ConstrainedCopy(headerBytes, 0, retData, 0, 3);

                for (int Cntr = 0; Cntr < dataLength; Cntr++)
                {
                    retData[Cntr + 3] = inData[Cntr];
                }

                retData[dataLength + 3] = trailingChar;

                dataLength = dataLength + 4;

                return true;
            }
            catch (Exception ex)
            {
                pmLogging.LogError("Error in CreatePantographRegion.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, false);
                return false;
            }

        }

        private void FileCleanup()
        {
            bool cont = true;
            int loopCntr = 0;
            FileInfo fileFromPM;
            FileInfo tempFile;
            FileInfo encFile;
            fileFromPM = new FileInfo(printFileName);
            tempFile = new FileInfo(tempFileName);

            while (cont)
            {
                try
                {
                    if ((!fileFromPM.Exists) && (!tempFile.Exists))
                    {
                        cont = false;
                    }
                    else if (pmConfig.DebugBackupFilesPath.Length > 0)
                    {
                        string mainFileBackup, tempFileBackup;
                        mainFileBackup = pmConfig.DebugBackupFilesPath + fileFromPM.Name;
                        tempFileBackup = pmConfig.DebugBackupFilesPath + tempFile.Name;
                        FileInfo CheckBackupMainFile = new FileInfo(mainFileBackup);
                        int cntr = 0;
                        while (CheckBackupMainFile.Exists)
                        {
                            cntr++;
                            mainFileBackup += cntr.ToString();
                            CheckBackupMainFile = new FileInfo(mainFileBackup);
                        }
                        if (fileFromPM.Exists)
                        {
                            fileFromPM.MoveTo(mainFileBackup);
                        }

                        FileInfo CheckBackupTempFile = new FileInfo(tempFileBackup);
                        cntr = 0;
                        while (CheckBackupTempFile.Exists)
                        {
                            cntr++;
                            tempFileBackup += cntr.ToString();
                            CheckBackupTempFile = new FileInfo(tempFileBackup);
                        }
                        if (tempFile.Exists)
                        {
                            tempFile.MoveTo(tempFileBackup);
                        }

                    }
                    else
                    {
                        fileFromPM.Delete();
                        if (tempFileName != "")
                        {
                            tempFile.Delete();
                        }
                    }
                    if (encFileName != "")
                    {
                        encFile = new FileInfo(encFileName);
                        if (encFile.Exists)
                        {
                            encFile.Delete();
                        }
                    }

                    cont = false;
                }
                catch (Exception ex)
                {
                    //try this for 60 seconds
                    Thread.Sleep(100);
                    if (loopCntr++ > 600)
                    {
                        cont = false;
                        if (fileFromPM.IsReadOnly)
                        {
                            pmLogging.LogError("Error in FileCleanup. File: " + printFileName + " File is read only.", EventLogEntryType.Error, true);
                        }
                        else if (tempFile.IsReadOnly)
                        {
                            pmLogging.LogError("Error in FileCleanup. File: " + tempFile.Name + " File is read only.", EventLogEntryType.Error, false);
                        }
                        else if ((!fileFromPM.Exists) && (!tempFile.Exists))
                        {
                            //Mission accomplished.  Don't log an error

                        }
                        else
                        {
                            pmLogging.LogError("Error in FileCleanup.  File: " + printFileName + " Error: " + ex.Message, EventLogEntryType.Error, true);
                        }
                    }
                    else
                    {
                        cont = true;
                    }
                }
            }
        }
    }
}
