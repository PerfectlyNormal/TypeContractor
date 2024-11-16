using System;

namespace ExampleContracts
{
	[Flags]
	public enum Permissions
	{
		None = 0,
		Read = 1,
		Write = 2,
		Execute = 4,
	}
}
