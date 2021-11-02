using Microsoft.AspNetCore.Mvc;

namespace Nop.Web.Controllers
{
    public partial class PromoRayoController : BasePublicController
    {
        public virtual IActionResult Index()
        {
            return View();
        }
    }
}