### SIMD Workshop

In this workshop, you will experiment with vectorization of simple and increasingly complex algorithms using C# and the **System.Numerics.Vectors** NuGet package. To perform this workshop, you will need Visual Studio 2015 or later, and a computer with a 64-bit edition of Windows installed (Windows 10 is recommended but not required).

The code in this workshop is organized in a single solution -- **simd-workshop.sln**. Clone the repository now and open this solution. Build (which will also restore NuGet packages) and make sure you don't get any errors.

#### Task 1: Vectorizing Dot Product and Matrix Multiplication

In this task, you will vectorize simple loops operating on floating-point values. Open the [SimpleVectors.cs](SimpleVectors.cs) file. There's an example of using `Vector<T>` to vectorize a loop that adds numbers together from two arrays. Review the example and the classroom material and make sure you understand what's going on. To see the actual gains from this vectorized loop, uncomment the line `BenchmarkRunner.Run<SimpleVectors>();` in [Program.cs](Program.cs) and run the application. Make sure you build and run in Release mode.

Your task is to implement the `DotProductSimd` function, which vectorizes the dot product operation over a vector. The dot product of a vector is produced by multiplying each vector element with itself, and then summing the resulting products. Specifically, the dot product of a vector is a single scalar (a single float).

> Hint: You can use `Vector.Dot` to find the dot product of two SIMD vectors, such as `Vector<float>`. This isn't necessarily the most efficient solution, however.

Next, implement the `MatrixMultSimd` function, which vectorizes matrix multiplication. Matrix multiplication is a simple, classic loop that can benefit greatly from vectorization.

Measure the results by simply running the application again, with the `BenchmarkRunner.Run<SimpleVectors>();` line enabled. You should see gains anywhere from 2x to 4x or even 8x from the vectorized versions of these simple algorithms.

#### Task 2: Vectorizing Vector Normalization

In this task, you will perform a slightly more challenging task, that will require constructing a custom data structure. Review the `VectorNorm` function in [SimpleVectors.cs](SimpleVectors.cs). This function takes an array of points in three-dimensional space, and normalizes each point by dividing each coordinate by the norm of the point's corresponding 3D vector. 

Your task is to implement the `VectorNormSimd` function, which should speed up this operation. Note that currently, the data layout is sub-optimal: if you look at the `Point3[] pts` array in memory, the X coordinates of multiple consecutive points are not aligned together. Rather, they are spread out with Y and Z values in between. To effectively vectorize the underlying operations, you need to think of an alternative way to organize the data.

> Hint: The most typical solution is moving from an Array of Structures (AoS) to a Structure of Arrays (SoA). That is, instead of storing the data as X1, Y1, Z1, X2, Y2, Z2 and so on, you'll store the data as X1, X2, ..., Y1, Y2, ..., Z1, Z2, ... -- three arrays of coordinates. This should make it easier to vectorize the operations involved in normalization -- multiplying and adding several values together.

#### Task 3: Vectorizing The Mandelbrot Fractal

In this task, you will vectorize an algorithm that calculates the [Mandelbrot Fractal](https://en.wikipedia.org/wiki/Mandelbrot_set). You don't need to know exactly what the Mandelbrot Fractal is and why it is important. There is an algorithm at hand that we need to speed up using vectorization.

Open the [Mandelbrot.cs](Mandelbrot.cs) file and review the algorithm in the `CalculateScalar` function. Essentially, there is a two-dimensional loop that visits all the coordinates within the desired grid. For each point, there is an inner loop that runs until convergence, or until a predefined maximum number of iterations is exceeded. The core of the algorithm performs complex number multiplication and addition, which, at the end of the day, are arithmetic operations on floating-point numbers.

Your task is to fill in the blanks in the `CalculateVector` function, which uses vectorization. Specifically, the core double loop that goes over the 2D points is already present. The key idea is that we will be visiting multiple points at the same time, by using `Vector<T>` in our implementation. Because we are visiting multiple points at the same time, we will need to keep track of multiple iteration counts (for each point), and so on.

#### Task 4: Vectorizing N-Body Computation

In this task, you will vectorize an algorithm that calculates the forces applied by multiple particles (celestial bodies) to each other in three-dimensional spaces. This is also known as the [N-body simulation](https://en.wikipedia.org/wiki/N-body_simulation). Efficiently calculating the bodies' effects on each other is important for a number of applications, including physics simulation, gaming, and graphics.

Open the [Particles.cs](Particles.cs) file and review the algorithm in the `Simulate` function. This is a loop that repeats multiple times (with a simulated time delta of 10ms between iterations), and calculates the effect of each body on each other body using the `Particle.ApplyForce` method. Presently, the code is written in a scalar style, and it's not immediately obvious how vectorization can be applied.

Your task is to implement the `SimulateSimd` function. Unlike previous tasks, this will require a significant rewrite and some creative thinking. You may find [this SSE programming tutorial](http://sci.tuomastonteri.fi/programming/sse) useful.