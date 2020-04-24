using System;
using System.Collections.Generic;
using System.IO;
using Mistware.Utils;
using Mistware.Files;

namespace MistwareFilesTest
{
	class Program
	{
		static void Main(string[] args)
		{
            Config.Setup("appsettings.json", Directory.GetCurrentDirectory(), null, "MistwareFilesTest");
            string connection = Config.Get("AzureConnectionString");
            string container  = Config.Get("AzureContainer");
            string root       = Config.Get("LocalFilesRoot");
            string logs       = Config.Get("Logs");
            string testfile   = "Test.log";
            string testfolder = "ZZTestZZ";

            // Setup Testing - creates a file called Test.log in ~/Temp
            Directory.SetCurrentDirectory(root);
			Log.Me.LogFile=testfile;
			Log.Me.Info("Hello, World");
            long length = (new FileInfo(testfile)).Length;
            Directory.CreateDirectory(testfolder);
            string dirsep = Path.DirectorySeparatorChar.ToString();
            string upload = root;
            string download = root + dirsep + testfolder;
      
            IFile local = new LocalFileStorage(root);
            Directory.SetCurrentDirectory(root);
            FileTest(local, "Local", testfile, upload, "Test1", "Test2", download, length, true);
            Console.WriteLine("");

            IFile azure = new AzureFileStorage(connection, container);
            FileTest(azure, "Azure", testfile, upload, "Test1", "Test2", download, length, true);

            // Clear up test artifacts
            Directory.SetCurrentDirectory(download);
            File.Delete(testfile);
            Directory.SetCurrentDirectory(upload);
            Directory.Delete(testfolder);
            File.Delete(testfile);

            // Test Azure Blob Logging
            Log.Me.Logger = new BlobLogger(connection, logs);
            Log.Me.LogFile = "LoggerTest.log";
            Log.Me.Info("This is a test");
        }

        public static void FileTest(IFile file, string name, string filename, string upload, string folder1, 
                                    string folder2, string download, long fileLength, bool tidy)
        {
            string sFail = "";

            file.MakeDirectory(folder1);
            Directory.SetCurrentDirectory(upload); // Set the local directory to upload from
            file.FileUpload(filename);

            if (!file.FileExists(filename)) sFail+="FileExists ";

            file.ChangeDirectory(folder1);
            if (!file.FileExists(filename)) sFail+="ChangeDirectory ";

            if (file.FileLength(filename) != fileLength)  sFail+="FileLength ";
    
            bool found = false;
            List<DirectoryEntry> files = file.FileList();
            foreach (DirectoryEntry de in files) if (de.Name == filename) found = true;
            if (!found) sFail+="FileList ";

            file.MakeDirectory(folder2);
            file.ChangeDirectory(folder1);
            file.FileCopy(filename, folder2);
            file.ChangeDirectory(folder2);
            if (!file.FileExists(filename)) sFail+="FileCopy ";

            file.FileDownload(filename, download);
            string dirsep = Path.DirectorySeparatorChar.ToString();
            if (!FileCompare(upload+dirsep+filename, download+dirsep+filename)) sFail+="FileDownload ";

            // Clean Up remote folders
            if (tidy)
            {
                file.FileDelete(filename);
                if (file.FileExists(filename)) sFail+="FileDelete ";
                file.ChangeDirectory(folder1);
                file.FileDelete(filename);
                file.DeleteDirectory(folder1);
                file.DeleteDirectory(folder2);
            }

            if (sFail.Length == 0) 
            {
                  Console.WriteLine("************************");
                  Console.WriteLine("** " + name + " Tests passed **");
                  Console.WriteLine("************************");      
            }            
            else 
            {
                  Console.WriteLine("!!!!!!!!!!!!!!!!!!!");                  
                  Console.WriteLine("!! " + name + " tests failed: " + sFail);                  
                  Console.WriteLine("!!!!!!!!!!!!!!!!!!!");                                    
            }
        }

        public static bool FileCompare(string file1, string file2)
        {
            bool result = false;
            using (Stream stream1 = File.OpenRead(file1))
            {
                using (Stream stream2 = File.OpenRead(file2))
                {
                    result = FileCompare(stream1, stream2);
                }
            }
            return result;
        }
        public static bool FileCompare(Stream stream1, Stream stream2)
        {
            try
            {
                bool match = true;
                int bufferSize = 4096;
                int n1, n2;
                byte[] buffer1 = new byte[bufferSize];
                byte[] buffer2 = new byte[bufferSize];

                n1 = stream1.Read(buffer1, 0, bufferSize);
                while (n1 > 0)
                {
                    n2 = stream2.Read(buffer2, 0, bufferSize);
                    if (n1 != n2) match = false;
                    else
                    {
                        for (int i = 0; i<n2; i++) if (buffer1[i] != buffer2[i]) match = false;
                    }
                    n1 = stream1.Read(buffer1, 0, bufferSize);
                }
                stream1.Close();
                stream2.Close();

                return match;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to FileCompare. Error was: " + ex.Message);
            }

        }
    }
}