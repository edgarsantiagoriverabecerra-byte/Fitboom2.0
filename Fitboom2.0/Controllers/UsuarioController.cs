using Microsoft.AspNetCore.Mvc;

namespace FITBOOM.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Index()
        {
            return Content("La vista Usuario funciona 👌");
        }
    }
}