using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Models
{
    public class ProductionLog
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Machine")]
        public int MachineId { get; set; }
        public Machine Machine { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        public double YieldRate { get; set; }
        public int OutputQty { get; set; }
        public DateTime Timestamp { get; set; }
    }
}