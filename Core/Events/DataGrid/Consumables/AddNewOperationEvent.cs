namespace Core.Events.DataGrid.Consumables
{
    public class AddNewOperationEvent: PubSubEvent<AddNewOperationEventArgs> {}

    public class AddNewOperationEventArgs
    {
        public string? OperationType { get; set; }
        public string? Quantity { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTime = System.DateTime.Now;
        public int User { get; set; }
        public byte[] ReceiptImageBytes { get; set; }
    }
}

