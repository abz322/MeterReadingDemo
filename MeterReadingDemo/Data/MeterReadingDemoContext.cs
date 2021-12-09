using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeterReadingDemo.Models;

namespace MeterReadingDemo.Data
{
    public class MeterReadingDemoContext : DbContext
    {
        public MeterReadingDemoContext (DbContextOptions<MeterReadingDemoContext> options)
            : base(options)
        {
        }

        public DbSet<MeterReadingDemo.Models.MeterReading> MeterReadings { get; set; }
        public DbSet<MeterReadingDemo.Models.Account> Accounts { get; set; }
    }
}
