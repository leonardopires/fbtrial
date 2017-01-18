using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using Microsoft.AspNetCore.Http;

namespace Journals.Web.Helpers
{
    public static class FileUploadExtensions
    {
        public static void PopulateFile(this IFormFile formFile, File storageFile)
        {
            if (formFile != null && formFile.Length > 0)
            {
                storageFile.FileName = System.IO.Path.GetFileName(formFile.FileName);
                storageFile.ContentType = formFile.ContentType;

                using (var readStream = formFile.OpenReadStream())
                {
                    using (var reader = new System.IO.BinaryReader(readStream))
                    {
                        storageFile.Content = reader.ReadBytes((int)formFile.Length);
                    }
                }
            }
        }
    }
}
