using System;
using System.Collections.Generic;

namespace M_Suite.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Sales Summary
        public decimal TotalSales { get; set; }
        public decimal SalesToday { get; set; }
        public decimal SalesThisMonth { get; set; }
        
        // Transaction Counts
        public int TotalTransactions { get; set; }
        public int TransactionsToday { get; set; }
        
        // Recent Transactions
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        
        // Item Stats
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        
        // Top Selling Items
        public List<TopSellingItemViewModel> TopSellingItems { get; set; } = new List<TopSellingItemViewModel>();
        
        // Customer Stats
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        
        // Top Customers
        public List<TopCustomerViewModel> TopCustomers { get; set; } = new List<TopCustomerViewModel>();
        
        // Warehouse Inventory
        public List<WarehouseInventoryViewModel> InventorySummary { get; set; } = new List<WarehouseInventoryViewModel>();
        
        // Transaction Status Distribution
        public List<TransactionStatusViewModel> TransactionStatusDistribution { get; set; } = new List<TransactionStatusViewModel>();
        
        // Monthly Sales Trend
        public List<MonthlySalesViewModel> MonthlySalesTrend { get; set; } = new List<MonthlySalesViewModel>();
    }

    public class TopSellingItemViewModel
    {
        public required string ItemName { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopCustomerViewModel
    {
        public required string CustomerName { get; set; }
        public decimal PurchaseAmount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class WarehouseInventoryViewModel
    {
        public required string WarehouseName { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class TransactionStatusViewModel
    {
        public required string StatusName { get; set; }
        public int Count { get; set; }
    }

    public class MonthlySalesViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalSales { get; set; }
        
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM");
        public string YearMonth => $"{Year}-{Month:D2}";
    }
} 