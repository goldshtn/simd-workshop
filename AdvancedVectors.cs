using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace simd_workshop
{
    public class AdvancedVectors
    {
        private string _haystack;
        private string _needle;
        private byte[] _haystackBytes;
        private byte[] _needleBytes;
        private short[] _set1;
        private short[] _set2;
        private short[] _output;

        [Setup]
        public void Setup()
        {
            var random = new Random(42);
            _haystackBytes = new byte[4096];
            random.NextBytes(_haystackBytes);
            _haystack = Encoding.ASCII.GetString(_haystackBytes);
            _needle = "Hello, earthling.";
            _needleBytes = Encoding.ASCII.GetBytes(_needle);

            _set1 = new short[4096];
            _set2 = new short[4096];
            _output = new short[4096]; // At most, all elements are shared
            for (int i = 0; i < _set1.Length; ++i)
            {
                _set1[i] = (short)random.Next(short.MinValue, short.MaxValue);
                _set2[i] = (short)random.Next(short.MinValue, short.MaxValue);
            }
            Array.Sort(_set1);
            Array.Sort(_set2);
        }

        [Benchmark]
        public bool StrStr()
        {
            return _haystack.Contains(_needle);
        }

        [Benchmark]
        public bool StrStrSimd()
        {
            return str_str(_haystackBytes, _needleBytes, _haystackBytes.Length, _needleBytes.Length) != -1;
        }

        [DllImport(@"C:\dev\simd-workshop\x64\Release\native-helpers.dll",
                   CallingConvention = CallingConvention.StdCall)]
        private static extern int str_str(byte[] haystack, byte[] needle, int hsSize, int needleSize);

        [Benchmark]
        public void SetIntersect()
        {
            int i = 0, j = 0;
            int outCounter = 0;
            while (i < _set1.Length && j < _set2.Length)
            {
                if (_set1[i] < _set2[j])
                {
                    i++;
                }
                else if (_set2[j] < _set1[i])
                {
                    j++;
                }
                else
                {
                    _output[outCounter++] = _set1[i];
                    i++; j++;
                }
            }
        }

        [Benchmark]
        public void SetIntersectSimd()
        {
            set_intersect(_set1, _set2, _set1.Length, _output);
        }

        [DllImport(@"C:\dev\simd-workshop\x64\Release\native-helpers.dll",
                   CallingConvention = CallingConvention.StdCall)]
        private static extern uint set_intersect(short[] A, short[] B, int size, short[] C);
    }
}