using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTransaccion
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTrasaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransaccion : IRepositorioTransaccion
    {
        private readonly string _connectionString;
        public RepositorioTransaccion(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                    
                }, commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
               
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTrasaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Transaccion>
            (@"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,
            cu.Nombre as Cuenta, c.TipoOperacionId
            FROM Transacciones t
            INNER JOIN Categoria c
            ON c.Id = t.CategoriaId
            INNER JOIN Cuenta cu
            ON cu.Id = t.CuentaId
            WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId
            AND FechaTransaccion BETWEEN @FechaInicio and @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Transaccion>
            (@"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,
            cu.Nombre as Cuenta, c.TipoOperacionId
            FROM Transacciones t
            INNER JOIN Categoria c
            ON c.Id = t.CategoriaId
            INNER JOIN Cuenta cu
            ON cu.Id = t.CuentaId
            WHERE t.UsuarioId = @UsuarioId
            AND FechaTransaccion BETWEEN @FechaInicio and @FechaFin
            ORDER BY t.FechaTransaccion DESC", modelo);
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior,
        int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
            new
            {
                transaccion.Id,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaId,
                transaccion.CuentaId,
                transaccion.Nota,
                montoAnterior,
                cuentaAnteriorId
            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
             @"SELECT Transacciones.*, cat.TipoOperacionId
             FROM Transacciones
             INNER JOIN Categoria cat
             ON cat.Id = Transacciones.CategoriaId
             WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId",
             new { id, usuarioId });
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(
    ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
        
            Select datediff(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana,
            SUM(Monto) as Monto, cat.TipoOperacionId
            FROM Transacciones
            INNER JOIN Categoria cat
            ON cat.Id = Transacciones.CategoriaId
            WHERE Transacciones.UsuarioId = @usuarioId AND
            FechaTransaccion BETWEEN @fechaInicio and @fechaFin
            GROUP BY datediff(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"
        
                SELECT MONTH(FechaTransaccion) as Mes,
                SUM(Monto) as Monto, cat.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categoria cat
                ON cat.Id = Transacciones.CategoriaId
                WHERE Transacciones.UsuarioId = @usuarioId AND YEAR(FechaTransaccion) = @Año
                GROUP BY Month(FechaTransaccion), cat.TipoOperacionId", new {usuarioId, año});
        }


        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new { id }, commandType: System.Data.CommandType.StoredProcedure);

        }


    }
}
