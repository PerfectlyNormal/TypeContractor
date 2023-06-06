namespace TypeContractor.Output;

public record OutputProperty(string SourceName, Type SourceType, Type? InnerSourceType, string DestinationName, string DestinationType, string ImportType, bool IsBuiltin, bool IsArray, bool IsNullable)
{
    public override string ToString()
    {
        return $"{DestinationName}{(IsNullable ? "?" : "")}: {DestinationType}{(IsArray ? "[]" : "")} (import {ImportType} from {SourceType}, {(IsBuiltin ? "builtin" : "custom")})";
    }
}
