using System;
using System.Collections.Generic;
using System.Text;
using Troy.Core.Configs;
using Troy.Core.Serialization;

namespace TroyPortMonitorService.Blob
{
    public class BlobProcessor
    {
        public struct InsertPclConfigurationArea
        {
            public byte[] pclByteArray;
            public int pclByteArrayLength;
            public int pageNumber;
        }
        public List<InsertPclConfigurationArea> insertPclConfigurationArea = new List<InsertPclConfigurationArea>();

        public struct InsertPclPrintedArea
        {
            public byte[] pclByteArray;
            public int pclByteArrayLength;
            public int pageNumber;
            public bool insertNewPage;
        }
        public List<InsertPclPrintedArea> insertPclPrintedArea = new List<InsertPclPrintedArea>();
        public List<InsertPclPrintedArea> insertPclOnNewPage = new List<InsertPclPrintedArea>();

        public PageConfig pgConfig;

        public bool ParseBlob(string SerializedData, int currentPageNum, ref string PrinterName, ref bool TSOMEnableDuplex)
        {
            try
            {
                InsertPclPrintedArea iPclAreaPA;
                InsertPclConfigurationArea iPclAreaCF;

                pgConfig = XMLSerialization.XmlDeserialiseDecompress64<PageConfig>(SerializedData);
 
                if ((pgConfig.PrinterName != null) && (pgConfig.PrinterName != ""))
                {
                    PrinterName = pgConfig.PrinterName;
                }

                string pclString = "";

                TSOMEnableDuplex = pgConfig.Duplex;

                if (pgConfig.InitialPaperSize != "")
                {
                    pclString = pgConfig.InitialPaperSize;
                    pclString = pclString.Replace("/e", "\u001B");
                    iPclAreaCF = new InsertPclConfigurationArea();
                    iPclAreaCF.pageNumber = currentPageNum;
                    iPclAreaCF.pclByteArrayLength = pclString.Length;
                    iPclAreaCF.pclByteArray = new UTF8Encoding(true).GetBytes(pclString);
                    insertPclConfigurationArea.Add(iPclAreaCF);
                }

                if (pgConfig.InitialPaperSource != "")
                {
                    pclString = pgConfig.InitialPaperSource;
                    pclString = pclString.Replace("/e", "\u001B");
                    iPclAreaCF = new InsertPclConfigurationArea();
                    iPclAreaCF.pageNumber = currentPageNum;
                    iPclAreaCF.pclByteArrayLength = pclString.Length;
                    iPclAreaCF.pclByteArray = new UTF8Encoding(true).GetBytes(pclString);
                    insertPclConfigurationArea.Add(iPclAreaCF);
                }

                if (pgConfig.AltPaperSize != "")
                {
                    pclString = pgConfig.AltPaperSize;
                    pclString = pclString.Replace("/e", "\u001B");
                    iPclAreaCF = new InsertPclConfigurationArea();
                    iPclAreaCF.pageNumber = currentPageNum + 1;
                    iPclAreaCF.pclByteArrayLength = pclString.Length;
                    iPclAreaCF.pclByteArray = new UTF8Encoding(true).GetBytes(pclString);
                    insertPclConfigurationArea.Add(iPclAreaCF);
                }

                if (pgConfig.AltPaperSource != "")
                {
                    pclString = pgConfig.AltPaperSource;
                    pclString = pclString.Replace("/e", "\u001B");
                    iPclAreaCF = new InsertPclConfigurationArea();
                    iPclAreaCF.pageNumber = currentPageNum + 1;
                    iPclAreaCF.pclByteArrayLength = pclString.Length;
                    iPclAreaCF.pclByteArray = new UTF8Encoding(true).GetBytes(pclString);
                    insertPclConfigurationArea.Add(iPclAreaCF);
                }

                //Added Jan 22, 2010:  If Suppress Security Calls is true then don't do the font stuff
                if (!pgConfig.SupressSecurityCalls)
                {
                    pclString = pgConfig.getFontInfoString(true);
                    if (pclString != "")
                    {
                        iPclAreaPA = new InsertPclPrintedArea();
                        iPclAreaPA.insertNewPage = pgConfig.InsertBackPage;
                        iPclAreaPA.pageNumber = currentPageNum;
                        //FOR CHUBB
                        pclString += "\u001B*p0x0Y";
                        iPclAreaPA.pclByteArrayLength = pclString.Length;
                        iPclAreaPA.pclByteArray = new UTF8Encoding(true).GetBytes(pclString);
                        insertPclPrintedArea.Add(iPclAreaPA);
                    }

                    pclString = pgConfig.getFontInfoString(false);
                    if (pclString != "")
                    {
                        iPclAreaPA = new InsertPclPrintedArea();
                        iPclAreaPA.insertNewPage = pgConfig.InsertBackPage;
                        iPclAreaPA.pageNumber = currentPageNum;
                        iPclAreaPA.pclByteArrayLength = pclString.Length;
                        iPclAreaPA.pclByteArray = new UTF8Encoding(true).GetBytes(pclString);
                        insertPclOnNewPage.Add(iPclAreaPA);
                    }
                }


                pclString = pgConfig.getPclConfiguration();
                if (pclString.Length > 0)
                {
                    iPclAreaCF = new InsertPclConfigurationArea();
                    iPclAreaCF.pageNumber = currentPageNum;
                    iPclAreaCF.pclByteArrayLength = pclString.Length;
                    iPclAreaCF.pclByteArray = new UTF8Encoding(true).GetBytes(pclString);
                    insertPclConfigurationArea.Add(iPclAreaCF);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
