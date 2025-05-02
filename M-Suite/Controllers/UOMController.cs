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
    public class UOMController : Controller
    {
        private readonly MSuiteContext _context;

        public UOMController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: UOM
        public async Task<IActionResult> Index()
        {
            return View(await _context.Uoms.ToListAsync());
        }

        // GET: UOM/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uom = await _context.Uoms
                .FirstOrDefaultAsync(m => m.UomId == id);
            if (uom == null)
            {
                return NotFound();
            }

            return View(uom);
        }

        // GET: UOM/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UOM/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UomId,UomCode,UomNameLan1,UomNameLan2,UomNameLan3,UomRoundingPrecision,UomIsBase,UomIsSaleable,UomOrder")] Uom uom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(uom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(uom);
        }

        // GET: UOM/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uom = await _context.Uoms.FindAsync(id);
            if (uom == null)
            {
                return NotFound();
            }
            return View(uom);
        }

        // POST: UOM/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UomId,UomCode,UomNameLan1,UomNameLan2,UomNameLan3,UomRoundingPrecision,UomIsBase,UomIsSaleable,UomOrder")] Uom uom)
        {
            if (id != uom.UomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UomExists(uom.UomId))
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
            return View(uom);
        }

        // GET: UOM/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uom = await _context.Uoms
                .FirstOrDefaultAsync(m => m.UomId == id);
            if (uom == null)
            {
                return NotFound();
            }

            return View(uom);
        }

        // POST: UOM/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uom = await _context.Uoms.FindAsync(id);
            if (uom != null)
            {
                _context.Uoms.Remove(uom);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UomExists(int id)
        {
            return _context.Uoms.Any(e => e.UomId == id);
        }
    }
}
