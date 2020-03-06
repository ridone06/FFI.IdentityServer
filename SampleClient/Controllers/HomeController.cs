using IdentityModel.Client;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace SampleClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;

            try
            {
                var client = new HttpClient();
                client.SetBearerToken(claims.Where(w => w.Type == "access_token").FirstOrDefault().Value);

                var json = await client.GetStringAsync("http://localhost:5001/identity");

                ViewBag.json = JArray.Parse(json).ToString();
            }
            catch (System.Exception)
            {
                ViewBag.json = "";
            }

            return View(claims);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void LogOff()
        {
            System.Web.HttpContext.Current.GetOwinContext().Authentication.SignOut();
        }
    }
}