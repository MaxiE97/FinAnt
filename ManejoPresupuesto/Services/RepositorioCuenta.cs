using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ManejoPresupuesto.Services
{

    public interface IRepositorioCuenta
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCuenta:IRepositorioCuenta
    {
        private readonly string _connectionString;

        public RepositorioCuenta(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }


        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
        INSERT INTO Cuenta(Nombre, TipoCuentaId, Descripcion, Balance)
        VALUES(@Nombre, @TipoCuentaId, @Descripcion, @Balance);
        SELECT SCOPE_IDENTITY();", cuenta);

            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Cuenta>(@"
        SELECT Cuenta.Id, Cuenta.Nombre, Balance, tc.Nombre AS TipoCuenta
        FROM Cuenta
        INNER JOIN TiposCuentas tc
        ON tc.Id = Cuenta.TipoCuentaId
        WHERE tc.UsuarioId = @UsuarioId
        ORDER BY tc.Orden", new { usuarioId });
        }


        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"
        SELECT Cuenta.Id, Cuenta.Nombre, Balance, Descripcion, TipoCuentaId
        FROM Cuenta
        INNER JOIN TiposCuentas tc
        ON tc.id = Cuenta.TipoCuentaId
        WHERE tc.UsuarioId = @UsuarioId AND Cuenta.id = @Id", new { id, usuarioId });
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"UPDATE Cuenta
             SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId
             WHERE Id = @Id;", cuenta);
            

        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"DELETE Cuenta WHERE Id = @Id", new {id });  

        }
    }
}
