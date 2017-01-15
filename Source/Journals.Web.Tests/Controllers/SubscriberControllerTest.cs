using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http.Results;
using System.Web.Mvc;
using Autofac;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Mvc;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Controllers;
using Journals.Web.Tests.Framework;
using Journals.Web.Tests.TestData;
using Telerik.JustMock.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Controllers
{
    public class SubscriberControllerTest :
        MvcControllerTest<SubscriberController, Subscription, SubscriptionTestData>
    {

        public SubscriberControllerTest(ITestOutputHelper output) : base(output)
        {
            Mapper.Initialize(c =>
            {
                c.CreateMap<Journal, JournalViewModel>();
                c.CreateMap<JournalViewModel, Journal>();

                c.CreateMap<Journal, SubscriptionViewModel>();
                c.CreateMap<SubscriptionViewModel, Journal>();
            });
        }

        protected override void InitializeContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<MocksModule>();
            base.InitializeContainer(builder);
        }

        [Theory]
        [InlineData(2, false)]
        [InlineData(0, true)]
        public void Index_Has_All_Journals(int count, bool testNullJournals)
        {
            var repo = Container.Resolve<ISubscriptionRepository>().As<IMockWrapper<ISubscriptionRepository>>();

            if (testNullJournals)
            {
                repo.GetMock().Arrange(r => r.GetAllJournals()).Returns((List<Journal>)null);
            }

            var controller = GetController();

            var result = controller.Index();

            result.Should().BeViewResult().WithDefaultViewName().Model.As<List<SubscriptionViewModel>>().Should().HaveCount(count);
        }

        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.SubscribeValidData))]
        public void Subscribe_With_Valid_Data_Adds_Subscription(int id)
        {
            var controller = GetController();

            var result = controller.Subscribe(id);

            result.Should().BeRedirectToRouteResult().WithAction(nameof(controller.Index));
        }


        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.SubscribeInvalidData))]
        public void Subscribe_With_Invalid_Data_Shows_Error(int id, HttpStatusCode expectedStatusCode)
        {
            var controller = GetController();

            var result = controller.Subscribe(id);

            result.Should().BeAssignableTo<HttpStatusCodeResult>().Which.StatusCode.Should().Be((int)expectedStatusCode);
        }


        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.SubscribeValidData))]
        public void Unsubscribe_With_Valid_Data_Removes_Subscription(int id)
        {
            var controller = GetController();

            var result = controller.UnSubscribe(id);

            result.Should().BeRedirectToRouteResult().WithAction(nameof(controller.Index));
        }


        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.SubscribeInvalidData))]
        public void Unsubscribe_With_Invalid_Data_Shows_Error(int id, HttpStatusCode expectedStatusCode)
        {
            var controller = GetController();

            var result = controller.UnSubscribe(id);

            result.Should().BeAssignableTo<HttpStatusCodeResult>().Which.StatusCode.Should().Be((int)expectedStatusCode);
        }
    }
}