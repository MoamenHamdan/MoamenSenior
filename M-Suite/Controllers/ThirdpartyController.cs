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
            ViewData["ThpCdIdTpg"] = new SelectList(_context.Codescs, "CdId", "CdId");
            ViewData["ThpCdIdTps"] = new SelectList(_context.Codescs, "CdId", "CdId");
            return View();
        }

        // POST: Thirdparty/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ThpId,ThpOrgId,ThpCdIdTpg,ThpCdIdTps,ThpCode,ThpNameLan1,ThpNameLan2,ThpNameLan3,ThpIsCustomer,ThpIsSupplier,ThpIsCompany,ThpCreateDate,ThpModifiedDate,ThpActive,ThpImpUid,ThpRemarks,ThpImported,ThpReadonly,ThpUsIdCreated,ThpNewcode,ThpPrintLang,ThpPrintarabic,ThpIsB2b")] Thirdparty thirdparty)
        {
            if (ModelState.IsValid)
            {
                _context.Add(thirdparty);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            ViewData["ThpCdIdTpg"] = new SelectList(_context.Codescs, "CdId", "CdId", thirdparty.ThpCdIdTpg);
            ViewData["ThpCdIdTps"] = new SelectList(_context.Codescs, "CdId", "CdId", thirdparty.ThpCdIdTps);
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
