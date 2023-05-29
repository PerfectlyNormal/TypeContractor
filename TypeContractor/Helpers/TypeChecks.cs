namespace TypeContractor.Helpers;

internal static class TypeChecks
{
    public static bool IsNullable(Type sourceType) => Nullable.GetUnderlyingType(sourceType) != null || sourceType.Name == "Nullable`1";

    public static bool ImplementsIEnumerable(Type sourceType)
    {
        if (sourceType.IsInterface && (sourceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || sourceType.Name == "IEnumerable`1"))
            return true;

        // We can treat a string as IEnumerable, which we don't want to happen here
        if (sourceType == typeof(string) || sourceType.FullName == "System.String")
            return false;

        foreach (Type interfaceUsed in sourceType.GetInterfaces())
            if (interfaceUsed.IsGenericType && (interfaceUsed.GetGenericTypeDefinition() == typeof(IEnumerable<>) || interfaceUsed.Name == "IEnumerable`1"))
                return true;

        return false;
    }
}
