using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HookHub.Web.Models
{
    public class ErrorSQLModel
    {
        private string _error;
        private string _serieBitacora;
        private string _serieRemision;

        [Key]
        public int ClaveError { get; set; }

        [Required]
        [StringLength(4, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string SerieBitacora { get { return (_serieBitacora ?? ""); } set { _serieBitacora = value; } }

        [Required]
        public int FolioBitacora { get; set; }

        [Required]
        [StringLength(4, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string SerieRemision { get { return (_serieRemision ?? ""); } set { _serieRemision = value; } }

        [Required]
        public int FolioRemision { get; set; }

        [StringLength(512, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string Error {
            get {
                _error ??= "";
                return (_error);
            }
            set { _error = value; }
        }

        public DateTime FechaRegistro { get; set; }
    }
}
