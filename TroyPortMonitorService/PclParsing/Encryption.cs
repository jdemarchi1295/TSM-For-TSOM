using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

using System.IO;

namespace TroyPortMonitorService.PclParsing
{

    class Encryption
    {
        //The password and salt used to decrypt the user configured password stored in XML
        // WARNING: Do not change this password string without also changing the password in the code where encryption occurs
        //          The password used for decryption and encryption must match.
        static byte[] salt = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        static string password = "s82'*4'Kng4#3LS01$e1gf+2";
        byte[] globalPw = new UTF8Encoding(true).GetBytes(password);  //Byte representation of the password

        //The salt (aka Initial Vector) used for encryption.  This must match the firmware.
        // WARNING:  Do not change IV_192.  Must match the IV used in the firmware.
        private static byte[] IV_192 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        //This function takes the encrypted password stored in the XML and decrypts it.
        private bool GetPassword(byte[] encPassword, byte[] decPassword)
        {
            try
            {
                //Define the decryptor
                TripleDESCryptoServiceProvider tdesPwDecrypt = new TripleDESCryptoServiceProvider();
                tdesPwDecrypt.BlockSize = 64;  //8 byte block size
                tdesPwDecrypt.Padding = PaddingMode.Zeros;

                //The buffer tied to the memory stream below
                byte[] decryptedPw = new byte[64]; 
                
                //Use the cryptostream to decrypt the password into the byte array
                MemoryStream aStream = new MemoryStream(decryptedPw);
                CryptoStream decStreamPw = new CryptoStream(aStream, tdesPwDecrypt.CreateDecryptor(globalPw, salt), CryptoStreamMode.Write);
                decStreamPw.Write(encPassword, 0, encPassword.Length);
                decStreamPw.FlushFinalBlock();

                //Copy the result into the decPassword buffer
                Array.Copy(decryptedPw, decPassword, decryptedPw.Length);

                if (decryptedPw.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //Main function called to encrypt the file
        public bool EncryptData(String inFileName, String outFileName, string encKeyFromCfgStr, int EnterPclEndLocation)
        {
            FileStream fileIn;
            FileStream fileOut;
            try
            {
                PortMonCustomException PortMonException;

                //Decrypt the key stored in the XML
                byte[] decPassword = new byte[64];
                //                byte[] encKeyFromCfg = Encoding.ASCII.GetBytes(encKeyFromCfgStr);
                byte[] encKeyFromCfg = Convert.FromBase64String(encKeyFromCfgStr);
                if (!GetPassword(encKeyFromCfg, decPassword))
                {
                    PortMonException = new PortMonCustomException("Fatal Error: Password was not decrypted properly.", true);
                    throw PortMonException;
                }

                //Create the in and out file streams
                fileIn = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                fileOut = new FileStream(outFileName, FileMode.OpenOrCreate, FileAccess.Write);
                fileOut.SetLength(0);

                //Setup the encryption on PCL byte array
                //This must be placed immediately after the Enter PCL line
                const int encryptionOnLen = 5;
                byte[] encryptOn = new byte[encryptionOnLen] { 0x1B, 0x25, 0x63, 0x33, 0x54 };

                //Setup the encryption off PCL byte array.
                //This has to be the last PCL in the encrypted data.
                const int encryptOffLen = 5;
                byte[] encryptOff = new byte[encryptOffLen] { 0x1B, 0x25, 0x63, 0x30, 0x54 };

                //Create variables to help with read and write.
                byte[] bin = new byte[280]; //This is intermediate storage for the encryption.
                long rdlen = 0;              //This is the total number of bytes written.
                long totlen = fileIn.Length;    //This is the total length of the input file.
                int len;                     //This is the number of bytes to be written at a time.
                byte[] result;
                byte[] finalPin = new byte[24];

                //Need to get the non-null characters out of the password buffer
                // There has to be a better way to do this but I can find it other than stepping throught the byte array to look for a null.  TrimEnd did not work.
                string decPwStr = new UTF8Encoding(true).GetString(decPassword);
                int validDataLoc = decPwStr.IndexOf('\0');
                string tempStr = decPwStr.Substring(0, validDataLoc);
                byte[] adjustedPw = new UTF8Encoding(true).GetBytes(tempStr);

                //The password needs to be "SHA'd" before its used
                SHA1CryptoServiceProvider shaEncrypt = new SHA1CryptoServiceProvider();
                result = shaEncrypt.ComputeHash(adjustedPw);
                result.CopyTo(finalPin, 0);
                //SHA only returns 20 bytes.  The remaining 4 bytes are calculated
                finalPin[20] = Convert.ToByte(result[4] & result[12] | result[8] & result[16]);
                finalPin[21] = Convert.ToByte(result[5] & result[13] | result[9] & result[17]);
                finalPin[22] = Convert.ToByte(result[6] & result[14] | result[10] & result[18]);
                finalPin[23] = Convert.ToByte(result[7] & result[15] | result[11] & result[19]);

                //Create the encryptor and stream
                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                CryptoStream encStream = new CryptoStream(fileOut, tdes.CreateEncryptor(finalPin, IV_192), CryptoStreamMode.Write);
                tdes.Mode = CipherMode.CBC;
                tdes.BlockSize = 64;

                int uelLoc = 0;
                int adjLen, adjLoc, padLen;
                byte[] adjBin = new byte[280];
                decimal calcRem;

                byte[] tempbuff = new byte[EnterPclEndLocation + 1];

                //Read in up to the end of the Enter PCL location
                len = fileIn.Read(tempbuff, 0, EnterPclEndLocation + 1);
                fileOut.Write(tempbuff, 0, len);

                //Write out the PCL to turn on TDES encryption (<ESC>%c3T)
                fileOut.Write(encryptOn, 0, encryptionOnLen);
                rdlen = len;

                //Read from the input file, then encrypt and write to the output file.
                while (rdlen < totlen)
                {
                    //Read in 256 byte chunks of data.
                    len = fileIn.Read(bin, 0, 256);

                    //Look for the closing UEL.
                    if (FindUEL(bin, len, ref uelLoc))
                    {
                        //Complete UEL found
                        Array.Copy(bin, 0, adjBin, 0, uelLoc);
                        Array.Copy(encryptOff, 0, adjBin, uelLoc, encryptOffLen);

                        //if the new length does not end on a 8 byte boundary then pad with esc's
                        adjLoc = uelLoc + encryptOffLen;
                        calcRem = decimal.Remainder(adjLoc, 8);


                        //IMPORTANT NOTE:
                        // The following code is a case where I finally gave up trying to understand why
                        // and instead just made it work. 
                        // How its supposed to work is the last block is supposed to be padded with ESC's to 
                        // make an 8 byte boundary.  I was accidentally cutting off the last byte after 
                        // padding. For example, I had a buffer 0-55 bytes filled (56 bytes, 8 byte boundary)
                        // but I was only sending 55 bytes into the encryption routine instead of 56.
                        // It worked though. When I fixed it to include all 8 bytes it stopped 
                        // working. I was getting errors on the printer.  So I went back to cutting off the 
                        // 8th byte to get it to work.  Problem is, when I don't need the padding the 8th 
                        // byte is meaningful data so I can't cut it off.  On top of that, the 5 byte
                        // encryption off escape sequence has to be in the last byte. 
                        // So the calcRem variable is used to determine if padding is needed.  If the calcRem
                        // is 0 then padding is not needed.  For this case,the encryption off is 
                        // moved into the last byte from the previous byte (replaced with ESC's) and
                        // the remaining 3 bytes are padded. Then I cut off the last byte.  It works.  Don't
                        // know how or why but it works.  
                        if (calcRem > 0)
                        {
                            //- 1 to account for 0 based
                            padLen = 8 - Convert.ToInt32(calcRem);
                            for (int cntr = 0; cntr < padLen; cntr++)
                            {
                                adjBin[adjLoc + cntr + 1] = 0x1b;
                            }
                            adjLen = uelLoc + encryptOffLen + padLen - 1;
                        }
                        else
                        {
                            for (int cntr = 0; cntr < 5; cntr++)
                            {
                                adjBin[adjLoc - 5 + cntr] = 0x1b;
                            }
                            Array.Copy(encryptOff, 0, adjBin, adjLoc, encryptOffLen);
                            for (int cntr = 0; cntr < 3; cntr++)
                            {
                                adjBin[adjLoc + 5 + cntr] = 0x1b;
                            }
                            adjLen = uelLoc + encryptOffLen + 7;
                        }
                        encStream.Write(adjBin, 0, adjLen);
                        //Move the file cursor back to the beginning of the UEL.
                        //  This will cause the UEL to be reread and then dumped to the file unencrypted
                        fileIn.Seek(rdlen + uelLoc, SeekOrigin.Begin);
                        break;
                    }
                    //Was part of the UEL found?
                    else if (uelLoc > 0)
                    {
                        //Partial UEL found possibly
                        // Move the file cursor back and catch the entire UEL on the next read
                        fileIn.Seek(rdlen + 248, SeekOrigin.Begin);
                        encStream.Write(bin, 0, 248);
                        rdlen = rdlen + 248;
                    }
                    //Else dump the encryption data to the out file.
                    else
                    {
                        encStream.Write(bin, 0, len);
                        rdlen = rdlen + len;
                    }
                }
                encStream.FlushFinalBlock();

                //Read the last of the file.
                //TODO what if the last of the file is greater than 256? Shouldn't be.
                len = fileIn.Read(bin, 0, 256);

                fileOut.Write(bin, 0, len);
                fileIn.Close();
                fileOut.Flush();
                encStream.Close();
                fileOut.Close();
                return true;
            }
            catch (PortMonCustomException pme)
            {
                throw pme;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        static bool FindUEL(byte[] buf, int bufLen, ref int uelLoc)
        {
            //Look for <ESC>%-12345 which is the UEL
            byte[] UEL_BUF = { 0x1b, 0x25, 0x2d, 0x31, 0x32, 0x33, 0x34, 0x35 };
            int findIndex = 0;
            int cntr;
            uelLoc = -1;
            for (cntr = 0; cntr < bufLen; cntr++)
            {
                if (buf[cntr] == UEL_BUF[findIndex])
                {
                    findIndex++;
                    if (findIndex == UEL_BUF.Length)
                    {
                        uelLoc = cntr - UEL_BUF.Length + 1;
                        return true;
                    }
                }
                else
                {
                    findIndex = 0;
                }
            }
            if (findIndex > 0)
            {
                uelLoc = cntr - findIndex;
            }
            return false;
        }
        
    }

}
