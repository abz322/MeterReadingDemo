using System.Linq;
using System.Data.Entity.Migrations;
using MeterReadingDemo.Models;
using Microsoft.EntityFrameworkCore;
using EntityFramework.Seeder;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Reflection;
using System.Text;
using CsvHelper.Configuration;
using System.Globalization;

namespace MeterReadingDemo.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<AccountMeterReadingContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(AccountMeterReadingContext context)
        {
            //This Seed override is used to consume the Test_Accounts.csv file
            //and creates the database tables using the given data
            //To set this up, open Package Manager Console for the project and run the following command
            //Update-Database
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MeterReadingDemo.Domain.SeedData.Test_Accounts.csv";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                };
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    CsvReader csvReader = new CsvReader(reader, conf);
                    var accounts = csvReader.GetRecords<Account>().ToArray();
                    context.Accounts.AddOrUpdate(c => c.AccountId, accounts);
                }
            }
        }
    }
}
