using Journals.Model;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace Journals.Web.Helpers
{
    public static class JournalHelper
    {
        public static void PopulateFile(IFormFile file, Journal journal)
        {
            if (file != null && file.Length > 0)
            {
                journal.FileName = System.IO.Path.GetFileName(file.FileName);
                journal.ContentType = file.ContentType;

                using (var readStream = file.OpenReadStream())
                {
                    using (var reader = new System.IO.BinaryReader(readStream))
                    {
                        journal.Content = reader.ReadBytes((int)file.Length);
                    }
                }
            }
        }
    }
}