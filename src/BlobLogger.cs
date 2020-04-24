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
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Mistware.Utils;

namespace Mistware.Files
{
    /// The BlobLogger writes logs to Azure Blobs.
    /// Each blob is identified by date, and only the last 60 days of logs are retained.  
    public class BlobLogger : ILog
    {
        private string _connectionString = null;
        private string _containerName = null;

        /// BlobLogger constructor takes: Azure connection string and  
        public BlobLogger(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName    = containerName;
        }

        /// Write Debug level log entry 
        public void Debug(object msg)
        {
            Write(LogLevel.Debug, msg);
        }

        /// Write Info level log entry 
        public void Info(object msg)
        {
            Write(LogLevel.Info, msg);
        }
        
        /// Write Warn level log entry 
        public void Warn(object msg)
        {
            Write(LogLevel.Warn, msg);
        }

        /// Write Error level log entry 
        public void Error(object msg)
        {
            Write(LogLevel.Error, msg);
        }

        /// Write Fatal level log entry 
        public void Fatal(object msg)
        {
            Write(LogLevel.Fatal, msg);
        }
      
        /// Fully qualified name of the logfile stem (a date stamp is appended to this name).
        public string LogFile { get; set; }    

        private string LogFileStamped
        {
            get
            {
                string ext = Path.GetExtension(LogFile);
                string fn  = Path.GetFileNameWithoutExtension(LogFile);
                return string.Format("{0}-{1}{2}", fn, DateTime.Today.ToString("yyyyMMdd"), ext);
            }
        }  
     
        private void Write(LogLevel level, object msg)
        {
            LogRecord lr = new LogRecord(level, msg);

            WriteLogLine(lr.ToString());

            DeleteOldLogFiles();
           
        }

        private Object thisLock = new Object();

        private void WriteLogLine(string line)
        {
            AppendBlobClient client = new AppendBlobClient(_connectionString, _containerName, LogFileStamped);
            client.CreateIfNotExists();

            lock (thisLock)
            {    
                using (MemoryStream stream = new MemoryStream())
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.Write(line);
                    sw.Write("\n");
                    sw.Flush();
                    stream.Position = 0;
                    client.AppendBlock(stream);
                } 
            }    
        }

        private void DeleteOldLogFiles()
        {
            BlobContainerClient client = new BlobContainerClient(_connectionString, _containerName);

            int retention = 60; // Keep the last 60 days of log files

            int today    = DateTime.Now.DOY() + 366;
            int thisyear = DateTime.Now.Year; 

            foreach(BlobItem item in client.GetBlobs())
            {
                string name = item.Name;
                string date = name.Mid((name.IndexOf("-"))+1, 8);
                int year  = date.Left(4).ToInteger();
                DateTime dt = new DateTime(year, date.Mid(4,2).ToInteger(), date.Mid(6,2).ToInteger());
            
                if ((dt.DOY() + 366 * (year - thisyear + 1)) < (today - retention)) 
                {
                    client.DeleteBlob(name);
                }
            }
        }
   
    }
}    