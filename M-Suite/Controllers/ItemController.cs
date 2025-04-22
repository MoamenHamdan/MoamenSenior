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
    public class ItemController : Controller
    {
        private readonly MSuiteContext _context;

        public ItemController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: Item
        public async Task<IActionResult> Index()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading items for Index view");
                var items = await _context.Items
                    .Include(i => i.ItIt)
                    .Include(i => i.ItUom)
                    .Include(i => i.ItCdIdItgNavigation)
                    .OrderBy(i => i.ItCode)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Found {items.Count} items");
                
                // Check for success message
                if (TempData["SuccessMessage"] != null)
                {
                    ViewBag.SuccessMessage = TempData["SuccessMessage"];
                }

                return View(items);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading items: {ex.Message}");
                ModelState.AddModelError("", "Error loading items. Please try again.");
                return View(new List<Item>());
            }
        }

        // GET: Item/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.ItIt)
                .Include(i => i.ItUom)
                .FirstOrDefaultAsync(m => m.ItId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Item/Create
        public IActionResult Create()
        {
            ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItCode");
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomCode");
            ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId");
            return View();
        }

        // POST: Item/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItId,ItItId,ItUomId,ItCdIdItg,ItCdIdIbd,ItCdIdIgp,ItCdIdIsg,ItCdIdItp,ItCode,ItDescriptionLan1,ItDescriptionLan2,ItDescriptionLan3,ItWeight,ItHasLot,ItHasProductionDate,ItHasExpiryDate,ItHasMultipleUom,ItHasSerial,ItIsDescription,ItIsSaleable,ItIsService,ItIsAsset,ItActive,ItImpUid,ItOrder,ItIsBadReturn")] Item item)
        {
            try
            {
                // Log the incoming item data
                System.Diagnostics.Debug.WriteLine($"Creating new item with code: {item.ItCode}");
                System.Diagnostics.Debug.WriteLine($"UOM ID: {item.ItUomId}");
                System.Diagnostics.Debug.WriteLine($"Item Group ID: {item.ItCdIdItg}");
                System.Diagnostics.Debug.WriteLine($"Description: {item.ItDescriptionLan1}");

                // Log model state errors
                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("Model state is invalid. Errors:");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                        }
                    }
                }

                // Set default values for required fields if not provided
                if (item.ItActive == 0)
                {
                    item.ItActive = 1;
                }

                // Set default values for checkbox fields
                item.ItHasLot = item.ItHasLot == 1 ? (short)1 : (short)0;
                item.ItHasProductionDate = item.ItHasProductionDate == 1 ? (short)1 : (short)0;
                item.ItHasExpiryDate = item.ItHasExpiryDate == 1 ? (short)1 : (short)0;
                item.ItIsService = item.ItIsService == 1 ? (short)1 : (short)0;
                item.ItIsAsset = item.ItIsAsset == 1 ? (short)1 : (short)0;
                item.ItIsSaleable = item.ItIsSaleable == 1 ? (short)1 : (short)0;
                item.ItHasSerial = item.ItHasSerial == 1 ? (short)1 : (short)0;
                item.ItHasMultipleUom = item.ItHasMultipleUom == 1 ? (short)1 : (short)0;
                item.ItIsBadReturn = item.ItIsBadReturn == 1 ? (short)1 : (short)0;

                // Check if the UOM exists
                var uomExists = await _context.Uoms.AnyAsync(u => u.UomId == item.ItUomId);
                if (!uomExists)
                {
                    System.Diagnostics.Debug.WriteLine($"UOM with ID {item.ItUomId} does not exist");
                    ModelState.AddModelError("ItUomId", "Selected UOM does not exist");
                    ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomCode", item.ItUomId);
                    ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
                    ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItCode", item.ItItId);
                    return View(item);
                }

                // Check if the Item Group exists
                var itemGroupExists = await _context.Codescs.AnyAsync(c => c.CdId == item.ItCdIdItg);
                if (!itemGroupExists)
                {
                    System.Diagnostics.Debug.WriteLine($"Item Group with ID {item.ItCdIdItg} does not exist");
                    ModelState.AddModelError("ItCdIdItg", "Selected Item Group does not exist");
                    ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomCode", item.ItUomId);
                    ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
                    ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItCode", item.ItItId);
                    return View(item);
                }

                // Check if item code already exists
                var existingItem = await _context.Items.FirstOrDefaultAsync(i => i.ItCode == item.ItCode);
                if (existingItem != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Item with code {item.ItCode} already exists");
                    ModelState.AddModelError("ItCode", "An item with this code already exists");
                    ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomCode", item.ItUomId);
                    ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
                    ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItCode", item.ItItId);
                    return View(item);
                }

                if (ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("Model is valid, attempting to save to database");
                    _context.Add(item);
                    var result = await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Save result: {result} rows affected");
                    
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Item created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No rows were affected during save");
                        ModelState.AddModelError("", "Failed to save the item. Please try again.");
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            // If we get here, something went wrong
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomCode", item.ItUomId);
            ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
            ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItCode", item.ItItId);
            return View(item);
        }

        // GET: Item/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItId", item.ItItId);
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomId", item.ItUomId);
            return View(item);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItId", item.ItItId);
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomId", item.ItUomId);
            return View(item);
        }

        // GET: Item/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.ItIt)
                .Include(i => i.ItUom)
                .FirstOrDefaultAsync(m => m.ItId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Item/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Item/CreateSimple
        public IActionResult CreateSimple()
        {
            try
            {
                ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomCode");
                ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId");
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateSimple GET: {ex.Message}");
                return View("Error");
            }
        }

        // POST: Item/CreateSimple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSimple([Bind("ItCode,ItDescriptionLan1,ItUomId,ItCdIdItg")] Item item)
        {
            try
            {
                // Log the incoming model state
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                }

                // Set default values for required fields
                item.ItActive = 1;
                item.ItHasLot = 0;
                item.ItHasProductionDate = 0;
                item.ItHasExpiryDate = 0;
                item.ItIsService = 0;
                item.ItIsAsset = 0;
                item.ItIsSaleable = 0;
                item.ItHasSerial = 0;
                item.ItHasMultipleUom = 0;
                item.ItIsBadReturn = 0;

                // Validate required fields
                if (string.IsNullOrWhiteSpace(item.ItCode))
                {
                    ModelState.AddModelError("ItCode", "Item Code is required");
                }
                if (string.IsNullOrWhiteSpace(item.ItDescriptionLan1))
                {
                    ModelState.AddModelError("ItDescriptionLan1", "Description is required");
                }
                if (item.ItUomId == 0)
                {
                    ModelState.AddModelError("ItUomId", "Unit of Measure is required");
                }
                if (item.ItCdIdItg == 0)
                {
                    ModelState.AddModelError("ItCdIdItg", "Item Group is required");
                }

                if (ModelState.IsValid)
                {
                    // Check if item code already exists
                    var existingItem = await _context.Items.FirstOrDefaultAsync(i => i.ItCode == item.ItCode);
                    if (existingItem != null)
                    {
                        ModelState.AddModelError("ItCode", "An item with this code already exists");
                        ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomCode", item.ItUomId);
                        ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
                        return View(item);
                    }

                    _context.Add(item);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            // If we got this far, something failed; redisplay form
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomCode", item.ItUomId);
            ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
            return View(item);
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItId == id);
        }
    }
}
