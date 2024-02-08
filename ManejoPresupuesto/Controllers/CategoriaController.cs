using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;



namespace ManejoPresupuesto.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly IRepositorioCategoria repositorioCategoria;
        private readonly IServicioUsuario servicioUsuario;

        public CategoriaController(IRepositorioCategoria repositorioCategoria,
            IServicioUsuario servicioUsuario)
        {
            this.repositorioCategoria = repositorioCategoria;
            this.servicioUsuario = servicioUsuario;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var categorias = await repositorioCategoria.Obtener(usuarioId);
            return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = servicioUsuario.ObternerUsuarioId();
            categoria.UsuarioId= usuarioId;
            await repositorioCategoria.Crear(categoria);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var categoria = await repositorioCategoria.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]

        public async Task<IActionResult> Editar(Categoria categoriaEditar)
        {
            if (!ModelState.IsValid)
            {
                return View(categoriaEditar);
            }

            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var categoria = await repositorioCategoria.ObtenerPorId(categoriaEditar.Id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoriaEditar.UsuarioId = usuarioId;
            await repositorioCategoria.Actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var cuenta = await repositorioCategoria.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]

        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = servicioUsuario.ObternerUsuarioId();
            var categoria = await repositorioCategoria.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCategoria.Borrar(id);
            return RedirectToAction("Index");

        }

    }
}
