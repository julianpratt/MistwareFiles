using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mistware.Files;
using Mistware.Files.Upload;
using Mistware.Files.Upload.Filters;
using Mistware.Utils;

namespace FileUploadSample.Controllers
{
    public class StreamingController : Controller
    {
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".txt", ".jpg", ".pdf" };
        private IFile _filesys = null;

        public StreamingController(IFile filesys)
        {
            _fileSizeLimit = Config.Get("FileSizeLimit").ToLong();
            _filesys = filesys;
        }

        // The following upload methods:
        //
        // 1. Disable the form value model binding to take control of handling 
        //    potentially large files.
        //
        // 2. Typically, antiforgery tokens are sent in request body. Since we 
        //    don't want to read the request body early, the tokens are sent via 
        //    headers. The antiforgery token filter first looks for tokens in 
        //    the request header and then falls back to reading the body.

        [HttpPost]
        [DisableFormValueModelBinding]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]        // Handle requests up to 50 MB
        public async Task<IActionResult> UploadPhysical()
        {
            string testDir = Config.Get("TestDirectory");
            _filesys.ChangeDirectory(testDir);
            string filename = Path.GetRandomFileName();

            try
            {
                int reason = await Uploader.Upload(Request, _filesys, testDir, filename, _fileSizeLimit, _permittedExtensions);

                if (reason > 0)
                {
                    string message = null;
                    if (reason == 415) message = "Type of file not accepted";
                    if (reason == 413) message = "File too large";
                    Log.Me.Warn(message);
                    return StatusCode(reason, message);
                }

            }
            catch (Exception ex)
            {
                Log.Me.Error(ex.Message);
                ModelState.AddModelError("File", ex.Message); 
                BadRequest(ModelState);
            }

            return Created(nameof(StreamingController), null);
        }
    }

    /// String extensions
    public static class Extensions
    {
        /// Convert string to long
        public static long ToLong(this string value, long deflt = 0L)
        {
            if (value == null) return deflt;

            long result = deflt;
            if (!long.TryParse(value, out result)) 
                throw new Exception("Unable to parse '" + value + "' as a long");
            
            return result;
        }

    }
   
}
