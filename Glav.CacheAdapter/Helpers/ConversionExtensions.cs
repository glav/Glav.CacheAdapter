namespace Glav.CacheAdapter.Helpers
{
    public static class ConversionExtensions
    {
        public static bool IsValidBoolean(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var lowerValue = value.ToLowerInvariant();
            return (lowerValue == "true" || lowerValue == "false");
        }

        public static bool ToBoolean(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var lowerValue = value.ToLowerInvariant().Trim();

            return (lowerValue == "true");
        }

        public static bool HasValue(this string value)
        {
            return (!string.IsNullOrWhiteSpace(value));
        }
    }
}
