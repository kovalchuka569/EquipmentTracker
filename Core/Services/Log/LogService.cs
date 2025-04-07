using Data.AppDbContext;
using Microsoft.EntityFrameworkCore;
using DbContext = Data.AppDbContext.DbContext;

namespace Core.Services.Log;


public class LogService 
{
    private readonly DbContext _context;

    public LogService(DbContext context)
    {
        _context = context;
    }

    public void AddLog(string description)
    {
       DateTime now = DateTime.Now;
       
       DateTime logDate = now.Date;
       TimeSpan logTime = now.TimeOfDay;
       logTime = new TimeSpan(logTime.Hours, logTime.Minutes, logTime.Seconds);

       var newLog = new Data.Entities.Log
       {
           Date = logDate,
           Time = logTime,
           Description = description,
       };

       _context.Logs.Add(newLog);
       _context.SaveChanges();
    }
}