using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System.Diagnostics;

namespace opencl_with_csharp_4test
{
    public class KernelCalculator
    {
        public const int X_SIZE = 128;
        public const int Y_SIZE = 128;
        public const int KERNEL_SIZE = 51;

        [Cudafy]
        public static int[,] MemoryMain2D = new int[X_SIZE, Y_SIZE];

        [Cudafy]
        public static float[,] MemoryKernel = new float[KERNEL_SIZE, KERNEL_SIZE];


        public static void Execute()
        {
            CudafyModes.Language = eLanguage.OpenCL;

            CudafyModule km = CudafyTranslator.Cudafy(eArchitecture.OpenCL11);


            GPGPU gpu = CudafyHost.GetDevice(eGPUType.OpenCL, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            int[,] a = new int[X_SIZE,Y_SIZE];
            float[,] kPict = new float[KERNEL_SIZE,KERNEL_SIZE];
            int[] output = new int[X_SIZE * Y_SIZE];

            int[] dev_output = gpu.Allocate<int>(X_SIZE * Y_SIZE);

            Random r = new Random();

            for (int y = 0; y < a.GetLength(1); y++)
                for (int x = 0; x < a.GetLength(0); x++)
                    a[x, y] = r.Next();

            for (int y = 0; y < kPict.GetLength(1); y++)
                for (int x = 0; x < kPict.GetLength(0); x++)
                    kPict[x , y] = 1.0f / (KERNEL_SIZE * KERNEL_SIZE);

            gpu.CopyToConstantMemory(a, MemoryMain2D);
            gpu.CopyToConstantMemory(kPict, MemoryKernel);

            gpu.Launch(X_SIZE, 1).ApplyKernel(dev_output);


            gpu.CopyFromDevice(dev_output, output);


            for (int i= 0; i < output.Length; i++)
                Console.WriteLine("({0},{1}):\t{2}", i % Y_SIZE, i / Y_SIZE, output[i]);


            gpu.Free(dev_output);

        }


        [Cudafy]
        public static void ApplyKernel(GThread thread, int[] outputData)
        {
            //int[,] cache = thread.AllocateShared<int>("cache", X_SIZE, Y_SIZE);
            int targetX = thread.blockIdx.x;
            int targetY = 0;

            float value = 0;

            while(targetY < Y_SIZE)
            {
                for (int kernelX = KERNEL_SIZE / -2; kernelX <= KERNEL_SIZE / 2; kernelX++)
                    for (int kernelY = KERNEL_SIZE / -2; kernelY <= KERNEL_SIZE / 2; kernelY++)
                    {
                        int realX = targetX + kernelX;
                        int realY = targetY + kernelY;

                        if (realX >= 0 && realX < X_SIZE &&
                            realY >= 0 && realY < Y_SIZE)
                            value += MemoryKernel[kernelX + KERNEL_SIZE / 2, kernelY + KERNEL_SIZE / 2] * MemoryMain2D[realX, realY];

                        //Debug.WriteLine(String.Format("hoge: {0}",kernelX));
                    }

                //cache[targetX, targetY] = (int)value;
                //outputData[targetX + targetY * X_SIZE] = cache[targetX, targetY];
                outputData[targetX + targetY * X_SIZE] = (int)value;
                targetY++;
                value = 0;
            }
        }
    }
}
