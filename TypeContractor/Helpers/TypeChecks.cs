namespace TypeContractor.Helpers;

internal static class TypeChecks
{
    public static bool IsNullable(Type sourceType) => Nullable.GetUnderlyingType(sourceType) != null || sourceType.Name == "Nullable`1";

    public static bool ImplementsIEnumerable(Type sourceType)
    {
        if (sourceType.IsInterface && (sourceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || sourceType.Name == "IEnumerable`1") && !IsDictionary(sourceType))
            return true;

        // We can treat a string as IEnumerable, which we don't want to happen here
        if (sourceType == typeof(string) || sourceType.FullName == "System.String")
            return false;

        // Dictionaries can be enumerated as well, but we don't want that here.
        if (IsDictionary(sourceType))
            return false;

        foreach (Type interfaceUsed in sourceType.GetInterfaces())
            if (IsDictionary(interfaceUsed))
                return false;
            else if (interfaceUsed.IsGenericType && (interfaceUsed.GetGenericTypeDefinition() == typeof(IEnumerable<>) || interfaceUsed.Name == "IEnumerable`1") && !IsDictionary(interfaceUsed))
                return true;

        return false;
    }

    public static bool ImplementsIDictionary(Type sourceType)
    {
        if (sourceType.IsInterface && IsDictionary(sourceType))
            return true;

        foreach (Type interfaceUsed in sourceType.GetInterfaces())
            if (IsDictionary(interfaceUsed))
                return true;

        return false;
    }

    private static bool IsDictionary(Type sourceType)
    {
        var genericTypeDefinition = (sourceType.IsGenericTypeDefinition || sourceType.IsGenericType) 
            ? sourceType.GetGenericTypeDefinition() 
            : null;

        if (genericTypeDefinition == typeof(IDictionary<,>))
            return true;

        if (genericTypeDefinition == typeof(IReadOnlyDictionary<,>))
            return true;

        if (sourceType.Name == "IDictionary`2")
            return true;

        if (sourceType.Name == "IReadOnlyDictionary`2")
            return true;

        return false;
    }

    public static Type GetGenericType(Type sourceType, int index = 0)
    {
        if (!sourceType.GenericTypeArguments.Any() && sourceType.BaseType is not null)
            return GetGenericType(sourceType.BaseType);

        if (!sourceType.GenericTypeArguments.Any())
            throw new InvalidOperationException($"Expected {sourceType.FullName} to have a generic type argument, but unable to find any.");

        return sourceType.GenericTypeArguments.ElementAt(index);
    }
}
