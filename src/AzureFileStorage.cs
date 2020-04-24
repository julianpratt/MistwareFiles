/*
License

Some of this may have been acquired from other sources, whose copyright has 
been lost. So no copyright is claimed and it is unreasonable to grant 
permission to use, copy, modify, etc (as in the normal MIT License). 

If any copyright holders identify their material herein, then the
appropriate copyright notice will be added. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;


namespace Mistware.Files
{
    /// Azure File Storage to match local equivalent (so facilitate switching between the two)
    /// One of the main differences between the Azure file system and local in System.IO is that
    /// the System.IO methods all accept fully qualified filenames. 
    public class AzureFileStorage : IFile
    {

        private ShareClient FileShare = null;
        private ShareDirectoryClient Directory = null;

        /// <summary>
        /// Azure File Storage constructor
        /// </summary>
        /// <param name="constr">The connection string</param>
        /// <param name="shareName">The name of the Share</param>
        public AzureFileStorage(string constr, string shareName)
        {
            FileShare = new ShareClient(constr, shareName);

            // Ensure that the share exists.
            if (!FileShare.Exists())
            {
                throw new Exception("Cannot access Azure File Share: " + shareName + ".");
            }
        }

        /// <summary>
        /// Make a directory (changes to it and makes it the current directory)
        /// </summary>
        /// <param name="directory">The name of the new directory (which is made current)</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        public bool MakeDirectory(string directory)
        {
            if (FileShare == null)
            {
                throw new Exception("Cannot make Azure directory: " + directory + " (null share).");
            }

            // Get a reference to the directory.
            Directory = FileShare.GetDirectoryClient(directory);

            // Ensure that the directory exists.
            if (Directory.Exists())
            {
                throw new Exception("Cannot make Azure directory: " + directory + " - it aleady exists.");
            }
            else
            {
                Directory.Create();
            }
            return true;
        }


        /// <summary>
        /// Delete a directory
        /// </summary>
        /// <param name="directory">The name of the directory to be deleted</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        public bool DeleteDirectory(string directory)
        {
            if (FileShare == null)
            {
                throw new Exception("Cannot make Azure directory: " + directory + " (null share).");
            }

            // Get a reference to the directory.
            Directory = FileShare.GetDirectoryClient(directory);

            // Ensure that the directory exists.
            if (!Directory.Exists())
            {
                throw new Exception("Cannot delete Azure directory: " + directory + " - it doesn't exist.");
            }
            else
            {
                Directory.Delete();
            }
            return true;
        }

        /// <summary>
        /// Change the current directory
        /// </summary>
        /// <param name="directory">The name of the new current directory</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        public bool ChangeDirectory(string directory)
        {
            if (FileShare == null)
            {
                throw new Exception("Cannot change to Azure directory: " + directory + " (null share).");
            }

            // Get a reference to the directory.
            Directory = FileShare.GetDirectoryClient(directory);

            // Ensure that the directory exists.
            if (!Directory.Exists())
            {
                throw new Exception("Cannot change to Azure directory: " + directory + ".");
            }
            return true;
        }

        /// <summary>
        /// Tests if a file exists in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        public bool FileExists(string filename)
        {
            if (FileShare == null || Directory == null)
            {
               throw new Exception("Cannot check to Azure file exists: " + filename + " (null share or directory).");
            }

            // Get a reference to the file
            ShareFileClient file = Directory.GetFileClient(filename);

            // Check whether it exists.
            return file.Exists();
        }

        /// <summary>
        /// Returns the length of a file exists in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>The length of the file. Any exceptions are thrown.</returns>
        public long FileLength(string filename)
        {
            if (FileShare == null || Directory == null)
            {
                throw new Exception("Cannot check to Azure file exists: " + filename + " (null share or directory).");
            }

            // Get a reference to the file
            ShareFileClient file = Directory.GetFileClient(filename);

            // Get the file length.
            ShareFileProperties props = file.GetProperties();

            return props.ContentLength;
        }

        /// <summary>
        /// Deletes a file in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to delete</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        public bool FileDelete(string filename)
        {
            if (FileShare == null || Directory == null)
            {
                throw new Exception("Cannot delete Azure file: " + filename + " (null share or directory).");
            }

            if (!this.FileExists(filename))
            {
                throw new Exception("Cannot delete Azure file: " + filename + " because it isn't there!");
            }

            // Get a reference to the file
            ShareFileClient file = Directory.GetFileClient(filename);

            // Check whether it exists.
            bool ret = file.DeleteIfExists();

            return ret;
        }

        /// <summary>
        /// Returns a list of file names in the current directory
        /// </summary>
        /// <returns>The list of directory entries. Any exceptions are thrown.</returns>
        public List<DirectoryEntry> FileList()
        {
            if (FileShare == null || Directory == null)
            {
                throw new Exception("Cannot list files (null share or directory).");
            }

            List<DirectoryEntry> l = new List<DirectoryEntry>();

            foreach (ShareFileItem i in Directory.GetFilesAndDirectories())
            {
                DirectoryEntry de = new DirectoryEntry();
                de.Name = i.Name;
                de.Length = this.FileLength(de.Name);
                l.Add(de);
            }

            return l;
        }

        /// <summary>
        /// Copy a file from the current directory
        /// </summary>
        /// <param name="filename">The name of the file to copy</param>
        /// <param name="target">The name of the directory to copy the file to</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileCopy(string filename, string target)
        {
            if (!this.FileExists(filename)) 
                throw new Exception("Failed to Copy File: '" + filename + "' from '" + Directory.Name + ". File was missing.");

            // Get a reference to the file
            ShareFileClient fileFrom = Directory.GetFileClient(filename);

            // Get a reference to the target directory.
            ShareDirectoryClient targetDir = FileShare.GetDirectoryClient(target);

            // Ensure that the directory exists.
            if (!targetDir.Exists())
            {
                throw new Exception("Cannot copy file to Azure directory: " + targetDir + ".");
            }
            try
            {
                ShareFileClient fileTo = targetDir.GetFileClient(filename);

                // Copy the file
                fileTo.StartCopy(fileFrom.Uri);
                ShareFileProperties props = fileTo.GetProperties();

                do
                {
                    Thread.Sleep(5);
                    //Console.WriteLine("Copy Progress: " + props.CopyProgress);
                } while (props.CopyStatus == CopyStatus.Pending);

                if (props.CopyStatus != CopyStatus.Success)
                throw new Exception("Failed to copy file: '" + filename + "' Error was: " + props.CopyStatusDescription);

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to copy file: '" + filename + "' Error was: " + ex.Message);
            }
        }


        /// <summary>
        /// Upload a file to the current directory
        /// </summary>
        /// <param name="filepath">The path and name of the file to upload</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileUpload(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            this.FileUpload(fi.Name, fi.OpenRead());
        }

        /// <summary>
        /// Upload a file to the stream
        /// </summary>
        /// <param name="filename">The name of the file to upload</param>
        /// <param name="stream">The stream associated with file to upload</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileUpload(string filename, Stream stream)
        {
            try
            {
                ShareFileClient file = Directory.GetFileClient(filename);
                file.Create(stream.Length);
                file.Upload(stream);
                // OR file.UploadRange(new HttpRange(0, stream.Length), stream);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Upload File: '" + filename + "' Error was: " + ex.Message);
            }
        }

        /// <summary>
        /// Download the file from the current directory to the target directory.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <param name="targetPath">The directory path to save the file to</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileDownload(string filename, string targetPath)
        {
            try
            {
                string path = filename;
                if (targetPath != null && targetPath.Length > 0) path = targetPath + PathDelimiter + filename;
                using (FileStream outputStream = new FileStream(path, FileMode.Create))
                {
                    this.FileDownload(filename, outputStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Download File: '" + filename + "' from '" + Directory.Name + "' to '" + targetPath + "' Error was: " + ex.Message);
            }
        }

        /// <summary>
        /// Download the file from the current directory to the stream.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <param name="toStream">The stream to copy the file to</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileDownload(string filename, Stream toStream)
        {
            if (!this.FileExists(filename)) 
                throw new Exception("Failed to Download File: '" + filename + "' from '" + Directory.Name + ". File was missing.");
                
            try
            {
                ShareFileClient file = Directory.GetFileClient(filename);
                ShareFileDownloadInfo download = file.Download();
                download.Content.CopyTo(toStream);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Download File: '" + filename + "' from '" + Directory.Name + "' Error was: " + ex.Message);
            }
        }
     
        /// <summary>
        /// Download the file from the current directory to a byte array.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <returns>The byte array. Any exceptions are thrown.</returns>
        public byte[] FileDownload(string filename)
        {
             byte[] fileContents = new byte[this.FileLength(filename)];

            if (!this.FileExists(filename)) 
                throw new Exception("Failed to Download File: '" + filename + "' from '" + Directory.Name + ". File was missing.");
                
            try
            {
                using (var stream = new MemoryStream(fileContents, true))
                {
                    ShareFileClient file = Directory.GetFileClient(filename);
                    ShareFileDownloadInfo download = file.Download();
                    download.Content.CopyTo(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Download File: '" + filename + "' from '" + Directory.Name + "' Error was: " + ex.Message);
            }

            return fileContents;
        }

        private string PathDelimiter
        {
            get { return Path.DirectorySeparatorChar.ToString(); }
        }
    
    }
}