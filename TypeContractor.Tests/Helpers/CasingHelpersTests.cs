using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeContractor.Helpers;

namespace TypeContractor.Tests.Helpers;
public class CasingHelpersTests
{
	[Theory]
	[InlineData("Test")]
	[InlineData("TestString")]
	[InlineData("TestStringIsLong")]
	[InlineData("You_ShouldAlways-Send A Pascal_Case_SoThisIsWrong")]
	public void ToCasing_Does_Not_Change_Pascal_Casing(string input)
	{
		// Arrange
		// Act
		var result = CasingHelpers.ToCasing(input, Casing.Pascal);
		// Assert
		Assert.Equal(input, result);
	}

	[Theory]
	[InlineData("Test", "test")]
	[InlineData("TestString", "testString")]
	[InlineData("TestStringIsLong", "testStringIsLong")]
	[InlineData("ABBRWord", "abbrWord")]
	public void ToCasing_Turns_String_Into_Camel_Correctly(string input, string expected)
	{
		// Act
		var result = CasingHelpers.ToCasing(input, Casing.Camel);
		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("Test", "test")]
	[InlineData("TestString", "test_string")]
	[InlineData("TestStringIsLong", "test_string_is_long")]
	[InlineData("ABBRWord", "abbr_word")]
	public void ToCasing_Turns_String_Into_Snake_Correctly(string input, string expected)
	{
		// Act
		var result = CasingHelpers.ToCasing(input, Casing.Snake);
		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("Test", "test")]
	[InlineData("TestString", "test-string")]
	[InlineData("TestStringIsLong", "test-string-is-long")]
	[InlineData("ABBRWord", "abbr-word")]
	public void ToCasing_Turn_String_Into_Kebab_Correctly(string input, string expected)
	{
		// Act
		var result = CasingHelpers.ToCasing(input, Casing.Kebab);
		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]

	public void ToCasing_Throws_When_Invalid_Casing()
	{
		// Act && Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => CasingHelpers.ToCasing("A string", (Casing)int.MaxValue));
	}
}
