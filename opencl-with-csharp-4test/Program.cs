using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using Cudafy.Types;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace opencl_with_csharp_4test
{
    class Program
    {
        static void Main(string[] args)
        {
            simple_kernel_params.Execute();
        }
    }

    public class simple_kernel_params
    {
        public static void Execute()
        {
            CudafyModule km = CudafyTranslator.Cudafy(eArchitecture.OpenCL11);

            GPGPU gpu = CudafyHost.GetDevice(eGPUType.OpenCL, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            // we cannot return any value from a device function...so our result is passed via parameter c
            // out keyword not supported.. so use a vector
            // allocating memory on the device even though it will only contain one Int32 value
            int c;
            int[] dev_c = gpu.Allocate<int>(); // cudaMalloc one Int32
            gpu.Launch().add(2, 7, dev_c); // or gpu.Launch(1, 1, "add", 2, 7, dev_c);
            //gpu.Launch(1000, 1000, "add", 2, 7, dev_c);
            // copying result back
            gpu.CopyFromDevice(dev_c, out c);

            Console.WriteLine("2 + 7 = {0}", c);
            //gpu.Launch().sub(2, 7, dev_c);
            //gpu.CopyFromDevice(dev_c, out c);

            //Console.WriteLine("2 - 7 = {0}", c);

            gpu.Free(dev_c);
        }

        [Cudafy]
        public static void add(int a, int b, int[] c)
        {
            c[0] = a + b;
        }

        [Cudafy]
        public static void sub(int a, int b, int[] c)
        {
            c[0] = a - b;
        }
    }
}