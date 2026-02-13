namespace MyApp.Models.Dto
{
    public class ProductionLogSimpleDto
    {
        public long Id { get; set; }
        public int MachineId { get; set; }
        public string Status { get; set; }
        public double YieldRate { get; set; }
        public int OutputQty { get; set; }
        public DateTime Timestamp { get; set; }
        public string MachineCode { get; set; }
        public string MachineName { get; set; }
    }
}
