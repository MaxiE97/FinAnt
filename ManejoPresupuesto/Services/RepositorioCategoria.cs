using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCategoria
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCategoria : IRepositorioCategoria
    {
        private readonly string _connectionString;
        public RepositorioCategoria(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
            INSERT INTO Categoria (Nombre, TipoOperacionId, UsuarioId)
            Values (@Nombre, @TipoOperacionId, @UsuarioId);
            SELECT SCOPE_IDENTITY();", categoria);


            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Categoria>(@"
            SELECT * FROM Categoria WHERE UsuarioId = @usuarioId", new { usuarioId });
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Categoria>(@"
            SELECT * FROM Categoria WHERE UsuarioId = @usuarioId
            AND TipoOperacionId = @tipoOperacionId ", new { usuarioId, tipoOperacionId });
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(
            @"SELECT * FROM Categoria WHERE Id = @Id AND UsuarioId = @usuarioId",
            new { id, usuarioId });
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"UPDATE Categoria
            SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionID
            WHERE Id = @Id", categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("DELETE Categoria WHERE Id = @Id", new { id });

        }

    }
}

