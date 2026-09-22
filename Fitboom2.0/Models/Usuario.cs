using System.ComponentModel.DataAnnotations;

namespace Fitboom2._0.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Contrasena { get; set; } = string.Empty;

        public string TipoUsuario { get; set; } = string.Empty;
    }
}