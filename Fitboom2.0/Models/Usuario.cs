using System.ComponentModel.DataAnnotations;

namespace TuProyecto.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

    public string Nombre { get; set; }

        public string Correo { get; set; }

        public string Contrasena { get; set; }

        public string TipoUsuario { get; set; }
    }

}
