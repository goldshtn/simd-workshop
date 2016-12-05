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
        private const int STEPS = 1000000;
        private const double FROM = 0.0;
        private const double TO = 1.0;

        private static double Function(double x)
        {
            return 4.0 / (1 + x * x);
        }

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

        [Benchmark]
        public double IntegrateSequential()
        {
            Func<double, double> f = Function;
            double integral = 0.0;
            double stepSize = (TO - FROM) / STEPS;
            for (int i = 0; i < STEPS; ++i)
            {
                integral += stepSize * f(FROM + ((i + 0.5) * stepSize));
            }
            return integral;
        }

        [Benchmark]
        public double IntegrateParallelSharing()
        {
            int parallelism = Environment.ProcessorCount;
            double[] partialIntegrals = new double[parallelism];
            double stepSize = (TO - FROM) / STEPS;
            double chunkSize = (TO - FROM) / parallelism;
            int chunkSteps = STEPS / parallelism;

            Thread[] threads = new Thread[parallelism];
            for (int i = 0; i < parallelism; ++i)
            {
                double myFrom = FROM + i * chunkSize;
                double myTo = myFrom + chunkSize;
                int myIndex = i;
                threads[i] = new Thread(() =>
                {
                    Func<double, double> f = Function;
                    for (int k = 0; k < STEPS; ++k)
                    {
                        partialIntegrals[myIndex] += stepSize * f(FROM + ((k + 0.5) * stepSize));
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads) thread.Join();
            return partialIntegrals.Sum();
        }

        [Benchmark]
        public double IntegrateParallelPrivate()
        {
            int parallelism = Environment.ProcessorCount;
            double[] partialIntegrals = new double[parallelism];
            double stepSize = (TO - FROM) / STEPS;
            double chunkSize = (TO - FROM) / parallelism;
            int chunkSteps = STEPS / parallelism;

            Thread[] threads = new Thread[parallelism];
            for (int i = 0; i < parallelism; ++i)
            {
                double myFrom = FROM + i * chunkSize;
                double myTo = myFrom + chunkSize;
                int myIndex = i;
                threads[i] = new Thread(() =>
                {
                    Func<double, double> f = Function;
                    double myIntegral = 0.0;
                    for (int k = 0; k < STEPS; ++k)
                    {
                        myIntegral += stepSize * f(FROM + ((k + 0.5) * stepSize));
                    }
                    partialIntegrals[myIndex] = myIntegral;
                });
                threads[i].Start();
            }

            foreach (var thread in threads) thread.Join();
            return partialIntegrals.Sum();
        }
    }
}