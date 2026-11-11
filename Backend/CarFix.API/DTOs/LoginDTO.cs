using System.ComponentModel.DataAnnotations;

namespace CarFix.API.DTOs
{
    public class LoginDTO
    {
        [Required, StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;
        [Required, StringLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}