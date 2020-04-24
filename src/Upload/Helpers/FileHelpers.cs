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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mistware.Files.Upload;

namespace Mistware.Files.Upload.Helpers
{
    /// Validate file being uploaded
    public static class FileHelpers
    {
 
        // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
        // and the official specifications for the file types you wish to add.
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },  
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".zip", new List<byte[]> 
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };

        // **WARNING!**
        // In the following file processing methods, the file's content isn't scanned.
        // In most production scenarios, an anti-virus/anti-malware scanner API is
        // used on the file before making the file available to users or other
        // systems. For more information, see the topic that accompanies this sample
        // app.

        /// Check the size of the file in the stream.
        /// If it is empty or too large, then throw an exception. 
        /// Returns 0 if OK or 413 (File too large)
        public static int CheckStreamSize(Stream stream, long sizeLimit)
        {
            long len = stream.Length;
            // Check if the file is empty or exceeds the size limit.
            if (len == 0)
            {
                throw new Exception("The file is empty.");
            }
            else if (len > sizeLimit)
            {
                long megabyteSizeLimit = sizeLimit / 1048576;
                return 413;
            }
            return 0;
        }

        /// Check the contents of a stream.
        /// First checks the source file name extension against a list of permitted extensions.
        /// Second inspects the contents of the stream to confirm it matches the extension.
        /// Returns 0 if OK or 415 (Invalid File Type)
        public static int CheckStreamContent(Stream stream, string sourceFilename, string[] permittedExtensions)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new Exception("Cannot process an empty stream in CheckStreamContent");
            }
            if (string.IsNullOrEmpty(sourceFilename))
            {
                throw new Exception("No filename in CheckStreamContent, so cannot determine file type.");
            }

            var ext = Path.GetExtension(sourceFilename).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext))
            {
                throw new Exception("File extension missing in CheckStreamContent.");
            }
            if (!permittedExtensions.Contains(ext)) return 415;
            
            stream.Position = 0;

            using (var reader = new BinaryReader(stream))
            {
                if (ext.Equals(".txt") || ext.Equals(".csv") )
                {
                    // Limits characters to ASCII encoding.
                    for (var i = 0; i < stream.Length; i++)
                    {
                        if (reader.ReadByte() > sbyte.MaxValue) return 415;
                    } 
                }
                else
                {
                    // File signature check
                    // --------------------
                    // With the file signatures provided in the _fileSignature dictionary, the following 
                    // code tests the input content's file signature.
                    var signatures = _fileSignature[ext];
                    var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                    bool ok = signatures.Any(signature => 
                        headerBytes.Take(signature.Length).SequenceEqual(signature));
                    if (!ok) return 415;
                }
            }
            return 0;
        }
    }
}
