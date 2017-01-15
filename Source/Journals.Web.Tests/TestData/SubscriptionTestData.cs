using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Security;
using Journals.Model;
using Journals.Repository;
using Serilog;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit.Sdk;

namespace Journals.Web.Tests.TestData
{
    public class SubscriptionTestData : TestData<Subscription>
    {

        private readonly IJournalRepository journalRepository;
        private readonly IStaticMembershipService membershipRepository;
        private readonly ILogger logger;

        public SubscriptionTestData(IJournalRepository journalRepository, IStaticMembershipService membershipRepository, ILogger logger)
        {
            this.journalRepository = journalRepository;
            this.membershipRepository = membershipRepository;
            this.logger = logger;
        }

        public override List<Subscription> GetDefaultData()
        {
            return new List<Subscription>
            {
                new Subscription
                {
                    Id = 1,
                    Journal = journalRepository.GetJournalById(1),
                    JournalId = 1,
                    UserId = 1,
                    User = membershipRepository.GetUserProfile(1)                
                },
                new Subscription
                {
                    Id = 2,
                    Journal = journalRepository.GetJournalById(2),
                    JournalId = 2,
                    UserId = 1,
                    User = membershipRepository.GetUserProfile(1)
                }
            };
        }


        public IEnumerable<object[]> SubscribeValidData => new List<object[]>()
        {
            new object[] {1},
            new object[] {2}
        };

        public IEnumerable<object[]> SubscribeInvalidData => new List<object[]>()
        {
            new object[] {-1, HttpStatusCode.NotFound},
            new object[] {3, HttpStatusCode.NotFound},
            new object[] {int.MaxValue, HttpStatusCode.NotFound},
            new object[] {int.MinValue, HttpStatusCode.NotFound},
            new object[] {10, HttpStatusCode.NotFound},
            new object[] {-10, HttpStatusCode.NotFound},
            new object[] {-654, HttpStatusCode.NotFound},
        };
    }
}