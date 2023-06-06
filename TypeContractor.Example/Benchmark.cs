using System.Diagnostics;

namespace TypeContractor.Example;

internal class Benchmark
{
    private static int _indentLevel = -1;
    private readonly Stopwatch _stopwatch = new();
    private readonly string _label;

    public static Benchmark Start(string label, bool writeInitial = false) => new Benchmark(label).Start(writeInitial);
    public static T Measure<T>(string label, Func<T> action) => new Benchmark(label).MeasureImpl(action);
    public static void Measure(string label, Action action) => new Benchmark(label).MeasureImpl(action);

    public Benchmark(string label)
    {
        _label = label;
    }

    public Benchmark Start(bool writeInitial = false)
    {
        _indentLevel++;
        if (writeInitial) Console.WriteLine($"{Indent}[START] {_label}");
        _stopwatch.Start();
        return this;
    }

    public T MeasureImpl<T>(Func<T> action)
    {
        Start();
        var result = action();
        Stop();
        return result;
    }


    public void MeasureImpl(Action action)
    {
        Start();
        action();
        Stop();
    }

    public void Stop()
    {
        _stopwatch.Stop();
        Console.WriteLine($"{Indent}[{_stopwatch.ElapsedMilliseconds,3}ms] {_label}");
        _indentLevel--;
    }

    private static string Indent => new(' ', _indentLevel * 4);
}
