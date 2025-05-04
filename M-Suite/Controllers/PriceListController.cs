using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;

namespace M_Suite.Controllers
{
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

        // POST: PriceList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LpId,LpBuId,LpCdIdCur,LpCode,LpDescriptionLan1,LpDescriptionLan2,LpDescriptionLan3,LpFromDate,LpToDate,LpActive")] Listprice listprice)
        {
        
        

                if (ModelState.IsValid)
                {
                    // Set default values
                    if (listprice.LpActive == 0)
                    {
                        listprice.LpActive = 1;
                    }

                    // Log the values being saved
                    System.Diagnostics.Debug.WriteLine($"Creating Price List with values:");
                    System.Diagnostics.Debug.WriteLine($"Code: {listprice.LpCode}");
                    System.Diagnostics.Debug.WriteLine($"Description: {listprice.LpDescriptionLan1}");
                    System.Diagnostics.Debug.WriteLine($"Business Unit: {listprice.LpBuId}");
                    System.Diagnostics.Debug.WriteLine($"Currency: {listprice.LpCdIdCur}");
                    System.Diagnostics.Debug.WriteLine($"From Date: {listprice.LpFromDate}");
                    System.Diagnostics.Debug.WriteLine($"To Date: {listprice.LpToDate}");
                    System.Diagnostics.Debug.WriteLine($"Active: {listprice.LpActive}");

                    _context.Add(listprice);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Price List created successfully!";
                    return RedirectToAction(nameof(Index));
                }
       
          
            // Repopulate dropdowns
            ViewData["LpCdIdCur"] = new SelectList(_context.VCodescCurs, "CdId", "CdDescriptionLan1", listprice.LpCdIdCur);
            ViewData["LpBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", listprice.LpBuId);
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

            ViewData["LpCdIdCur"] = new SelectList(_context.VCodescCurs, "CdId", "CdDescriptionLan1", listprice.LpCdIdCur);
            ViewData["LpBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", listprice.LpBuId);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listprice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListpriceExists(listprice.LpId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["LpCdIdCur"] = new SelectList(_context.VCodescCurs, "CdId", "CdDescriptionLan1", listprice.LpCdIdCur);
            ViewData["LpBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", listprice.LpBuId);
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: PriceList/AddItems/5
        public async Task<IActionResult> AddItems(int? id)
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

            // Get all items not already in the price list
            var existingItemIds = await _context.ListpriceItems
                .Where(lpi => lpi.LpiLpId == id)
                .Select(lpi => lpi.LpiItId)
                .ToListAsync();

            var availableItems = await _context.Items
                .Where(i => !existingItemIds.Contains(i.ItId))
                .Include(i => i.ItUom)
                .OrderBy(i => i.ItCode)
                .ToListAsync();

            ViewBag.Listprice = listprice;
            ViewBag.AvailableItems = availableItems;
            return View();
        }

        // POST: PriceList/AddItems/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItems(int id, List<int> selectedItems, List<decimal> prices)
        {
            if (selectedItems == null || prices == null || selectedItems.Count != prices.Count)
            {
                ModelState.AddModelError("", "Invalid item selection or pricing data");
                return RedirectToAction(nameof(AddItems), new { id });
            }

            var listprice = await _context.Listprices.FindAsync(id);
            if (listprice == null)
            {
                return NotFound();
            }

            for (int i = 0; i < selectedItems.Count; i++)
            {
                var itemId = selectedItems[i];
                var price = prices[i];

                var listpriceItem = new ListpriceItem
                {
                    LpiLpId = id,
                    LpiItId = itemId,
                    LpiPrice = price,
                    LpiUomId = _context.Items.Find(itemId)?.ItUomId ?? 0
                };

                _context.ListpriceItems.Add(listpriceItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: PriceList/EditItemPrice/5
        public async Task<IActionResult> EditItemPrice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listpriceItem = await _context.ListpriceItems
                .Include(lpi => lpi.LpiIt)
                .Include(lpi => lpi.LpiUom)
                .FirstOrDefaultAsync(m => m.LpiId == id);

            if (listpriceItem == null)
            {
                return NotFound();
            }

            return View(listpriceItem);
        }

        // POST: PriceList/EditItemPrice/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItemPrice(int id, [Bind("LpiId,LpiPrice,LpiDiscount,LpiMaxDiscount")] ListpriceItem listpriceItem)
        {
            if (id != listpriceItem.LpiId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _context.ListpriceItems.FindAsync(id);
                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    existingItem.LpiPrice = listpriceItem.LpiPrice;
                    existingItem.LpiDiscount = listpriceItem.LpiDiscount;
                    existingItem.LpiMaxDiscount = listpriceItem.LpiMaxDiscount;

                    _context.Update(existingItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListpriceItemExists(listpriceItem.LpiId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = listpriceItem.LpiLpId });
            }

            return View(listpriceItem);
        }

        // GET: PriceList/SelectItems
        public IActionResult SelectItems()
        {
            // Get all active items
            var availableItems = _context.Items
                .Include(i => i.ItUom)
                .Where(i => i.ItActive == 1)
                .OrderBy(i => i.ItCode)
                .ToList();

            ViewBag.AvailableItems = availableItems;
            ViewData["LpCdIdCur"] = new SelectList(_context.VCodescCurs, "CdId", "CdDescriptionLan1");
            ViewData["LpBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1");
            return View();
        }

        // POST: PriceList/CreateWithItems
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWithItems([Bind("LpId,LpBuId,LpCdIdCur,LpCode,LpDescriptionLan1,LpDescriptionLan2,LpDescriptionLan3,LpFromDate,LpToDate,LpActive")] Listprice listprice, 
            List<int> selectedItems, List<decimal> prices)
        {
            try
            {
                if (ModelState.IsValid && selectedItems != null && prices != null && selectedItems.Count == prices.Count)
                {
                    // Set default values
                    if (listprice.LpActive == 0)
                    {
                        listprice.LpActive = 1;
                    }

                    // Create the price list
                    _context.Add(listprice);
                    await _context.SaveChangesAsync();

                    // Add the selected items with their prices
                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        var itemId = selectedItems[i];
                        var price = prices[i];

                        var listpriceItem = new ListpriceItem
                        {
                            LpiLpId = listprice.LpId,
                            LpiItId = itemId,
                            LpiPrice = price,
                            LpiUomId = _context.Items.Find(itemId)?.ItUomId ?? 0
                        };

                        _context.ListpriceItems.Add(listpriceItem);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Price List created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Log validation errors
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        foreach (var error in state.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in {key}: {error.ErrorMessage}");
                        }
                    }

                    if (selectedItems == null || prices == null || selectedItems.Count != prices.Count)
                    {
                        ModelState.AddModelError("", "Please select at least one item and provide its price.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception while creating Price List: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", "An error occurred while creating the Price List. Please try again.");
            }

            // Repopulate the view data
            var availableItems = _context.Items
                .Include(i => i.ItUom)
                .Where(i => i.ItActive == 1)
                .OrderBy(i => i.ItCode)
                .ToList();

            ViewBag.AvailableItems = availableItems;
            ViewData["LpCdIdCur"] = new SelectList(_context.VCodescCurs, "CdId", "CdDescriptionLan1", listprice.LpCdIdCur);
            ViewData["LpBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", listprice.LpBuId);
            return View("SelectItems", listprice);
        }

        private bool ListpriceExists(int id)
        {
            return _context.Listprices.Any(e => e.LpId == id);
        }

        private bool ListpriceItemExists(int id)
        {
            return _context.ListpriceItems.Any(e => e.LpiId == id);
        }
    }
} 