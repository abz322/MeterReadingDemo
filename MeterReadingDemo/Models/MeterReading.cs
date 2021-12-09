using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MeterReadingDemo.Models
{
    public class MeterReading
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Account")]
        public int AccountId { get; set; }

        public DateTime MeterReadingDateTime { get; set; }

        public int MeterReadValue { get; set; }

        public virtual Account Account { get; set; }
    }
}
