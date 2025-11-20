using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using Microsoft.AspNetCore.Authorization;

namespace M_Suite.Controllers
{
    [Authorize]
    public class PriceListController : Controller
    {
        private readonly MSuiteContext _context;

        public PriceListController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: PriceList
        public async Task<IActionResult> Index()
        {
            var priceLists = await _context.Listprices
                .Include(lp => lp.LpCdIdCurNavigation)
                .Include(lp => lp.LpBu)
                .OrderBy(lp => lp.LpCode)
                .ToListAsync();

            return View(priceLists);
        }

        // GET: PriceList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var listprice = await _context.Listprices
                .Include(lp => lp.LpCdIdCurNavigation)
                .Include(lp => lp.LpBu)
                .Include(lp => lp.ListpriceItems)
                    .ThenInclude(lpi => lpi.LpiIt)
                .Include(lp => lp.ListpriceItems)
                    .ThenInclude(lpi => lpi.LpiUom)
                .FirstOrDefaultAsync(m => m.LpId == id);

            if (listprice == null)
            {
                return NotFound();
            }

            return View(listprice);
        }

        // GET: PriceList/Create
        public IActionResult Create()
        {
            ViewData["LpCdIdCur"] = new SelectList(_context.VCodescCurs, "CdId", "CdDescriptionLan1");
            ViewData["LpBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LpId,LpBuId,LpCdIdCur,LpCode,LpDescriptionLan1,LpDescriptionLan2,LpDescriptionLan3,LpFromDate,LpToDate,LpActive")] Listprice listprice)
        {
            if (true)
            {
                // Ensure LpActive is set to 0 or 1
                listprice.LpActive = (short)(listprice.LpActive == 1 ? 1 : 0);
                
                _context.Add(listprice);
                await _context.SaveChangesAsync();
                return RedirectToAction("AddItems", "PriceListItems", new { id = listprice.LpId });
            }

            PopulateDropdowns(listprice);
            return View(listprice);
        }

        // GET: PriceList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listprice = await _context.Listprices.FindAsync(id);
            if (listprice == null)
            {
                return NotFound();
            }

            PopulateDropdowns(listprice);
            return View(listprice);
        }

        // POST: PriceList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LpId,LpBuId,LpCdIdCur,LpCode,LpDescriptionLan1,LpDescriptionLan2,LpDescriptionLan3,LpFromDate,LpToDate,LpActive")] Listprice listprice)
        {
            if (id != listprice.LpId)
            {
                return NotFound();
            }

            if (true)
            {
              
                    // Ensure LpActive is set to 0 or 1
                    listprice.LpActive = (short)(listprice.LpActive == 1 ? 1 : 0);
                    
                    _context.Update(listprice);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Price list updated successfully!";
                    return RedirectToAction(nameof(Index));
                
          
            }

            PopulateDropdowns(listprice);
            return View(listprice);
        }

        // GET: PriceList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listprice = await _context.Listprices
                .Include(lp => lp.LpCdIdCurNavigation)
                .Include(lp => lp.LpBu)
                .FirstOrDefaultAsync(m => m.LpId == id);

            if (listprice == null)
            {
                return NotFound();
            }

            return View(listprice);
        }

        // POST: PriceList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listprice = await _context.Listprices.FindAsync(id);
            if (listprice != null)
            {
                _context.Listprices.Remove(listprice);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Price List deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns(Listprice listprice)
        {
            ViewData["LpCdIdCur"] = new SelectList(_context.VCodescCurs, "CdId", "CdDescriptionLan1", listprice.LpCdIdCur);
            ViewData["LpBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", listprice.LpBuId);
        }

        private bool ListpriceExists(int id)
        {
            return _context.Listprices.Any(e => e.LpId == id);
        }
    }
}