using Microsoft.AspNetCore.Mvc;

namespace TuProyecto.Controllers
{
    public class ProgramasPagosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GuardarMembresia(int id, string nombre, string duracion, decimal precio, string beneficios)
        {
            // Lógica de inserción/actualización de la membresía en la base de datos SQL Server
            return RedirectToAction("Index");
        }

        public IActionResult EliminarMembresia(int id)
        {
            // Lógica para eliminar la membresía en SQL Server
            return RedirectToAction("Index");
        }

        public IActionResult AnularPago(int id)
        {
            // Lógica para anular el pago en SQL Server
            return RedirectToAction("Index");
        }
    }
}