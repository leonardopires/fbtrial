using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Journals.Model
{
    public class JournalViewModel : IEquatable<JournalViewModel>
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required, DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public byte[] Content { get; set; }

        [Required, ValidateFile]
        public HttpPostedFileBase File { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(JournalViewModel other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Id == other.Id && string.Equals(Title, other.Title) && string.Equals(Description, other.Description) && string.Equals(FileName, other.FileName) && string.Equals(ContentType, other.ContentType) && (other.Content == Content || Content.SequenceEqual(other.Content)) && UserId == other.UserId;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((JournalViewModel) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
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

        public static bool operator ==(JournalViewModel left, JournalViewModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(JournalViewModel left, JournalViewModel right)
        {
            return !Equals(left, right);
        }

    }
}