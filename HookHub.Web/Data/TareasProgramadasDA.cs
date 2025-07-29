using HookHub.Web.Models;
using HookHub.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace HookHub.Web.Data
{
    public static class TareasProgramadasDA
    {

        public static List<ScheduledTaskModel> ObtenerTareasProgramadas(bool? estatus = null)
        {
            List<ScheduledTaskModel> tareas = new List<ScheduledTaskModel>();
            string sqlConn = Config.GetConnectionString("SqlConn");
            using (SqlConnection connection = new SqlConnection(sqlConn))
            {
                string queryname = "Tareas_ObtenerTareas";
                SqlCommand cmd = new SqlCommand(queryname, connection);

                if (estatus.HasValue) { cmd.Parameters.AddWithValue("@Estatus", estatus.Value); }

                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ScheduledTaskModel tarea = new ScheduledTaskModel();
                            tarea.ClaveTarea = Convert.ToInt32(reader["ClaveTarea"]);
                            tarea.Nombre = Convert.ToString(reader["Nombre"]);
                            tarea.Schedule = Convert.ToString(reader["Schedule"]);
                            tarea.ScheduledTaskType = (ScheduledTaskType)Convert.ToInt32(reader["ScheduledTaskType"]);
                            tarea.Tarea = Convert.ToString(reader["Tarea"]);
                            tarea.Estatus = Convert.ToBoolean(reader["Estatus"]);
                            tareas.Add(tarea);
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            return (tareas);
        }
    }
}
