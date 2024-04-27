using System.Globalization;
using System.Text;
using TypeContractor.Helpers;
using TypeContractor.Output;

namespace TypeContractor.TypeScript
{
    public static class ZodSchemaWriter
    {
        public const string? LibraryImport = "import { z } from 'zod';";

        public static void Write(OutputType type, StringBuilder builder)
        {
            builder.AppendLine("");

            if (type.IsEnum)
            {
                var members = (type.EnumMembers ?? Enumerable.Empty<OutputEnumMember>()).Select(x => $"\"{x.DestinationName}\"");
                builder.AppendLine(CultureInfo.InvariantCulture, $"export const {type.Name}Enum = z.enum([{string.Join(", ", members)}]);");
                builder.AppendLine(CultureInfo.InvariantCulture, $"export type {type.Name}EnumType = z.infer<typeof {type.Name}Enum>;");
            }
            else
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"export const {type.Name}Schema = z.object({{");
                foreach (var property in type.Properties ?? Enumerable.Empty<OutputProperty>())
                {
                    var output = GetZodOutputType(property) ?? "z.any()";

                    builder.AppendLine(CultureInfo.InvariantCulture, $"  {property.DestinationName}: {output},");
                }

                builder.AppendLine(CultureInfo.InvariantCulture, $"}});");
            }
        }

        public static string BuildImport(OutputProperty import)
        {
            var sourceType = import.IsNullable ? TypeChecks.GetGenericType(import.SourceType) : import.InnerSourceType ?? import.SourceType;
            var suffix = sourceType.IsEnum ? "Enum" : "Schema";
            var sourceName = sourceType.Name;
            var name = import.InnerSourceType?.Name ?? sourceName;
            return $"{name}{suffix}";
        }

        private static string? GetZodOutputType(OutputProperty property)
        {
            if (!property.IsBuiltin && property.SourceType.IsEnum)
                return $"{property.SourceType.Name}Enum";
            else if (!property.IsBuiltin && property.IsNullable)
            {
                var sourceType = TypeChecks.GetGenericType(property.SourceType);
                if (sourceType.IsEnum)
                    return $"{sourceType.Name}Enum.optional()";
            }

            string? output;
            // FIXME: Handle dictionaries better
            if (TypeChecks.ImplementsIDictionary(property.SourceType))
            {
                var keyOutput = "z.string()";
                var valueOutput = property.InnerSourceType is not null ? GetZodOutputType(property.InnerSourceType, augment: true) : "z.any()";
                output = $"z.record({keyOutput}, {valueOutput})";
            }
            else if (!property.IsBuiltin && !property.IsNullable)
            {
                var name = property.InnerSourceType?.Name ?? property.SourceType.Name;
                output = $"{name}Schema";
            }
            else if (property.IsBuiltin)
            {
                output = GetZodOutputType(property.SourceType);
            }
            else
            {
                throw new InvalidOperationException($"Unable to convert {property.SourceType.FullName}->{property.DestinationType} to a Zod schema");
            }

            if (property.IsNullable)
                output += ".optional()";
            else if (property.IsReadonly)
                output += ".readonly()";
            else if (property.IsArray)
                output = $"z.array({output})";

            return output;
        }

        private static string? GetZodOutputType(Type sourceType, bool augment = false)
        {
            string? output;

            if (IsOfType(sourceType, typeof(string)))
                output = "z.string()";
            else if (IsOfType(sourceType, typeof(Guid)))
                output = "z.string().uuid()";
            else if (IsOfType(sourceType, typeof(int), typeof(long), typeof(float), typeof(double), typeof(short), typeof(byte), typeof(decimal)))
                output = "z.number()";
            else if (IsOfType(sourceType, typeof(bool)))
                output = "z.boolean()";
            else if (IsOfType(sourceType, typeof(DateTime), typeof(DateTimeOffset)))
                output = "z.string().datetime({ offset: true })";
            else if (IsOfType(sourceType, typeof(DateOnly)))
                output = "z.string().date()";
            else if (IsOfType(sourceType, typeof(TimeOnly)))
                output = "z.string().time()";
            else if (IsOfType(sourceType, typeof(TimeSpan)))
                output = "z.string()"; // FIXME: Can assume some formatting here
            else
                output = "z.any()";

            if (augment)
                if (TypeChecks.ImplementsIEnumerable(sourceType))
                    output = $"z.array({output})";
                else if (TypeChecks.IsNullable(sourceType))
                    output += ".optional()";

            return string.IsNullOrWhiteSpace(output) ? null : output;
        }

        private static bool IsOfType(Type check, params Type[] against)
        {
            foreach (var checkAgainst in against)
                if (check.FullName == checkAgainst.FullName)
                    return true;
                else
                {
                    if (TypeChecks.ImplementsIEnumerable(check))
                        return IsOfType(TypeChecks.GetGenericType(check, 0), against);

                    var type = Nullable.GetUnderlyingType(checkAgainst) ?? checkAgainst;
                    if (type.IsValueType && typeof(Nullable<>).MakeGenericType(type).FullName == check.FullName)
                        return true;
                    else if (!type.IsValueType && type.FullName == check.FullName)
                        return true;
                }

            return false;
        }
    }
}
