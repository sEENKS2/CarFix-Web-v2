using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Entidades.Core
{
    public class Usuario
    {
        private int id;
        private string nombreUsuario;
        private string email;
        private string passwordHash;
        private DateTime fechaCreacion;
        private DateTime? ultimoAcceso;
        private bool activo;
        private List<Grupo> grupos;

        [Key]
        public int Id { get => id; set => id = value; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get => nombreUsuario; set => nombreUsuario = value; }

        [Required]
        [StringLength(100)]
        public string Email { get => email; set => email = value; }

        [JsonIgnore]
        [Required]
        [StringLength(255)]
        public string PasswordHash { get => passwordHash; set => passwordHash = value; }

        public DateTime FechaCreacion { get => fechaCreacion; set => fechaCreacion = value; }
        public DateTime? UltimoAcceso { get => ultimoAcceso; set => ultimoAcceso = value; }
        public bool Activo { get => activo; set => activo = value; }

        public virtual List<Grupo> Grupos { get => grupos ??= new List<Grupo>(); set => grupos = value; }

        // Método para establecer contraseña con hash
        public void EstablecerPassword(string password)
        {
            PasswordHash = PasswordSecurity.Hash(password);
        }

        // Método para verificar contraseña
        public bool VerificarPassword(string password)
        {
            return PasswordSecurity.Verify(PasswordHash, password);
        }

    }
}
