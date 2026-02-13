namespace MyApp.Models.Dto
{
    public class ProductionLogDto
    {
        public long Id { get; set; }
        public int MachineId { get; set; }
        public string? MachineCode { get; set; }
        // 其他欄位可依需求擴充
    }
}
