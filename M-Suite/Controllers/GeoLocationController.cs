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
    public class GeoLocationController : Controller
    {
        private readonly MSuiteContext _context;

        public GeoLocationController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: GeoLocation
        public async Task<IActionResult> Index()
        {
            return View(await _context.GeoLocations.ToListAsync());
        }

        // GET: GeoLocation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var geoLocation = await _context.GeoLocations
                .FirstOrDefaultAsync(m => m.GlId == id);
            if (geoLocation == null)
            {
                return NotFound();
            }

            // Get parent location if exists
            if (geoLocation.GlGlId.HasValue)
            {
                var parentLocation = await _context.GeoLocations
                    .FirstOrDefaultAsync(p => p.GlId == geoLocation.GlGlId);
                ViewBag.ParentLocation = parentLocation;
            }

            return View(geoLocation);
        }

        // GET: GeoLocation/Create
        public async Task<IActionResult> Create()
        {
            // Get all existing geolocations for the parent dropdown
            var parentLocations = await _context.GeoLocations
                .Select(g => new SelectListItem
                {
                    Value = g.GlId.ToString(),
                    Text = $"{g.GlCode} - {g.GlDescriptionLan1}"
                })
                .ToListAsync();

            ViewBag.ParentLocations = parentLocations;
            return View();
        }

        // POST: GeoLocation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GlId,GlCdIdGeo,GlGlId,GlLevel,GlCode,GlDescriptionLan1,GlDescriptionLan2,GlDescriptionLan3")] GeoLocation geoLocation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(geoLocation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(geoLocation);
        }

        // GET: GeoLocation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var geoLocation = await _context.GeoLocations.FindAsync(id);
            if (geoLocation == null)
            {
                return NotFound();
            }

            // Get all existing geolocations for the parent dropdown, excluding the current one
            var parentLocations = await _context.GeoLocations
                .Where(g => g.GlId != id) // Exclude current location from parent options
                .Select(g => new SelectListItem
                {
                    Value = g.GlId.ToString(),
                    Text = $"{g.GlCode} - {g.GlDescriptionLan1}"
                })
                .ToListAsync();

            ViewBag.ParentLocations = parentLocations;
            return View(geoLocation);
        }

        // POST: GeoLocation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GlId,GlCdIdGeo,GlGlId,GlLevel,GlCode,GlDescriptionLan1,GlDescriptionLan2,GlDescriptionLan3")] GeoLocation geoLocation)
        {
            if (id != geoLocation.GlId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(geoLocation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeoLocationExists(geoLocation.GlId))
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
            return View(geoLocation);
        }

        // GET: GeoLocation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var geoLocation = await _context.GeoLocations
                .FirstOrDefaultAsync(m => m.GlId == id);
            if (geoLocation == null)
            {
                return NotFound();
            }

            // Get parent location if exists
            if (geoLocation.GlGlId.HasValue)
            {
                var parentLocation = await _context.GeoLocations
                    .FirstOrDefaultAsync(p => p.GlId == geoLocation.GlGlId);
                ViewBag.ParentLocation = parentLocation;
            }

            return View(geoLocation);
        }

        // POST: GeoLocation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var geoLocation = await _context.GeoLocations.FindAsync(id);
            if (geoLocation != null)
            {
                _context.GeoLocations.Remove(geoLocation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GeoLocationExists(int id)
        {
            return _context.GeoLocations.Any(e => e.GlId == id);
        }
    }
}
