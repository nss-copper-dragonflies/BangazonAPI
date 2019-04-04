using System.Net;

namespace BangazonAPI.Controllers
{
    internal class HttpStatusCodeResult
    {
        private HttpStatusCode forbidden;

        public HttpStatusCodeResult(HttpStatusCode forbidden)
        {
            this.forbidden = forbidden;
        }
    }
}