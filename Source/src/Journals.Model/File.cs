using System;

namespace Journals.Model
{
    public class File
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public byte[] Content { get; set; }

        public long Length { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    }
}