using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{

    public class TransaccionController: Controller
    {
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioCuenta repositorioCuenta;
        private readonly IRepositorioCategoria repositorioCategoria;
        private readonly IRepositorioTransaccion repositorioTransaccion;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionController(IServicioUsuario servicioUsuario,
            IRepositorioCuenta repositorioCuenta,
            IRepositorioCategoria repositorioCategoria,
            IRepositorioTransaccion repositorioTransaccion,
            IMapper mapper,
            IServicioReportes servicioReportes)
        {
            this.servicioUsuario = servicioUsuario;
            this.repositorioCuenta = repositorioCuenta;
            this.repositorioCategoria = repositorioCategoria;
            this.repositorioTransaccion = repositorioTransaccion;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult>  Index(int mes, int año)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, año, ViewBag);
            return View(modelo);
        }


        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana =
                await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, año, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x=> x.Semana).Select(x =>
                new ResultadoObtenerPorSemana()
                {
                    Semana =  x.Key,
                    Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                    Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),
                }).ToList();


            if (año == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana); 

                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }

            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int año)
        {

            var usuarioId = servicioUsuario.ObternerUsuarioId();
            if (año == 0)
            {
                año = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransaccion.ObtenerPorMes(usuarioId, año);
            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                .Select(x => new ResultadoObtenerPorMes()
                {
                    Mes = x.Key,
                    Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                    Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()

                }).ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);
                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes=mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.Año = año;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();

            var transacciones = await repositorioTransaccion.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario
                {
                    UsuarioId =usuarioId,
                    FechaInicio = start,
                    FechaFin = end
                });

            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (transaccion.TipoOperacionId == TipoOperacion.Gasto) ? "Red" : null

            });

            return Json(eventosCalendario);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();

            var transacciones = await repositorioTransaccion.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fecha,
                    FechaFin = fecha
                });

            return Json(transacciones);


        }



        public async Task <IActionResult> Crear()
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);

        }

        [HttpPost]

        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo) ;
            }

            var categoria = await repositorioCategoria.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");

            }

            modelo.UsuarioId = usuarioId;

            if  (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }

            await repositorioTransaccion.Crear(modelo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Transferir()
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var modelo = new TransferenciaViewModel
            {
                // Inicializar las listas de selección, etc.
                Cuentas = await ObtenerCuentas(usuarioId)
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Transferir(TransferenciaViewModel modelo)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();

            var transaccionGasto = new TransaccionCreacionViewModel
            {
                CuentaId = modelo.CuentaId,
                FechaTransaccion = modelo.FechaTransaccion,
                Monto = -modelo.Monto, // Monto negativo porque es un gasto
                Nota = modelo.Nota,
                TipoOperacionId = TipoOperacion.Gasto,
                UsuarioId = usuarioId,
                CategoriaId = 1 //id categoría transacción origen persistida en bd
            };
            await repositorioTransaccion.Crear(transaccionGasto);

            // Crear la transacción de ingreso en la cuenta destino
            var transaccionIngreso = new TransaccionCreacionViewModel
            {
                CuentaId = modelo.CuentaDestinoId,
                FechaTransaccion = modelo.FechaTransaccion,
                Monto = modelo.Monto, // Monto positivo porque es un ingreso
                Nota = modelo.Nota,
                TipoOperacionId = TipoOperacion.Ingreso,
                UsuarioId = usuarioId,
                CategoriaId = 2 //id categoría transacción destino persistida en bd

            };
            await repositorioTransaccion.Crear(transaccionIngreso);

            return RedirectToAction("Index"); // Redirigir al usuario a donde consideres apropiado
        }


        [HttpGet]
        public async Task<IActionResult> Editar(int id,string urlRetorno = null)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var transaccion = await repositorioTransaccion.ObtenerPorId(id, usuarioId);

            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuenta.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategoria.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<TransaccionActualizacionViewModel>(modelo);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }

            await repositorioTransaccion.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");

            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }

        }




        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuenta.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId,
                        TipoOperacion tipoOperacionId)
        {
            var categorias = await repositorioCategoria.Obtener(usuarioId, tipoOperacionId);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacionId)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacionId);
            return Ok(categorias);
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var transaccion = await repositorioTransaccion.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTransaccion.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");

            }
            else
            {
                return LocalRedirect(urlRetorno);
            }

        }

    }
}
