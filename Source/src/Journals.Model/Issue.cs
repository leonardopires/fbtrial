using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Journals.Model
{
    public class Issue
    {
        public int Id { get; set; }

        public int JournalId { get; set; }

        [ForeignKey("JournalId")]
        public Journal Journal { get; set; }

        public int FileId { get; set; }

        [ForeignKey("FileId")]
        public File File { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }


        public DateTime ModifiedDate {get; set;}

        public DateTime CreatedDate { get; set; }

        public DateTime PublishedDate { get; set; }

    }
}