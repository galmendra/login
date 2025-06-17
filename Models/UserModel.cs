// Este modelo representa la estructura de la tabla users en Supabase
using System.ComponentModel.DataAnnotations;

namespace login1.Models  // 🔄 Cambia "tu_proyecto" por el nombre real de tu proyecto
{
    public class UserModel
    {
        // Corresponde a la columna 'id' en Supabase
        public int Id { get; set; }

        // Corresponde a la columna 'username' en Supabase
        [Required]
        public string Username { get; set; } = string.Empty;

        // Corresponde a la columna 'email' en Supabase
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Corresponde a la columna 'password_hash' en Supabase
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Corresponde a la columna 'created_at' en Supabase
        public DateTime CreatedAt { get; set; }

        // Corresponde a la columna 'last_login' en Supabase
        public DateTime? LastLogin { get; set; }

        // Corresponde a la columna 'is_active' en Supabase
        public bool IsActive { get; set; } = true;
    }
}