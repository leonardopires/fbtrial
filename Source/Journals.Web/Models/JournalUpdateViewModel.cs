using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Journals.Model
{
    public class JournalUpdateViewModel : IEquatable<JournalUpdateViewModel>
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required, DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string ContentType { get; set; }

        public byte[] Content { get; set; }
//
//        [ValidateFile]
//        public HttpPostedFileBase File { get; set; }

        public int UserId { get; set; }

        public bool Equals(JournalUpdateViewModel other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Id == other.Id && string.Equals(Title, other.Title) && string.Equals(Description, other.Description)
                   && string.Equals(FileName, other.FileName) && string.Equals(ContentType, other.ContentType)
                   && (Equals(Content, other.Content) || Content.SequenceEqual(other.Content)) && UserId == other.UserId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((JournalUpdateViewModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContentType != null ? ContentType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Content != null ? Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ UserId;
                return hashCode;
            }
        }

        public static bool operator ==(JournalUpdateViewModel left, JournalUpdateViewModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(JournalUpdateViewModel left, JournalUpdateViewModel right)
        {
            return !Equals(left, right);
        }

    }
}