using ReserveBlockCore.Data;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReserveBlockCore.Utilities
{
    internal class BackupUtil
    {
        public static void BackupWalletData2()
        {
            string path = Directory.GetCurrentDirectory() + @"\Databases\rsrvwaldata.db";
            
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read,  FileShare.ReadWrite))
            {
                    using (StreamReader reader = new StreamReader(stream))
                    {
          
                    //Read the db fully into memory.
                    reader.ReadToEnd();

                    
                    FileStream sourceFile = stream;//File.OpenRead(path);
                    FileStream destinationFile = File.Create(path + ".gz");

                    
                    byte[] buffer = new byte[sourceFile.Length];
                    sourceFile.Read(buffer, 0, buffer.Length);


                    using (GZipStream output = new GZipStream(destinationFile,
                    CompressionMode.Compress))
                    {

                        Console.WriteLine("Compressing {0} to {1}.", sourceFile.Name,
                            destinationFile.Name, false);

                        output.Write(buffer, 0, buffer.Length);

            //stream.Flush();
            //stream.Close();
            //Still working on encrypting file.
            //var encryptedStream = EncryptFileUtil.EncryptFile(destinationFile, "testingPassword");

            //CopyStream(encryptedStream, Directory.GetCurrentDirectory() + @"\rsrvwaldata.encrypted");

            //encryptedStream.Flush();
            //encryptedStream.Dispose();

            //TestDecrypt();
                     }
                    
                    // Close the files.
                    sourceFile.Close();
                    destinationFile.Close();
                }
        

      }

        }
    public static void BackupWalletData()
    {
      var accounts = AccountData.GetAccounts();
      var accountsAll = accounts.FindAll();
      List<string> accountList = new List<string>();
/*      if (accountsAll.Count() > 0)
      {
        accountList = accountsAll.Select(x => x.PrivateKey).ToList().FindAll(z => z.Length == 64).ToList();
      }*/
      string path = Directory.GetCurrentDirectory() + @"\Databases\Backup.txt";
      File.AppendAllLines(path, accountList);
    }
    public static void RestoreWalletData()
    {
      //var fileCOntent = File.ReadAllText(args[])
    }
    
    private static void TestDecrypt()
        {
            var encryptedFilePath = Directory.GetCurrentDirectory() + @"\rsrvwaldata.encrypted";

            using (FileStream stream = File.Open(encryptedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    //reader.ReadToEnd();
                    var decryptedStream = DecryptFileUtil.DecryptFile(stream, "testingPassword");
                    //CopyStream(stream, Directory.GetCurrentDirectory() + @"\rsrvwaldata.decrypted.gz");
                }
            }
                    
            
        }
    }
}
