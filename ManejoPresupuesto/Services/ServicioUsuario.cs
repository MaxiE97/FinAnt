using System.Security.Claims;

namespace ManejoPresupuesto.Services
{
    public interface IServicioUsuario
    {
        int ObternerUsuarioId();
    }

    public class ServicioUsuario: IServicioUsuario 

    {
        private readonly HttpContext _httpContext;
        public ServicioUsuario(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }
        public int ObternerUsuarioId()
        {
            if (_httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = _httpContext.User.Claims.Where
                    (x=> x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = int.Parse(idClaim.Value);  
                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no está autenticado");

            }
        }
    }
}
