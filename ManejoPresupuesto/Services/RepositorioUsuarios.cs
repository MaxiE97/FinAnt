using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<Usuario> BuscarUsuarioPorId(int usuarioId);
        Task CargarDatosParaInvitado(int usuarioId);
        Task<int> CrearUsuario(Usuario usuario);
        Task<int> CrearUsuarioInvitado();
        Task EliminarUsuarioInvitado(int usuarioId);
        Task<(Dictionary<int, string> categorias, Dictionary<int, string> cuentas)> ObtenerCategoriasYCuentasInvitado(int usuarioId);

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

        public async Task<(Dictionary<int, string> categorias, Dictionary<int, string> cuentas)> ObtenerCategoriasYCuentasInvitado(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);

            var categorias = new Dictionary<int, string>();
            var cuentas = new Dictionary<int, string>();

            // Obtener categorías
            var categoriasQuery = await connection.QueryAsync<dynamic>("SELECT Id, Nombre FROM Categoria WHERE UsuarioId = @UsuarioId", new { UsuarioId = usuarioId });
            foreach (var categoria in categoriasQuery)
            {
                categorias.Add(categoria.Id, categoria.Nombre);
            }

            // Obtener cuentas
            var cuentasQuery = await connection.QueryAsync<dynamic>(
                "SELECT Cuenta.Id, Cuenta.Nombre " +
                "FROM Cuenta " +
                "JOIN TiposCuentas ON Cuenta.TipoCuentaId = TiposCuentas.Id " +
                "WHERE TiposCuentas.UsuarioId = @UsuarioId",
                new { UsuarioId = usuarioId }
            );
            foreach (var cuenta in cuentasQuery)
            {
                cuentas.Add(cuenta.Id, cuenta.Nombre);
            }

            return (categorias, cuentas);
        }


        public async Task CargarDatosParaInvitado(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);

            // Obtener categorías y cuentas del usuario invitado
            var (categoriasDict, cuentasDict) = await ObtenerCategoriasYCuentasInvitado(usuarioId);

            foreach (var transaccion in DatosEjemploInvitado.Transacciones)
            {
                int categoriaId = categoriasDict.FirstOrDefault(x => x.Value == transaccion.CategoriaNombre).Key;
                int cuentaId = cuentasDict.FirstOrDefault(x => x.Value == transaccion.CuentaNombre).Key;



                // Actualizar el balance de la cuenta
                await connection.ExecuteAsync(
                    "UPDATE Cuenta SET Balance = Balance + @Monto WHERE Id = @CuentaId",
                    new { Monto = transaccion.Monto, CuentaId = cuentaId });


                await connection.ExecuteAsync(
                    "INSERT INTO Transacciones (UsuarioId, FechaTransaccion, Monto, CuentaId, CategoriaId) VALUES (@UsuarioId, @FechaTransaccion, ABS(@Monto), @CuentaId, @CategoriaId)",
                    new
                    {
                        UsuarioId = usuarioId,
                        FechaTransaccion = transaccion.FechaTransaccion,
                        Monto = transaccion.Monto,
                        CuentaId = cuentaId,
                        CategoriaId = categoriaId
                    });

            }
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
