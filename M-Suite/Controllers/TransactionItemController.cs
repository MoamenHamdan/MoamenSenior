// TransactionItemController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using M_Suite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace M_Suite.Controllers
{
    [Authorize]
    public class TransactionItemController : Controller
    {
        private readonly MSuiteContext _context;

        public TransactionItemController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: TransactionItem
        public async Task<IActionResult> Index()
        {
            var items = await _context.TransactionItems
                .Include(t => t.TsiIt)
                .Include(t => t.TsiTs)
                .Include(t => t.TsiUom)
                .Include(t => t.TsiPlIdWhsNavigation)
                .ToListAsync();

            return View(items);
        }

        // GET: TransactionItem/Create
        public IActionResult Create()
        {
            var viewModel = new TransactionItemViewModel
            {
                Transactions = new SelectList(_context.Transactions, "TsId", "TsNumber"),
                Items = new SelectList(_context.Items, "ItId", "ItName"),
                Uoms = new SelectList(_context.Uoms, "UomId", "UomName"),
                Warehouses = new SelectList(_context.PhysicalLocations, "PlId", "PlName"),
                TransactionTypeOptions = new SelectList(_context.TransactiontypeOptions, "TstoId", "TstoDescription"),
                ListPrices = new SelectList(_context.Listprices, "LpId", "LpDescription"),
                ParentTransactionItems = new SelectList(_context.TransactionItems, "TsiId", "TsiId")
            };

            // Add one empty line item by default
            viewModel.LineItems.Add(new TransactionItemLineViewModel());

            return View(viewModel);
        }

        // POST: TransactionItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionItemViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save main transaction item
                    var transactionItem = new TransactionItem
                    {
                        TsiTsId = viewModel.TsiTsId,
                        TsiOrgId = viewModel.TsiOrgId,
                        TsiTsOrgId = viewModel.TsiTsOrgId,
                        TsiItId = viewModel.TsiItId,
                        TsiUomId = viewModel.TsiUomId,
                        TsiTstoId = viewModel.TsiTstoId,
                        TsiLpiId = viewModel.TsiLpiId,
                        TsiTsiId = viewModel.TsiTsiId,
                        TsiPlIdWhs = viewModel.TsiPlIdWhs,
                        TsiLineSequence = viewModel.TsiLineSequence,
                        TsiQuantity = viewModel.TsiQuantity,
                        TsiQuantity2 = viewModel.TsiQuantity2,
                        TsiPrice = viewModel.TsiPrice,
                        TsiDiscountPercentage = viewModel.TsiDiscountPercentage,
                        TsiDiscountAmount = viewModel.TsiDiscountAmount,
                        TsiRemarks = viewModel.TsiRemarks,
                        TsiFreeComment = viewModel.TsiFreeComment,
                        TsiTotalAmount = viewModel.TsiTotalAmount
                    };

                    _context.Add(transactionItem);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving: " + ex.Message);
                }
            }

            // Repopulate dropdowns if validation fails
            viewModel.Transactions = new SelectList(_context.Transactions, "TsId", "TsNumber", viewModel.TsiTsId);
            viewModel.Items = new SelectList(_context.Items, "ItId", "ItName", viewModel.TsiItId);
            viewModel.Uoms = new SelectList(_context.Uoms, "UomId", "UomName", viewModel.TsiUomId);
            viewModel.Warehouses = new SelectList(_context.PhysicalLocations, "PlId", "PlName", viewModel.TsiPlIdWhs);
            viewModel.TransactionTypeOptions = new SelectList(_context.TransactiontypeOptions, "TstoId", "TstoDescription", viewModel.TsiTstoId);
            viewModel.ListPrices = new SelectList(_context.Listprices, "LpId", "LpDescription", viewModel.TsiLpiId);
            viewModel.ParentTransactionItems = new SelectList(_context.TransactionItems, "TsiId", "TsiId", viewModel.TsiTsiId);

            return View(viewModel);
        }

        // GET: TransactionItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionItem = await _context.TransactionItems.FindAsync(id);
            if (transactionItem == null)
            {
                return NotFound();
            }
            ViewData["TsiItId"] = new SelectList(_context.Items, "ItId", "ItId", transactionItem.TsiItId);
            ViewData["TsiLpiId"] = new SelectList(_context.Listprices, "LpId", "LpId", transactionItem.TsiLpiId);
            ViewData["TsiPlIdWhs"] = new SelectList(_context.PhysicalLocations, "PlId", "PlId", transactionItem.TsiPlIdWhs);
            ViewData["TsiTsId"] = new SelectList(_context.Transactions, "TsId", "TsId", transactionItem.TsiTsId);
            ViewData["TsiTsiId"] = new SelectList(_context.TransactionItems, "TsiId", "TsiId", transactionItem.TsiTsiId);
            ViewData["TsiTstoId"] = new SelectList(_context.TransactiontypeOptions, "TstoId", "TstoId", transactionItem.TsiTstoId);
            ViewData["TsiUomId"] = new SelectList(_context.Uoms, "UomId", "UomId", transactionItem.TsiUomId);
            return View(transactionItem);
        }

        // POST: TransactionItem/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TsiId,TsiOrgId,TsiTsOrgId,TsiTsId,TsiItId,TsiUomId,TsiTstoId,TsiLpiId,TsiTsiId,TsiPlIdWhs,TsiLineSequence,TsiQuantity,TsiQuantity2,TsiPrice,TsiDiscountPercentage,TsiDiscountAmount,TsiRemarks,TsiPriceChanged,TsiTotalDiscount,TsiTotalTax,TsiTotalAmountBc,TsiTotalDiscountBc,TsiTotalTaxBc,TsiCdIdRett,TsiFreeComment,TsiCdIdFrc,TsiThpsDiscount,TsiTotalAmount")] TransactionItem transactionItem)
        {
            if (id != transactionItem.TsiId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionItemExists(transactionItem.TsiId))
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
            ViewData["TsiItId"] = new SelectList(_context.Items, "ItId", "ItId", transactionItem.TsiItId);
            ViewData["TsiLpiId"] = new SelectList(_context.Listprices, "LpId", "LpId", transactionItem.TsiLpiId);
            ViewData["TsiPlIdWhs"] = new SelectList(_context.PhysicalLocations, "PlId", "PlId", transactionItem.TsiPlIdWhs);
            ViewData["TsiTsId"] = new SelectList(_context.Transactions, "TsId", "TsId", transactionItem.TsiTsId);
            ViewData["TsiTsiId"] = new SelectList(_context.TransactionItems, "TsiId", "TsiId", transactionItem.TsiTsiId);
            ViewData["TsiTstoId"] = new SelectList(_context.TransactiontypeOptions, "TstoId", "TstoId", transactionItem.TsiTstoId);
            ViewData["TsiUomId"] = new SelectList(_context.Uoms, "UomId", "UomId", transactionItem.TsiUomId);
            return View(transactionItem);
        }

        // GET: TransactionItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionItem = await _context.TransactionItems
                .Include(t => t.TsiIt)
                .Include(t => t.TsiLpi)
                .Include(t => t.TsiPlIdWhsNavigation)
                .Include(t => t.TsiTs)
                .Include(t => t.TsiTsi)
                .Include(t => t.TsiTsto)
                .Include(t => t.TsiUom)
                .FirstOrDefaultAsync(m => m.TsiId == id);
            if (transactionItem == null)
            {
                return NotFound();
            }

            return View(transactionItem);
        }

        // POST: TransactionItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transactionItem = await _context.TransactionItems.FindAsync(id);
            if (transactionItem != null)
            {
                _context.TransactionItems.Remove(transactionItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionItemExists(int id)
        {
            return _context.TransactionItems.Any(e => e.TsiId == id);
        }
    }
}
