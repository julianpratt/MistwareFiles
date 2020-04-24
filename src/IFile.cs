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

using System.Collections.Generic;
using System.IO;

namespace Mistware.Files 
{
    /// Defines methods required by each filesystem
    public interface IFile
    {
        /// <summary>
        /// Make a directory (changes to it and makes it the current directory)
        /// </summary>
        /// <param name="directory">The name of the new directory (which is made current)</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        bool MakeDirectory(string directory);

        /// <summary>
        /// Delete a directory
        /// </summary>
        /// <param name="directory">The name of the directory to be deleted</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        bool DeleteDirectory(string directory);

        /// <summary>
        /// Change the current directory
        /// </summary>
        /// <param name="directory">The name of the new current directory</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        bool ChangeDirectory(string directory);

        /// <summary>
        /// Tests if a file exists in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        bool FileExists(string filename);

        /// <summary>
        /// Returns the length of a file exists in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>The length of the file. Any exceptions are thrown.</returns>
        long FileLength(string filename);

        /// <summary>
        /// Deletes a file in the current directory
        /// </summary>
        /// <param name="filename">The name of the file to delete</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        bool FileDelete(string filename);

        /// <summary>
        /// Returns a list of file names in the current directory
        /// </summary>
        /// <returns>The list of directory entries. Any exceptions are thrown.</returns>
        List<DirectoryEntry> FileList();

        /// <summary>
        /// Copy a file from the current directory
        /// </summary>
        /// <param name="filename">The name of the file to copy</param>
        /// <param name="target">The name of the directory to copy the file to</param>
        /// <returns>True if successful. False if not. Any exceptions are thrown.</returns>
        void FileCopy(string filename, string target);

        /// <summary>
        /// Upload a file to the current directory
        /// </summary>
        /// <param name="filepath">The path and name of the file to upload</param>
        /// <returns>Any exceptions are thrown.</returns>
        void FileUpload(string filepath);

        /// <summary>
        /// Upload a file to the stream
        /// </summary>
        /// <param name="filename">The name of the file to upload</param>
        /// <param name="stream">The stream associated with file to upload</param>
        /// <returns>Any exceptions are thrown.</returns>
        void FileUpload(string filename, Stream stream);

        /// <summary>
        /// Download the file from the current directory to the target directory.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <param name="targetPath">The directory path to save the file to</param>
        /// <returns>Any exceptions are thrown.</returns>
        void FileDownload(string filename, string targetPath);

        /// <summary>
        /// Download the file from the current directory to the stream.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <param name="toStream">The stream to copy the file to</param>
        /// <returns>Any exceptions are thrown.</returns>
        void FileDownload(string filename, Stream toStream);

        /// <summary>
        /// Download the file from the current directory to a byte array.
        /// </summary>
        /// <param name="filename">The name of the file to download</param>
        /// <returns>The byte array. Any exceptions are thrown.</returns>
        byte[] FileDownload(string filename);

    }
}