namespace TypeContractor.Example;

public static class Program
{
    private static void Main(string[] args)
    {
        var contractor = Benchmark.Measure("Configure", () =>
            Contractor.FromDefaultConfiguration(configuration => configuration
                .AddAssembly("ExampleContracts", "ExampleContracts.dll")
                .SetOutputDirectory(Path.Combine(Directory.GetCurrentDirectory(), "output")))
        );

        // FIXME: Cleanup while debugging
#if DEBUG
        Benchmark.Measure("Cleanup", () =>
        {
            var configuration = contractor.Configuration;
            if (Directory.Exists(configuration.OutputPath))
            {
                Directory.Delete(configuration.OutputPath, true);
                Directory.CreateDirectory(configuration.OutputPath);
            }
        });
#endif

        Benchmark.Measure("Build", () => contractor.Build());
    }
}
