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
        public IActionResult GuardarPrograma(int id, string nombre, string duracion, string enfoque)
        {
            // Aquí irá la lógica de guardado/actualización en SQL Server
            return RedirectToAction("Index");
        }

        public IActionResult EliminarPrograma(int id)
        {
            // Aquí irá la lógica para eliminar o desactivar en SQL Server
            return RedirectToAction("Index");
        }

        public IActionResult AnularPago(int id)
        {
            // Aquí irá la lógica para anular el registro de pago
            return RedirectToAction("Index");
        }
    }
}