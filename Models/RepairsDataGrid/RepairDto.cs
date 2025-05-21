namespace Models.RepairsDataGrid
{
    public class RepairDto
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string RepairDescription { get; set; }
        public string EquipmentInventoryNumber { get; set; }
        public string EquipmentBrand { get; set; }
        public string EquipmentModel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan Duration { get; set; }
        public int Worker { get; set; }
        public string Status {get; set; }
    }
}
