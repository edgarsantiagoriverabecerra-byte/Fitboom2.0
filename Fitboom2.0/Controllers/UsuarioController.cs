using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitboom2._0.Models;

namespace Fitboom2._0.Controllers
{
    public class UsuarioController : Controller
    {
        // Panel del usuario (protegido)
        [Authorize]
        public IActionResult Index()
        {
            var usuarios = new List<Usuario>(); // Reemplazar con datos reales ADO.NET
            return View(usuarios);
        }

        // Login ejecutado desde el modal del Index en Home
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginFromHome(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["LoginError"] = "Por favor, ingresa correo y contraseña.";
                return RedirectToAction("Index", "Home");
            }

            // Validar credenciales
            Usuario? usuario = ValidarUsuarioEnBD(email, password);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.TipoUsuario)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Flujo: Home -> Panel Usuario
                return RedirectToAction("Index", "Usuario");
            }

            TempData["LoginError"] = "Correo o contraseña incorrectos.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ⚠️ Método de validación (Sustituir por tu consulta ADO.NET)
        private Usuario? ValidarUsuarioEnBD(string email, string password)
        {
            // Ejemplo de prueba temporal
            if (email == "admin@fitboom.co" && password == "123456")
            {
                return new Usuario
                {
                    IdUsuario = 1,
                    Nombre = "Usuario Demo",
                    Correo = email,
                    TipoUsuario = "Cliente"
                };
            }
            return null;
        }
    }
}