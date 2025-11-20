using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace M_Suite.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly MSuiteContext _context;
        private readonly ILogger<ExportController> _logger;

        public ExportController(MSuiteContext context, ILogger<ExportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Export/Transactions
        [HttpGet]
        public async Task<IActionResult> ExportTransactions(string format = "csv")
        {
            try
            {
                var transactions = await _context.Transactions
                    .Include(t => t.TsBu)
                    .Include(t => t.TsThpsIdBillNavigation)
                    .Include(t => t.TsTst)
                    .Include(t => t.TsTss)
                    .OrderByDescending(t => t.TsDate)
                    .ToListAsync();

                if (format.ToLower() == "csv")
                {
                    return await ExportToCsv(transactions, "Transactions");
                }

                return BadRequest("Unsupported export format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting transactions");
                TempData["Error"] = "An error occurred while exporting data.";
                return RedirectToAction("Index", "Transaction");
            }
        }

        // GET: Export/Items
        [HttpGet]
        public async Task<IActionResult> ExportItems(string format = "csv")
        {
            try
            {
                var items = await _context.Items
                    .Include(i => i.ItUom)
                    .OrderBy(i => i.ItCode)
                    .ToListAsync();

                if (format.ToLower() == "csv")
                {
                    return await ExportItemsToCsv(items, "Items");
                }

                return BadRequest("Unsupported export format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting items");
                TempData["Error"] = "An error occurred while exporting data.";
                return RedirectToAction("Index", "Item");
            }
        }

        private async Task<IActionResult> ExportToCsv(List<Transaction> transactions, string fileName)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Write header
            csv.WriteField("Transaction Number");
            csv.WriteField("Date");
            csv.WriteField("Type");
            csv.WriteField("Status");
            csv.WriteField("Business Unit");
            csv.WriteField("Customer");
            csv.WriteField("Total");
            csv.WriteField("Total Final");
            csv.NextRecord();

            // Write data
            foreach (var transaction in transactions)
            {
                csv.WriteField(transaction.TsNumber ?? "");
                csv.WriteField(transaction.TsDate.ToString("yyyy-MM-dd"));
                csv.WriteField(transaction.TsTst?.TstDescriptionLan1 ?? "");
                csv.WriteField(transaction.TsTss?.TssDescriptionLan1 ?? "");
                csv.WriteField(transaction.TsBu?.BuDescriptionLan1 ?? "");
                csv.WriteField(transaction.TsThpsIdBillNavigation?.ThpsNameLan1 ?? "");
                csv.WriteField(transaction.TsTotal?.ToString("F2") ?? "0.00");
                csv.WriteField(transaction.TsTotalFinal?.ToString("F2") ?? "0.00");
                csv.NextRecord();
            }

            await writer.FlushAsync();
            memoryStream.Position = 0;

            return File(memoryStream.ToArray(), "text/csv", $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private async Task<IActionResult> ExportItemsToCsv(List<Item> items, string fileName)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Write header
            csv.WriteField("Item Code");
            csv.WriteField("Description");
            csv.WriteField("UOM");
            csv.WriteField("Active");
            csv.WriteField("Saleable");
            csv.NextRecord();

            // Write data
            foreach (var item in items)
            {
                csv.WriteField(item.ItCode ?? "");
                csv.WriteField(item.ItDescriptionLan1 ?? "");
                csv.WriteField(item.ItUom?.UomNameLan1 ?? "");
                csv.WriteField(item.ItActive == 1 ? "Yes" : "No");
                csv.WriteField(item.ItIsSaleable == 1 ? "Yes" : "No");
                csv.NextRecord();
            }

            await writer.FlushAsync();
            memoryStream.Position = 0;

            return File(memoryStream.ToArray(), "text/csv", $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }
}

