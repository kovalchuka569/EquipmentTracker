namespace Models.ConsumablesDataGrid
{
    public class ConsumableItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? NameDisplay => Name;
        public string Category { get; set; }
        public string CategoryDisplay => Category;
        public decimal? Balance { get; set; }
        public decimal? BalanceDisplay => Balance;
        public string? Unit { get; set; }
        public string? UnitDisplay => Unit;
        public decimal? MinBalance { get; set; }
        public decimal? MaxBalance { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public string? LastModifiedDateDisplay =>
            LastModifiedDate.HasValue ? LastModifiedDate.Value.ToString("dd.MM.yyyy HH:mm") : "-";
        public string? Notes { get; set; }
        public string? NotesDisplay => Notes;
        
        public double BalanceWidth => CalculateBalanceWidth();
        public string BalanceColor => WidthColorSelector(BalanceWidth, 200);

        public bool IsCritical => (BalanceWidth / 200) * 100 < 15;
        
        private double CalculateBalanceWidth()
        {
            if (!MinBalance.HasValue || !MaxBalance.HasValue || !Balance.HasValue || MaxBalance == MinBalance)
                return 0;

            const double maxWidth = 200.0;
            double percent = (double)((Balance - MinBalance) / (MaxBalance - MinBalance));
            percent = Math.Clamp(percent, 0, 1);

            return percent * maxWidth;
        }
        private string WidthColorSelector(double width, double maxWidth)
        {
            if (maxWidth <= 0) return "Gray";

            double percent = (width / maxWidth) * 100;
            if (percent < 15)
                return "Red";
            else if (percent < 50)
                return "DarkGoldenrod";
            else
                return "Green";
        }
    }
    
}
