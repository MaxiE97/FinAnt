using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<Usuario> BuscarUsuarioPorId(int usuarioId);
        Task<int> CrearUsuario(Usuario usuario);
        Task<int> CrearUsuarioInvitado();
        Task EliminarUsuarioInvitado(int usuarioId);
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

        public async Task<int> CrearUsuarioInvitado()
        {
            using var connection = new SqlConnection(_connectionString);
            // Ejecuta el procedimiento almacenado y retorna el ID del usuario invitado creado
            var usuarioId = await connection.QuerySingleAsync<int>("CrearUsuarioInvitado", commandType: System.Data.CommandType.StoredProcedure);
            return usuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorId(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(
                "SELECT * FROM Usuario WHERE Id = @usuarioId",
                new { usuarioId });
        }

        public async Task EliminarUsuarioInvitado(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            // Asegúrate de que el nombre del procedimiento almacenado coincida con el que has creado
            await connection.ExecuteAsync("EliminarUsuarioInvitado", new { UsuarioId = usuarioId },
                commandType: System.Data.CommandType.StoredProcedure);
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
