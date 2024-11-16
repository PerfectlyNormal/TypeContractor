using System.Reflection;

namespace TypeContractor.Helpers;

internal static class EnumHelpers
{
	public static Array GetEnumValuesAsUnderlyingType(this Type enumType)
	{
		ArgumentNullException.ThrowIfNull(enumType);
		if (!enumType.IsEnum)
			throw new ArgumentException("Argument must be an enum");

		FieldInfo[] enumFields = enumType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
		int numValues = enumFields.Length;
		Array ret = Type.GetTypeCode(enumType.GetEnumUnderlyingType()) switch
		{
			TypeCode.Byte => new byte[numValues],
			TypeCode.SByte => new sbyte[numValues],
			TypeCode.UInt16 => new ushort[numValues],
			TypeCode.Int16 => new short[numValues],
			TypeCode.UInt32 => new uint[numValues],
			TypeCode.Int32 => new int[numValues],
			TypeCode.UInt64 => new ulong[numValues],
			TypeCode.Int64 => new long[numValues],
			_ => throw new NotSupportedException(),
		};

		for (int i = 0; i < numValues; i++)
		{
			ret.SetValue(enumFields[i].GetRawConstantValue(), i);
		}

		return ret;
	}
}
