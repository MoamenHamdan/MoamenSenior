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

         // GET: Item/Create
    public IActionResult Create()
    {
        // Populate dropdowns for foreign keys (use LINQ to get the correct data for each dropdown)
        ViewBag.Uoms = new SelectList(_context.Uoms, "UomId", "UomName");
        ViewBag.CdIdItg = new SelectList(_context.Codescs.Where(c => c.CdId == 4), "CdId", "CdDescriptionLan1"); // Example for ItCdIdItg
        ViewBag.CdIdIbd = new SelectList(_context.Codescs.Where(c => c.CdId == 5), "CdId", "CdDescriptionLan1");
        ViewBag.CdIdIgp = new SelectList(_context.Codescs.Where(c => c.CdId == 6), "CdId", "CdDescriptionLan1");
        ViewBag.CdIdIsg = new SelectList(_context.Codescs.Where(c => c.CdId == 7), "CdId", "CdDescriptionLan1");
        ViewBag.CdIdItp = new SelectList(_context.Codescs.Where(c => c.CdId == 16), "CdId", "CdDescriptionLan1");

        return View();
    }

    // POST: Item/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Item item)
    {
        if (ModelState.IsValid)
        {
            // Insert the item into the database
            _context.Items.Add(item);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index)); // Or another action like "Details"
        }

        // If model is invalid, return to the create view with current data
        ViewBag.Uoms = new SelectList(_context.Uoms, "UomId", "UomName", item.ItUomId);
        ViewBag.CdIdItg = new SelectList(_context.Codescs.Where(c => c.CdId == 4), "CdId", "CdDescriptionLan1", item.ItCdIdItg);
        ViewBag.CdIdIbd = new SelectList(_context.Codescs.Where(c => c.CdId == 5), "CdId", "CdDescriptionLan1", item.ItCdIdIbd);
        ViewBag.CdIdIgp = new SelectList(_context.Codescs.Where(c => c.CdId == 6), "CdId", "CdDescriptionLan1", item.ItCdIdIgp);
        ViewBag.CdIdIsg = new SelectList(_context.Codescs.Where(c => c.CdId == 7), "CdId", "CdDescriptionLan1", item.ItCdIdIsg);
        ViewBag.CdIdItp = new SelectList(_context.Codescs.Where(c => c.CdId == 16), "CdId", "CdDescriptionLan1", item.ItCdIdItp);

        return View(item);
    }

       

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItId == id);
        }
    }
}
