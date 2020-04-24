using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mistware.Files;
using Mistware.Utils;


namespace FileUpload.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IFile _filesys;
        private readonly string testDir;

        public IndexModel(IFile filesys)
        {
            _filesys = filesys;
            testDir = Config.Get("TestDirectory");
        }

        public List<DirectoryEntry> Files { get; private set; }

        public void OnGet()
        {
            _filesys.ChangeDirectory(testDir);
            Files = _filesys.FileList();
        }

        public IActionResult OnGetDownload(string filename)
        {
            string ext = Path.GetExtension(filename);
            _filesys.ChangeDirectory(testDir);
            byte[] file = _filesys.FileDownload(filename);
            return new FileContentResult(file, MIME.GetMimeType(ext));
        }
    }
}
