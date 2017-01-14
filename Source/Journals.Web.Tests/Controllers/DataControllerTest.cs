using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Tests.TestData;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.Controllers
{
    /// <summary>
    /// Implements the base functionality for a test that exercises an ASP.NET MVC controller that uses a repository
    /// </summary>
    /// <typeparam name="TController">The type of the controller to be tested.</typeparam>
    /// <typeparam name="TModel">The type of the model the controller uses.</typeparam>
    /// <typeparam name="TRepository">The type of the main repository the controller uses.</typeparam>
    /// <typeparam name="TTestData">The type of the test data provider the test uses.</typeparam>
    public abstract class DataControllerTest<TController, TModel, TRepository, TTestData> : MvcControllerTest<TController, TModel, TTestData> 
        where TController : Controller
            where TModel : class, new()
            where TRepository : IDisposable
            where TTestData : class, ITestData<TModel, TRepository>, new()
    {


        protected virtual TController GetController(List<TModel> modelItems = null, Action<List<TModel>, TRepository> setupAction = null, string httpMethod = "GET")
        {
            if (modelItems == null)
            {
                modelItems = Data.GetDefaultData();
            }

            Mapper.CreateMap<Journal, JournalViewModel>();
            //Arrange
            var membershipRepository = Mock.Create<IStaticMembershipService>();
            var userMock = Mock.Create<MembershipUser>();
            userMock.Arrange((u) => u.ProviderUserKey).Returns(1);
            membershipRepository.Arrange((m) => m.GetUser()).Returns(userMock);

            var repository = Mock.Create<TRepository>();

            var itemsCopy = new List<TModel>(modelItems);


            if (setupAction == null)
            {
                setupAction = Data.SetUpRepository;
            }
            setupAction.Invoke(itemsCopy, repository);

            //Act
            TController controller = CreateControllerInstance(repository, membershipRepository);

            return controller;
        }

        protected abstract TController CreateControllerInstance(TRepository repository, IStaticMembershipService membershipService);

        protected override TController GetController(string httpMethod = "GET")
        {
            return this.GetController(null, null, httpMethod);
        }

    }
}