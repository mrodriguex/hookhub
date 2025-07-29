using HookHub.Web.Models;

using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;

namespace HookHub.Web.Tasks
{
    public static class SqlTask
    {
        public static async void SqlSPAsync(string sqlSP)
        {
            await SqlSP(sqlSP);
        }

        public static async Task<ResultModel> SqlSP(string queryname)
        {
            ResultModel resultado = new ResultModel();
                string sqlConn = Config.GetConnectionString("SqlConn");
                using (SqlConnection connection = new SqlConnection(sqlConn))
                {
                    SqlCommand cmd = new SqlCommand(queryname, connection);

                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        connection.Open();
                        await cmd.ExecuteNonQueryAsync();
                        resultado.Success = true;
                        resultado.Data = $"La ejecución del procedimiento ({queryname}) con éxito.";
                    }
                    catch (Exception ex)
                    {
                        resultado.Success = false;
                    resultado.Data = $"Ocurrió un error al ejecutar el procedimiento ({queryname}).";
                    resultado.Errors.Add($"{ex.Message}.");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            return (resultado);
        }
    }
}
