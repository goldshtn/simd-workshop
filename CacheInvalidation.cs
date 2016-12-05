using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace simd_workshop
{
    public class CacheInvalidation
    {
        private const int STEPS = 10000000;
        private const double FROM = 0.0;
        private const double TO = 1.0;

        private static double Function(double x)
        {
            return 4.0 / (1 + x * x);
        }

        [Params(1, 2, 4)]
        public int Parallelism { get; set; }

        private void IntegrateHelper(Func<double, double> f, double from, double to, int steps, out double integral)
        {
            integral = 0.0;
            double stepSize = (to - from) / steps;
            for (int i = 0; i < steps; ++i)
            {
                integral += stepSize * f(from + ((i + 0.5) * stepSize));
            }
        }

        [Benchmark]
        public double IntegrateSequential()
        {
            double integral = 0.0;
            IntegrateHelper(Function, FROM, TO, STEPS, out integral);
            return integral;
        }

        [Benchmark]
        public double IntegrateParallelSharing()
        {
            double[] partialIntegrals = new double[Parallelism];
            double chunkSize = (TO - FROM) / Parallelism;
            int chunkSteps = STEPS / Parallelism;

            Thread[] threads = new Thread[Parallelism];
            for (int i = 0; i < Parallelism; ++i)
            {
                double myFrom = FROM + i * chunkSize;
                double myTo = myFrom + chunkSize;
                int myIndex = i;
                threads[i] = new Thread(() =>
                {
                    IntegrateHelper(Function, myFrom, myTo, chunkSteps, out partialIntegrals[myIndex]);
                });
                threads[i].Start();
            }

            foreach (var thread in threads) thread.Join();
            return partialIntegrals.Sum();
        }

        [Benchmark]
        public double IntegrateParallelPrivate()
        {
            double[] partialIntegrals = new double[Parallelism];
            double chunkSize = (TO - FROM) / Parallelism;
            int chunkSteps = STEPS / Parallelism;

            Thread[] threads = new Thread[Parallelism];
            for (int i = 0; i < Parallelism; ++i)
            {
                double myFrom = FROM + i * chunkSize;
                double myTo = myFrom + chunkSize;
                int myIndex = i;
                threads[i] = new Thread(() =>
                {
                    double myIntegral = 0.0;
                    IntegrateHelper(Function, myFrom, myTo, chunkSteps, out myIntegral);
                    partialIntegrals[myIndex] = myIntegral;
                });
                threads[i].Start();
            }

            foreach (var thread in threads) thread.Join();
            return partialIntegrals.Sum();
        }
    }
}
