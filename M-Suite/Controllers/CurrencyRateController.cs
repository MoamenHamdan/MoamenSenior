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
    public class CurrencyRateController : Controller
    {
        private readonly MSuiteContext _context;

        public CurrencyRateController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: CurrencyRate
        public async Task<IActionResult> Index()
        {
            return View(await _context.CurrencyRates.ToListAsync());
        }

        // GET: CurrencyRate/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currencyRate = await _context.CurrencyRates
                .FirstOrDefaultAsync(m => m.CrId == id);
            if (currencyRate == null)
            {
                return NotFound();
            }

            return View(currencyRate);
        }

        // GET: CurrencyRate/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CurrencyRate/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CrId,CrDate,CrCdIdCurFrom,CrCdIdCurTo,CrBuId,CrRateBuy,CrRateSell,CrMaxRateBuy,CrMaxRateSell,CrMinRateBuy,CrMinRateSell")] CurrencyRate currencyRate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(currencyRate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(currencyRate);
        }

        // GET: CurrencyRate/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currencyRate = await _context.CurrencyRates.FindAsync(id);
            if (currencyRate == null)
            {
                return NotFound();
            }
            return View(currencyRate);
        }

        // POST: CurrencyRate/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CrId,CrDate,CrCdIdCurFrom,CrCdIdCurTo,CrBuId,CrRateBuy,CrRateSell,CrMaxRateBuy,CrMaxRateSell,CrMinRateBuy,CrMinRateSell")] CurrencyRate currencyRate)
        {
            if (id != currencyRate.CrId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(currencyRate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrencyRateExists(currencyRate.CrId))
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
            return View(currencyRate);
        }

        // GET: CurrencyRate/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currencyRate = await _context.CurrencyRates
                .FirstOrDefaultAsync(m => m.CrId == id);
            if (currencyRate == null)
            {
                return NotFound();
            }

            return View(currencyRate);
        }

        // POST: CurrencyRate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currencyRate = await _context.CurrencyRates.FindAsync(id);
            if (currencyRate != null)
            {
                _context.CurrencyRates.Remove(currencyRate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CurrencyRateExists(int id)
        {
            return _context.CurrencyRates.Any(e => e.CrId == id);
        }
    }
}
