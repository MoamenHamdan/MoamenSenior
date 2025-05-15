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
    public class ItemController : Controller
    {
        private readonly MSuiteContext _context;

        public ItemController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: Item
        public async Task<IActionResult> Index()
        {
            var mSuiteContext = _context.Items.Include(i => i.ItCdIdIbdNavigation).Include(i => i.ItCdIdIgpNavigation).Include(i => i.ItCdIdIsgNavigation).Include(i => i.ItCdIdItgNavigation).Include(i => i.ItCdIdItpNavigation).Include(i => i.ItIt).Include(i => i.ItUom);
            return View(await mSuiteContext.ToListAsync());
        }

        // GET: Item/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.ItCdIdIbdNavigation)
                .Include(i => i.ItCdIdIgpNavigation)
                .Include(i => i.ItCdIdIsgNavigation)
                .Include(i => i.ItCdIdItgNavigation)
                .Include(i => i.ItCdIdItpNavigation)
                .Include(i => i.ItIt)
                .Include(i => i.ItUom)
                .FirstOrDefaultAsync(m => m.ItId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Item/Create
        public IActionResult Create()
        {
            ViewData["ItCdIdIbd"] = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1");
            ViewData["ItCdIdIgp"] = new SelectList(_context.Codescs, "CdId", "CdCode");
            ViewData["ItCdIdIsg"] = new SelectList(_context.Codescs, "CdId", "CdFcCode");
            ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdNum1");
            ViewData["ItCdIdItp"] = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1");
            ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItDescriptionLan1");
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomNameLan1");
            return View();
        }

        // POST: Item/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItId,ItItId,ItUomId,ItCdIdItg,ItCdIdIbd,ItCdIdIgp,ItCdIdIsg,ItCdIdItp,ItCode,ItDescriptionLan1,ItDescriptionLan2,ItDescriptionLan3,ItWeight,ItHasLot,ItHasProductionDate,ItHasExpiryDate,ItHasMultipleUom,ItHasSerial,ItIsDescription,ItIsSaleable,ItIsService,ItIsAsset,ItActive,ItImpUid,ItOrder,ItIsBadReturn")] Item item)
        {
          
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            ViewData["ItCdIdIbd"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIbd);
            ViewData["ItCdIdIgp"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIgp);
            ViewData["ItCdIdIsg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIsg);
            ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
            ViewData["ItCdIdItp"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItp);
            ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItId", item.ItItId);
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomId", item.ItUomId);
            return View(item);
        }

        // GET: Item/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["ItCdIdIbd"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIbd);
            ViewData["ItCdIdIgp"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIgp);
            ViewData["ItCdIdIsg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIsg);
            ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
            ViewData["ItCdIdItp"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItp);
            ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItId", item.ItItId);
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomId", item.ItUomId);
            return View(item);
        }

        // POST: Item/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItId,ItItId,ItUomId,ItCdIdItg,ItCdIdIbd,ItCdIdIgp,ItCdIdIsg,ItCdIdItp,ItCode,ItDescriptionLan1,ItDescriptionLan2,ItDescriptionLan3,ItWeight,ItHasLot,ItHasProductionDate,ItHasExpiryDate,ItHasMultipleUom,ItHasSerial,ItIsDescription,ItIsSaleable,ItIsService,ItIsAsset,ItActive,ItImpUid,ItOrder,ItIsBadReturn")] Item item)
        {
            if (id != item.ItId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.ItId))
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
            ViewData["ItCdIdIbd"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIbd);
            ViewData["ItCdIdIgp"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIgp);
            ViewData["ItCdIdIsg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdIsg);
            ViewData["ItCdIdItg"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItg);
            ViewData["ItCdIdItp"] = new SelectList(_context.Codescs, "CdId", "CdId", item.ItCdIdItp);
            ViewData["ItItId"] = new SelectList(_context.Items, "ItId", "ItId", item.ItItId);
            ViewData["ItUomId"] = new SelectList(_context.Uoms, "UomId", "UomId", item.ItUomId);
            return View(item);
        }

        // GET: Item/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.ItCdIdIbdNavigation)
                .Include(i => i.ItCdIdIgpNavigation)
                .Include(i => i.ItCdIdIsgNavigation)
                .Include(i => i.ItCdIdItgNavigation)
                .Include(i => i.ItCdIdItpNavigation)
                .Include(i => i.ItIt)
                .Include(i => i.ItUom)
                .FirstOrDefaultAsync(m => m.ItId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Item/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItId == id);
        }
    }
}
