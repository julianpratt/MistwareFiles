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
using Mistware.Utils;

namespace Mistware.Files 
{
    /// Bootstrap the File System
    public static class FileBootstrap
    {
        /// Construct a file system 
        /// Typically used in Startup.ConfigureServices thus:
        ///   services.AddSingleton&lt;IFile&gt;(FileBootstrap.SetupFileSys(connection, container, root));
        /// Three configuration strings must be supplied. Usually only the 2 Azure strings or root will
        /// have a value, but if all 3 are specified then Azure is used.
        /// Also sets up logging.
        /// <param name="connection">The Azure connection string</param>
        /// <param name="container">The Azure File Share container</param>
        /// <param name="root">The local file system root</param>
        /// <param name="logs">The BLOB (or local) folder containing log files</param>
        /// <returns>The file system. Any exceptions are thrown.</returns>
        public static IFile SetupFileSys(string connection, string container, string root, string logs)
        {
            if (!string.IsNullOrEmpty(connection))
            {
                // Using Azure for storing files, so put logs there also 
                Log.Me.Logger = new BlobLogger(connection, logs);
                if (!Log.Me.LogFile.HasValue()) Log.Me.LogFile = container + ".logs"; // Ensure LogFile has a value
                return new AzureFileStorage(connection, container);
            }
            else if (!string.IsNullOrEmpty(root))
            {
                // TODO: For consistency, adjust Mistware.Utils.Config so it takes logs as an option
                //       Currently it puts log files in contentroot/Logs (which assumes that logs="Logs").
                return new LocalFileStorage(root);
            }
            else
            {
                throw new Exception("Cannot SetupFileSys - neither Azure nor local option was configured.");
            }
        }
    }
}