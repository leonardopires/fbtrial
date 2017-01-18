using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using LP.Test.Framework.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Xunit;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Integration
{
    public class IntegrationTests : TestBase
    {

        private const string TEST_PASSWORD = "Passw0rd!";

        private readonly TestServer server;
        private readonly HttpClient client;


        public IntegrationTests(ITestOutputHelper output) : base(output)
        {
            server = new TestServer(
                new WebHostBuilder()
                .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory(), "..","..", "..", "..", "..", "..", "src","Journals.Web"))
                .UseEnvironment(EnvironmentName.Development)
                .UseStartup<Startup>()                
                );

            client = server.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
        }

        [Fact]
        public async Task Home_Must_Not_Throw_Exceptions()
        {
            var response = await client.GetAsync("/");

            var responseString = await response.Content.ReadAsStringAsync();        
            Log.Logger.Debug(responseString);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task Echo_Must_Return_404()
        {
            var guid = Guid.NewGuid();
            var response = await client.GetAsync($"/echo/{guid}");

            var responseString = await response.Content.ReadAsStringAsync();
            Log.Logger.Debug(responseString);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);            
        }

        [Fact]
        public async Task Login_Page_Must_Appear()
        {
            var response = await client.GetAsync("/account/login");

            var responseString = await response.Content.ReadAsStringAsync();
            Log.Logger.Debug(responseString);

            response.EnsureSuccessStatusCode();

            responseString.Should().Contain("<!-- $View(Login) -->");
        }

        [Theory]
        [InlineData("pappy", TEST_PASSWORD, true)]
        [InlineData("pappu", TEST_PASSWORD, true)]
        [InlineData("serge", TEST_PASSWORD, true)]
        [InlineData("daniel", TEST_PASSWORD, true)]
        [InlineData("andrew", TEST_PASSWORD, false)]
        [InlineData("harold", TEST_PASSWORD, false)]
        public async Task Login_With_Valid_Users(string username, string password, bool rememberMe)
        {

            var values = new Dictionary<string, string>
            {
                { "UserName", username},
                { "Password", password},
                { "RememberMe", rememberMe.ToString()}
            };

            var response = await client.PostAsync("/account/login", new FormUrlEncodedContent(values));

            var responseString = await response.Content.ReadAsStringAsync();
            Log.Logger.Debug(responseString);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect, responseString);
        }

        protected override void InitializeContainer(ContainerBuilder builder)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                server.Dispose();
                client.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
