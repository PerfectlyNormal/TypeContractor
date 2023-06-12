namespace TypeContractor.Helpers;

public static class TypeChecks
{
    public static bool IsNullable(Type sourceType)
    {
        ArgumentNullException.ThrowIfNull(sourceType, nameof(sourceType));
        return Nullable.GetUnderlyingType(sourceType) != null || sourceType.Name == "Nullable`1";
    }

    /// <summary>
    /// Returns <c>true</c> if the given type or any interfaces it implements is <c>IEnumerable&lt;T></c>.
    /// 
    /// <para>
    /// This only returns true for types with generics, since we need to know the inner type to correctly map it over.
    /// Non-generic enumerable collections will return false, even though they *technically* are enumerable.
    /// </para>
    /// </summary>
    /// <param name="sourceType"></param>
    /// <returns></returns>
    public static bool ImplementsIEnumerable(Type sourceType)
    {
        ArgumentNullException.ThrowIfNull(sourceType, nameof(sourceType));

        if (sourceType.IsInterface && (GetGenericTypeDefinition(sourceType) == typeof(IEnumerable<>) || sourceType.Name == "IEnumerable`1") && !IsDictionary(sourceType))
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
            else if (interfaceUsed.IsGenericType && (GetGenericTypeDefinition(interfaceUsed) == typeof(IEnumerable<>) || interfaceUsed.Name == "IEnumerable`1") && !IsDictionary(interfaceUsed))
                return true;

        return false;
    }

    public static bool ImplementsIDictionary(Type sourceType)
    {
        ArgumentNullException.ThrowIfNull(sourceType, nameof(sourceType));

        if (sourceType.IsInterface && IsDictionary(sourceType))
            return true;

        foreach (Type interfaceUsed in sourceType.GetInterfaces())
            if (IsDictionary(interfaceUsed))
                return true;

        return false;
    }

    public static bool IsValueTuple(Type sourceType)
    {
        ArgumentNullException.ThrowIfNull(sourceType, nameof(sourceType));
        return sourceType.IsGenericType && sourceType.Name.StartsWith("ValueTuple`", StringComparison.InvariantCulture);
    }

    public static Type GetGenericType(Type sourceType, int index = 0)
    {
        ArgumentNullException.ThrowIfNull(sourceType, nameof(sourceType));

        if (!sourceType.GenericTypeArguments.Any() && sourceType.BaseType is not null)
            return GetGenericType(sourceType.BaseType);

        if (!sourceType.GenericTypeArguments.Any())
            throw new InvalidOperationException($"Expected {sourceType.FullName} to have a generic type argument, but unable to find any.");

        return sourceType.GenericTypeArguments.ElementAt(index);
    }

    private static Type? GetGenericTypeDefinition(Type sourceType)
    {
        return sourceType.IsGenericType || sourceType.IsGenericTypeDefinition
            ? sourceType.GetGenericTypeDefinition()
            : null;
    }

    private static bool IsDictionary(Type sourceType)
    {
        var genericTypeDefinition = GetGenericTypeDefinition(sourceType);

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
}
