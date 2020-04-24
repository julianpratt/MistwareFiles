/*
The MIT License (MIT)
Copyright (c) Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial 
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Mistware.Files.Upload.Helpers;

using Mistware.Files;
using Mistware.Utils;

namespace Mistware.Files.Upload
{
    /// Uploader for Streamed Upload
    public static class Uploader
    {
        /// Streamed Upload
        public async static Task<int> Upload(HttpRequest request, IFile filesys, string targetFolder, 
                                                string filename, long fileSizeLimit, string[] permittedExtensions)
        {
            long _fileSizeLimit = fileSizeLimit;
            int reason = 0;

            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
            {
                throw new Exception("Failed on Request.ContentType = " + request.ContentType + " (Error 1)");
            }

            MediaTypeHeaderValue contentType = MediaTypeHeaderValue.Parse(request.ContentType);
            // Use the default form options to set the default limits for request body data.
            int lengthLimit = (new FormOptions()).MultipartBoundaryLengthLimit;
            string boundary = MultipartRequestHelper.GetBoundary(contentType, lengthLimit);
            MultipartReader  reader  = new MultipartReader(boundary, request.Body);
            MultipartSection section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = 
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file present without form data. 
                    // If form data is present, this method immediately fails and returns the error.
                    if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        throw new Exception("Failed on Content Disposition (Error 2)");
                    }
                    else
                    {   
                        // In the  following, the file is saved without scanning the file's contents. 
                        // In most production scenarios, an anti-virus/anti-malware scanner API
                        // is used on the file before making the file available for download or 
                        // for use by other systems. 

                        
                        using (var stream = new MemoryStream())
                        {
                            await section.Body.CopyToAsync(stream);

                            string sourceFilename = contentDisposition.FileName.Value;

                            reason = FileHelpers.CheckStreamSize(stream, _fileSizeLimit);
                            if (reason == 0) 
                            {
                                reason = FileHelpers.CheckStreamContent(stream, sourceFilename, permittedExtensions);
                            }

                            if (reason == 0)
                            {
                                byte[] bytes = stream.ToArray();
                                Stream stream2 = new MemoryStream(bytes);

                                filesys.ChangeDirectory(targetFolder);
                                string ext = Path.GetExtension(sourceFilename).ToLowerInvariant();
                                if (filename == null) filename = WebUtility.HtmlEncode(sourceFilename);
                                filename = Path.ChangeExtension(filename, ext);
                                filesys.FileUpload(filename, stream2);

                                Log.Me.Info("Uploaded " + WebUtility.HtmlEncode(sourceFilename) + " as " + filename);
                            } 
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return reason;
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = 
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }

}
