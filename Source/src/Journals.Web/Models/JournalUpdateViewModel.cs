using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Journals.Model
{

    public class IssueViewModel
    {
        public int Id { get; set; }

        public int JournalId { get; set; }

        public int? FileId { get; set; }

        [Required]
        public IFormFile File { get; set; }

    }


    public class JournalUpdateViewModel : IEquatable<JournalUpdateViewModel>
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required, DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public string UserId { get; set; }

        public bool Equals(JournalUpdateViewModel other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return
                Id == other.Id 
                && string.Equals(Title, other.Title) 
                && string.Equals(Description, other.Description)
                && UserId == other.UserId;
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
                hashCode = (hashCode * 397) ^ (Title?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (UserId?.GetHashCode() ?? 0); ;
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