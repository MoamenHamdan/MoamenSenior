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
using Microsoft.Extensions.Logging;

namespace M_Suite.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly MSuiteContext _context;
        private readonly ILogger<ItemController> _logger;

        public ItemController(MSuiteContext context, ILogger<ItemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Item
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            _logger.LogInformation("Item Index accessed - Search: {SearchString}, Sort: {SortOrder}, Page: {Page}", 
                searchString, sortOrder, page);
            
            int pageSize = 20;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CodeSortParam = string.IsNullOrEmpty(sortOrder) ? "code_desc" : "";
            ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.CurrentFilter = searchString;

            var query = _context.Items
                .Include(i => i.ItCdIdIbdNavigation)
                .Include(i => i.ItCdIdIgpNavigation)
                .Include(i => i.ItCdIdIsgNavigation)
                .Include(i => i.ItCdIdItgNavigation)
                .Include(i => i.ItCdIdItpNavigation)
                .Include(i => i.ItIt)
                .Include(i => i.ItUom)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i =>
                    i.ItCode.Contains(searchString) ||
                    i.ItDescriptionLan1.Contains(searchString) ||
                    i.ItDescriptionLan2.Contains(searchString)
                );
            }

            switch (sortOrder)
            {
                case "code_desc":
                    query = query.OrderByDescending(i => i.ItCode);
                    break;
                case "name":
                    query = query.OrderBy(i => i.ItDescriptionLan1);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(i => i.ItDescriptionLan1);
                    break;
                default:
                    query = query.OrderBy(i => i.ItCode);
                    break;
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(items);
        }

        // GET: Item/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var item = await _context.Items
                    .Include(i => i.ItCdIdIbdNavigation)
                    .Include(i => i.ItCdIdIgpNavigation)
                    .Include(i => i.ItCdIdIsgNavigation)
                    .Include(i => i.ItCdIdItgNavigation)
                    .Include(i => i.ItCdIdItpNavigation)
                    .Include(i => i.ItIt)
                    .Include(i => i.ItUom)
                    .FirstOrDefaultAsync(m => m.ItId == id);
                
                if (item == null)
                {
                    return NotFound();
                }

                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading item details for ID: {ItemId}", id);
                TempData["Error"] = "An error occurred while loading item details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Item/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        private void PopulateDropdowns(Item item = null)
        {
            ViewData["ItCdIdIbd"] = new SelectList(_context.Codescs.Where(c => c.CdDescriptionLan1 != null), "CdId", "CdDescriptionLan1", item?.ItCdIdIbd);
            ViewData["ItCdIdIgp"] = new SelectList(_context.Codescs.Where(c => c.CdCode != null), "CdId", "CdCode", item?.ItCdIdIgp);
            ViewData["ItCdIdIsg"] = new SelectList(_context.Codescs.Where(c => c.CdFcCode != null), "CdId", "CdFcCode", item?.ItCdIdIsg);
            ViewData["ItCdIdItg"] = new SelectList(_context.Codescs.Where(c => c.CdNum1 != null), "CdId", "CdNum1", item?.ItCdIdItg);
            ViewData["ItCdIdItp"] = new SelectList(_context.Codescs.Where(c => c.CdDescriptionLan1 != null), "CdId", "CdDescriptionLan1", item?.ItCdIdItp);
            ViewData["ItItId"] = new SelectList(_context.Items.Where(i => i.ItDescriptionLan1 != null), "ItId", "ItDescriptionLan1", item?.ItItId);
            ViewData["ItUomId"] = new SelectList(_context.Uoms.Where(u => u.UomNameLan1 != null), "UomId", "UomNameLan1", item?.ItUomId);
        }

        // POST: Item/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItId,ItItId,ItUomId,ItCdIdItg,ItCdIdIbd,ItCdIdIgp,ItCdIdIsg,ItCdIdItp,ItCode,ItDescriptionLan1,ItDescriptionLan2,ItDescriptionLan3,ItWeight,ItHasLot,ItHasProductionDate,ItHasExpiryDate,ItHasMultipleUom,ItHasSerial,ItIsDescription,ItIsSaleable,ItIsService,ItIsAsset,ItActive,ItImpUid,ItOrder,ItIsBadReturn")] Item item)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(item.ItCode))
            {
                ModelState.AddModelError("ItCode", "Item code is required.");
            }
            else if (await _context.Items.AnyAsync(i => i.ItCode == item.ItCode))
            {
                ModelState.AddModelError("ItCode", "Item code already exists.");
            }

            if (string.IsNullOrWhiteSpace(item.ItDescriptionLan1))
            {
                ModelState.AddModelError("ItDescriptionLan1", "Item description is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set default values
                    if (item.ItActive == null)
                    {
                        item.ItActive = 1;
                    }

                    _context.Add(item);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Item created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error creating item");
                    ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating item");
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                }
            }

            PopulateDropdowns(item);
            return View(item);
        }

        // GET: Item/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    return NotFound();
                }
                PopulateDropdowns(item);
                return View(item);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the item.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Item/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItId,ItItId,ItUomId,ItCdIdItg,ItCdIdIbd,ItCdIdIgp,ItCdIdIsg,ItCdIdItp,ItCode,ItDescriptionLan1,ItDescriptionLan2,ItDescriptionLan3,ItWeight,ItHasLot,ItHasProductionDate,ItHasExpiryDate,ItHasMultipleUom,ItHasSerial,ItIsDescription,ItIsSaleable,ItIsService,ItIsAsset,ItActive,ItImpUid,ItOrder,ItIsBadReturn")] Item item)
        {
            if (id != item.ItId)
            {
                return NotFound();
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(item.ItCode))
            {
                ModelState.AddModelError("ItCode", "Item code is required.");
            }
            else if (await _context.Items.AnyAsync(i => i.ItCode == item.ItCode && i.ItId != id))
            {
                ModelState.AddModelError("ItCode", "Item code already exists.");
            }

            if (string.IsNullOrWhiteSpace(item.ItDescriptionLan1))
            {
                ModelState.AddModelError("ItDescriptionLan1", "Item description is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Item updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.ItId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating: " + ex.Message);
                }
            }

            PopulateDropdowns(item);
            return View(item);
        }

        // GET: Item/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var item = await _context.Items
                    .Include(i => i.ItCdIdIbdNavigation)
                    .Include(i => i.ItCdIdIgpNavigation)
                    .Include(i => i.ItCdIdIsgNavigation)
                    .Include(i => i.ItCdIdItgNavigation)
                    .Include(i => i.ItCdIdItpNavigation)
                    .Include(i => i.ItIt)
                    .Include(i => i.ItUom)
                    .FirstOrDefaultAsync(m => m.ItId == id);
                
                if (item == null)
                {
                    return NotFound();
                }

                // Check if item is used in transactions
                var isUsed = await _context.TransactionItems.AnyAsync(ti => ti.TsiItId == id);
                if (isUsed)
                {
                    TempData["Error"] = "Cannot delete item that is used in transactions.";
                    return RedirectToAction(nameof(Index));
                }

                return View(item);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the item.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Item/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item != null)
                {
                    // Check if item is used in transactions
                    var isUsed = await _context.TransactionItems.AnyAsync(ti => ti.TsiItId == id);
                    if (isUsed)
                    {
                        TempData["Error"] = "Cannot delete item that is used in transactions.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Items.Remove(item);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Item deleted successfully";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Unable to delete item. It may be in use.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the item.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Item/Correlations/5
        public async Task<IActionResult> Correlations(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var item = await _context.Items
                    .Include(i => i.ItCdIdItgNavigation)
                    .FirstOrDefaultAsync(m => m.ItId == id);
                
                if (item == null)
                {
                    return NotFound();
                }

                return View(item);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading item correlations.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItId == id);
        }
    }
}
