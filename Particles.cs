using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simd_workshop
{
    public class Particles
    {
        private Particle[] particles = new Particle[PARTICLES];
        private const int ITERATIONS = 10;
        private const int PARTICLES = 100;

        [Setup]
        public void Setup()
        {
            var rand = new Random(42);
            for (int i = 0; i < particles.Length; ++i)
            {
                float mass = (float)rand.NextDouble();
                Vec3 position = new Vec3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                Vec3 velocity = new Vec3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                particles[i] = new Particle(position, velocity, mass);
            }
        }

        [Benchmark]
        public void Simulate()
        {
            float dt = 0.01f; // 10ms
            for (int k = 0; k < ITERATIONS; ++k)
            {
                for (int i = 0; i < particles.Length; ++i)
                {
                    for (int j = i + 1; j < particles.Length; ++j)
                    {
                        particles[i].ApplyForce(ref particles[j]);
                    }
                }
                for (int i = 0; i < particles.Length; ++i)
                {
                    particles[i].Velocity = particles[i].Velocity + particles[i].Acceleration * dt;
                    particles[i].Position = particles[i].Position + particles[i].Velocity * dt;
                }
            }
        }

        [Benchmark]
        public void SimulateSimd()
        {
            // TODO Implement this
        }
    }

    struct Particle
    {
        public Vec3 Position;
        public Vec3 Velocity;
        public Vec3 Acceleration;
        public float Mass;

        public Particle(Vec3 position, Vec3 velocity, float mass)
        {
            Position = position; Velocity = velocity; Mass = mass;
            Acceleration = new Vec3(0, 0, 0);
        }

        public void ApplyForce(ref Particle other)
        {
            Vec3 diff = Position - other.Position;
            float r2 = (diff * diff).Sum();
            diff = diff * (1.0f / ((float)Math.Sqrt(r2) * (r2 + 1.0f)));
            Acceleration = Acceleration - diff * other.Mass;
            other.Acceleration = other.Acceleration + diff * Mass;
        }
    }

    struct Vec3
    {
        public float X, Y, Z;

        public Vec3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        public static Vec3 operator+(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vec3 operator-(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vec3 operator*(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vec3 operator*(Vec3 a, float scalar)
        {
            return new Vec3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }

        public float Sum()
        {
            return X + Y + Z;
        }
    }
}
