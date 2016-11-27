using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace simd_workshop
{
    public class SimpleVectors
    {
        private readonly float[] a = new float[4096];
        private readonly float[] b = new float[4096];
        private readonly float[] c = new float[4096];
        private readonly Point3[] pts = new Point3[4096];

        [Setup]
        public void Setup()
        {
            var rand = new Random(42);
            for (int i = 0; i < a.Length; ++i)
            {
                a[i] = b[i] = c[i] = (float)rand.NextDouble();
                pts[i] = new Point3
                {
                    X = a[i], Y = b[i], Z = c[i]
                };
            }
        }

        [Benchmark]
        public void AddVectors()
        {
            int length = a.Length;
            for (int i = 0; i < length; ++i)
                c[i] = a[i] + b[i];
        }

        [Benchmark]
        public void AddVectorsSimd()
        {
            int vectorLength = Vector<float>.Count;
            int remainder = a.Length % vectorLength, length = a.Length - remainder;
            for (int i = 0; i < length; i += vectorLength)
            {
                Vector<float> va = new Vector<float>(a, i);
                Vector<float> vb = new Vector<float>(b, i);
                Vector<float> vc = va + vb;
                vc.CopyTo(c, i);
            }
            for (int i = 0; i < remainder; ++i)
            {
                c[length + i] = a[length + i] + b[length + i];
            }
        }

        [Benchmark]
        public float DotProduct()
        {
            float sum = 0.0f;
            for (int i = 0; i < a.Length; ++i)
                sum += a[i] * b[i];
            return sum;
        }

        [Benchmark]
        public float DotProductSimd()
        {
            // TODO Implement this
            return 0.0f;
        }

        [Benchmark]
        public void MatrixMult()
        {
            for (int i = 0; i < 64; ++i)
            {
                for (int k = 0; k < 64; ++k)
                {
                    for (int j = 0; j < 64; ++j)
                    {
                        c[i * 64 + j] += a[i * 64 + k] * b[k * 64 + j];
                    }
                }
            }
        }

        [Benchmark]
        public void MatrixMultSimd()
        {
            // TODO Implement this
        }

        [Benchmark]
        public void VectorNorm()
        {
            for (int i = 0; i < pts.Length; ++i)
            {
                Point3 pt = pts[i];
                float norm = (float)Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y + pt.Z * pt.Z);
                pt.X /= norm;
                pt.Y /= norm;
                pt.Z /= norm;
                pts[i] = pt;
            }
        }

        [Benchmark]
        public void VectorNormSimd()
        {
            // TODO Implement this
        }
    }

    struct Point3
    {
        public float X;
        public float Y;
        public float Z;
    }
}
