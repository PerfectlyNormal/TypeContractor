namespace TypeContractor.Helpers;

internal static class IoHelpers
{
	internal static FileStream? WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
	{
		for (var numTries = 0; numTries < 10; numTries++)
		{
			FileStream? fs = null;
			try
			{
				fs = new FileStream(fullPath, mode, access, share);
				return fs;
			}
			catch (IOException)
			{
				fs?.Dispose();
				Thread.Sleep(50);
			}
		}

		return null;
	}
}
