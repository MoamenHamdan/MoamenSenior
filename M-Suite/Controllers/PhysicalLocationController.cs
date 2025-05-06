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
    public class PhysicalLocationController : Controller
    {
        private readonly MSuiteContext _context;

        public PhysicalLocationController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: PhysicalLocation
        public async Task<IActionResult> Index()
        {
            var mSuiteContext = _context.PhysicalLocations.Include(p => p.PlBu).Include(p => p.PlCdIdPltNavigation).Include(p => p.PlMd).Include(p => p.PlPl);
            return View(await mSuiteContext.ToListAsync());
        }

        // GET: PhysicalLocation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var physicalLocation = await _context.PhysicalLocations
                .Include(p => p.PlBu)
                .Include(p => p.PlCdIdPltNavigation)
                .Include(p => p.PlMd)
                .Include(p => p.PlPl)
                .FirstOrDefaultAsync(m => m.PlId == id);
            if (physicalLocation == null)
            {
                return NotFound();
            }

            return View(physicalLocation);
        }

     // GET: PhysicalLocation/Create
public IActionResult Create()
{
    PopulateDropDowns();
    return View();
}

// POST: PhysicalLocation/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("PlId,PlPlId,PlCdIdPlt,PlMdId,PlLevel,PlCode,PlDescriptionLan1,PlDescriptionLan2,PlDescriptionLan3,PlBuId,PlActive,PlImpUid")] PhysicalLocation physicalLocation)
{
 
        _context.Add(physicalLocation);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    

    // If we got this far, something failed; re-populate dropdowns
    PopulateDropDowns(physicalLocation);
    return View(physicalLocation);
}

// Helper method to populate dropdowns
private void PopulateDropDowns(PhysicalLocation? physicalLocation = null)
{
    ViewData["PlBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", physicalLocation?.PlBuId);
    ViewData["PlCdIdPlt"] = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", physicalLocation?.PlCdIdPlt);
    ViewData["PlMdId"] = new SelectList(_context.Modules, "MdId", "MdDescription", physicalLocation?.PlMdId);
    ViewData["PlPlId"] = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1", physicalLocation?.PlPlId);
}
// GET: PhysicalLocation/Edit/5
public async Task<IActionResult> Edit(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var physicalLocation = await _context.PhysicalLocations.FindAsync(id);
    if (physicalLocation == null)
    {
        return NotFound();
    }

    ViewData["PlBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", physicalLocation.PlBuId);
    ViewData["PlCdIdPlt"] = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", physicalLocation.PlCdIdPlt);
    ViewData["PlMdId"] = new SelectList(_context.Modules, "MdId", "MdDescription", physicalLocation.PlMdId);
    ViewData["PlPlId"] = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1", physicalLocation.PlPlId);

    return View(physicalLocation);
}


   // POST: PhysicalLocation/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("PlId,PlPlId,PlCdIdPlt,PlMdId,PlLevel,PlCode,PlDescriptionLan1,PlDescriptionLan2,PlDescriptionLan3,PlBuId,PlActive,PlImpUid")] PhysicalLocation physicalLocation)
{
    if (id != physicalLocation.PlId)
    {
        return NotFound();
    }

            _context.Update(physicalLocation);
            await _context.SaveChangesAsync();
        

        return RedirectToAction(nameof(Index));
    

    // Rebuild dropdowns if ModelState is invalid
    ViewData["PlBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", physicalLocation.PlBuId);
    ViewData["PlCdIdPlt"] = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", physicalLocation.PlCdIdPlt);
    ViewData["PlMdId"] = new SelectList(_context.Modules, "MdId", "MdDescription", physicalLocation.PlMdId);
    ViewData["PlPlId"] = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1", physicalLocation.PlPlId);

    return View(physicalLocation);
}

// Helper method
private bool PhysicalLocationExists(int id)
{
    return _context.PhysicalLocations.Any(e => e.PlId == id);
}


        // GET: PhysicalLocation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var physicalLocation = await _context.PhysicalLocations
                .Include(p => p.PlBu)
                .Include(p => p.PlCdIdPltNavigation)
                .Include(p => p.PlMd)
                .Include(p => p.PlPl)
                .FirstOrDefaultAsync(m => m.PlId == id);
            if (physicalLocation == null)
            {
                return NotFound();
            }

            return View(physicalLocation);
        }

        // POST: PhysicalLocation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var physicalLocation = await _context.PhysicalLocations.FindAsync(id);
            if (physicalLocation != null)
            {
                _context.PhysicalLocations.Remove(physicalLocation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

     
    }
}
