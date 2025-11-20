using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using Microsoft.AspNetCore.Authorization;

namespace M_Suite.Controllers
{
    [Authorize]
    public class BusinessUnitController : Controller
    {
        private readonly MSuiteContext _context;

        public BusinessUnitController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: BusinessUnit
        public async Task<IActionResult> Index()
        {
            var maliaContext = _context.BusinessUnits.Include(b => b.BuBu).Include(b => b.BuCp);
            return View(await maliaContext.ToListAsync());
        }

        // GET: BusinessUnit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessUnit = await _context.BusinessUnits
                .Include(b => b.BuBu)
                .Include(b => b.BuCp)
                .FirstOrDefaultAsync(m => m.BuId == id);
            if (businessUnit == null)
            {
                return NotFound();
            }

            return View(businessUnit);
        }

        // GET: BusinessUnit/Create
        public IActionResult Create()
        {
            ViewData["BuBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1");
            ViewData["BuCpId"] = new SelectList(_context.Companies, "CpId", "CpNameLan1");
            return View();
        }

        // POST: BusinessUnit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BuId,BuBuId,BuCpId,BuCode,BuDescriptionLan1,BuDescriptionLan2,BuDescriptionLan3,BuPath,BuImpUid,BuLeId,BuOuId,BuOrgCode,BuPrefix")] BusinessUnit businessUnit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(businessUnit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", businessUnit.BuBuId);
            ViewData["BuCpId"] = new SelectList(_context.Companies, "CpId", "CpNameLan1", businessUnit.BuCpId);
            return View(businessUnit);
        }

        // GET: BusinessUnit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessUnit = await _context.BusinessUnits.FindAsync(id);
            if (businessUnit == null)
            {
                return NotFound();
            }
            ViewData["BuBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", businessUnit.BuBuId);
            ViewData["BuCpId"] = new SelectList(_context.Companies, "CpId", "CpNameLan1", businessUnit.BuCpId);
            return View(businessUnit);
        }

        // POST: BusinessUnit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BuId,BuBuId,BuCpId,BuCode,BuDescriptionLan1,BuDescriptionLan2,BuDescriptionLan3,BuPath,BuImpUid,BuLeId,BuOuId,BuOrgCode,BuPrefix")] BusinessUnit businessUnit)
        {
            if (id != businessUnit.BuId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(businessUnit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessUnitExists(businessUnit.BuId))
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
            ViewData["BuBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", businessUnit.BuBuId);
            ViewData["BuCpId"] = new SelectList(_context.Companies, "CpId", "CpNameLan1", businessUnit.BuCpId);
            return View(businessUnit);
        }

        // GET: BusinessUnit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessUnit = await _context.BusinessUnits
                .Include(b => b.BuBu)
                .Include(b => b.BuCp)
                .FirstOrDefaultAsync(m => m.BuId == id);
            if (businessUnit == null)
            {
                return NotFound();
            }

            return View(businessUnit);
        }

        // POST: BusinessUnit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var businessUnit = await _context.BusinessUnits.FindAsync(id);
            if (businessUnit != null)
            {
                _context.BusinessUnits.Remove(businessUnit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusinessUnitExists(int id)
        {
            return _context.BusinessUnits.Any(e => e.BuId == id);
        }
    }
}
