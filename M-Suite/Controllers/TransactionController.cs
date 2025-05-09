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
    public class TransactionController : Controller
    {
        private readonly MSuiteContext _context;

        public TransactionController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var mSuiteContext = _context.Transactions.Include(t => t.TsBu).Include(t => t.TsCdIdCmsNavigation).Include(t => t.TsCdIdCurNavigation).Include(t => t.TsCdIdSrcNavigation).Include(t => t.TsSgd).Include(t => t.TsThpsIdBillNavigation).Include(t => t.TsThpsIdShipNavigation).Include(t => t.TsTss).Include(t => t.TsTst).Include(t => t.TsUs).Include(t => t.TsVt);
            return View(await mSuiteContext.ToListAsync());
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.TsBu)
                .Include(t => t.TsCdIdCmsNavigation)
                .Include(t => t.TsCdIdCurNavigation)
                .Include(t => t.TsCdIdSrcNavigation)
                .Include(t => t.TsSgd)
                .Include(t => t.TsThpsIdBillNavigation)
                .Include(t => t.TsThpsIdShipNavigation)
                .Include(t => t.TsTss)
                .Include(t => t.TsTst)
                .Include(t => t.TsUs)
                .Include(t => t.TsVt)
                .FirstOrDefaultAsync(m => m.TsId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            ViewData["TsBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1");
            ViewData["TsCdIdCms"] = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1");
            ViewData["TsCdIdCur"] = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1");
            ViewData["TsCdIdSrc"] = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1");
            ViewData["TsSgdId"] = new SelectList(_context.SignatureDetails, "SgdId", "SgdDescription");
            ViewData["TsThpsIdBill"] = new SelectList(_context.Thirdparties, "ThpId", "ThpNameLan1");
            ViewData["TsThpsIdShip"] = new SelectList(_context.Thirdparties, "ThpId", "ThpNameLan1");
            ViewData["TsTssId"] = new SelectList(_context.Transactionstatuses, "TssId", "TssDescriptionLan1");
            ViewData["TsTstId"] = new SelectList(_context.Transactiontypes, "TstId", "TstDescriptionLan1");
            ViewData["TsUsId"] = new SelectList(_context.Users, "UsId", "UsFirstName");
            ViewData["TsVtId"] = new SelectList(_context.Visits, "VtId", "VtOperation");
            return View();
        }

        // POST: Transaction/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TsId,TsOrgId,TsUsId,TsBuId,TsVtId,TsTstId,TsTssId,TsCdIdSrc,TsCdIdCur,TsCdIdCms,TsThpsIdBill,TsThpsIdShip,TsSgdId,TsNumber,TsOurReference,TsTheirReference,TsDueDate,TsDate,TsDiscount,TsTotalDiscount,TsTotalDiscountBc,TsTotalTax,TsTotalTaxBc,TsTotal,TsTotalBc,TsRemarks,TsCreateDate,TsModifiedDate,TsCurRate,TsInvCurRate,TsUsIdCreatedby,TsPtId,TsAttribute01,TsAttribute02,TsAttribute03,TsDiscountAmount,TsUid,TsTotalFinal,TsExportedDate")] Transaction transaction)
        {
          
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            ViewData["TsBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuId", transaction.TsBuId);
            ViewData["TsCdIdCms"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdCms);
            ViewData["TsCdIdCur"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdCur);
            ViewData["TsCdIdSrc"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdSrc);
            ViewData["TsSgdId"] = new SelectList(_context.SignatureDetails, "SgdId", "SgdId", transaction.TsSgdId);
            ViewData["TsThpsIdBill"] = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsId", transaction.TsThpsIdBill);
            ViewData["TsThpsIdShip"] = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsId", transaction.TsThpsIdShip);
            ViewData["TsTssId"] = new SelectList(_context.Transactionstatuses, "TssId", "TssId", transaction.TsTssId);
            ViewData["TsTstId"] = new SelectList(_context.Transactiontypes, "TstId", "TstId", transaction.TsTstId);
            ViewData["TsUsId"] = new SelectList(_context.Users, "UsId", "UsId", transaction.TsUsId);
            ViewData["TsVtId"] = new SelectList(_context.Visits, "VtId", "VtId", transaction.TsVtId);
            return View(transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["TsBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuId", transaction.TsBuId);
            ViewData["TsCdIdCms"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdCms);
            ViewData["TsCdIdCur"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdCur);
            ViewData["TsCdIdSrc"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdSrc);
            ViewData["TsSgdId"] = new SelectList(_context.SignatureDetails, "SgdId", "SgdId", transaction.TsSgdId);
            ViewData["TsThpsIdBill"] = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsId", transaction.TsThpsIdBill);
            ViewData["TsThpsIdShip"] = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsId", transaction.TsThpsIdShip);
            ViewData["TsTssId"] = new SelectList(_context.Transactionstatuses, "TssId", "TssId", transaction.TsTssId);
            ViewData["TsTstId"] = new SelectList(_context.Transactiontypes, "TstId", "TstId", transaction.TsTstId);
            ViewData["TsUsId"] = new SelectList(_context.Users, "UsId", "UsId", transaction.TsUsId);
            ViewData["TsVtId"] = new SelectList(_context.Visits, "VtId", "VtId", transaction.TsVtId);
            return View(transaction);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TsId,TsOrgId,TsUsId,TsBuId,TsVtId,TsTstId,TsTssId,TsCdIdSrc,TsCdIdCur,TsCdIdCms,TsThpsIdBill,TsThpsIdShip,TsSgdId,TsNumber,TsOurReference,TsTheirReference,TsDueDate,TsDate,TsDiscount,TsTotalDiscount,TsTotalDiscountBc,TsTotalTax,TsTotalTaxBc,TsTotal,TsTotalBc,TsRemarks,TsCreateDate,TsModifiedDate,TsCurRate,TsInvCurRate,TsUsIdCreatedby,TsPtId,TsAttribute01,TsAttribute02,TsAttribute03,TsDiscountAmount,TsUid,TsTotalFinal,TsExportedDate")] Transaction transaction)
        {
            if (id != transaction.TsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TsId))
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
            ViewData["TsBuId"] = new SelectList(_context.BusinessUnits, "BuId", "BuId", transaction.TsBuId);
            ViewData["TsCdIdCms"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdCms);
            ViewData["TsCdIdCur"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdCur);
            ViewData["TsCdIdSrc"] = new SelectList(_context.Codescs, "CdId", "CdId", transaction.TsCdIdSrc);
            ViewData["TsSgdId"] = new SelectList(_context.SignatureDetails, "SgdId", "SgdId", transaction.TsSgdId);
            ViewData["TsThpsIdBill"] = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsId", transaction.TsThpsIdBill);
            ViewData["TsThpsIdShip"] = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsId", transaction.TsThpsIdShip);
            ViewData["TsTssId"] = new SelectList(_context.Transactionstatuses, "TssId", "TssId", transaction.TsTssId);
            ViewData["TsTstId"] = new SelectList(_context.Transactiontypes, "TstId", "TstId", transaction.TsTstId);
            ViewData["TsUsId"] = new SelectList(_context.Users, "UsId", "UsId", transaction.TsUsId);
            ViewData["TsVtId"] = new SelectList(_context.Visits, "VtId", "VtId", transaction.TsVtId);
            return View(transaction);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.TsBu)
                .Include(t => t.TsCdIdCmsNavigation)
                .Include(t => t.TsCdIdCurNavigation)
                .Include(t => t.TsCdIdSrcNavigation)
                .Include(t => t.TsSgd)
                .Include(t => t.TsThpsIdBillNavigation)
                .Include(t => t.TsThpsIdShipNavigation)
                .Include(t => t.TsTss)
                .Include(t => t.TsTst)
                .Include(t => t.TsUs)
                .Include(t => t.TsVt)
                .FirstOrDefaultAsync(m => m.TsId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TsId == id);
        }
    }
}
