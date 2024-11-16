using TypeContractor.TypeScript;

namespace TypeContractor.Tests.TypeScript
{
	public class NamingExtensionsTests
	{
		[Theory]
		[InlineData("Hello", "hello")]
		[InlineData("Date", "date")]
		[InlineData("HelloWorld", "helloWorld")]
		[InlineData("MyTypeHere", "myTypeHere")]
		[InlineData("_weirdFlex", "weirdFlex")]
		[InlineData("_weirder_flex", "weirderFlex")]
		[InlineData("MyID", "myID")]
		[InlineData("IANATimeZoneName", "ianaTimeZoneName")]
		public void Converts_Input_To_Ideomatic_TypeScript(string input, string expectedOutput)
		{
			input.ToTypeScriptName().Should().Be(expectedOutput);
		}
	}
}
