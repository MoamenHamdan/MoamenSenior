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

            return View(geoLocation);
        }

        // GET: GeoLocation/Create
        public IActionResult Create()
        {
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
