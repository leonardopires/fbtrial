using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using Microsoft.AspNetCore.Http;

namespace Journals.Web.Models
{
    public class IssueViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int JournalId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        
        public string Description { get; set; }

        [Required]
        public int FileId { get; set; }

        [ValidateFile]
        [Required]
        public IFormFile File { get; set; }

        public DateTime ModifiedDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime PublishedDate { get; set; }        
    }
}
