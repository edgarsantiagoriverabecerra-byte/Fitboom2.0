using Microsoft.AspNetCore.Mvc;
using FITBOOM.Data;
using FITBOOM.Models;

namespace FITBOOM.Controllers
{
    public class AdminController : Controller
    {
        private readonly FITBOOMContext _context;

        public AdminController(FITBOOMContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var usuarios = _context.Usuarios.ToList();

            return View(usuarios);
        }
        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Aquí agregas la lógica para guardar en tu base de datos SQL Server
                // _context.Add(usuario);
                // await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }
    }
}