﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Journals.Model
{
    public class Subscription
    {
        public int Id { get; set; }

        [ForeignKey("JournalId")]
        public Journal Journal { get; set; }

        public int JournalId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public string UserId { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    }
}