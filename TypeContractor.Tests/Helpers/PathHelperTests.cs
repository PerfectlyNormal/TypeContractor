using TypeContractor.Helpers;

namespace TypeContractor.Tests.Helpers
{
	public class PathHelperTests
	{
		[Fact]
		public void Empty_Path_Returns_Current_Directory()
		{
			var result = PathHelpers.RelativePath("", "");

			result.Should().Be(".");
		}

		[Fact]
		public void Same_Path_Returns_Current_Directory()
		{
			var result = PathHelpers.RelativePath("Some.Deeply.Nested", "Some.Deeply.Nested");

			result.Should().Be(".");
		}

		[Theory]
		[InlineData("ExampleContracts", "ExampleContracts.v1")]
		public void Target_One_Level_Up_Returns_Doubledot(string target, string relative)
		{
			var result = PathHelpers.RelativePath(target, relative);

			result.Should().Be("../");
		}

		[Fact]
		public void Target_One_Level_Down_Returns_Nested()
		{
			var result = PathHelpers.RelativePath("Some.Nested.Deeper", "Some.Nested");

			result.Should().Be("./Deeper/");
		}

		[Fact]
		public void No_Common_Path_Throws()
		{
			Assert.Throws<ArgumentException>(() => PathHelpers.RelativePath("These.Are.Different", "Not.Even.Close"));
		}
	}
}
