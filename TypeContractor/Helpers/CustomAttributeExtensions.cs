using System.Reflection;
using TypeContractor.Logger;

namespace TypeContractor.Helpers;

internal static class CustomAttributeExtensions
{
	internal static bool HasCustomAttribute(this PropertyInfo propertyInfo, string attributeType)
		=> propertyInfo.GetCustomAttribute(attributeType) is not null;

	internal static bool HasCustomAttribute(this MemberInfo memberInfo, string attributeType)
		=> memberInfo.GetCustomAttribute(attributeType) is not null;

	internal static bool HasCustomAttribute(this ParameterInfo parameterInfo, string attributeType)
		=> parameterInfo.GetCustomAttribute(attributeType) is not null;

	internal static CustomAttributeData? GetCustomAttribute(this PropertyInfo propertyInfo, string attributeType)
	{
		try
		{
			return propertyInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == attributeType);
		}
		catch (FileLoadException ex)
		{
			Log.Instance.LogError(ex, $"Looking for custom attributes on {propertyInfo.Name} in {propertyInfo.DeclaringType!.FullName} failed");
			return null;
		}
	}

	internal static CustomAttributeData? GetCustomAttribute(this MemberInfo memberInfo, string attributeType)
	{
		try
		{
			return memberInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == attributeType);
		}
		catch (FileLoadException ex)
		{
			Log.Instance.LogError(ex, $"Looking for custom attributes on {memberInfo.Name} in {memberInfo.DeclaringType!.FullName} failed");
			return null;
		}
	}

	internal static CustomAttributeData? GetCustomAttribute(this ParameterInfo parameterInfo, string attributeType)
	{
		try
		{
			return parameterInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == attributeType);
		}
		catch (FileLoadException ex)
		{
			Log.Instance.LogError(ex, $"Looking for custom attributes on {parameterInfo.Name} in {parameterInfo.Member.DeclaringType!.FullName} failed");
			return null;
		}
	}
}
