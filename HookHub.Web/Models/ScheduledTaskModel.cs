using System.Collections.Generic;
using System.IO;

namespace HookHub.Web.Models
{
    public class ScheduledTaskModel
    {
        private string _nombre;
        private string _tarea;
        private string _schedule;

        public int ClaveTarea { get; set; }

        public string Nombre {
            get {
                if (string.IsNullOrEmpty(_nombre)) { _nombre = ""; }
                return (_nombre);
            }
            set {
                _nombre = value;
            }
        }

        public string Tarea {
            get {
                if (string.IsNullOrEmpty(_tarea)) { _tarea = ""; }
                return (_tarea);
            }
            set {
                _tarea = value;
            }
        }

        public string Schedule {
            get {
                if (_schedule is null) { _schedule = "0 */1 * * * *"; }
                return (_schedule);
            }
            set {
                _schedule = value;
            }
        }

        public ScheduledTaskType ScheduledTaskType { get; set; }

        public bool Estatus { get; set; } = false;

    }
    public enum ScheduledTaskType
    {
        Custom = 0,
        StoredProcedure = 1,
        HttpGet = 2,
        HttpPost = 3
    }
}
