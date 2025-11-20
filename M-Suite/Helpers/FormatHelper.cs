using System;
using System.Globalization;

namespace M_Suite.Helpers
{
    public static class FormatHelper
    {
        /// <summary>
        /// Formats a decimal value as currency
        /// </summary>
        public static string FormatCurrency(decimal? value, string currencyCode = "USD")
        {
            if (!value.HasValue)
                return "0.00";

            return value.Value.ToString("C", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a date value
        /// </summary>
        public static string FormatDate(DateTime? date, string format = "MM/dd/yyyy")
        {
            if (!date.HasValue)
                return string.Empty;

            return date.Value.ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a date and time value
        /// </summary>
        public static string FormatDateTime(DateTime? dateTime, string format = "MM/dd/yyyy HH:mm")
        {
            if (!dateTime.HasValue)
                return string.Empty;

            return dateTime.Value.ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a number with decimal places
        /// </summary>
        public static string FormatNumber(decimal? value, int decimals = 2)
        {
            if (!value.HasValue)
                return "0";

            return value.Value.ToString($"N{decimals}", CultureInfo.CurrentCulture);
        }
    }
}

