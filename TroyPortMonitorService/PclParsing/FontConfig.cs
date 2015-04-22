using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

using System.Diagnostics;

namespace TroyPortMonitorService.PclParsing
{
    public class FontInfo
    {
        public string FontName;
        public string LeadingPcl;
        public string TrailingPcl;
        public string GlyphMapFile;
        public string FontType;
        public string ConfigurationPage;
    }

    public class FontConfig
    {
        public List<FontInfo> FontMap = new List<FontInfo>();

    }

    class FontConfigOrig
    {
        public FontConfigOrig()
        {

        }

        private string fontName;
        public string FontName
        {
            get { return fontName; }
            set { fontName = value; }
        }

        private string leadingPcl;
        public string LeadingPCL
        {
            get { return leadingPcl; }
            set { leadingPcl = value; }
        }

        private string trailingPcl;
        public string TrailingPCL
        {
            get { return trailingPcl; }
            set { trailingPcl = value; }
        }

        private string glyphMapFile;
        public string GlyphMapFile
        {
            get { return glyphMapFile; }
            set { glyphMapFile = value; }
        }

        private string fontType;
        public string FontType
        {
            get { return fontType; }
            set { fontType = value; }
        }

        private bool configPage;
        public bool ConfigPage
        {
            get { return configPage; }
            set { configPage = value; }
        }

        byte[] glyphToCharMap;
        public bool LoadGlyphToCharMap(string fileName)
        {
            try
            {
                if (glyphToCharMap != null)
                {
                    glyphToCharMap = null;
                }

                glyphToCharMap = new byte[255];

                if (!File.Exists(fileName))
                {
                    MessageBox.Show("Unable to find Glyph to Char Map file named " + fileName, "TROY Port Monitor Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }

                string inputLine;
                int commaLoc, glyphId, fontChar;
                FileInfo mapFile = new FileInfo(fileName);

                StreamReader mapData = mapFile.OpenText();
                inputLine = mapData.ReadLine().TrimStart(' ');

                while (inputLine != null)
                {
                    inputLine = inputLine.TrimStart(' ');
                    if (inputLine.StartsWith("//"))
                    {
                        //ignore this line, commented
                    }
                    else if ((commaLoc = inputLine.IndexOf(',')) < 1)
                    {
                        MessageBox.Show("Invalid data in the Glyph To Char file " + fileName + ".  Missing comma.", "TROY Port Monitor Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    }
                    else
                    {
                        glyphId = Convert.ToInt32(inputLine.Substring(0, commaLoc));
                        fontChar = Convert.ToInt32(inputLine.Substring(commaLoc + 1));
                        glyphToCharMap[glyphId] = Convert.ToByte(fontChar);
                    }

                    inputLine = mapData.ReadLine();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error In LoadGlyphToCharMap(). " + ex.Message.ToString(), "TROY Port Monitor Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                throw ex;
            }

        }

        public char GetCharFromGlyphId(int glyphId)
        {
            return Convert.ToChar(glyphToCharMap[glyphId]);

        }

        public bool GetLeadingPcl(ref byte[] pcl, ref int pclLength, ref bool deleteFont)
        {
            try
            {
                bool slashFound = false;
                int arrayIndex = 0;
                int holdSize;


                if (pclLength < leadingPcl.Length)
                {
                    return false;
                }

                if (leadingPcl.ToUpper() == "/DELETE")
                {
                    deleteFont = true;
                    return true;
                }

                foreach (char fontChar in leadingPcl)
                {
                    if (fontChar == '/')
                    {
                        slashFound = true;
                    }
                    else
                    {
                        if (slashFound)
                        {
                            if (fontChar == 'e')
                            {
                                pcl[arrayIndex] = 0x1b;
                            }
                            else if (fontChar == 'r')
                            {
                                pcl[arrayIndex] = 0x0d;
                            }
                            else if (fontChar == 'n')
                            {
                                pcl[arrayIndex] = 0x0a;
                            }
                            else if (fontChar == '0')
                            {
                                pcl[arrayIndex] = 0;
                            }
                            else if (fontChar == '/')
                            {
                                pcl[arrayIndex] = 0x2F;
                            }
                            else
                            {
                                MessageBox.Show("Invalid character after the slash in GetLeadingPcl(). Character " + pcl[arrayIndex], "TROY Port Monitor Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                                return false;
                            }
                            slashFound = false;
                            arrayIndex++;
                        }
                        else
                        {
                            pcl[arrayIndex] = Convert.ToByte(fontChar);
                            arrayIndex++;
                        }
                    }
                }
                holdSize = arrayIndex;
                for (int cntr = arrayIndex; cntr < pclLength; cntr++)
                {
                    pcl[cntr] = 0;
                }
                pclLength = holdSize;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error In GetLeadingPcl(). " + ex.Message.ToString(), "TROY Port Monitor Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                throw ex;
            }
        }

        public bool GetTrailingPcl(ref byte[] pcl, ref int pclLength)
        {
            try
            {
                bool slashFound = false;
                int arrayIndex = 0;
                int holdSize;


                if (pclLength < trailingPcl.Length)
                {
                    return false;
                }

                if (trailingPcl.Length == 0)
                {
                    for (int cntr2 = arrayIndex; cntr2 < pclLength; cntr2++)
                    {
                        pcl[cntr2] = 0;
                    }

                }

                foreach (char fontChar in trailingPcl)
                {
                    if (fontChar == '/')
                    {
                        slashFound = true;
                    }
                    else
                    {
                        if (slashFound)
                        {
                            if (fontChar == 'e')
                            {
                                pcl[arrayIndex] = 0x1b;
                            }
                            else if (fontChar == 'r')
                            {
                                pcl[arrayIndex] = 0x0d;
                            }
                            else if (fontChar == 'n')
                            {
                                pcl[arrayIndex] = 0x0a;
                            }
                            else if (fontChar == '0')
                            {
                                pcl[arrayIndex] = 0;
                            }
                            else if (fontChar == '/')
                            {
                                pcl[arrayIndex] = 0x2F;
                            }
                            else
                            {
                                MessageBox.Show("Invalid character after the slash in GetLeadingPcl(). Character " + pcl[arrayIndex], "TROY Port Monitor Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                                return false;
                            }
                            slashFound = false;
                            arrayIndex++;
                        }
                        else
                        {
                            pcl[arrayIndex] = Convert.ToByte(fontChar);
                            arrayIndex++;
                        }
                    }
                }
                holdSize = arrayIndex;
                for (int cntr = arrayIndex; cntr < pclLength; cntr++)
                {
                    pcl[cntr] = 0;
                }
                pclLength = holdSize;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error In GetTrailingPcl(). " + ex.Message.ToString(), "TROY Port Monitor Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                throw ex;
            }
        }
    }
}
