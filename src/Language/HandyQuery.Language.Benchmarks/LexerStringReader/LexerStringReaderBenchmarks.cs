using BenchmarkDotNet.Attributes;

namespace HandyQuery.Language.Benchmarks.LexerStringReader
{
    public static class LexerStringReaderBenchmarks
    {
        public class HeapAllocVsNoHeapAlloc
        {
            private V1.LexerStringReader _v1;
            private V2.LexerStringReader _v2;

            [Params("15151.2105 abc", "1.2", "1", "test", "151511235345345.2105 abc")]
            public string Query;

            [GlobalSetup]
            public void GlobalSetup()
            {
                _v1 = new V1.LexerStringReader(Query, 0);
                _v2 = new V2.LexerStringReader(Query, 0);
            }

            [Benchmark]
            public string HeapAlloc() => _v1.ReadTillEndOfNumber('.');

            [Benchmark]
            public string NoHeapAlloc() => _v2.ReadTillEndOfNumber('.');
        }
    }
}