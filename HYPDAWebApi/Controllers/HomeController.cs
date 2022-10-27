using System.Web.Mvc;

namespace HYPDAWebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

           return Redirect("~/swagger/ui/index");
        }
    }
}
