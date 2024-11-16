using System.Text;
using TypeContractor.Output;

namespace TypeContractor.Helpers;

#pragma warning disable CA1305 // Specify IFormatProvider
public static class StringBuilderExtensions
{
	public static void AppendDeprecationComment(this StringBuilder builder, ObsoleteInfo? obsoleteInfo, int indent = 2)
	{
		if (obsoleteInfo is null)
			return;

		var pad = "".PadLeft(indent);

		builder.AppendLine($"{pad}/**");
		builder.AppendLine($"{pad} * @deprecated {obsoleteInfo.Reason ?? ""}".TrimEnd());
		builder.AppendLine($"{pad} */");
	}
}
#pragma warning restore CA1305 // Specify IFormatProvider
