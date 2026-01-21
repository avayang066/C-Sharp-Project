namespace MyApp.Models
{
    public class Machine
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string MachineCode { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string MachineName { get; set; }

        public bool IsActive { get; set; }
    }
}
