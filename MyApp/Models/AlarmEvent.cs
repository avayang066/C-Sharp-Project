using System;

namespace MyApp.Models
{
    public class AlarmEvent
    {
        public int Id { get; set; }

        public int MachineId { get; set; }
        public Machine Machine { get; set; }

        public long ProductionLogId { get; set; }
        public ProductionLog ProductionLog { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string AlarmType { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
