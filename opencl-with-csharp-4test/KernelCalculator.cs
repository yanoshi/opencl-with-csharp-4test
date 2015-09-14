using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace opencl_with_csharp_4test
{
    class KernelCalculator
    {
        public static void Execute()
        {
            CudafyModes.Language = eLanguage.OpenCL;

            CudafyModule km = CudafyTranslator.Cudafy(eArchitecture.OpenCL11);


            GPGPU gpu = CudafyHost.GetDevice(eGPUType.OpenCL, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            int[,] a = new int[1024, 1024];
            float[,] kPict = new float[127, 127];
            int[,] z = new int[1024, 1024];

            int[,] dev_a = gpu.Allocate<int>(1024, 1024);
            float[,] dev_kpict = gpu.Allocate<float>(127, 127);
            int[,] dev_z = gpu.Allocate<int>(1024, 1024);

            Random r = new Random();

            for (int y = 0; y < a.GetLength(1); y++)
                for (int x = 0; x < a.GetLength(0); x++)
                    z[x, y] = r.Next();

            for (int y = 0; y < a.GetLength(1); y++)
                for (int x = 0; x < a.GetLength(0); x++)
                    kPict[x, y] = 1.0f / (127.0f * 127.0f);

            gpu.CopyToDevice(a, dev_a);
            gpu.CopyToDevice(kPict, dev_kpict);

            gpu.Launch(new dim3(1024,1024), 1).MuxArray(dev_a, dev_kpict, dev_z);
            //gpu.Launch(1000, 1000, "add", 2, 7, dev_c);
            // copying result back
            gpu.CopyFromDevice(dev_z, z);


            for (int y = 0; y < z.GetLength(1); y++)
                for (int x = 0; x < z.GetLength(0); x++)
                    Console.WriteLine("({0},{1}):\t{2}", x, y, z[x, y]);
            //gpu.Launch().sub(2, 7, dev_c);
            //gpu.CopyFromDevice(dev_c, out c);

            //Console.WriteLine("2 - 7 = {0}", c);
            gpu.Free(dev_a);
            gpu.Free(dev_z);
            gpu.Free(dev_kpict);
        }


        [Cudafy]
        public static void MuxArray(GThread thread, int[,] a, float[,] kernel, int[,] z)
        {
            int targetX = thread.gridDim.x;
            int targetY = thread.gridDim.y;

            float value = 0;

            for (int kernelX = 127 / -2; kernelX <= 127 / 2; kernelX++)
                for (int kernelY = 127 / -2; kernelY <= 127 / 2; kernelY++)
                {
                    int realX = targetX + kernelX;
                    int realY = targetY + kernelY;

                    if (realX >= 0 && realX < 1024 &&
                        realY >= 0 && realY < 1024)
                        value += kernel[kernelX + 127 / 2, kernelY + 127 / 2] * a[realX, realY];
                }

            z[targetX, targetY] = (int)value;
        }
    }
}
