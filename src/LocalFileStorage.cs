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

using Mistware.Utils;


namespace Mistware.Files
{
    /// Local File Storage to match Azure equivalent (so facilitate switching between the two)
    /// One of the main differences between the Azure file system and local in System.IO is that
    /// the System.IO methods all accept fully qualified filenames. To fully replicate that members 
    /// of this class will only accept names of files in the current directory.
    public class LocalFileStorage : IFile
    {
        private string CurrentDirectory = null;

        private string Root = null;

        /// Local File Storage constructor (empty)
        public LocalFileStorage(string root)
        {
            Root = root;
            if (Root.Right(1) != PathDelimiter) Root += PathDelimiter;
        }

        /// <summary>
        /// Make a directory (changes to it and makes it the current directory)
        /// </summary>
        /// <param name="directory">The name of the new directory (which is made current)</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        public bool MakeDirectory(string directory)
        {
            // Ensure that the directory doesn't exist.
            if (Directory.Exists(FullPath(directory))) 
            {
                CurrentDirectory = null;
                return false;
            } 
            else
            {
                Directory.SetCurrentDirectory(Root);
                Directory.CreateDirectory(directory);
                CurrentDirectory = FullPath(directory);
                Directory.SetCurrentDirectory(CurrentDirectory);
                return true;
            }                            
        }

        /// <summary>
        /// Delete a directory
        /// </summary>
        /// <param name="directory">The name of the directory to be deleted</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        public bool DeleteDirectory(string directory)
        {
            // Ensure that the directory exists.
            if (!Directory.Exists(FullPath(directory))) 
            {
                return false;
            }
            else
            {
                Directory.SetCurrentDirectory(Root);
                Directory.Delete(directory);
                return true;
            }                            
        }

        /// <summary>
        /// Change the current directory
        /// </summary>
        /// <param name="directory">The name of the new current directory</param>
        /// <returns>True if directory exists. False if not.</returns>
        public bool ChangeDirectory(string directory)
        {
            // Ensure that the directory exists.
            if (!Directory.Exists(FullPath(directory))) 
            {
                CurrentDirectory = null;
                return false;
            }
            else
            {
                CurrentDirectory = FullPath(directory);
                Directory.SetCurrentDirectory(CurrentDirectory);
                
                return true;
            }                            
        }

        /// <summary>
        /// Tests if a file exists in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>True if it exists. False if not. Any exceptions are thrown.</returns>
        public bool FileExists(string filename)
        {
            if (!FilenameOK(filename))
                throw new Exception("Cannot test if file exists - Bad filename");
            if (CurrentDirectory == null) 
                throw new Exception("Cannot test if file exists - Current Directory not set");

            string filepath = CurrentDirectory + PathDelimiter + filename;
            return File.Exists(filepath);
        }

        /// <summary>
        /// Returns the length of a file exists in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>The length of the file. Any exceptions are thrown.</returns>
        public long FileLength(string filename)
        {
            if (!FilenameOK(filename))
                throw new Exception("Cannot get length of file - Bad filename");
            if (CurrentDirectory == null) 
                throw new Exception("Cannot get length of file - Current Directory not set");

            string filepath = CurrentDirectory + PathDelimiter + filename;
            FileInfo fi = new FileInfo(filepath);
            return fi.Length;
        }

        /// <summary>
        /// Deletes a file in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to delete</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        public bool FileDelete(string filename)
        {
            if (!FilenameOK(filename))
                throw new Exception("Cannot delete file - Bad filename");
            if (CurrentDirectory == null) 
                throw new Exception("Cannot delete file - Current Directory not set");

            if (!this.FileExists(filename))
            {
                throw new Exception("Cannot delete file " + filename + ", because it isn't there!");
            }

            string filepath = CurrentDirectory + PathDelimiter + filename;
            try
            {
                File.Delete(filepath);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot delete file " + filename + ", error was: " + ex.Message);
            }
        }

        /// <summary>
        /// Returns a list of file names in the current directory
        /// </summary>
        /// <returns>The list of directory entries. Any exceptions are thrown.</returns>
        public List<DirectoryEntry> FileList()
        {
            if (CurrentDirectory == null) 
                throw new Exception("Cannot list files - Current Directory not set");

            List<DirectoryEntry> l = new List<DirectoryEntry>();
            foreach (string f in Directory.EnumerateFiles(CurrentDirectory))
            {
                DirectoryEntry de = new DirectoryEntry();
                de.Name = GetFilename(f);
                de.Length = this.FileLength(de.Name);
                l.Add(de);
            } 
            
            return l;
        }

        /// <summary>
        /// Copy a file
        /// </summary>
        /// <param name="filename">The name of the file to copy</param>
        /// <param name="target">The name of the directory to copy the file to</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileCopy(string filename, string target)
        {
            FileDownload(filename, FullPath(target));
        }

        /// <summary>
        /// Upload a local file
        /// </summary>
        /// <param name="filepath">The path and name of the file to upload</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileUpload(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            this.FileUpload(fi.Name, fi.OpenRead());
        }

        /// <summary>
        /// Upload a file
        /// </summary>
        /// <param name="filename">The name of the file to upload</param>
        /// <param name="fromStream">The stream associated with file to upload</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileUpload(string filename, Stream fromStream)
        {
            if (!FilenameOK(filename))
                throw new Exception("Cannot upload file - Bad filename");
            if (CurrentDirectory == null) 
                throw new Exception("Cannot upload file - Current Directory not set");

            try
            {
                string path = CurrentDirectory + PathDelimiter + filename;
                using (FileStream toStream = new FileStream(path, FileMode.Create))
                {
                    StreamCopy(fromStream, toStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Upload File: '" + filename + "' Error was: " + ex.Message);
            }
        }

        /// <summary>
        /// Download the file from Azure source to the target directory.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <param name="target">The directory path to save the file to</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileDownload(string filename, string target)
        {
            if (!FilenameOK(filename))
                throw new Exception("Cannot copy/download file - Bad filename");
            if (CurrentDirectory == null) 
                throw new Exception("Cannot copy/download file - Current Directory not set");

            if (!this.FileExists(filename)) 
                throw new Exception("Cannot copy/download file - It doesn't exist");
            if (File.Exists(target + PathDelimiter + filename)) 
                throw new Exception("Cannot copy/download file - It already exists in the target folder: " + target);


            try
            {
                string toPath = filename;
                if (target != null && target.Length > 0) toPath = target + PathDelimiter + filename;
                using (FileStream toStream = new FileStream(toPath, FileMode.Create))
                {
                    string fromPath = CurrentDirectory + PathDelimiter + filename;
                    using (Stream fromStream = File.OpenRead(fromPath))
                    {
                        StreamCopy(fromStream, toStream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to copy/download File: '" + filename + "' from '" + CurrentDirectory + "' to '" + target + "' Error was: " + ex.Message);
            }
        }

        /// <summary>
        /// Download the file from Azure source to the target directory.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <param name="toStream">The stream to coy to</param>
        /// <returns>Any exceptions are thrown.</returns>
        public void FileDownload(string filename, Stream toStream)
        {
            if (!FilenameOK(filename))
                throw new Exception("Cannot copy/download file - Bad filename");
            if (CurrentDirectory == null) 
                throw new Exception("Cannot copy/download file - Current Directory not set");

            if (!this.FileExists(filename)) 
                throw new Exception("Failed to copy/download File: '" + filename + "' from '" + CurrentDirectory + ". File was missing.");

            try
            {
                string path = CurrentDirectory + PathDelimiter + filename;
                using (Stream fromStream = File.OpenRead(path))
                {
                    StreamCopy(fromStream, toStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to copy/download File: '" + filename + "' from '" + CurrentDirectory + "' Error was: " + ex.Message);
            }
        }

        /// <summary>
        /// Download the file from the current directory to a byte array.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <returns>The byte array. Any exceptions are thrown.</returns>
        public byte[] FileDownload(string filename)
        {
            byte[] file = null;

            if (!FilenameOK(filename))
                throw new Exception("Cannot download file - Bad filename");
            if (CurrentDirectory == null) 
                throw new Exception("Cannot download file - Current Directory not set");

            if (!this.FileExists(filename)) 
                throw new Exception("Cannot download file - It doesn't exist");
     
            try
            {
                string path = CurrentDirectory + PathDelimiter + filename;
                file = File.ReadAllBytes(path); 
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to download File: '" + filename + "' from '" + CurrentDirectory + "' to byte array. Error was: " + ex.Message);
            }

            return file;
        }


        /// <summary>
        /// Copy a file from one stream to another
        /// </summary>
        /// <param name="from">The stream to copy from</param>
        /// <param name="to">The stream to copy to</param>
        /// <returns>Any exceptions are thrown.</returns>
        private void StreamCopy(Stream from, Stream to)
        {
            try
            {
                int bufferSize = 4096;
                int n;
                long fileSize = 0;
                byte[] buffer = new byte[bufferSize];

                n = from.Read(buffer, 0, bufferSize);
                while (n > 0)
                {
                    to.Write(buffer, 0, n);
                    fileSize += n;
                    n = from.Read(buffer, 0, bufferSize);
                }
                from.Close();
                to.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to StreamCopy. Error was: " + ex.Message);
            }
        }

        private string FullPath(string directory)
        {
            return Root + directory;
        }

        private string PathDelimiter
        {
            get { return Path.DirectorySeparatorChar.ToString(); }
        }

        private bool FilenameOK(string filename)
        {
            if (filename == null) return false;
            string[] segments = filename.Split(Path.DirectorySeparatorChar);
            return (segments.Length == 1);
        }

        private string GetFilename(string filepath)
        {
            if (filepath == null) throw new Exception("Cannot get a file name from a null path");
            string[] segments = filepath.Split(Path.DirectorySeparatorChar);
            return segments[segments.Length - 1];
        }
    
    }
}