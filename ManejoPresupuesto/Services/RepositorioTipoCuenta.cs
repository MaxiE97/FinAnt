using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTipoCuenta
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenado);
    }

    public class RepositorioTipoCuenta : IRepositorioTipoCuenta
    {
        private readonly string _connectionString;

        public RepositorioTipoCuenta(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>
                ("Insertar_TipoCuenta",
                new { usuarioId = tipoCuenta.UsuarioId, nombre = tipoCuenta.Nombre },
                commandType: System.Data.CommandType.StoredProcedure);
            tipoCuenta.Id = id;
        }


        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                 @"SELECT 1 
                    FROM TiposCuentas 
                    WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                new { nombre, usuarioId });
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId ORDER BY Orden", new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre WHERE Id = @Id", tipoCuenta);
        }


        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(
                @"SELECT Id, Nombre, Orden
                FROM TiposCuentas
                WHERE Id = @Id AND UsuarioId = @UsuarioId",
                new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("DELETE TiposCuentas WHERE Id = @Id", new { id });

        } 

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenado)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE id = @id;";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, tipoCuentasOrdenado);
        }
    }
      
}
