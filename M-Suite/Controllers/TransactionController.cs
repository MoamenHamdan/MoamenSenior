using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using M_Suite.Services;
using M_Suite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace M_Suite.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly MSuiteContext _context;
        private readonly TransactionService _transactionService;

        public TransactionController(MSuiteContext context, TransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
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
                .OrderByDescending(t => t.TsDate)
                .ToListAsync();
            
            return View(transactions);
        }

        // GET: Transaction/SalesOrders
        public async Task<IActionResult> SalesOrders(int? status, string search, int page = 1)
        {
            // Default page size
            int pageSize = 10;
            
            // Base query
            var query = _context.Transactions
                .Where(t => t.TsTstId == 2) // Customer Order type
                .Include(t => t.TsBu)
                .Include(t => t.TsThpsIdBillNavigation)
                .Include(t => t.TsTss)
                .Include(t => t.TsTst)
                .OrderByDescending(t => t.TsDate)
                .AsQueryable();
            
            // Apply status filter if provided
            if (status.HasValue)
            {
                query = query.Where(t => t.TsTssId == status.Value);
            }
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(t => 
                    t.TsNumber.ToLower().Contains(search) ||
                    t.TsOurReference.ToLower().Contains(search) ||
                    t.TsTheirReference.ToLower().Contains(search) ||
                    t.TsThpsIdBillNavigation.ThpsNameLan1.ToLower().Contains(search)
                );
            }
            
            // Get total count for pagination
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            // Adjust current page if needed
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            // Apply pagination
            var salesOrders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Add values to ViewBag for the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            
            return View(salesOrders);
        }
        
        // GET: Transaction/Invoices
        public async Task<IActionResult> Invoices(int? status, string search, int page = 1)
        {
            // Default page size
            int pageSize = 10;
            
            // Base query - get transactions with TsTstId = 3 (Invoice type)
            var query = _context.Transactions
                .Where(t => t.TsTstId == 3) // Invoice type
                .Include(t => t.TsBu)
                .Include(t => t.TsThpsIdBillNavigation)
                .Include(t => t.TsTss)
                .Include(t => t.TsTst)
                .OrderByDescending(t => t.TsDate)
                .AsQueryable();
            
            // Apply status filter if provided
            if (status.HasValue)
            {
                query = query.Where(t => t.TsTssId == status.Value);
            }
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(t => 
                    t.TsNumber.ToLower().Contains(search) ||
                    t.TsOurReference.ToLower().Contains(search) ||
                    t.TsTheirReference.ToLower().Contains(search) ||
                    t.TsThpsIdBillNavigation.ThpsNameLan1.ToLower().Contains(search)
                );
            }
            
            // Get total count for pagination
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            // Adjust current page if needed
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            // Apply pagination
            var invoices = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Get order numbers map for related orders
            var orderIds = invoices.Where(i => i.TsAttribute01 != null)
                                  .Select(i => i.TsAttribute01)
                                  .Distinct()
                                  .ToList();
            
            var orderNumbers = await _context.Transactions
                .Where(t => orderIds.Contains(t.TsId.ToString()))
                .ToDictionaryAsync(o => o.TsId.ToString(), o => o.TsNumber);
            
            // Add values to ViewBag for the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.OrderNumbers = orderNumbers;
            
            return View(invoices);
        }

        // POST: Transaction/ApproveOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var order = await _context.Transactions.FindAsync(id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Ensure it's a sales order with NEW status
            if (order.TsTstId != 2 || order.TsTssId != 1000)
            {
                return BadRequest("Only NEW sales orders can be approved.");
            }
            
            // Update status to Approved (1001)
            order.TsTssId = 1001;
            order.TsModifiedDate = DateTime.Now;
            
            _context.Update(order);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(SalesOrders));
        }
        
        // POST: Transaction/RejectOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOrder(int id)
        {
            var order = await _context.Transactions.FindAsync(id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Ensure it's a sales order with NEW status
            if (order.TsTstId != 2 || order.TsTssId != 1000)
            {
                return BadRequest("Only NEW sales orders can be rejected.");
            }
            
            // Update status to Rejected (1002)
            order.TsTssId = 1002;
            order.TsModifiedDate = DateTime.Now;
            
            _context.Update(order);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(SalesOrders));
        }
        
        // POST: Transaction/CreateInvoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvoice(int orderId)
        {
            var order = await _transactionService.GetTransactionWithItemsAsync(orderId);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Ensure it's an approved sales order
            if (order.TsTstId != 2 || order.TsTssId != 1001)
            {
                return BadRequest("Only APPROVED sales orders can be converted to invoices.");
            }
            
            // Create a new invoice based on the order
            var invoice = new Transaction
            {
                TsTstId = 3, // Invoice type
                TsTssId = 1003, // Open status
                TsDate = DateTime.Now,
                TsDueDate = DateTime.Now.AddDays(30), // Default 30 days
                TsBuId = order.TsBuId,
                TsThpsIdBill = order.TsThpsIdBill,
                TsThpsIdShip = order.TsThpsIdShip,
                TsCdIdCur = order.TsCdIdCur,
                TsCdIdSrc = order.TsCdIdSrc,
                TsDiscount = order.TsDiscount,
                TsTotal = order.TsTotal,
                TsTotalDiscount = order.TsTotalDiscount,
                TsTotalFinal = order.TsTotalFinal,
                TsCreateDate = DateTime.Now,
                TsOurReference = "Order #" + order.TsNumber,
                TsTheirReference = order.TsTheirReference,
                TsRemarks = order.TsRemarks,
                TsAttribute01 = order.TsId.ToString() // Store order ID reference
            };
            
            // Copy items from order to invoice
            var invoiceItems = new List<TransactionItem>();
            foreach (var orderItem in order.TransactionItems)
            {
                var invoiceItem = new TransactionItem
                {
                    TsiItId = orderItem.TsiItId,
                    TsiUomId = orderItem.TsiUomId,
                    TsiPlIdWhs = orderItem.TsiPlIdWhs,
                    TsiQuantity = orderItem.TsiQuantity,
                    TsiQuantity2 = orderItem.TsiQuantity2,
                    TsiPrice = orderItem.TsiPrice,
                    TsiDiscountPercentage = orderItem.TsiDiscountPercentage,
                    TsiDiscountAmount = orderItem.TsiDiscountAmount,
                    TsiTotalAmount = orderItem.TsiTotalAmount,
                    TsiRemarks = orderItem.TsiRemarks,
                    TsiFreeComment = orderItem.TsiFreeComment
                };
                
                invoiceItems.Add(invoiceItem);
            }
            
            // Create the invoice with items
            var createdInvoice = await _transactionService.CreateTransactionWithItemsAsync(invoice, invoiceItems);
            
            return RedirectToAction(nameof(Details), new { id = createdInvoice.TsId });
        }
        
        // POST: Transaction/MarkAsPaid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var invoice = await _context.Transactions.FindAsync(id);
            
            if (invoice == null)
            {
                return NotFound();
            }
            
            // Ensure it's an open invoice
            if (invoice.TsTstId != 3 || invoice.TsTssId != 1003)
            {
                return BadRequest("Only OPEN invoices can be marked as paid.");
            }
            
            // Update status to Paid (1004)
            invoice.TsTssId = 1004;
            invoice.TsModifiedDate = DateTime.Now;
            
            _context.Update(invoice);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Invoices));
        }
        
        // GET: Transaction/PrintInvoice/5
        public async Task<IActionResult> PrintInvoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _transactionService.GetTransactionWithItemsAsync(id.Value);
            
            if (invoice == null || invoice.TsTstId != 3) // Ensure it's an invoice
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _transactionService.GetTransactionWithItemsAsync(id.Value);
            
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create(int? type, int? status)
        {
            var viewModel = new TransactionCreateViewModel
            {
                Transaction = new Transaction
                {
                    TsDate = DateTime.Now,
                    TsCreateDate = DateTime.Now,
                    TsTstId = type ?? 1, // Default to Regular Transaction if not specified
                    TsTssId = status ?? 1 // Default to regular status if not specified
                },
                TransactionItems = new List<TransactionItem>(),
                BusinessUnits = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1"),
                CurrencyCodes = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1"),
                SourceCodes = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1"),
                Customers = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsNameLan1"),
                TransactionStatuses = new SelectList(_context.Transactionstatuses, "TssId", "TssDescriptionLan1"),
                TransactionTypes = new SelectList(_context.Transactiontypes, "TstId", "TstDescriptionLan1"),
                Users = new SelectList(_context.Users, "UsId", "UsFirstName"),
                Items = GetItemsSelectList(),
                Uoms = new SelectList(_context.Uoms, "UomId", "UomNameLan1"),
                Warehouses = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1")
            };
            
            return View(viewModel);
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionCreateViewModel viewModel)
        {
            // Additional validation
            if (viewModel.Transaction == null)
            {
                ModelState.AddModelError("", "Transaction data is required.");
            }
            else
            {
                if (viewModel.Transaction.TsDate == default)
                {
                    viewModel.Transaction.TsDate = DateTime.Now;
                }

                if (viewModel.Transaction.TsBuId == null || viewModel.Transaction.TsBuId == 0)
                {
                    ModelState.AddModelError("Transaction.TsBuId", "Business Unit is required.");
                }

                if (viewModel.Transaction.TsTstId == null || viewModel.Transaction.TsTstId == 0)
                {
                    ModelState.AddModelError("Transaction.TsTstId", "Transaction Type is required.");
                }

                // Validate items if provided
                if (viewModel.TransactionItems != null && viewModel.TransactionItems.Any())
                {
                    for (int i = 0; i < viewModel.TransactionItems.Count; i++)
                    {
                        var item = viewModel.TransactionItems[i];
                        if (item.TsiItId == null || item.TsiItId == 0)
                        {
                            ModelState.AddModelError($"TransactionItems[{i}].TsiItId", "Item is required.");
                        }
                        if (item.TsiQuantity <= 0)
                        {
                            ModelState.AddModelError($"TransactionItems[{i}].TsiQuantity", "Quantity must be greater than 0.");
                        }
                        if (item.TsiPrice < 0)
                        {
                            ModelState.AddModelError($"TransactionItems[{i}].TsiPrice", "Price cannot be negative.");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set current user if not set
                    if (viewModel.Transaction.TsUsId == null && User.Identity?.IsAuthenticated == true)
                    {
                        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UsLogin == User.Identity.Name);
                        if (currentUser != null)
                        {
                            viewModel.Transaction.TsUsId = currentUser.UsId;
                        }
                    }

                    // Create the transaction with items
                    var transaction = await _transactionService.CreateTransactionWithItemsAsync(
                        viewModel.Transaction, 
                        viewModel.TransactionItems ?? new List<TransactionItem>());
                    
                    TempData["Success"] = "Transaction created successfully";
                    
                    // Redirect to appropriate page based on transaction type
                    if (transaction.TsTstId == 2) // Sales Order
                    {
                        return RedirectToAction(nameof(SalesOrders));
                    }
                    else if (transaction.TsTstId == 3) // Invoice
                    {
                        return RedirectToAction(nameof(Invoices));
                    }
                    else // Regular transaction
                    {
                        return RedirectToAction(nameof(Details), new { id = transaction.TsId });
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the transaction: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            viewModel.BusinessUnits = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", viewModel.Transaction?.TsBuId);
            viewModel.CurrencyCodes = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", viewModel.Transaction?.TsCdIdCur);
            viewModel.SourceCodes = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", viewModel.Transaction?.TsCdIdSrc);
            viewModel.Customers = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsNameLan1", viewModel.Transaction?.TsThpsIdBill);
            viewModel.TransactionStatuses = new SelectList(_context.Transactionstatuses, "TssId", "TssDescriptionLan1", viewModel.Transaction?.TsTssId);
            viewModel.TransactionTypes = new SelectList(_context.Transactiontypes, "TstId", "TstDescriptionLan1", viewModel.Transaction?.TsTstId);
            viewModel.Users = new SelectList(_context.Users.Where(u => u.UsDeleted == 0), "UsId", "UsLogin", viewModel.Transaction?.TsUsId);
            viewModel.Items = GetItemsSelectList();
            viewModel.Uoms = new SelectList(_context.Uoms, "UomId", "UomNameLan1");
            viewModel.Warehouses = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1");
            
            return View(viewModel);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _transactionService.GetTransactionWithItemsAsync(id.Value);
            
            if (transaction == null)
            {
                return NotFound();
            }
            
            var viewModel = new TransactionEditViewModel
            {
                Transaction = transaction,
                BusinessUnits = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", transaction.TsBuId),
                CurrencyCodes = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", transaction.TsCdIdCur),
                SourceCodes = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", transaction.TsCdIdSrc),
                Customers = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsNameLan1", transaction.TsThpsIdBill),
                TransactionStatuses = new SelectList(_context.Transactionstatuses, "TssId", "TssDescriptionLan1", transaction.TsTssId),
                TransactionTypes = new SelectList(_context.Transactiontypes, "TstId", "TstDescriptionLan1", transaction.TsTstId),
                Users = new SelectList(_context.Users, "UsId", "UsFirstName", transaction.TsUsId),
                Items = GetItemsSelectList(),
                Uoms = new SelectList(_context.Uoms, "UomId", "UomNameLan1"),
                Warehouses = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1")
            };
            
            return View(viewModel);
        }

        // POST: Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransactionEditViewModel viewModel)
        {
            if (id != viewModel.Transaction.TsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewModel.Transaction);
                    await _context.SaveChangesAsync();
                    
                    // Recalculate transaction totals
                    await _transactionService.RecalculateTransactionTotalsAsync(id);
                    
                    // Redirect to appropriate page based on transaction type
                    if (viewModel.Transaction.TsTstId == 2) // Sales Order
                    {
                        return RedirectToAction(nameof(SalesOrders));
                    }
                    else if (viewModel.Transaction.TsTstId == 3) // Invoice
                    {
                        return RedirectToAction(nameof(Invoices));
                    }
                    else // Regular transaction
                    {
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(viewModel.Transaction.TsId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            // If we got this far, something failed, redisplay form
            viewModel.BusinessUnits = new SelectList(_context.BusinessUnits, "BuId", "BuDescriptionLan1", viewModel.Transaction.TsBuId);
            viewModel.CurrencyCodes = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", viewModel.Transaction.TsCdIdCur);
            viewModel.SourceCodes = new SelectList(_context.Codescs, "CdId", "CdDescriptionLan1", viewModel.Transaction.TsCdIdSrc);
            viewModel.Customers = new SelectList(_context.ThirdpartySites, "ThpsId", "ThpsNameLan1", viewModel.Transaction.TsThpsIdBill);
            viewModel.TransactionStatuses = new SelectList(_context.Transactionstatuses, "TssId", "TssDescriptionLan1", viewModel.Transaction.TsTssId);
            viewModel.TransactionTypes = new SelectList(_context.Transactiontypes, "TstId", "TstDescriptionLan1", viewModel.Transaction.TsTstId);
            viewModel.Users = new SelectList(_context.Users, "UsId", "UsFirstName", viewModel.Transaction.TsUsId);
            viewModel.Items = GetItemsSelectList();
            viewModel.Uoms = new SelectList(_context.Uoms, "UomId", "UomNameLan1");
            viewModel.Warehouses = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1");
            
            return View(viewModel);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _transactionService.GetTransactionWithItemsAsync(id.Value);
            
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
            // Get transaction to check type
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            
            // Store transaction type for redirection
            int transactionType = transaction.TsTstId;
            
            // Delete related transaction items first
            var items = await _context.TransactionItems
                .Where(ti => ti.TsiTsId == id)
                .ToListAsync();
            
            _context.TransactionItems.RemoveRange(items);
            
            // Now delete the transaction
            _context.Transactions.Remove(transaction);

            await _context.SaveChangesAsync();
            
            // Redirect to appropriate page based on transaction type
            if (transactionType == 2) // Sales Order
            {
                return RedirectToAction(nameof(SalesOrders));
            }
            else if (transactionType == 3) // Invoice
            {
                return RedirectToAction(nameof(Invoices));
            }
            else // Regular transaction
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Transaction/AddItem/5
        public async Task<IActionResult> AddItem(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            
            var viewModel = new TransactionItemAddViewModel
            {
                TsiTsId = id,
                TransactionNumber = transaction.TsNumber,
                Items = GetItemsSelectList(),
                Uoms = new SelectList(_context.Uoms, "UomId", "UomNameLan1"),
                Warehouses = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1")
            };
            
            return View(viewModel);
        }

        // POST: Transaction/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(TransactionItemAddViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var item = new TransactionItem
                {
                    TsiTsId = viewModel.TsiTsId,
                    TsiItId = viewModel.TsiItId,
                    TsiUomId = viewModel.TsiUomId,
                    TsiPlIdWhs = viewModel.TsiPlIdWhs,
                    TsiQuantity = viewModel.TsiQuantity,
                    TsiQuantity2 = viewModel.TsiQuantity,
                    TsiPrice = viewModel.TsiPrice,
                    TsiDiscountPercentage = viewModel.TsiDiscountPercentage,
                    TsiDiscountAmount = viewModel.TsiDiscountAmount,
                    TsiRemarks = viewModel.TsiRemarks
                };
                
                await _transactionService.AddItemsToTransactionAsync(viewModel.TsiTsId, new List<TransactionItem> { item });
                
                // Get transaction to check type
                var transaction = await _context.Transactions.FindAsync(viewModel.TsiTsId);
                
                // Redirect to appropriate details page based on transaction type
                if (transaction.TsTstId == 2) // Sales Order
                {
                    return RedirectToAction(nameof(Details), new { id = viewModel.TsiTsId });
                }
                else if (transaction.TsTstId == 3) // Invoice
                {
                    return RedirectToAction(nameof(Details), new { id = viewModel.TsiTsId });
                }
                else // Regular transaction
                {
                    return RedirectToAction(nameof(Details), new { id = viewModel.TsiTsId });
                }
            }
            
            // If we got this far, something failed, redisplay form
            viewModel.Items = GetItemsSelectList(viewModel.TsiItId);
            viewModel.Uoms = new SelectList(_context.Uoms, "UomId", "UomNameLan1", viewModel.TsiUomId);
            viewModel.Warehouses = new SelectList(_context.PhysicalLocations, "PlId", "PlDescriptionLan1", viewModel.TsiPlIdWhs);
            
            return View(viewModel);
        }

        // POST: Transaction/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int transactionId, int itemId)
        {
            await _transactionService.RemoveItemFromTransactionAsync(transactionId, itemId);
            
            // Get transaction to check type
            var transaction = await _context.Transactions.FindAsync(transactionId);
            
            // Redirect to appropriate details page based on transaction type
            return RedirectToAction(nameof(Details), new { id = transactionId });
        }

        // Helper methods
        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TsId == id);
        }
        
        private SelectList GetItemsSelectList(int? selectedId = null)
        {
            // Get the base data from the database
            var items = _context.Items
                .Where(i => i.ItActive == 1 && i.ItIsSaleable == 1)
                .AsEnumerable() // Switch to client evaluation
                .Select(i => new 
                { 
                    i.ItId, 
                    Name = $"{i.ItCode} - {i.ItDescriptionLan1}" 
                })
                .OrderBy(i => i.Name)
                .ToList();
                
            return new SelectList(items, "ItId", "Name", selectedId);
        }
    }
}
