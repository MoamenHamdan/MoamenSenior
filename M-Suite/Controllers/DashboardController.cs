using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using M_Suite.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace M_Suite.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly MSuiteContext _context;

        public DashboardController(MSuiteContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Create a dashboard view model to hold all our data
            var dashboardVM = new DashboardViewModel();

            // 1. Total Sales Revenue Summary
            dashboardVM.TotalSales = await _context.Transactions
                .Where(t => t.TsTotalFinal.HasValue)
                .SumAsync(t => t.TsTotalFinal.Value);

            dashboardVM.SalesToday = await _context.Transactions
                .Where(t => t.TsDate.Date == DateTime.Today && t.TsTotalFinal.HasValue)
                .SumAsync(t => t.TsTotalFinal.Value);

            dashboardVM.SalesThisMonth = await _context.Transactions
                .Where(t => t.TsDate.Month == DateTime.Today.Month && 
                            t.TsDate.Year == DateTime.Today.Year && 
                            t.TsTotalFinal.HasValue)
                .SumAsync(t => t.TsTotalFinal.Value);

            // 2. Transaction Counts
            dashboardVM.TotalTransactions = await _context.Transactions.CountAsync();
            dashboardVM.TransactionsToday = await _context.Transactions
                .Where(t => t.TsDate.Date == DateTime.Today)
                .CountAsync();

            // 3. Recent Transactions
            dashboardVM.RecentTransactions = await _context.Transactions
                .Include(t => t.TsThpsIdBillNavigation)
                .ThenInclude(tp => tp.ThpsThp)
                .Include(t => t.TsTst)
                .Include(t => t.TsTss)
                .OrderByDescending(t => t.TsDate)
                .Take(5)
                .ToListAsync();

            // 4. Item Stats
            dashboardVM.TotalItems = await _context.Items.CountAsync();
            dashboardVM.ActiveItems = await _context.Items.Where(i => i.ItActive == 1).CountAsync();

            // 5. Best Selling Items
            dashboardVM.TopSellingItems = await _context.TransactionItems
                .Include(ti => ti.TsiIt)
                .Where(ti => ti.TsiIt != null)
                .GroupBy(ti => ti.TsiIt.ItDescriptionLan1)
                .Select(g => new TopSellingItemViewModel
                {
                    ItemName = g.Key,
                    QuantitySold = g.Sum(ti => ti.TsiQuantity),
                    Revenue = g.Sum(ti => (ti.TsiTotalAmount ?? 0))
                })
                .OrderByDescending(i => i.Revenue)
                .Take(5)
                .ToListAsync();

            // 6. Customer Stats
            dashboardVM.TotalCustomers = await _context.Thirdparties
                .Where(t => t.ThpIsCustomer == 1)
                .CountAsync();

            dashboardVM.ActiveCustomers = await _context.Thirdparties
                .Where(t => t.ThpIsCustomer == 1 && t.ThpActive == 1)
                .CountAsync();

            // 7. Top Customers
            dashboardVM.TopCustomers = await _context.Transactions
                .Include(t => t.TsThpsIdBillNavigation)
                .ThenInclude(tp => tp.ThpsThp)
                .Where(t => t.TsThpsIdBillNavigation != null && t.TsThpsIdBillNavigation.ThpsThp != null && t.TsTotalFinal.HasValue)
                .GroupBy(t => t.TsThpsIdBillNavigation.ThpsThp.ThpNameLan1)
                .Select(g => new TopCustomerViewModel
                {
                    CustomerName = g.Key,
                    PurchaseAmount = g.Sum(t => t.TsTotalFinal ?? 0),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(c => c.PurchaseAmount)
                .Take(5)
                .ToListAsync();

            // 8. Warehouse Inventory Summary
            dashboardVM.InventorySummary = await _context.ItemWarehouses
                .Include(iw => iw.ItwIt)
                .Include(iw => iw.ItwPlIdWhsNavigation)
                .GroupBy(iw => iw.ItwPlIdWhsNavigation.PlDescriptionLan1)
                .Select(g => new WarehouseInventoryViewModel
                {
                    WarehouseName = g.Key,
                    ItemCount = g.Count(),
                    TotalQuantity = g.Sum(iw => iw.ItwQuantity)
                })
                .ToListAsync();

            // 9. Transaction Status Distribution
            dashboardVM.TransactionStatusDistribution = await _context.Transactions
                .Include(t => t.TsTss)
                .Where(t => t.TsTss != null)
                .GroupBy(t => t.TsTss.TssDescriptionLan1)
                .Select(g => new TransactionStatusViewModel
                {
                    StatusName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // 10. Monthly Sales Trend
            var startDate = DateTime.Today.AddMonths(-6);
            var salesTrend = await _context.Transactions
                .Where(t => t.TsDate >= startDate && t.TsTotalFinal.HasValue)
                .GroupBy(t => new { t.TsDate.Year, t.TsDate.Month })
                .Select(g => new MonthlySalesViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSales = g.Sum(t => t.TsTotalFinal.Value)
                })
                .OrderBy(s => s.Year)
                .ThenBy(s => s.Month)
                .ToListAsync();

            dashboardVM.MonthlySalesTrend = salesTrend;

            return View(dashboardVM);
        }

        // GET: Dashboard/AI
        public IActionResult AI()
        {
            return View();
        }
    }
} 