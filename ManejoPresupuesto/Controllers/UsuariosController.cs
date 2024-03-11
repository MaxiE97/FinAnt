using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class UsuariosController : Controller

       
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IRepositorioUsuarios repositorioUsuarios;
        private readonly IServicioUsuario servicioUsuario;

        public UsuariosController(UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager, IRepositorioUsuarios repositorioUsuarios, IServicioUsuario servicioUsuario)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.repositorioUsuarios = repositorioUsuarios;
            this.servicioUsuario = servicioUsuario;
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]


        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);    
            }

            var resultado = await signInManager.PasswordSignInAsync(modelo.Email,
                modelo.Password, modelo.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transaccion");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto.");
                return View(modelo);
            }
        }

        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario() { Email = modelo.Email };   

            var resultado = await userManager.CreateAsync(usuario, modelo.Password);

            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Transaccion");
            }
            else
            { 
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(modelo);    

            }

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AccederComoInvitado()
        {
            // Crear un usuario invitado en la base de datos
            var usuarioId = await repositorioUsuarios.CrearUsuarioInvitado();

<<<<<<< HEAD
            await repositorioUsuarios.CargarDatosParaInvitado(usuarioId);

            var usuario = await repositorioUsuarios.BuscarUsuarioPorId(usuarioId);

            // Iniciar sesión con el usuario invitado
            await signInManager.SignInAsync(usuario, isPersistent: false);

            return RedirectToAction("Index", "Transaccion");
=======
            // Aquí asumimos que tienes una forma de obtener el usuario recién creado por ID
            // Esto podría requerir modificar tu repositorio para incluir tal método
            var usuario = await repositorioUsuarios.BuscarUsuarioPorId(usuarioId);

            // Iniciar sesión con el usuario invitado
            // Asegúrate de tener una forma de diferenciar a este usuario para poder borrar sus datos más tarde si es necesario
            await signInManager.SignInAsync(usuario, isPersistent: false);

            return RedirectToAction("Index", "Transaccion"); // O donde quieras dirigir al usuario invitado
>>>>>>> 780e977998c67ff0afd2a1dd0708c8619cf57c92
        }



        [HttpPost]

        public async Task<IActionResult> Logout()
        {
            int usuarioId = servicioUsuario.ObternerUsuarioId();
            Usuario user =  await repositorioUsuarios.BuscarUsuarioPorId(usuarioId);

            if (user.Email.Equals("invitado@admin.com"))
            {
                await repositorioUsuarios.EliminarUsuarioInvitado(usuarioId);
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return RedirectToAction("Index", "Transaccion");
            }
            else
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return RedirectToAction("Index", "Transaccion");
            }


        }

    }
}
