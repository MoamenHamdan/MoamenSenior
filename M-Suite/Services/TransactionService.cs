using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M_Suite.Services
{
    public class TransactionService
    {
        private readonly MSuiteContext _context;

        public TransactionService(MSuiteContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a transaction with all its items and related entities
        /// </summary>
        public async System.Threading.Tasks.Task<Transaction> GetTransactionWithItemsAsync(int transactionId)
        {
            return await _context.Transactions
                .Include(t => t.TsBu)
                .Include(t => t.TsCdIdCmsNavigation)
                .Include(t => t.TsCdIdCurNavigation)
                .Include(t => t.TsCdIdSrcNavigation)
                .Include(t => t.TsThpsIdBillNavigation)
                .Include(t => t.TsThpsIdShipNavigation)
                .Include(t => t.TsTss)
                .Include(t => t.TsTst)
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.TsiIt)
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.TsiUom)
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.TsiPlIdWhsNavigation)
                .FirstOrDefaultAsync(t => t.TsId == transactionId);
        }

        /// <summary>
        /// Adds items to a transaction and recalculates totals
        /// </summary>
        public async System.Threading.Tasks.Task<bool> AddItemsToTransactionAsync(int transactionId, List<TransactionItem> items)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            
            if (transaction == null)
                return false;
            
            // Set transaction ID for all items
            foreach (var item in items)
            {
                item.TsiTsId = transactionId;
                // Set line sequence
                item.TsiLineSequence = await GetNextLineSequenceAsync(transactionId);
                // Calculate item total
                CalculateItemTotal(item);
                _context.TransactionItems.Add(item);
            }
            
            await _context.SaveChangesAsync();
            
            // Recalculate transaction totals
            await RecalculateTransactionTotalsAsync(transactionId);
            
            return true;
        }

        /// <summary>
        /// Removes an item from a transaction and recalculates totals
        /// </summary>
        public async System.Threading.Tasks.Task<bool> RemoveItemFromTransactionAsync(int transactionId, int itemId)
        {
            var item = await _context.TransactionItems
                .FirstOrDefaultAsync(ti => ti.TsiId == itemId && ti.TsiTsId == transactionId);
            
            if (item == null)
                return false;
            
            _context.TransactionItems.Remove(item);
            await _context.SaveChangesAsync();
            
            // Recalculate transaction totals
            await RecalculateTransactionTotalsAsync(transactionId);
            
            return true;
        }

        /// <summary>
        /// Updates an item in a transaction and recalculates totals
        /// </summary>
        public async System.Threading.Tasks.Task<bool> UpdateTransactionItemAsync(TransactionItem item)
        {
            var existingItem = await _context.TransactionItems.FindAsync(item.TsiId);
            
            if (existingItem == null)
                return false;
            
            // Update item fields
            existingItem.TsiItId = item.TsiItId;
            existingItem.TsiUomId = item.TsiUomId;
            existingItem.TsiPlIdWhs = item.TsiPlIdWhs;
            existingItem.TsiQuantity = item.TsiQuantity;
            existingItem.TsiQuantity2 = item.TsiQuantity2;
            existingItem.TsiPrice = item.TsiPrice;
            existingItem.TsiDiscountPercentage = item.TsiDiscountPercentage;
            existingItem.TsiDiscountAmount = item.TsiDiscountAmount;
            existingItem.TsiRemarks = item.TsiRemarks;
            existingItem.TsiFreeComment = item.TsiFreeComment;
            
            // Calculate item total
            CalculateItemTotal(existingItem);
            
            _context.TransactionItems.Update(existingItem);
            await _context.SaveChangesAsync();
            
            // Recalculate transaction totals
            await RecalculateTransactionTotalsAsync(existingItem.TsiTsId);
            
            return true;
        }

        /// <summary>
        /// Recalculates all transaction totals based on its items
        /// </summary>
        public async System.Threading.Tasks.Task RecalculateTransactionTotalsAsync(int transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .FirstOrDefaultAsync(t => t.TsId == transactionId);
            
            if (transaction == null)
                return;

            // Calculate subtotal (sum of all item totals)
            decimal subtotal = transaction.TransactionItems.Sum(i => i.TsiTotalAmount ?? 0);
            
            // Calculate total discount
            decimal totalDiscount = transaction.TransactionItems.Sum(i => 
                i.TsiDiscountAmount ?? 
                (i.TsiPrice ?? 0) * (i.TsiQuantity) * (i.TsiDiscountPercentage ?? 0) / 100);
            
            // Apply transaction level discount if specified
            if (transaction.TsDiscount.HasValue && transaction.TsDiscount > 0)
            {
                totalDiscount += subtotal * (transaction.TsDiscount.Value / 100);
            }
            
            // Set transaction totals
            transaction.TsTotalDiscount = totalDiscount;
            transaction.TsTotal = subtotal;
            transaction.TsTotalFinal = subtotal - totalDiscount;
            
            _context.Update(transaction);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates total for a single transaction item
        /// </summary>
        private void CalculateItemTotal(TransactionItem item)
        {
            if (item.TsiPrice.HasValue && item.TsiQuantity > 0)
            {
                decimal lineTotal = item.TsiPrice.Value * item.TsiQuantity;
                
                // Apply discount percentage if specified
                if (item.TsiDiscountPercentage.HasValue && item.TsiDiscountPercentage > 0)
                {
                    lineTotal -= lineTotal * (item.TsiDiscountPercentage.Value / 100);
                }
                
                // Apply direct discount amount if specified
                if (item.TsiDiscountAmount.HasValue && item.TsiDiscountAmount > 0)
                {
                    lineTotal -= item.TsiDiscountAmount.Value;
                }
                
                item.TsiTotalAmount = lineTotal;
            }
            else
            {
                item.TsiTotalAmount = 0;
            }
        }

        /// <summary>
        /// Gets the next line sequence number for a transaction
        /// </summary>
        private async System.Threading.Tasks.Task<short> GetNextLineSequenceAsync(int transactionId)
        {
            var maxSequence = await _context.TransactionItems
                .Where(t => t.TsiTsId == transactionId)
                .Select(t => (short?)t.TsiLineSequence)
                .MaxAsync() ?? 0;
            
            return (short)(maxSequence + 1);
        }

        /// <summary>
        /// Creates a new transaction with items
        /// </summary>
        public async System.Threading.Tasks.Task<Transaction> CreateTransactionWithItemsAsync(Transaction transaction, List<TransactionItem> items)
        {
            // Set default values if not specified
            transaction.TsDate = transaction.TsDate == default ? DateTime.Now : transaction.TsDate;
            transaction.TsCreateDate = DateTime.Now;
            
            // Generate transaction number if not specified
            if (string.IsNullOrEmpty(transaction.TsNumber))
            {
                transaction.TsNumber = await GenerateTransactionNumberAsync(transaction.TsTstId);
            }
            
            // Add transaction to context
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            
            // Add items to transaction
            if (items.Any())
            {
                short lineSequence = 1;
                foreach (var item in items)
                {
                    item.TsiTsId = transaction.TsId;
                    item.TsiLineSequence = lineSequence++;
                    CalculateItemTotal(item);
                    _context.TransactionItems.Add(item);
                }
                
                await _context.SaveChangesAsync();
            }
            
            // Calculate transaction totals
            await RecalculateTransactionTotalsAsync(transaction.TsId);
            
            return transaction;
        }

        /// <summary>
        /// Generates a transaction number based on type and date
        /// </summary>
        private async System.Threading.Tasks.Task<string> GenerateTransactionNumberAsync(int transactionTypeId)
        {
            var type = await _context.Transactiontypes.FindAsync(transactionTypeId);
            string prefix = "TR";
            
            if (type != null && !string.IsNullOrEmpty(type.TstDescriptionLan1))
            {
                // Use the first 2 characters of the description as prefix
                prefix = type.TstDescriptionLan1.Length > 2 
                    ? type.TstDescriptionLan1.Substring(0, 2).ToUpper() 
                    : type.TstDescriptionLan1.ToUpper();
            }
            
            // Get current sequence for this type
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1).AddTicks(-1);
            
            var maxNumber = await _context.Transactions
                .Where(t => t.TsTstId == transactionTypeId && t.TsDate >= todayStart && t.TsDate <= todayEnd)
                .CountAsync();
            
            // Format: PREFIX-YYYYMMDD-SEQUENCE
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            string sequenceStr = (maxNumber + 1).ToString("D4");
            
            return $"{prefix}-{dateStr}-{sequenceStr}";
        }
    }
} 