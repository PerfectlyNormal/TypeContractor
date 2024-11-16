using System.Reflection;

namespace TypeContractor.Helpers;

public static class TypeChecks
{
	private static readonly NullabilityInfoContext _nullabilityContext = new();
	private static readonly string[] _httpAttributes = [
		"Microsoft.AspNetCore.Mvc.HttpGetAttribute",
		"Microsoft.AspNetCore.Mvc.HttpPostAttribute",
		"Microsoft.AspNetCore.Mvc.HttpPutAttribute",
		"Microsoft.AspNetCore.Mvc.HttpPatchAttribute",
		"Microsoft.AspNetCore.Mvc.HttpDeleteAttribute",
		"Microsoft.AspNetCore.Mvc.HttpHeadAttribute",
		"Microsoft.AspNetCore.Mvc.HttpOptionsAttribute",
	];

	public static bool IsNullable(FieldInfo fieldInfo)
	{
		ArgumentNullException.ThrowIfNull(fieldInfo);
		return IsNullable(fieldInfo.FieldType) || _nullabilityContext.Create(fieldInfo).WriteState == NullabilityState.Nullable;
	}

	public static bool IsNullable(PropertyInfo propertyInfo)
	{
		ArgumentNullException.ThrowIfNull(propertyInfo);
		return IsNullable(propertyInfo.PropertyType) || _nullabilityContext.Create(propertyInfo).WriteState == NullabilityState.Nullable;
	}

	public static bool IsNullable(Type sourceType)
	{
		ArgumentNullException.ThrowIfNull(sourceType);
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

		foreach (var interfaceUsed in sourceType.GetInterfaces())
		{
			if (IsDictionary(interfaceUsed))
				return false;
			else if (interfaceUsed.IsGenericType && (GetGenericTypeDefinition(interfaceUsed) == typeof(IEnumerable<>) || interfaceUsed.Name == "IEnumerable`1") && !IsDictionary(interfaceUsed))
				return true;
		}

		return false;
	}

	public static bool ImplementsIDictionary(Type sourceType)
	{
		ArgumentNullException.ThrowIfNull(sourceType, nameof(sourceType));

		if (sourceType.IsInterface && IsDictionary(sourceType))
			return true;

		foreach (var interfaceUsed in sourceType.GetInterfaces())
		{
			if (IsDictionary(interfaceUsed))
				return true;
		}

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

		if (sourceType.GenericTypeArguments.Length == 0 && sourceType.BaseType is not null)
			return GetGenericType(sourceType.BaseType);

		return sourceType.GenericTypeArguments.Length > 0
			? sourceType.GenericTypeArguments.ElementAt(index)
			: throw new InvalidOperationException($"Expected {sourceType.FullName} to have a generic type argument, but unable to find any.");
	}

	public static bool IsController(Type type)
	{
		Logger.Log.Instance.LogDebug($"Checking {type.FullName}");
		ArgumentNullException.ThrowIfNull(type, nameof(type));

		if (type.FullName == "Microsoft.AspNetCore.Mvc.ControllerBase")
			return true;

		try
		{
			if (type.BaseType is not null && !type.BaseType.FullName!.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
				return IsController(type.BaseType);
		}
		catch (FileLoadException ex)
		{
			Logger.Log.Instance.LogError(ex, $"Unable to load base type for {type.FullName}");
		}

		return false;
	}

	public static bool ReturnsActionResult(MethodInfo methodInfo)
	{
		ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));

		if (methodInfo.CustomAttributes.Any(a => a.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.NonActionAttribute"))
			return false;

		if (methodInfo.ReturnType.Name is "ActionResult`1" or "ActionResult" or "IActionResult")
			return true;

		if (methodInfo.ReturnType.Name == "Task`1" && methodInfo.ReturnType.GenericTypeArguments.Any(rt => rt.Name is "ActionResult`1" or "ActionResult" or "IActionResult"))
			return true;

		return false;
	}

	public static Type? FullyUnwrappedReturnType(MethodInfo methodInfo)
	{
		ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));

		if (methodInfo.ReturnType.Name == "Task`1")
			return UnwrappedResult(methodInfo.ReturnType.GenericTypeArguments.First());

		if (methodInfo.ReturnType.Name == "ActionResult`1")
			return UnwrappedResult(methodInfo.ReturnType.GenericTypeArguments.First());

		return null;
	}

	public static Type? UnwrappedReturnType(MethodInfo methodInfo)
	{
		ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));

		if (methodInfo.ReturnType.Name == "Task`1")
			return UnwrappedReturnType(methodInfo.ReturnType.GenericTypeArguments.First());

		if (methodInfo.ReturnType.Name == "ActionResult`1")
			return methodInfo.ReturnType.GenericTypeArguments.First();

		return null;
	}

	public static Type? UnwrappedReturnType(Type type)
	{
		if (type.Name == "ActionResult`1")
			return type.GenericTypeArguments.First();

		if (type.Name == "ActionResult")
			return null;

		return type;
	}

	public static Type[] UnwrappedParameters(MethodInfo methodInfo)
	{
		ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));

		return methodInfo.GetParameters()
			.Where(p => !FromServices(p))
			.Select(p => UnwrappedResult(p.ParameterType))
			.Where(p => p is not null)
			.Cast<Type>()
			.ToArray();
	}

	private static bool FromServices(ParameterInfo p) => p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromServicesAttribute");

	public static bool IsHttpAttribute(CustomAttributeData data)
	{
		var fullName = data.AttributeType.FullName;
		if (string.IsNullOrWhiteSpace(fullName))
			return false;

		return _httpAttributes.Contains(fullName);
	}

	public static Type? UnwrappedResult(Type type)
	{
		if (type.Name == "ActionResult`1")
			return UnwrappedResult(type.GenericTypeArguments[0]);

		if (type.Name == "ActionResult")
			return null;

		if (ImplementsIEnumerable(type))
			return UnwrappedResult(type.GenericTypeArguments[0]);

		if (type.FullName!.StartsWith("System.", StringComparison.InvariantCulture))
			return null;

		return type;
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
