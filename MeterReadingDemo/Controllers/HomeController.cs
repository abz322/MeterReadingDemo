using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeterReadingDemo.Data;
using MeterReadingDemo.Models;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IO;
using System.Net;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace MeterReadingDemo.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly MeterReadingDemoContext _context;

        public HomeController(MeterReadingDemoContext context)
        {
            _context = context;
        }

        [Route("/index")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Route("/meter-reading-uploads")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MeterReadingUploads(IFormCollection form)
        {
            AccountsController accountsController = new AccountsController(_context);
            MeterReadingsController meterReadingsController = new MeterReadingsController(_context);

            if (form == null || form.Files.Count == 0)
            {
                return Json("File could not be found.");
            }
            var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            };
            using (StreamReader reader = new StreamReader(form.Files[0].OpenReadStream(), Encoding.UTF8))
            {
                CsvReader csvReader = new CsvReader(reader, conf);
                List<MeterReading> records = new List<MeterReading>();
                List<string> erroredRecords = new List<string>();
                csvReader.Read();
                csvReader.ReadHeader();
                int columns = csvReader.HeaderRecord.Length;
                while (csvReader.Read())
                {
                    try
                    {
                        if (csvReader.Parser.Count > columns || csvReader.Parser.Record[columns - 1].Length > 0)
                        {
                            string err = string.Format("Account {0}, failed validation at line {1} - Too many columns on this row", csvReader.GetField("AccountId"), csvReader.Parser.Row);
                            erroredRecords.Add(err);
                            continue;
                        }

                        int mReading;
                        bool isInt = int.TryParse(csvReader.GetField("MeterReadValue"), out mReading);

                        if (!isInt)
                        {
                            string err = string.Format("Account {0}, failed validation at line {1} - Meter Reading '{2}' is not a number", csvReader.GetField("AccountId"), csvReader.Parser.Row, csvReader.GetField("MeterReadValue"));
                            erroredRecords.Add(err);
                            continue;
                        }
                        else if (mReading < 0 || mReading > 99999)
                        {
                            string err = string.Format("Account {0}, failed validation at line {1} - Meter Reading {2} is not within the valid range", csvReader.GetField("AccountId"), csvReader.Parser.Row, csvReader.GetField("MeterReadValue"));
                            erroredRecords.Add(err);
                            continue;
                        }

                        MeterReading res = csvReader.GetRecord<MeterReading>();
                        try
                        {
                            var acc = accountsController.GetAccount(res.AccountId).Result.Value;
                            if (acc != null)
                            {
                                var reading = meterReadingsController.GetMeterReading(res.AccountId).Result.Value;
                                if (reading != null)
                                {
                                    var err = string.Format("Account {0}, failed at line {1} - Meter reading already exists", csvReader.GetField("AccountId"), csvReader.Parser.Row);
                                    erroredRecords.Add(err);
                                    continue;
                                }
                                var postSuccessful = await meterReadingsController.PostMeterReading(res);

                                if (postSuccessful != null)
                                {
                                    records.Add(res);
                                }
                                else
                                {
                                    var err = string.Format("Account {0}, failed at line {1} - Failed to save record.", csvReader.GetField("AccountId"), csvReader.Parser.Row);
                                    erroredRecords.Add(err);
                                    continue;
                                }
                            }
                            else
                            {
                                var err = string.Format("Account {0}, failed at line {1} - Account does not exists", csvReader.GetField("AccountId"), csvReader.Parser.Row);
                                erroredRecords.Add(err);
                                continue;
                            }
                        }
                        catch(Exception e)
                        {
                            var err = string.Format("Account {0}, failed at line {1} - Failed to save record.", csvReader.GetField("AccountId"), csvReader.Parser.Row);
                            erroredRecords.Add(err);
                            continue;
                        }
                    }
                    catch
                    {
                        var err = string.Format("Account {0}, failed validation at line {1}",csvReader.GetField("AccountId"), csvReader.Parser.Row);
                        erroredRecords.Add(err);
                        continue;
                    }
                }

                StringBuilder result = new StringBuilder();
                result.AppendLine(string.Format("Processed {0} rows", records.Count() + erroredRecords.Count()));
                result.AppendLine(string.Format("Successful {0} / Failed {1}", records.Count() , erroredRecords.Count()));
                result.AppendLine(Environment.NewLine);
                result.AppendJoin(Environment.NewLine, erroredRecords.Select(x => x));

                return Json(result.ToString());
            }
        }
    }
}
