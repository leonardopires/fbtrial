using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Tests.Framework;
using Serilog;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit.Sdk;
using LP.Test.Framework.Core;

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
                    UserId = JournalTestData.GUID_ONE,
                    User = membershipRepository.GetUser(JournalTestData.GUID_ONE)
                },
                new Subscription
                {
                    Id = 2,
                    Journal = journalRepository.GetJournalById(2),
                    JournalId = 2,
                    UserId = JournalTestData.GUID_ONE,
                    User = membershipRepository.GetUser(JournalTestData.GUID_ONE)
                }
            };
        }


        public IEnumerable<object[]> SubscribeValidData =>
            Data(
                Item(1),
                Item(2)
            );

        public IEnumerable<object[]> SubscribeInvalidData =>
            Data(

                Item(-1, HttpStatusCode.NotFound),
                Item(3, HttpStatusCode.NotFound),
                Item(int.MaxValue, HttpStatusCode.NotFound),
                Item(int.MinValue, HttpStatusCode.NotFound),
                Item(10, HttpStatusCode.NotFound),
                Item(-10, HttpStatusCode.NotFound),
                Item(-654, HttpStatusCode.NotFound)
            );

    }
}