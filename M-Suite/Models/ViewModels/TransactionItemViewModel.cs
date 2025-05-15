// TransactionItemViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace M_Suite.Models.ViewModels
{
    public class TransactionItemViewModel
    {
        public int TsiId { get; set; }

        [Display(Name = "Transaction")]
        [Required(ErrorMessage = "Transaction is required")]
        public int TsiTsId { get; set; }

        [Display(Name = "Organization")]
        public int? TsiOrgId { get; set; }

        [Display(Name = "Transaction Organization")]
        public int? TsiTsOrgId { get; set; }

        [Display(Name = "Item")]
        public int? TsiItId { get; set; }

        [Display(Name = "Unit of Measure")]
        public int? TsiUomId { get; set; }

        [Display(Name = "Transaction Type Option")]
        public int? TsiTstoId { get; set; }

        [Display(Name = "List Price")]
        public int? TsiLpiId { get; set; }

        [Display(Name = "Parent Transaction Item")]
        public int? TsiTsiId { get; set; }

        [Display(Name = "Warehouse")]
        public int? TsiPlIdWhs { get; set; }

        [Display(Name = "Line Sequence")]
        public short TsiLineSequence { get; set; }

        [Display(Name = "Quantity")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be positive")]
        public decimal TsiQuantity { get; set; }

        [Display(Name = "Quantity 2")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be positive")]
        public decimal TsiQuantity2 { get; set; }

        [Display(Name = "Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal? TsiPrice { get; set; }

        [Display(Name = "Discount %")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public decimal? TsiDiscountPercentage { get; set; }

        [Display(Name = "Discount Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Discount must be positive")]
        public decimal? TsiDiscountAmount { get; set; }

        [Display(Name = "Remarks")]
        public string? TsiRemarks { get; set; }

        [Display(Name = "Free Comment")]
        public string? TsiFreeComment { get; set; }

        [Display(Name = "Total Amount")]
        public decimal? TsiTotalAmount { get; set; }

        // Transaction header properties
        public string? TransactionNumber { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public int? BillToCustomerId { get; set; }
        public int? ShipToCustomerId { get; set; }
        public decimal? TransactionDiscount { get; set; }
        public decimal? TransactionSubTotal => LineItems.Sum(i => i.LineTotal);
        public decimal? TransactionTotalDiscount => 
            LineItems.Sum(i => i.DiscountAmount) + 
            (TransactionSubTotal * (TransactionDiscount ?? 0) / 100);
        public decimal? TransactionFinalTotal => TransactionSubTotal - TransactionTotalDiscount;

        // Navigation properties for dropdowns
        public SelectList? Transactions { get; set; }
        public SelectList? Items { get; set; }
        public SelectList? Uoms { get; set; }
        public SelectList? Warehouses { get; set; }
        public SelectList? TransactionTypeOptions { get; set; }
        public SelectList? ListPrices { get; set; }
        public SelectList? ParentTransactionItems { get; set; }
        public SelectList? Customers { get; set; }
        public SelectList? TransactionTypes { get; set; }
        public SelectList? BusinessUnits { get; set; }

        // For line items
        public List<TransactionItemLineViewModel> LineItems { get; set; } = new List<TransactionItemLineViewModel>();

        // Convert view model to domain models
        public Transaction ToTransactionModel()
        {
            return new Transaction
            {
                TsId = TsiTsId,
                TsNumber = TransactionNumber ?? "",
                TsDate = TransactionDate,
                TsThpsIdBill = BillToCustomerId,
                TsThpsIdShip = ShipToCustomerId,
                TsDiscount = TransactionDiscount,
                TsTotalDiscount = TransactionTotalDiscount ?? 0,
                TsTotal = TransactionSubTotal ?? 0,
                TsTotalFinal = TransactionFinalTotal ?? 0
            };
        }

        public List<TransactionItem> ToTransactionItemModels()
        {
            List<TransactionItem> items = new List<TransactionItem>();
            
            foreach (var lineItem in LineItems)
            {
                items.Add(new TransactionItem
                {
                    TsiTsId = TsiTsId,
                    TsiItId = lineItem.TsiItId,
                    TsiUomId = lineItem.TsiUomId,
                    TsiPlIdWhs = lineItem.TsiPlIdWhs,
                    TsiLineSequence = (short)items.Count,
                    TsiQuantity = lineItem.TsiQuantity,
                    TsiQuantity2 = lineItem.TsiQuantity,
                    TsiPrice = lineItem.TsiPrice,
                    TsiDiscountPercentage = lineItem.TsiDiscountPercentage,
                    TsiDiscountAmount = lineItem.TsiDiscountAmount,
                    TsiTotalAmount = lineItem.LineTotal
                });
            }
            
            return items;
        }
    }

    public class TransactionItemLineViewModel
    {
        public int? TsiItId { get; set; }
        public string? ItemName { get; set; }
        public int? TsiUomId { get; set; }
        public string? UomName { get; set; }
        public int? TsiPlIdWhs { get; set; }
        public string? WarehouseName { get; set; }
        public decimal TsiQuantity { get; set; } = 1;
        public decimal? TsiPrice { get; set; }
        public decimal? TsiDiscountPercentage { get; set; }
        public decimal? TsiDiscountAmount { get; set; }
        public string? Remarks { get; set; }

        // Calculated properties
        public decimal SubTotal => TsiPrice.GetValueOrDefault() * TsiQuantity;
        
        public decimal DiscountAmount => 
            TsiDiscountAmount.GetValueOrDefault() + 
            (SubTotal * TsiDiscountPercentage.GetValueOrDefault() / 100);
        
        public decimal LineTotal => Math.Max(0, SubTotal - DiscountAmount);
    }
}