using DotNetConfig;

namespace TypeContractor.Tool.vendor;

public static class ConfigurationExtensions
{
	private const string Section = "typecontractor";

	public static string GetStringWithFallback(this Config config, string key, string fallbackValue)
	{
		if (config.TryGetString(Section, key, out var value)) return value;
		return fallbackValue;
	}

	public static int GetNumberWithFallback(this Config config, string key, int fallbackValue)
	{
		if (config.TryGetNumber(Section, key, out var value)) return (int)value;
		return fallbackValue;
	}

	public static string? TryGetString(this Config config, string key)
	{
		if (config.TryGetString(Section, key, out var value))
			return value;

		return null;
	}

	public static string[] GetStrings(this Config config, string key)
	{
		return [.. config.GetAll(Section, key).Select(x => x.GetString())];
	}

	public static bool GetBoolean(this Config config, string key, bool fallbackValue)
	{
		if (config.TryGetBoolean(Section, key, out var value))
			return value;

		return fallbackValue;
	}

	public static T GetEnum<T>(this Config config, string key, T fallbackValue) where T : struct
	{
		if (config.TryGetString(Section, key, out var value) && Enum.TryParse<T>(value, ignoreCase: true, out var enumValue))
			return enumValue;

		return fallbackValue;
	}
}
