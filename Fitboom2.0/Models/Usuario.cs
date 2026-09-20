using System.ComponentModel.DataAnnotations;

namespace FITBOOM.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Correo { get; set; }

        public string Contraseña { get; set; }

        public string Rol { get; set; }
    }
}