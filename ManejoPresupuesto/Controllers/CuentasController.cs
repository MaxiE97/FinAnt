using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController: Controller
    {
        private readonly IRepositorioTipoCuenta repositorioTipoCuenta;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioCuenta repositorioCuenta;

        public CuentasController(IRepositorioTipoCuenta repositorioTipoCuenta, IServicioUsuario servicioUsuario,
            IRepositorioCuenta repositorioCuenta)
        {
            this.repositorioTipoCuenta = repositorioTipoCuenta;
            this.servicioUsuario = servicioUsuario;
            this.repositorioCuenta = repositorioCuenta;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuenta.Buscar(usuarioId);

            var modelo = cuentasConTipoCuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentasViewModel
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();

            return View(modelo);
        }



        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuenta = await ObtenerTipoCuenta(usuarioId);


            return View(modelo);
        }


        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var tipoCuenta = await repositorioTipoCuenta.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuenta = await ObtenerTipoCuenta(usuarioId);
                return View(cuenta);
            }

            await repositorioCuenta.Crear(cuenta);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTipoCuenta(int usuarioId)
        {
            var tiposCuenta = await repositorioTipoCuenta.Obtener(usuarioId);
            return tiposCuenta.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
