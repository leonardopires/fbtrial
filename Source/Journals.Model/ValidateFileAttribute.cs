using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Journals.Model
{
    public class ValidateFileAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            double maxContentLength = 1024 * 1024 * 3.5; //3.5 MB
            string AllowedFileExtensions = ".pdf";

            var file = value as HttpPostedFileBase;

            if (file == null)
                return true;

            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                ErrorMessage = "The uploaded file must have a name";
                return false;
            }

            if (string.IsNullOrWhiteSpace(file.ContentType))
            {
                ErrorMessage = "The uploaded file content type must be specified";
                return false;
            }


            if (!AllowedFileExtensions.Equals(file.FileName.Substring(file.FileName.LastIndexOf('.')), StringComparison.InvariantCultureIgnoreCase))
            {
                ErrorMessage = "Please upload journal in pdf format";
                return false;
            }
            else if (file.ContentLength > maxContentLength)
            {
                ErrorMessage = "Journal is too large, maximum allowed size is : " + ((maxContentLength / 1024) / 1024).ToString() + "MB";
                return false;
            }
            else
                return true;
        }
    }
}