namespace Core.Services.Consumables.Operations
{
    public class Operation
    {
        public int? Id { get; set; }
        public string? OperationType { get; set; }
        public double Quantity { get; set; }
        public string? QuantityDisplay => GetFormattedValue(OperationType, Quantity);
        public double? BalanceAfter { get; set; }
        public string? BalanceAfterDisplay => BalanceAfter?.ToString();
        public string? OperationForeground => GetOperationColor(OperationType);
        public string? OperationTypeDisplay => OperationType;
        public string? CellBackground => GetCellBackgroundColor(OperationType);
        public string? Description { get; set; }
        public string? DescriptionDisplay => Description;
        public DateTime DateTime { get; set; }
        public string? DateTimeDisplay => DateTime.ToString("dd.MM.yyyy HH:mm");
        public string? Worker { get; set; }
        public string? WorkerDisplay => Worker;
        public byte[]? Receipt { get; set; }
        public string? ReceiptDisplay => Receipt != null ? "Переглянути квитанцію" : "";
        public bool? ReceiptVisibility => Receipt != null;
        
        private string GetFormattedValue(string type, double value)
        {
            return type switch
            {
                "Прихід" => $"+{value}",
                "Списання" => $"-{value}",
                _ => value.ToString()
            };
        }

        private string GetOperationColor(string type)
        {
            return type switch
            {
                "Прихід" => "Green",
                "Списання" => "Red",
                _ => "Black"
            };
        }

        private string GetCellBackgroundColor(string operationType)
        {
            if (string.IsNullOrEmpty(operationType))
                return "Transparent";

            return operationType switch
            {
                "Прихід" => "#607D9C6B",
                "Списання" => "#609C6B6B"
            };
        }
    }
}
