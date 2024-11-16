using System.Text.RegularExpressions;

namespace TypeContractor.TypeScript;

public static partial class NamingExtensions
{
	public static string ToTypeScriptName(this string sourceName)
	{
		return TypeScriptPropertyNameRegex()
			.Replace(sourceName, match => match.Groups[1].Value.ToUpperInvariant())
			.TrimStart('_')
			.ToCamelCase();
	}

	private static string ToCamelCase(this string s)
	{
		if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
			return s;

		char[] chars = s.ToCharArray();

		for (int i = 0; i < chars.Length; i++)
		{
			if (i == 1 && !char.IsUpper(chars[i]))
			{
				break;
			}

			bool hasNext = (i + 1 < chars.Length);
			if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
			{
				break;
			}

			chars[i] = char.ToLowerInvariant(chars[i]);
		}

		return new string(chars);
	}

	[GeneratedRegex("(?:^|_| +)(.)")]
	private static partial Regex TypeScriptPropertyNameRegex();
}
