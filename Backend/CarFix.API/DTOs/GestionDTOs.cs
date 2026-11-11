using System.ComponentModel.DataAnnotations;

namespace CarFix.API.DTOs
{
    public class ClienteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int Dni { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }

    public class VehiculoDTO
    {
        public int ClienteId { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Año { get; set; }
        public string Dominio { get; set; } = string.Empty;
        public int Kilometraje { get; set; }
    }

    public class CrearUsuarioDTO
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string NombreUsuario { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(128, MinimumLength = 12)]
        public string Password { get; set; } = string.Empty;
        public int? GrupoId { get; set; }
    }

    public class CambiarPasswordDTO
    {
        [Required, StringLength(128, MinimumLength = 12)]
        public string PasswordNuevo { get; set; } = string.Empty;
    }

    public class AsignarGrupoDTO
    {
        public int UsuarioId { get; set; }
        public int GrupoId { get; set; }
    }
}