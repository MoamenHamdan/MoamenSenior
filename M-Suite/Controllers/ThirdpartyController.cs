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
    public class ThirdpartyController : Controller
    {
        private readonly MSuiteContext _context;

        public ThirdpartyController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: Thirdparty
        public async Task<IActionResult> Index()
        {
            var mSuiteContext = _context.Thirdparties.Include(t => t.ThpCdIdTpgNavigation).Include(t => t.ThpCdIdTpsNavigation);
            return View(await mSuiteContext.ToListAsync());
        }

        // GET: Thirdparty/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thirdparty = await _context.Thirdparties
                .Include(t => t.ThpCdIdTpgNavigation)
                .Include(t => t.ThpCdIdTpsNavigation)
                .FirstOrDefaultAsync(m => m.ThpId == id);
            if (thirdparty == null)
            {
                return NotFound();
            }

            return View(thirdparty);
        }

        // GET: Thirdparty/Create
        public IActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Create GET: {ex.Message}");
                return View("Error");
            }
        }

        // POST: Thirdparty/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ThpCode,ThpNameLan1,ThpNameLan2,ThpIsCustomer,ThpIsSupplier,ThpIsCompany")] Thirdparty thirdparty)
        {
            try
            {
                // Set default values for required fields
                thirdparty.ThpActive = 1;
                thirdparty.ThpCreateDate = DateTime.Now;
                thirdparty.ThpImpUid = Guid.NewGuid().ToString();
                thirdparty.ThpImported = 0;
                thirdparty.ThpReadonly = 0;
                thirdparty.ThpPrintarabic = 0;
                thirdparty.ThpIsB2b = 0;
                thirdparty.ThpCdIdTpg = 1; // Default value
                thirdparty.ThpCdIdTps = 1; // Default value

                // Convert checkbox values to short
                thirdparty.ThpIsCustomer = (short)(thirdparty.ThpIsCustomer == 1 ? 1 : 0);
                thirdparty.ThpIsSupplier = (short)(thirdparty.ThpIsSupplier == 1 ? 1 : 0);
                thirdparty.ThpIsCompany = (short)(thirdparty.ThpIsCompany == 1 ? 1 : 0);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(thirdparty.ThpCode))
                {
                    ModelState.AddModelError("ThpCode", "Code is required");
                }
                if (string.IsNullOrWhiteSpace(thirdparty.ThpNameLan1))
                {
                    ModelState.AddModelError("ThpNameLan1", "Name (English) is required");
                }

                if (ModelState.IsValid)
                {
                    // Check if code already exists
                    var existingThirdparty = await _context.Thirdparties
                        .FirstOrDefaultAsync(t => t.ThpCode == thirdparty.ThpCode);
                    
                    if (existingThirdparty != null)
                    {
                        ModelState.AddModelError("ThpCode", "A third party with this code already exists");
                        return View(thirdparty);
                    }

                    _context.Add(thirdparty);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Third party created successfully!";
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

            return View(thirdparty);
        }

        // GET: Thirdparty/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thirdparty = await _context.Thirdparties.FindAsync(id);
            if (thirdparty == null)
            {
                return NotFound();
            }
            ViewData["ThpCdIdTpg"] = new SelectList(_context.Codescs, "CdId", "CdId", thirdparty.ThpCdIdTpg);
            ViewData["ThpCdIdTps"] = new SelectList(_context.Codescs, "CdId", "CdId", thirdparty.ThpCdIdTps);
            return View(thirdparty);
        }

        // POST: Thirdparty/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ThpId,ThpOrgId,ThpCdIdTpg,ThpCdIdTps,ThpCode,ThpNameLan1,ThpNameLan2,ThpNameLan3,ThpIsCustomer,ThpIsSupplier,ThpIsCompany,ThpCreateDate,ThpModifiedDate,ThpActive,ThpImpUid,ThpRemarks,ThpImported,ThpReadonly,ThpUsIdCreated,ThpNewcode,ThpPrintLang,ThpPrintarabic,ThpIsB2b")] Thirdparty thirdparty)
        {
            if (id != thirdparty.ThpId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thirdparty);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThirdpartyExists(thirdparty.ThpId))
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
            ViewData["ThpCdIdTpg"] = new SelectList(_context.Codescs, "CdId", "CdId", thirdparty.ThpCdIdTpg);
            ViewData["ThpCdIdTps"] = new SelectList(_context.Codescs, "CdId", "CdId", thirdparty.ThpCdIdTps);
            return View(thirdparty);
        }

        // GET: Thirdparty/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thirdparty = await _context.Thirdparties
                .Include(t => t.ThpCdIdTpgNavigation)
                .Include(t => t.ThpCdIdTpsNavigation)
                .FirstOrDefaultAsync(m => m.ThpId == id);
            if (thirdparty == null)
            {
                return NotFound();
            }

            return View(thirdparty);
        }

        // POST: Thirdparty/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thirdparty = await _context.Thirdparties.FindAsync(id);
            if (thirdparty != null)
            {
                _context.Thirdparties.Remove(thirdparty);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ThirdpartyExists(int id)
        {
            return _context.Thirdparties.Any(e => e.ThpId == id);
        }
    }
}
