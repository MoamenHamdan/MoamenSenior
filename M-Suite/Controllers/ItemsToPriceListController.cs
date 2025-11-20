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
    public class PriceListItemsController : Controller
    {
        private readonly MSuiteContext _context;

        public PriceListItemsController(MSuiteContext context)
        {
            _context = context;
        }

        // STEP 1: CREATE PRICE LIST
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Listprice listprice)
        {
         
                _context.Listprices.Add(listprice);
                await _context.SaveChangesAsync();
              return RedirectToAction("AddItems", "ItemsToPriceList", new { id = listprice.LpId });

            

            PopulateDropdowns(listprice);
            return View(listprice);
        }

        // GET: PriceListItems/AddItems/5
        public async Task<IActionResult> AddItems(int id)
        {
            var listprice = await _context.Listprices
                .Include(lp => lp.LpBu)
                .Include(lp => lp.LpCdIdCurNavigation)
                .FirstOrDefaultAsync(m => m.LpId == id);

            if (listprice == null)
            {
                return NotFound();
            }

            var existingItemIds = await _context.ListpriceItems
                .Where(x => x.LpiLpId == id)
                .Select(x => x.LpiItId)
                .ToListAsync();

            var availableItems = await _context.Items
                .Where(x => !existingItemIds.Contains(x.ItId))
                .Include(x => x.ItUom)
                .ToListAsync();

            ViewBag.Listprice = listprice;
            ViewBag.AvailableItems = availableItems;
            return View();
        }

        // POST: PriceListItems/AddItems/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItems(int id, List<int> selectedItems, List<decimal> prices)
        {
            if (selectedItems == null || prices == null || selectedItems.Count != prices.Count)
            {
                ModelState.AddModelError("", "Please select items and provide their prices.");
                return RedirectToAction("AddItems", new { id });
            }

            for (int i = 0; i < selectedItems.Count; i++)
            {
                var itemId = selectedItems[i];
                var item = await _context.Items.FindAsync(itemId);

                var priceItem = new ListpriceItem
                {
                    LpiLpId = id,
                    LpiItId = itemId,
                    LpiPrice = prices[i],
                    LpiUomId = item?.ItUomId ?? 1
                };

                _context.ListpriceItems.Add(priceItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Items added successfully!";
            return RedirectToAction("Details", "PriceList", new { id });
        }

        // GET: PriceListItems/EditItemPrice/5
        public async Task<IActionResult> EditItemPrice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.ListpriceItems
                .Include(x => x.LpiIt)
                .Include(x => x.LpiUom)
                .FirstOrDefaultAsync(x => x.LpiId == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: PriceListItems/EditItemPrice/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItemPrice(int id, [Bind("LpiId,LpiPrice,LpiDiscount,LpiMaxDiscount")] ListpriceItem model)
        {
            if (id != model.LpiId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var item = await _context.ListpriceItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                item.LpiPrice = model.LpiPrice;
                item.LpiDiscount = model.LpiDiscount;
                item.LpiMaxDiscount = model.LpiMaxDiscount;

                _context.Update(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item price updated successfully!";

                return RedirectToAction("Details", "PriceList", new { id = item.LpiLpId });
            }

            return View(model);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var list = await _context.Listprices
                .Include(lp => lp.ListpriceItems)
                .ThenInclude(li => li.LpiIt)
                .FirstOrDefaultAsync(lp => lp.LpId == id);

            if (list == null) return NotFound();

            return View(list);
        }

        // Helpers
        private void PopulateDropdowns(Listprice listprice = null)
        {
            ViewData["LpCdIdCur"] = new SelectList(_context.VCodescCurs, "CdId", "CdDescriptionLan1", listprice?.LpCdIdCur);
            ViewData["LpBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", listprice?.LpBuId);
        }

        private bool ListpriceItemExists(int id)
        {
            return _context.ListpriceItems.Any(e => e.LpiId == id);
        }
    }
}