using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace simd_workshop
{
    public class MinMax
    {
        private int[] _data = new int[512*1024];

        [Setup]
        public void Setup()
        {
            var random = new Random(42);
            for (int i = 0; i < _data.Length; ++i)
                _data[i] = random.Next();
        }

        [Benchmark]
        public Tuple<int, int> MinMaxNaive()
        {
            int max = int.MinValue, min = int.MaxValue;
            foreach (var i in _data)
            {
                min = Math.Min(min, i);
                max = Math.Max(max, i);
            }
            return new Tuple<int, int>(min, max);
        }

        [Benchmark]
        public Tuple<int, int> MinMaxILP()
        {
            int max1 = int.MinValue, max2 = int.MinValue, min1 = int.MaxValue, min2 = int.MaxValue;
            for (int i = 0; i < _data.Length; i += 2)
            {
                int d1 = _data[i], d2 = _data[i + 1];
                min1 = Math.Min(min1, d1);
                min2 = Math.Min(min2, d2);
                max1 = Math.Max(max1, d1);
                max2 = Math.Max(max2, d2);
            }
            return new Tuple<int, int>(Math.Min(min1, min2), Math.Max(max1, max2));
        }

        [Benchmark]
        public Tuple<int, int> MinMaxSimd()
        {
            Vector<int> vmin = new Vector<int>(int.MaxValue), vmax = new Vector<int>(int.MinValue);
            int vecSize = Vector<int>.Count;
            for (int i = 0; i < _data.Length; i += vecSize)
            {
                Vector<int> vdata = new Vector<int>(_data, i);
                Vector<int> minMask = Vector.LessThan(vdata, vmin);
                Vector<int> maxMask = Vector.GreaterThan(vdata, vmax);
                vmin = Vector.ConditionalSelect(minMask, vdata, vmin);
                vmax = Vector.ConditionalSelect(maxMask, vdata, vmax);
            }
            int min = int.MaxValue, max = int.MinValue;
            for (int i = 0; i < vecSize; ++i)
            {
                min = Math.Min(min, vmin[i]);
                max = Math.Max(max, vmax[i]);
            }
            return new Tuple<int, int>(min, max);
        }
        
        [Benchmark]
        public Tuple<int, int> MinMaxParallel()
        {
            int threads = Environment.ProcessorCount;
            int[] mins = new int[threads], maxs = new int[threads];
            int chunkSize = _data.Length / threads;
            Parallel.For(0, threads, i =>
            {
                int min = int.MaxValue, max = int.MinValue;
                int from = chunkSize * i, to = chunkSize * (i + 1);
                for (int j = from; j < to; ++j)
                {
                    min = Math.Min(min, _data[j]);
                    max = Math.Max(max, _data[j]);
                }
                mins[i] = min;
                maxs[i] = max;
            });
            return new Tuple<int, int>(mins.Min(), maxs.Max());
        }
    }
}
