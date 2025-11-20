// Updated ItemWarehouseController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace M_Suite.Controllers
{
    [Authorize]
    public class ItemWarehouseController : Controller
    {
        private readonly MSuiteContext _context;

        public ItemWarehouseController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: ItemWarehouse
        public async Task<IActionResult> Index()
        {
            var itemWarehouses = await _context.ItemWarehouses
                .Include(i => i.ItwIt)
                .Include(i => i.ItwPlIdWhsNavigation)
                .Include(i => i.ItwUom)
                .ToListAsync();

            return View(itemWarehouses);
        }

        // GET: ItemWarehouse/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemWarehouse = await _context.ItemWarehouses
                .Include(i => i.ItwIt)
                .Include(i => i.ItwPlIdWhsNavigation)
                .Include(i => i.ItwUom)
                .FirstOrDefaultAsync(m => m.ItwId == id);

            if (itemWarehouse == null)
            {
                return NotFound();
            }

            return View(itemWarehouse);
        }

        // GET: ItemWarehouse/Create
        public IActionResult Create()
        {
            // Debugging: Check if reference tables have data
            var hasItems = _context.Items.Any();
            var hasLocations = _context.PhysicalLocations.Any();
            var hasUoms = _context.Uoms.Any();

            if (!hasItems || !hasLocations || !hasUoms)
            {
                TempData["Error"] = $"Missing data - Items: {hasItems}, Locations: {hasLocations}, UOMs: {hasUoms}";
            }

            PopulateDropdowns();
            return View();
        }

        // POST: ItemWarehouse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItwPlIdWhs,ItwItId,ItwUomId,ItwQuantity,ItwQuantityDamage,ItwQuantityReserved,ItwQuantityPreviewed,ItwRemarks")] ItemWarehouse itemWarehouse)
        {
            // Additional validation
            if (itemWarehouse.ItwItId == null || itemWarehouse.ItwItId == 0)
            {
                ModelState.AddModelError("ItwItId", "Item is required.");
            }
            if (itemWarehouse.ItwPlIdWhs == null || itemWarehouse.ItwPlIdWhs == 0)
            {
                ModelState.AddModelError("ItwPlIdWhs", "Warehouse is required.");
            }
            if (itemWarehouse.ItwQuantity < 0)
            {
                ModelState.AddModelError("ItwQuantity", "Quantity cannot be negative.");
            }

            // Check for duplicate item-warehouse combination
            if (itemWarehouse.ItwItId.HasValue && itemWarehouse.ItwPlIdWhs.HasValue)
            {
                var exists = await _context.ItemWarehouses
                    .AnyAsync(iw => iw.ItwItId == itemWarehouse.ItwItId.Value && 
                                   iw.ItwPlIdWhs == itemWarehouse.ItwPlIdWhs.Value);
                if (exists)
                {
                    ModelState.AddModelError("", "This item already exists in the selected warehouse. Please edit the existing record instead.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    itemWarehouse.ItwCreationDate = DateTime.UtcNow;
                    _context.Add(itemWarehouse);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Item warehouse record created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. The item may already exist in this warehouse.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                }
            }

            PopulateDropdowns(itemWarehouse);
            return View(itemWarehouse);
        }

        // GET: ItemWarehouse/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemWarehouse = await _context.ItemWarehouses.FindAsync(id);
            if (itemWarehouse == null)
            {
                return NotFound();
            }

            PopulateDropdowns(itemWarehouse);
            return View(itemWarehouse);
        }

        // POST: ItemWarehouse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItwId,ItwPlIdWhs,ItwItId,ItwUomId,ItwQuantity,ItwQuantityDamage,ItwQuantityReserved,ItwQuantityPreviewed,ItwRemarks")] ItemWarehouse itemWarehouse)
        {
            if (id != itemWarehouse.ItwId)
            {
                return NotFound();
            }

            // Additional validation
            if (itemWarehouse.ItwItId == null || itemWarehouse.ItwItId == 0)
            {
                ModelState.AddModelError("ItwItId", "Item is required.");
            }
            if (itemWarehouse.ItwPlIdWhs == null || itemWarehouse.ItwPlIdWhs == 0)
            {
                ModelState.AddModelError("ItwPlIdWhs", "Warehouse is required.");
            }
            if (itemWarehouse.ItwQuantity < 0)
            {
                ModelState.AddModelError("ItwQuantity", "Quantity cannot be negative.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _context.ItemWarehouses.FindAsync(id);
                    existingItem.ItwPlIdWhs = itemWarehouse.ItwPlIdWhs;
                    existingItem.ItwItId = itemWarehouse.ItwItId;
                    existingItem.ItwUomId = itemWarehouse.ItwUomId;
                    existingItem.ItwQuantity = itemWarehouse.ItwQuantity;
                    existingItem.ItwQuantityDamage = itemWarehouse.ItwQuantityDamage;
                    existingItem.ItwQuantityReserved = itemWarehouse.ItwQuantityReserved;
                    existingItem.ItwQuantityPreviewed = itemWarehouse.ItwQuantityPreviewed;
                    existingItem.ItwRemarks = itemWarehouse.ItwRemarks;
                    existingItem.ItwUpdateDate = DateTime.UtcNow;
                    // existingItem.ItwUpdatedBy = User.Identity.IsAuthenticated ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) : 0;

                    _context.Update(existingItem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Item warehouse record updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemWarehouseExists(itemWarehouse.ItwId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Please refresh and try again.");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Please check your data and try again.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                }
            }

            PopulateDropdowns(itemWarehouse);
            return View(itemWarehouse);
        }

        // GET: ItemWarehouse/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemWarehouse = await _context.ItemWarehouses
                .Include(i => i.ItwIt)
                .Include(i => i.ItwPlIdWhsNavigation)
                .Include(i => i.ItwUom)
                .FirstOrDefaultAsync(m => m.ItwId == id);

            if (itemWarehouse == null)
            {
                return NotFound();
            }

            return View(itemWarehouse);
        }

        // POST: ItemWarehouse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var itemWarehouse = await _context.ItemWarehouses.FindAsync(id);
                if (itemWarehouse != null)
                {
                    // Check if item has transactions
                    var hasTransactions = await _context.TransactionItems
                        .AnyAsync(ti => ti.TsiItId == itemWarehouse.ItwItId && ti.TsiPlIdWhs == itemWarehouse.ItwPlIdWhs);
                    
                    if (hasTransactions)
                    {
                        TempData["Error"] = "Cannot delete item warehouse record that has associated transactions.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.ItemWarehouses.Remove(itemWarehouse);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Item warehouse record deleted successfully";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Unable to delete item warehouse record. It may be in use.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the record.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ItemWarehouseExists(int id)
        {
            return _context.ItemWarehouses.Any(e => e.ItwId == id);
        }
        private void PopulateDropdowns(ItemWarehouse? itemWarehouse = null)
        {
            // Safely get lists or empty lists if null
            var items = _context?.Items?.ToList() ?? new List<Item>();
            var locations = _context?.PhysicalLocations?.ToList() ?? new List<PhysicalLocation>();
            var uoms = _context?.Uoms?.ToList() ?? new List<Uom>();

            ViewData["ItwItId"] = items.Any()
                ? new SelectList(items, "ItId", "ItName", itemWarehouse?.ItwItId)
                : new SelectList(new List<Item>(), "ItId", "ItName");

            ViewData["ItwPlIdWhs"] = locations.Any()
                ? new SelectList(locations, "PlId", "PlName", itemWarehouse?.ItwPlIdWhs)
                : new SelectList(new List<PhysicalLocation>(), "PlId", "PlName");

            ViewData["ItwUomId"] = uoms.Any()
                ? new SelectList(uoms, "UomId", "UomName", itemWarehouse?.ItwUomId)
                : new SelectList(new List<Uom>(), "UomId", "UomName");

            // Add warning if no data
            if (!items.Any() || !locations.Any() || !uoms.Any())
            {
                TempData["DropdownWarning"] = "Warning: Some dropdowns have no data. Please configure Items, Warehouses, and UOMs first.";
            }
        }
    }
}