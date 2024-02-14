using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTipoCuenta repositorioTipoCuenta;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioCuenta repositorioCuenta;
        private readonly IMapper mapper;
        private readonly IRepositorioTransaccion repositorioTransaccion;
        private readonly IServicioReportes servicioReportes;

        public CuentasController(IRepositorioTipoCuenta repositorioTipoCuenta, IServicioUsuario servicioUsuario,
            IRepositorioCuenta repositorioCuenta, IMapper mapper,
            IRepositorioTransaccion repositorioTransaccion,
            IServicioReportes servicioReportes)
        {
            this.repositorioTipoCuenta = repositorioTipoCuenta;
            this.servicioUsuario = servicioUsuario;
            this.repositorioCuenta = repositorioCuenta;
            this.mapper = mapper;
            this.repositorioTransaccion = repositorioTransaccion;
            this.servicioReportes = servicioReportes;
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


        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var cuenta = await repositorioCuenta.ObtenerPorId(id, usuarioId);

            if (cuenta is null )
            {
                return RedirectToAction("NoEncontrado", "Home");

            }



            ViewBag.Cuenta = cuenta.Nombre;

            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladasPorCuenta(usuarioId,id,mes,año,ViewBag); 

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

            if (tipoCuenta is null)
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

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var cuenta = await repositorioCuenta.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");

            }

            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TiposCuenta = await ObtenerTipoCuenta(usuarioId);
            return View(modelo);
        }

        [HttpPost]

        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var cuenta = await repositorioCuenta.ObtenerPorId(cuentaEditar.Id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioTipoCuenta.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuenta.Actualizar(cuentaEditar);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar( int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var cuenta = await repositorioCuenta.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }


        [HttpPost]

        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var cuenta = await repositorioCuenta.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuenta.Borrar(id);
            return RedirectToAction("Index");

        }
 
        private async Task<IEnumerable<SelectListItem>> ObtenerTipoCuenta(int usuarioId)
        {
            var tiposCuenta = await repositorioTipoCuenta.Obtener(usuarioId);
            return tiposCuenta.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }


    }
}
