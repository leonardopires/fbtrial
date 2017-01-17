using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Journals.Web
{
    public class NullAntiForgery : IAntiforgery
    {
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
        {
            return new AntiforgeryTokenSet(string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
        {
            return new AntiforgeryTokenSet(string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public Task<bool> IsRequestValidAsync(HttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        public Task ValidateRequestAsync(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        public void SetCookieTokenAndHeader(HttpContext httpContext)
        {            
        }

    }
}