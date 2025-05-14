// TransactionItemViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

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

        // Navigation properties for dropdowns
        public SelectList? Transactions { get; set; }
        public SelectList? Items { get; set; }
        public SelectList? Uoms { get; set; }
        public SelectList? Warehouses { get; set; }
        public SelectList? TransactionTypeOptions { get; set; }
        public SelectList? ListPrices { get; set; }
        public SelectList? ParentTransactionItems { get; set; }

        // For line items
        public List<TransactionItemLineViewModel> LineItems { get; set; } = new List<TransactionItemLineViewModel>();
    }

    public class TransactionItemLineViewModel
    {
        public int? TsiItId { get; set; }
        public int? TsiUomId { get; set; }
        public int? TsiPlIdWhs { get; set; }
        public decimal TsiQuantity { get; set; }
        public decimal? TsiPrice { get; set; }
        public decimal? TsiDiscountPercentage { get; set; }
        public decimal? TsiDiscountAmount { get; set; }
        public decimal LineTotal { get; set; }
    }
}