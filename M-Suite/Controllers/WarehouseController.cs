using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M_Suite.Models;
using M_Suite.Context;
using System.Threading.Tasks;
using System.Linq;

namespace M_Suite.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehouseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Warehouse
        public async Task<IActionResult> Index()
        {
            var warehouses = await _context.Warehouses
                .Include(w => w.Location)
                .ToListAsync();
            return View(warehouses);
        }

        // GET: Warehouse/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Warehouses
                .Include(w => w.Location)
                .FirstOrDefaultAsync(m => m.WarehouseId == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            return View(warehouse);
        }

        // GET: Warehouse/Create
        public IActionResult Create()
        {
            ViewBag.Locations = _context.Locations.ToList();
            return View();
        }

        // POST: Warehouse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WarehouseId,Name,LocationId,Capacity,Description")] Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                _context.Add(warehouse);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Warehouse created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Locations = _context.Locations.ToList();
            return View(warehouse);
        }

        // GET: Warehouse/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                return NotFound();
            }
            ViewBag.Locations = _context.Locations.ToList();
            return View(warehouse);
        }

        // POST: Warehouse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WarehouseId,Name,LocationId,Capacity,Description")] Warehouse warehouse)
        {
            if (id != warehouse.WarehouseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(warehouse);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Warehouse updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarehouseExists(warehouse.WarehouseId))
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
            ViewBag.Locations = _context.Locations.ToList();
            return View(warehouse);
        }

        // GET: Warehouse/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Warehouses
                .Include(w => w.Location)
                .FirstOrDefaultAsync(m => m.WarehouseId == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            return View(warehouse);
        }

        // POST: Warehouse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Warehouse deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool WarehouseExists(int id)
        {
            return _context.Warehouses.Any(e => e.WarehouseId == id);
        }
    }
} 