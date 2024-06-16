using System;

namespace Domain.Repositories.Impl.Helpers.Extensions
{
    public static class BooleanExtensions
    {
        public static bool StringToBoolean(this string str)
        {
            string cleanValue = (str ?? "").Trim();

            if (string.Equals(cleanValue, "True", StringComparison.OrdinalIgnoreCase) ||
                (cleanValue == "1") ||
                (cleanValue == "S")
            ) return true;

            return false;
        }
    }
}