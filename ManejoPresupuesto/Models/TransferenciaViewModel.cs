using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TransferenciaViewModel : TransaccionCreacionViewModel
    {
        [Display(Name = "Cuenta Destino")]
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta destino")]
        public int CuentaDestinoId { get; set; }
    }
}
