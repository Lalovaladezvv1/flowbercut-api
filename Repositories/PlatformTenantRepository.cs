using System.Data;

using Flowbercut.Api.Data;
using Flowbercut.Api.Models.Results;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Flowbercut.Api.Repositories;

public sealed class PlatformTenantRepository
    : IPlatformTenantRepository
{
    private readonly FlowbercutDbContext _dbContext;

    public PlatformTenantRepository(
        FlowbercutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TenantResult>>
        ObtenerBarberiasAsync(
            CancellationToken cancellationToken)
    {
        var connection =
            _dbContext.Database.GetDbConnection();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "flow.usp_Flow_ObtenerBarberias";

        command.CommandType =
            CommandType.StoredProcedure;

        command.CommandTimeout = 30;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(
                cancellationToken);
        }

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var barberias =
            new List<TenantResult>();

        while (await reader.ReadAsync(
            cancellationToken))
        {
            barberias.Add(new TenantResult
            {
                Ten_Id = reader.GetInt64(
                    reader.GetOrdinal("Ten_Id")),

                Ten_Codigo = reader.GetString(
                    reader.GetOrdinal("Ten_Codigo")),

                Ten_Nombre = reader.GetString(
                    reader.GetOrdinal("Ten_Nombre")),

                Ten_Subdominio = reader.GetString(
                    reader.GetOrdinal("Ten_Subdominio")),

                Ten_ZonaHoraria = reader.GetString(
                    reader.GetOrdinal("Ten_ZonaHoraria")),

                Est_Id = reader.GetInt32(
                    reader.GetOrdinal("Est_Id")),

                Est_Codigo = reader.GetString(
                    reader.GetOrdinal("Est_Codigo")),

                Est_Nombre = reader.GetString(
                    reader.GetOrdinal("Est_Nombre")),

                Aud_FechaCreacion =
                    reader.GetFieldValue<DateTimeOffset>(
                        reader.GetOrdinal(
                            "Aud_FechaCreacion")),

                Usu_Id =
                    reader.IsDBNull(
                        reader.GetOrdinal("Usu_Id"))
                        ? null
                        : reader.GetInt64(
                            reader.GetOrdinal("Usu_Id")),

                Usu_Email =
                    reader.IsDBNull(
                        reader.GetOrdinal("Usu_Email"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal("Usu_Email"))
            });
        }

        return barberias;
    }

    public async Task<TenantResult?>
        CrearBarberiaAsync(
            string codigo,
            string nombre,
            string subdominio,
            string zonaHoraria,
            long usuarioCreacion,
            CancellationToken cancellationToken)
    {
        var connection =
            _dbContext.Database.GetDbConnection();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "flow.usp_Flow_CrearBarberia";

        command.CommandType =
            CommandType.StoredProcedure;

        command.CommandTimeout = 30;

        command.Parameters.Add(
            new SqlParameter(
                "@Ten_Codigo",
                SqlDbType.VarChar,
                50)
            {
                Value = codigo
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Ten_Nombre",
                SqlDbType.NVarChar,
                300)
            {
                Value = nombre
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Ten_Subdominio",
                SqlDbType.VarChar,
                100)
            {
                Value = subdominio
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Ten_ZonaHoraria",
                SqlDbType.VarChar,
                100)
            {
                Value = zonaHoraria
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Aud_UsuarioCreacion",
                SqlDbType.BigInt)
            {
                Value = usuarioCreacion
            });

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(
                cancellationToken);
        }

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
            cancellationToken))
        {
            return null;
        }

        return new TenantResult
        {
            Ten_Id = reader.GetInt64(
                reader.GetOrdinal("Ten_Id")),

            Ten_Codigo = reader.GetString(
                reader.GetOrdinal("Ten_Codigo")),

            Ten_Nombre = reader.GetString(
                reader.GetOrdinal("Ten_Nombre")),

            Ten_Subdominio = reader.GetString(
                reader.GetOrdinal("Ten_Subdominio")),

            Ten_ZonaHoraria = reader.GetString(
                reader.GetOrdinal("Ten_ZonaHoraria")),

            Est_Id = reader.GetInt32(
                reader.GetOrdinal("Est_Id"))
        };
    }

    public async Task<TenantAdminResult?>
        CrearAdministradorAsync(
            long tenantId,
            string nombreUsuario,
            string email,
            string passwordHash,
            long usuarioCreacion,
            CancellationToken cancellationToken)
    {
        var connection =
            _dbContext.Database.GetDbConnection();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "flow.usp_Flow_CrearAdministradorBarberia";

        command.CommandType =
            CommandType.StoredProcedure;

        command.CommandTimeout = 30;

        command.Parameters.Add(
            new SqlParameter(
                "@Ten_Id",
                SqlDbType.BigInt)
            {
                Value = tenantId
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Usu_NombreUsuario",
                SqlDbType.VarChar,
                150)
            {
                Value = nombreUsuario
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Usu_Email",
                SqlDbType.VarChar,
                320)
            {
                Value = email
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Usu_ContrasenaHash",
                SqlDbType.VarChar,
                500)
            {
                Value = passwordHash
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Aud_UsuarioCreacion",
                SqlDbType.BigInt)
            {
                Value = usuarioCreacion
            });

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(
                cancellationToken);
        }

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
            cancellationToken))
        {
            return null;
        }

        return new TenantAdminResult
        {
            Usu_Id = reader.GetInt64(
                reader.GetOrdinal("Usu_Id")),

            Ten_Id = reader.GetInt64(
                reader.GetOrdinal("Ten_Id")),

            Usu_NombreUsuario = reader.GetString(
                reader.GetOrdinal(
                    "Usu_NombreUsuario")),

            Usu_Email = reader.GetString(
                reader.GetOrdinal(
                    "Usu_Email"))
        };
    }

    public async Task<TenantAdminResult?>
        RestablecerContrasenaAdministradorAsync(
            long tenantId,
            string passwordHash,
            long usuarioActualizacion,
            CancellationToken cancellationToken)
    {
        var connection =
            _dbContext.Database.GetDbConnection();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "flow.usp_Flow_RestablecerContrasenaAdministrador";

        command.CommandType =
            CommandType.StoredProcedure;

        command.CommandTimeout = 30;

        command.Parameters.Add(
            new SqlParameter(
                "@Ten_Id",
                SqlDbType.BigInt)
            {
                Value = tenantId
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Usu_ContrasenaHash",
                SqlDbType.VarChar,
                500)
            {
                Value = passwordHash
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Aud_UsuarioActualizacion",
                SqlDbType.BigInt)
            {
                Value = usuarioActualizacion
            });

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(
                cancellationToken);
        }

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
            cancellationToken))
        {
            return null;
        }

        return new TenantAdminResult
        {
            Usu_Id = reader.GetInt64(
                reader.GetOrdinal("Usu_Id")),

            Ten_Id = reader.GetInt64(
                reader.GetOrdinal("Ten_Id")),

            Usu_NombreUsuario = reader.GetString(
                reader.GetOrdinal(
                    "Usu_NombreUsuario")),

            Usu_Email = reader.GetString(
                reader.GetOrdinal(
                    "Usu_Email"))
        };
    }

    public async Task<TenantResult?>
        CambiarEstatusAsync(
            long tenantId,
            int estatusId,
            long usuarioActualizacion,
            CancellationToken cancellationToken)
    {
        var connection =
            _dbContext.Database.GetDbConnection();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "flow.usp_Flow_CambiarEstatusBarberia";

        command.CommandType =
            CommandType.StoredProcedure;

        command.CommandTimeout = 30;

        command.Parameters.Add(
            new SqlParameter(
                "@Ten_Id",
                SqlDbType.BigInt)
            {
                Value = tenantId
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Est_Id",
                SqlDbType.Int)
            {
                Value = estatusId
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Aud_UsuarioActualizacion",
                SqlDbType.BigInt)
            {
                Value = usuarioActualizacion
            });

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(
                cancellationToken);
        }

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
            cancellationToken))
        {
            return null;
        }

        return new TenantResult
        {
            Ten_Id = reader.GetInt64(
                reader.GetOrdinal("Ten_Id")),

            Ten_Codigo = reader.GetString(
                reader.GetOrdinal("Ten_Codigo")),

            Ten_Nombre = reader.GetString(
                reader.GetOrdinal("Ten_Nombre")),

            Ten_Subdominio = reader.GetString(
                reader.GetOrdinal("Ten_Subdominio")),

            Ten_ZonaHoraria = reader.GetString(
                reader.GetOrdinal("Ten_ZonaHoraria")),

            Est_Id = reader.GetInt32(
                reader.GetOrdinal("Est_Id")),

            Est_Codigo = reader.GetString(
                reader.GetOrdinal("Est_Codigo")),

            Est_Nombre = reader.GetString(
                reader.GetOrdinal("Est_Nombre")),

            Aud_FechaCreacion =
                reader.GetFieldValue<DateTimeOffset>(
                    reader.GetOrdinal(
                        "Aud_FechaCreacion"))
        };
    }
}