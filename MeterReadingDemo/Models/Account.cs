using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MeterReadingDemo.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        public int AccountId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

    }
}
