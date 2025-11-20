using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace M_Suite.Models.ViewModels
{
    public class TransactionCreateViewModel
    {
        [Required(ErrorMessage = "Transaction is required")]
        public Transaction Transaction { get; set; } = new Transaction();
        
        public List<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
        
        // For dropdowns
        public SelectList? BusinessUnits { get; set; }
        public SelectList? CurrencyCodes { get; set; }
        public SelectList? SourceCodes { get; set; }
        public SelectList? Customers { get; set; }
        public SelectList? TransactionStatuses { get; set; }
        public SelectList? TransactionTypes { get; set; }
        public SelectList? Users { get; set; }
        public SelectList? Items { get; set; }
        public SelectList? Uoms { get; set; }
        public SelectList? Warehouses { get; set; }
        
        // For item entry
        public TransactionItemAddViewModel? NewItem { get; set; }
    }
    
    public class TransactionEditViewModel
    {
        public Transaction Transaction { get; set; } = new Transaction();
        
        // For dropdowns
        public SelectList? BusinessUnits { get; set; }
        public SelectList? CurrencyCodes { get; set; }
        public SelectList? SourceCodes { get; set; }
        public SelectList? Customers { get; set; }
        public SelectList? TransactionStatuses { get; set; }
        public SelectList? TransactionTypes { get; set; }
        public SelectList? Users { get; set; }
        public SelectList? Items { get; set; }
        public SelectList? Uoms { get; set; }
        public SelectList? Warehouses { get; set; }
        
        // For item entry
        public TransactionItemAddViewModel? NewItem { get; set; }
    }
    
    public class TransactionItemAddViewModel
    {
        public int TsiId { get; set; }
        public int TsiTsId { get; set; }
        
        [Display(Name = "Item")]
        [Required(ErrorMessage = "Item is required")]
        public int TsiItId { get; set; }
        
        [Display(Name = "Unit of Measure")]
        [Required(ErrorMessage = "Unit of Measure is required")]
        public int TsiUomId { get; set; }
        
        [Display(Name = "Warehouse")]
        [Required(ErrorMessage = "Warehouse is required")]
        public int? TsiPlIdWhs { get; set; }
        
        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal TsiQuantity { get; set; } = 1;
        
        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal? TsiPrice { get; set; }
        
        [Display(Name = "Discount %")]
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        public decimal? TsiDiscountPercentage { get; set; }
        
        [Display(Name = "Discount Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be non-negative")]
        public decimal? TsiDiscountAmount { get; set; }
        
        [Display(Name = "Remarks")]
        public string? TsiRemarks { get; set; }
        public string? TsiFreeComment { get; set; }
        
        // For display purposes
        public string TransactionNumber { get; set; } = string.Empty;
        
        // For dropdowns
        public SelectList? Items { get; set; }
        public SelectList? Uoms { get; set; }
        public SelectList? Warehouses { get; set; }
        
        // Calculated properties
        public decimal SubTotal => (TsiPrice ?? 0) * TsiQuantity;
        
        public decimal DiscountAmount => 
            (TsiDiscountAmount ?? 0) + 
            (SubTotal * (TsiDiscountPercentage ?? 0) / 100);
        
        public decimal LineTotal => Math.Max(0, SubTotal - DiscountAmount);
    }
} 