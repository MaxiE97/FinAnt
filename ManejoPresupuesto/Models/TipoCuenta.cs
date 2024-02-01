using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo nombre es requerido, flaco")]
        [StringLength(maximumLength:50, MinimumLength = 3, ErrorMessage ="La longitud del campo {0} debe tener entre {2} y {1} caracteres")]
        [Remote(action:"VerificarExisteTipoCuenta", controller:"TipoCuenta")]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int MyProperty { get; set; }
        public int Orden { get; set; }

    }
}
