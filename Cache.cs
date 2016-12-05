using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace simd_workshop
{
    public class Cache
    {
        private const int ROWS = 1024;
        private const int COLS = 1024;
        private float[] _image = new float[ROWS * COLS];
        private float[] _rotated = new float[ROWS * COLS];

        [Setup]
        public void Setup()
        {
            var random = new Random(42);
            for (int i = 0; i < _image.Length; ++i)
            {
                _image[i] = random.Next();
            }
        }

        [Benchmark]
        public void RotateNaive()
        {
            for (int y = 0; y < ROWS; ++y)
            {
                for (int x = 0; x < COLS; ++x)
                {
                    int from = y * COLS + x;
                    int to = x * ROWS + y;
                    _rotated[to] = _image[from];
                }
            }
        }

        [Benchmark]
        public void RotateTiled()
        {
            const int blockWidth = 8, blockHeight = 8; // TODO Need to test appropriate values
            for (int y = 0; y < ROWS; y += blockHeight)
            {
                for (int x = 0; x < COLS; x += blockWidth)
                {
                    for (int by = 0; by < blockHeight; ++by)
                    {
                        for (int bx = 0; bx < blockWidth; ++bx)
                        {
                            int from = (y + by) * COLS + (x + bx);
                            int to = (x + bx) * ROWS + (y + by);
                            _rotated[to] = _image[from];
                        }
                    }
                }
            }
        }
    }
}