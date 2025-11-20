using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace M_Suite.Helpers
{
    public static class ViewHelper
    {
        /// <summary>
        /// Creates breadcrumb items for navigation
        /// </summary>
        public static List<BreadcrumbItem> CreateBreadcrumbs(string controller, string action, string? itemName = null)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Action = "Index", Controller = "Home", IsActive = false }
            };

            if (!string.IsNullOrEmpty(controller) && controller != "Home")
            {
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Text = FormatControllerName(controller),
                    Action = "Index",
                    Controller = controller,
                    IsActive = action == "Index"
                });
            }

            if (!string.IsNullOrEmpty(action) && action != "Index")
            {
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Text = FormatActionName(action, itemName),
                    Action = action,
                    Controller = controller,
                    IsActive = true
                });
            }

            return breadcrumbs;
        }

        private static string FormatControllerName(string controller)
        {
            return controller.Replace("Controller", "").Replace("_", " ");
        }

        private static string FormatActionName(string action, string? itemName)
        {
            if (!string.IsNullOrEmpty(itemName))
                return itemName;

            return action switch
            {
                "Create" => "Create New",
                "Edit" => "Edit",
                "Details" => "Details",
                "Delete" => "Delete",
                "Index" => "List",
                _ => action
            };
        }
    }

    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

