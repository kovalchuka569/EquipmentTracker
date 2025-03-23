namespace Data.Entities;

public class Log
{
    public int Id { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
}