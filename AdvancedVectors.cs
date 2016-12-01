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

        [Setup]
        public void Setup()
        {
            var random = new Random(42);
            _haystackBytes = new byte[4096];
            random.NextBytes(_haystackBytes);
            _haystack = Encoding.ASCII.GetString(_haystackBytes);
            _needle = "Hello, earthling.";
            _needleBytes = Encoding.ASCII.GetBytes(_needle);
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
    }
}