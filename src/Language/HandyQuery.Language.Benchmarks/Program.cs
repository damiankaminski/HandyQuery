using BenchmarkDotNet.Running;
using HandyQuery.Language.Benchmarks.LexerStringReader;

namespace HandyQuery.Language.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<LexerStringReaderBenchmarks.HeapAllocVsNoHeapAlloc>();
        }
    }
}