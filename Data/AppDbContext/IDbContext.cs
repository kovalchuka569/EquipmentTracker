namespace Data.AppDbContext
{
    public interface IDbContext
    {
        DbContext Create();
    }
}
