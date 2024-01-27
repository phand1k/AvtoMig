namespace MainWebApplication.ViewModels
{
    public class ModelForWashAddOrder
    {
        public int Id { get; set; }
        public string NameService { get; set; }
        public string NameMaster { get; set; }
        public int? Price { get; set; }
        public string? PriceForParse { get; set; }
        public int? SalaryMaster { get; set; }
        public string? Status { get; set; }
        public int? Profit { get; set; }
    }
}
