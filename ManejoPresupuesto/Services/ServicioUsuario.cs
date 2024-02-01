namespace ManejoPresupuesto.Services
{
    public interface IServicioUsuario
    {
        int ObternerUsuarioId();
    }

    public class ServicioUsuario: IServicioUsuario 
    {
        public int ObternerUsuarioId()
        {
            return 1;
        }
    }
}
