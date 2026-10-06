using System.Data;
using Flowbercut.Api.Data;
using Flowbercut.Api.Models.Results;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Flowbercut.Api.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly FlowbercutDbContext _dbContext;

    public AuthRepository(
        FlowbercutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<UsuarioLoginResult>> ObtenerUsuarioLoginAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var connection =
            _dbContext.Database.GetDbConnection();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "flow.usp_Flow_ObtenerUsuarioLogin";

        command.CommandType =
            CommandType.StoredProcedure;

        command.CommandTimeout = 30;

        command.Parameters.Add(
            new SqlParameter(
                "@Usu_Email",
                SqlDbType.VarChar,
                320)
            {
                Value = email.Trim()
            });

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var usuarios = new List<UsuarioLoginResult>();

        while (await reader.ReadAsync(cancellationToken))
        {
            usuarios.Add(new UsuarioLoginResult
            {
                Usu_Id = reader.GetInt64(
                    reader.GetOrdinal("Usu_Id")),

                Ten_Id = reader.GetInt64(
                    reader.GetOrdinal("Ten_Id")),

                Usu_NombreUsuario = reader.GetString(
                    reader.GetOrdinal("Usu_NombreUsuario")),

                Usu_Email = reader.GetString(
                    reader.GetOrdinal("Usu_Email")),

                Usu_ContrasenaHash = reader.GetString(
                    reader.GetOrdinal("Usu_ContrasenaHash")),

                Usu_EsAdministradorPlataforma =
                    reader.GetBoolean(
                        reader.GetOrdinal(
                            "Usu_EsAdministradorPlataforma")),

                Ten_Codigo = reader.GetString(
                    reader.GetOrdinal("Ten_Codigo")),

                Ten_Nombre = reader.GetString(
                    reader.GetOrdinal("Ten_Nombre")),

                Ten_Subdominio = reader.GetString(
                    reader.GetOrdinal("Ten_Subdominio")),


                Rol_Codigo = reader.IsDBNull(
                    reader.GetOrdinal("Rol_Codigo"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Rol_Codigo")),

                Rol_Nombre = reader.IsDBNull(
                    reader.GetOrdinal("Rol_Nombre"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Rol_Nombre"))
            });
        }

        return usuarios;
    }
}