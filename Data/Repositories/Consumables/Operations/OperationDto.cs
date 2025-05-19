namespace Data.Repositories.Consumables.Operations
{
    public class OperationDto
    {
        public int Id { get; set; }
        public string OperationType { get; set; }
        public double Quantity { get; set; }
        public double? BalanceAfter { get; set; }
        public string Description { get; set; }
        public string Worker { get; set; }
        public byte[] Receipt { get; set; }
        public DateTime DateTime { get; set; }
    }
}
