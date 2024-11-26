using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TypeContractor.Helpers;

internal partial class CasingHelpers
{
	[GeneratedRegex(@"(?<!^)(?=[A-Z][a-z])|(?<=(?<=[a-z])(?=[A-Z]))|(?<=(?<=[A-Z])(?=[A-Z][a-z]))")]
	private static partial Regex SeperateWordsWithRegex();

	[GeneratedRegex(@"([A-Z]+(?=[A-Z][a-z]|\d|\W|$)|\d+|[A-Z][a-z]+)")]
	private static partial Regex FindWordsRegex();

	private static readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

	public static string ToCasing(string input, Casing casing)
	{
		return casing switch
		{
			Casing.Pascal => ToPascalCase(input),
			Casing.Camel => ToCamelCase(input),
			Casing.Snake => ToSnakeCase(input),
			Casing.Kebab => ToKebabCase(input),
			_ => throw new ArgumentOutOfRangeException(nameof(casing), casing, $"{casing} is not a supported casing, supported casings are 'pascal', 'camel', 'snake' and 'kebab'")
		};
	}

	private static string ToPascalCase(string input) => input;

	private static string ToCamelCase(string input)
	{
		var words = FindWordsRegex().Matches(input);

		if (words.Count == 0)
			return input;

		var sb = new StringBuilder();
		sb.Append(words.First().Value.ToLower(_cultureInfo));

		foreach (var word in words.Skip(1))
			sb.Append(word.Value);

		return sb.ToString();
	}

	private static string ToSnakeCase(string input) => SeperateWordsWithRegex().Replace(input, "_").ToLower(_cultureInfo);

	private static string ToKebabCase(string input) => SeperateWordsWithRegex().Replace(input, "-").ToLower(_cultureInfo);
}
