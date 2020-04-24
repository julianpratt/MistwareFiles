using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mistware.Files;
using Mistware.Utils;

namespace FileUpload.Pages
{
    public class DeleteFileModel : PageModel
    {
        private readonly IFile _filesys;

        public DeleteFileModel(IFile filesys)
        {
            _filesys = filesys;
        }

        public DirectoryEntry RemoveFile { get; private set; }

        public IActionResult OnGet(string filename)
        {
            if (filename.HasValue() && _filesys.FileExists(filename))
            { 
                RemoveFile = new DirectoryEntry(){ Name = filename, Length = _filesys.FileLength(filename) };
                return Page();
            }
            return RedirectToPage("./Index");
        }

        public IActionResult OnPost(string filename)
        {
            if (filename.HasValue() && _filesys.FileExists(filename)) _filesys.FileDelete(filename);
      
            return RedirectToPage("./Index");
        }
    }
}
