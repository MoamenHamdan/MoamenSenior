// Updated ItemWarehouseController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace M_Suite.Controllers
{
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
            if (ModelState.IsValid)
            {
                itemWarehouse.ItwCreationDate = DateTime.UtcNow;


                _context.Add(itemWarehouse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemWarehouseExists(itemWarehouse.ItwId))
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
            var itemWarehouse = await _context.ItemWarehouses.FindAsync(id);
            if (itemWarehouse != null)
            {
                _context.ItemWarehouses.Remove(itemWarehouse);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ItemWarehouseExists(int id)
        {
            return _context.ItemWarehouses.Any(e => e.ItwId == id);
        }
        private void PopulateDropdowns(ItemWarehouse itemWarehouse = null)
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