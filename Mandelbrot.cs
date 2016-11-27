using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace simd_workshop
{
    public class Mandelbrot
    {
        private const float MIN_X = -2;
        private const float MAX_X = 1;
        private const float MIN_Y = -1;
        private const float MAX_Y = 1;

        private const int ITERATIONS = 1000;
        private static readonly float THRESHOLD = (float)Math.Sqrt(2);

        private const int WIDTH = 40;
        private const int HEIGHT = 40;

        private int[] _fractal = new int[WIDTH * HEIGHT];

        [Benchmark]
        public void CalculateScalar()
        {
            float xStep = (MAX_X - MIN_X) / WIDTH;
            float yStep = (MAX_Y - MIN_Y) / HEIGHT;

            for (int x = 0; x < WIDTH; ++x)
            {
                for (int y = 0; y < HEIGHT; ++y)
                {
                    ComplexF c = new ComplexF(MIN_X + x * xStep, MIN_Y + y * yStep);
                    ComplexF z = c;
                    int iters;
                    for (iters = 0; iters < ITERATIONS; ++iters)
                    {
                        if (z.MagnitudeSquared >= THRESHOLD)
                        {
                            break;
                        }
                        z = z * z + c;
                    }
                    _fractal[y * WIDTH + x] = iters;
                }
            }
        }

        [Benchmark]
        public void CalculateVector()
        {
            float xStep = (MAX_X - MIN_X) / WIDTH;
            float yStep = (MAX_Y - MIN_Y) / HEIGHT;

            int vecSize = Vector<int>.Count;
            for (int x = 0; x < WIDTH; ++x)
            {
                // The real component in all the numbers in the inner loop is 
                // going to be the same.
                Vector<float> vReal = new Vector<float>(MIN_X + x * xStep);
                for (int y = 0; y < HEIGHT; y += vecSize)
                {
                    // The imaginary component has multiple separate values
                    // because we're traversing through multiple values of 'y'
                    // in each loop iteration.
                    float[] imags = new float[vecSize];
                    for (int i = 0; i < imags.Length; ++i)
                    {
                        imags[i] = MIN_Y + (y + i) * yStep;
                    }
                    Vector<float> vImag = new Vector<float>(imags);

                    ComplexVF vC = new ComplexVF(vReal, vImag);
                    ComplexVF vZ = vC;

                    Vector<int> vIters = Vector<int>.Zero;
                    Vector<int> vMaxIters = new Vector<int>(ITERATIONS);
                    Vector<int> vIncrement = Vector<int>.One;
                    Vector<float> vThreshold = new Vector<float>(THRESHOLD);
                    do
                    {
                        // This is vector multiplication and addition
                        vZ = vZ * vZ + vC;

                        // TODO Increment the number of iterations so far, for the elements
                        //      that haven't been phased out yet (magnitude less than threshold).
                        vIters += Vector<int>.Zero;

                        // TODO Which vZ's have magnitude less than the threshold?
                        Vector<int> vLessThanThreshold = Vector<int>.Zero;
                        // TODO Which vIters have # iterations less than the maximum?
                        Vector<int> vLessThanMaxIters = Vector<int>.Zero;
                        // TODO Which elements should proceed to the next iteration?
                        Vector<int> vShouldContinue = Vector<int>.Zero;

                        // TODO Which vIters elements should be incremented in the next iteration?
                        vIncrement &= Vector<int>.Zero;
                    }
                    while (vIncrement != Vector<int>.Zero);

                    for (int i = 0; i < vecSize; ++i)
                    {
                        _fractal[y * WIDTH + x + i] = vIters[i];
                    }
                }
            }
        }


    }
    struct ComplexF
    {
        private float _real;
        private float _imaginary;

        public ComplexF(float real, float imaginary)
        {
            _real = real;
            _imaginary = imaginary;
        }

        public float MagnitudeSquared
        {
            get { return _real * _real + _imaginary * _imaginary; }
        }

        public static ComplexF operator +(ComplexF a, ComplexF b)
        {
            return new ComplexF(a._real + b._real, a._imaginary + b._imaginary);
        }

        public static ComplexF operator *(ComplexF a, ComplexF b)
        {
            return new ComplexF(
                a._real * b._real - a._imaginary * b._imaginary,
                a._real * b._imaginary + a._imaginary * b._real
                );
        }
    }
    struct ComplexVF
    {
        private Vector<float> _real;
        private Vector<float> _imaginary;

        public ComplexVF(Vector<float> real, Vector<float> imaginary)
        {
            _real = real;
            _imaginary = imaginary;
        }

        public Vector<float> MagnitudeSquared
        {
            get { return _real * _real + _imaginary * _imaginary; }
        }

        public static ComplexVF operator +(ComplexVF a, ComplexVF b)
        {
            return new ComplexVF(a._real + b._real, a._imaginary + b._imaginary);
        }

        public static ComplexVF operator *(ComplexVF a, ComplexVF b)
        {
            return new ComplexVF(
                a._real * b._real - a._imaginary * b._imaginary,
                a._real * b._imaginary + a._imaginary * b._real
                );
        }
    }
}
