using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Controllers
{
    public class TipoCuentaController : Controller
    {
        private readonly IRepositorioTipoCuenta repositorioTipoCuenta;
        private readonly IServicioUsuario servicioUsuario;

        public TipoCuentaController(IRepositorioTipoCuenta repositorioTipoCuenta, IServicioUsuario servicioUsuario)
        {
            this.repositorioTipoCuenta = repositorioTipoCuenta;
            this.servicioUsuario = servicioUsuario;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var tipoCuenta = await repositorioTipoCuenta.Obtener(usuarioId);
            return View(tipoCuenta);
        }
        public IActionResult Crear()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta) 
        {
            if (!ModelState.IsValid) 
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = servicioUsuario.ObternerUsuarioId();

            var yaExisteTipoCuenta = await repositorioTipoCuenta.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre),
                    $"El nombre {tipoCuenta.Nombre} ya existe.");
                return View(tipoCuenta);
            }


            await repositorioTipoCuenta.Crear(tipoCuenta);


            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var tipoCuenta = await repositorioTipoCuenta.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var tipoCuentaExiste = await repositorioTipoCuenta.ObtenerPorId(tipoCuenta.Id,usuarioId);

            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTipoCuenta.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

      




        [HttpGet]

        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var yaExisteTipoCuenta = await repositorioTipoCuenta.Existe(nombre, usuarioId);

            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }


        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var tipoCuenta = await repositorioTipoCuenta.ObtenerPorId(id, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }


            return View(tipoCuenta);
        }



        [HttpPost]

        public async Task<IActionResult> BorrarTipoCuenta (int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var tipoCuenta = await repositorioTipoCuenta.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTipoCuenta.Borrar(id);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var tipoCuenta = await repositorioTipoCuenta.Obtener(usuarioId);
            var idsTiposCuentas = tipoCuenta.Select(x => x.Id);

            var idTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenado = ids.Select((valor, indice) =>
                new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();

            await repositorioTipoCuenta.Ordenar(tiposCuentasOrdenado);

            return Ok();
        }
            
       

    }
}
