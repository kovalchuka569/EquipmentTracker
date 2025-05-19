namespace Models.ConsumablesDataGrid
{
    public class ConsumableDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public decimal? Balance { get; set; }
        public string? Unit { get; set; }
        public decimal? MinBalance { get; set; }
        public decimal? MaxBalance { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? Notes { get; set; }
    }
}