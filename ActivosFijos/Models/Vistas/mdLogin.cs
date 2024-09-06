using System.ComponentModel.DataAnnotations;

namespace ActivosFijos.Models.Vistas
{
    public class mdLogin
    {
        [Display(Name = "Usuario"), Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [StringLength(100)]
        public string nombreUsuario { get; set; }
        [Display(Name = "Contraseña"), Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Password { get; set; }
    }
}
