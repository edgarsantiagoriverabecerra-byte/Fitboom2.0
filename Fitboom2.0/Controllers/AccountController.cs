using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TuProyecto.Models;

namespace TuProyecto.Controllers
{
    public class AccountController : Controller
    {
        private readonly string cadenaConexion;
        private readonly IWebHostEnvironment _environment;

        public AccountController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            cadenaConexion = configuration.GetConnectionString("ConexionSQL");
            _environment = environment;
        }

        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            UsuarioBD usuario = ValidarUsuarioBD(correo, contrasena);

            if (usuario == null)
            {
                TempData["ErrorLogin"] = "Usuario o contraseña incorrectos. No registrado en FITBOOM.";
                return RedirectToAction("Index", "Home");
            }

            // Guardamos ID, Nombre, Rol y FotoRuta en las variables de sesión
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "Usuario");
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol ?? "Cliente");
            HttpContext.Session.SetString("UsuarioFoto", string.IsNullOrEmpty(usuario.FotoRuta) ? "/images/imagen predeterminada.jpg" : usuario.FotoRuta);

            switch (usuario.Rol.ToLower().Trim())
            {
                case "admin":
                case "administrador":
                    return RedirectToAction("Index", "Admin");

                case "cliente":
                    return RedirectToAction("Index", "Cliente");

                case "entrenador":
                    return RedirectToAction("Index", "Entrenador");

                case "nutricionista":
                    return RedirectToAction("Index", "Nutricionista");

                default:
                    TempData["ErrorLogin"] = "El usuario no tiene un rol válido asignado.";
                    return RedirectToAction("Index", "Home");
            }
        }

        // ==========================================
        // VISTA: VER Y EDITAR PERFIL
        // ==========================================
        [HttpGet]
        public IActionResult Perfil()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Home");
            }

            PerfilViewModel model = ObtenerPerfilBD(idUsuario.Value);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Perfil(PerfilViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Home");
            }

            model.Id = idUsuario.Value;

            // 1. Procesar la imagen de perfil si el usuario subió un nuevo archivo
            if (model.FotoArchivo != null && model.FotoArchivo.Length > 0)
            {
                string folderPath = Path.Combine(_environment.WebRootPath, "images", "perfiles");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = $"user_{model.Id}_{Guid.NewGuid()}{Path.GetExtension(model.FotoArchivo.FileName)}";
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.FotoArchivo.CopyToAsync(stream);
                }

                model.FotoRuta = $"/images/perfiles/{fileName}";
            }

            // 2. Actualizar los datos personales en la base de datos
            using (SqlConnection ConexionSQL = new SqlConnection(cadenaConexion))
            {
                ConexionSQL.Open();

                string queryDatos = @"UPDATE Usuarios 
                                    SET Nombre = @Nombre, 
                                        RestriccionesAlimenticias = @Restricciones, 
                                        FotoRuta = COALESCE(@FotoRuta, FotoRuta) 
                                    WHERE IdUsuario = @IdUsuario";

                using (SqlCommand cmd = new SqlCommand(queryDatos, ConexionSQL))
                {
                    cmd.Parameters.AddWithValue("@Nombre", model.Nombre ?? "");
                    cmd.Parameters.AddWithValue("@Restricciones", (object)model.RestriccionesAlimenticias ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FotoRuta", (object)model.FotoRuta ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdUsuario", model.Id);

                    cmd.ExecuteNonQuery();
                }
            }

            // 3. Actualizar variables de sesión (Nombre y Foto)
            HttpContext.Session.SetString("UsuarioNombre", model.Nombre ?? "Usuario");
            if (!string.IsNullOrEmpty(model.FotoRuta))
            {
                HttpContext.Session.SetString("UsuarioFoto", model.FotoRuta);
            }

            TempData["MensajeExito"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }

        // ==========================================
        // VISTA: CONFIGURACIÓN BÁSICA (CAMBIO DE CONTRASEÑA)
        // ==========================================
        [HttpGet]
        public IActionResult Configuracion()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");
            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CambiarContrasena(string contrasenaActual, string nuevaContrasena, string confirmarContrasena)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");
            if (idUsuario == null) return RedirectToAction("Index", "Home");

            if (nuevaContrasena != confirmarContrasena)
            {
                TempData["ErrorConfig"] = "La nueva contraseña y su confirmación no coinciden.";
                return RedirectToAction("Configuracion");
            }

            using (SqlConnection ConexionSQL = new SqlConnection(cadenaConexion))
            {
                string query = "UPDATE Usuarios SET Contraseña = @Nueva WHERE IdUsuario = @IdUsuario AND Contraseña = @Actual";

                using (SqlCommand cmd = new SqlCommand(query, ConexionSQL))
                {
                    cmd.Parameters.AddWithValue("@Nueva", nuevaContrasena);
                    cmd.Parameters.AddWithValue("@Actual", contrasenaActual);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);

                    ConexionSQL.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        TempData["MensajeExitoConfig"] = "Contraseña actualizada correctamente.";
                    }
                    else
                    {
                        TempData["ErrorConfig"] = "La contraseña actual no es correcta.";
                    }
                }
            }

            return RedirectToAction("Configuracion");
        }

        // ==========================================
        // CERRAR SESIÓN
        // ==========================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ==========================================
        private UsuarioBD ValidarUsuarioBD(string correo, string contrasena)
        {
            UsuarioBD usuarioEncontrado = null;

            using (SqlConnection ConexionSQL = new SqlConnection(cadenaConexion))
            {
                string query = "SELECT IdUsuario, Nombre, Correo, Rol, FotoRuta FROM Usuarios WHERE Correo = @Correo AND Contraseña = @Contrasena";

                using (SqlCommand cmd = new SqlCommand(query, ConexionSQL))
                {
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    cmd.Parameters.AddWithValue("@Contrasena", contrasena);

                    ConexionSQL.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuarioEncontrado = new UsuarioBD
                            {
                                Id = Convert.ToInt32(reader["IdUsuario"]),
                                Nombre = reader["Nombre"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                Rol = reader["Rol"].ToString(),
                                FotoRuta = reader["FotoRuta"] != DBNull.Value ? reader["FotoRuta"].ToString() : null
                            };
                        }
                    }
                }
            }

            return usuarioEncontrado;
        }

        private PerfilViewModel ObtenerPerfilBD(int idUsuario)
        {
            PerfilViewModel usuario = new PerfilViewModel();

            using (SqlConnection ConexionSQL = new SqlConnection(cadenaConexion))
            {
                string query = "SELECT IdUsuario, Nombre, Correo, Rol, FotoRuta, RestriccionesAlimenticias FROM Usuarios WHERE IdUsuario = @IdUsuario";

                using (SqlCommand cmd = new SqlCommand(query, ConexionSQL))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    ConexionSQL.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario.Id = Convert.ToInt32(reader["IdUsuario"]);
                            usuario.Nombre = reader["Nombre"].ToString();
                            usuario.Correo = reader["Correo"].ToString();
                            usuario.Rol = reader["Rol"].ToString();
                            usuario.FotoRuta = reader["FotoRuta"] != DBNull.Value ? reader["FotoRuta"].ToString() : "/images/imagen predeterminada.jpg";
                            usuario.RestriccionesAlimenticias = reader["RestriccionesAlimenticias"] != DBNull.Value ? reader["RestriccionesAlimenticias"].ToString() : "";
                        }
                    }
                }
            }

            return usuario;
        }

        public class UsuarioBD
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public string Correo { get; set; }
            public string Rol { get; set; }
            public string FotoRuta { get; set; }
        }
    }
}