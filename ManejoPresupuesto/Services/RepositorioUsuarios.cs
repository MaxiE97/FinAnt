using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }
    public class RepositorioUsuarios: IRepositorioUsuarios
    {
        private readonly string _connectionString;

        public RepositorioUsuarios(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }


        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var usuarioId = await connection.QuerySingleAsync<int>(@"
                INSERT INTO Usuario (Email, EmailNormalizado, PasswordHash)
                VALUES (@Email, @EmailNormalizado, @PasswordHash)
                SELECT SCOPE_IDENTITY();
                ", usuario);

            await connection.ExecuteAsync("CrearDatosUsuarioNuevo", new { usuarioId },
                commandType: System.Data.CommandType.StoredProcedure);

            return usuarioId;

        }        

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(
                "SELECT * FROM Usuario WHERE EmailNormalizado = @emailNormalizado",
                new {emailNormalizado});
        }

    }
}
