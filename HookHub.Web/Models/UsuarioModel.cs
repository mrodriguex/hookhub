using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HookHub.Web.Models
{
    public class UsuarioModel
    {
        private string _pass;
        private string _user;

        [Key]
        [Required(ErrorMessage = "User es requerido")]
        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string ClaveUsuario {
            get {
                if (_user is null) { _user = ""; }
                return (_user);
            }
            set { _user = value; }
        }

        [Required(ErrorMessage = "Pass es requerido")]
        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string Contraseña {
            get {
                if (_pass is null) { _pass = ""; }
                return (_pass);
            }
            set { _pass = value; }
        }


        [NotMapped]
        public string User {
            get {
                return (ClaveUsuario);
            }
            set {
                ClaveUsuario = value;
            }
        }

        [NotMapped]
        public string Pass {
            get {
                return (Contraseña);
            }
            set {
                Contraseña = value;
            }
        }
    }
}
