using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mistware.Files.Upload.Filters;

namespace FileUpload.Pages
{
    [DisableFormValueModelBinding]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(52428800)]        // Handle requests up to 50 MB
    public class FileUploadModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
