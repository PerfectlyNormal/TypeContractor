using System.Globalization;
using System.Text;

namespace TypeContractor.Output;

public record OutputType(string Name, string FullName, string FileName, ContractedType ContractedType, bool IsEnum, ICollection<OutputProperty>? Properties, ICollection<OutputEnumMember>? EnumMembers)
{
	public override string ToString()
	{
		var sb = new StringBuilder();

		sb.Append(Name);
		if (IsEnum) sb.Append(" (enum)");
		sb.AppendLine(":");

		foreach (var property in Properties ?? Enumerable.Empty<OutputProperty>())
			sb.AppendFormat(CultureInfo.InvariantCulture, "    {0}\n", property);

		foreach (var member in EnumMembers ?? Enumerable.Empty<OutputEnumMember>())
			sb.AppendFormat(CultureInfo.InvariantCulture, "    {0}\n", member);

		return sb.ToString();
	}
}
