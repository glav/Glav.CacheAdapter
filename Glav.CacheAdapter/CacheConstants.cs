namespace Glav.CacheAdapter
{
	public static class CacheConstants
	{
		public const char ConfigItemPairSeparator = ';';
		public const string ConfigItemKeyValuePairSeparator = "=";
		public const string ConfigValueTrueText = "true";
		public const string ConfigValueTrueNumeric = "1";
		public const char ConfigDistributedServerSeparator = ';';
		// The item below may seem a bit oddly named, but this was the character used in the original release
		// of this product. Since there may be many people using this, I have decided to keep it in for now
		// probably until next major release, after which itwill no longer be supported in favour of the ';'
		// char defined earlier. It may seem unimportant but I need to use the ';' as a separator so it works in
		// in the cache specific provider config, where as using',' was problematic.
		public const char ConfigDistributedServerSeparatorObsolete = ',';
		public const char ConfigDistributedServerPortSeparator = ':';
	}
}
