namespace TypeContractor.Output;

public record OutputEnumMember(string SourceName, string DestinationName, object DestinationValue, ObsoleteInfo? Obsolete)
{
    public override string ToString()
    {
        return $"{DestinationName} = {DestinationValue}";
    }
}
