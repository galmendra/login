// Este modelo representa los datos que el usuario ingresará en el formulario
using System.ComponentModel.DataAnnotations;

namespace login.Models
{
    public class LoginViewModel
    {
        // Campo para el nombre de usuario
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de Usuario")]
        public string Username { get; set; } = string.Empty;

        // Campo para la contraseña
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]  // Esto hace que el campo se muestre como password
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        // Propiedad para mostrar mensajes de error personalizados
        public string ErrorMessage { get; set; } = string.Empty;
    }
}