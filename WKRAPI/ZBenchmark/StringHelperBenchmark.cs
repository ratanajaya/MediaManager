using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace ZBenchmark;

[InProcess]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringHelperBenchmark
{
    [Benchmark]
    public void StringSplit() {
        var source = new[] {
            new{ val = "aaa:bbb", idx = 4 },
            new{ val = "artist:wegfgn ihhiouhiho", idx = 6}
        };

        foreach(var s in source) {
            var rez = s.val.Split(':');
        }
    }

    [Benchmark]
    public void StringSubstring() {
        var source = new[] {
            new{ val = "aaa:bbb", idx = 4 },
            new{ val = "artist:wegfgn ihhiouhiho", idx = 6}
        };

        foreach(var s in source) {
            var key = s.val.Substring(0, s.idx);
            var val = s.val.Substring(s.idx + 1, s.val.Length - (s.idx + 1));
        }
    }
}