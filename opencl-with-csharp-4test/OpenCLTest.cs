using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opencl_with_csharp_4test
{

    public class OpenCLTest
    {
        private const int N = 1024 * 1024;

        public static void Execute()
        {
            CudafyModes.Language = eLanguage.OpenCL;
            CudafyModule km = CudafyTranslator.Cudafy(eArchitecture.OpenCL11);

            GPGPU gpu = CudafyHost.GetDevice(eGPUType.OpenCL, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            int[] a = new int[N];
            int[] b = new int[N];
            int[] c = new int[N];

            int[] dev_a = gpu.Allocate<int>(N);
            int[] dev_b = gpu.Allocate<int>(N);
            int[] dev_c = gpu.Allocate<int>(N);

            for(int i=0;i<a.Length;i++)
            {
                a[i] = i;
                b[i] = i;
            }

            gpu.CopyToDevice(a, dev_a);
            gpu.CopyToDevice(b, dev_b);

            gpu.Launch(N,1).MuxArray(dev_a,dev_b,dev_c);
            //gpu.Launch(1000, 1000, "add", 2, 7, dev_c);
            // copying result back
            gpu.CopyFromDevice(dev_c,c);


            for(int i =0;i<c.Length;i++)
                Console.WriteLine("{0}^2 = {1}", i, c[i]);
            //gpu.Launch().sub(2, 7, dev_c);
            //gpu.CopyFromDevice(dev_c, out c);

            //Console.WriteLine("2 - 7 = {0}", c);
            gpu.Free(dev_a);
            gpu.Free(dev_b);
            gpu.Free(dev_c);
        }



        [Cudafy]
        public static void MuxArray(GThread thread, int[] a, int[] b, int[] c)
        {
            int tid = thread.blockIdx.x;

            if (tid < N)
                c[tid] = a[tid] * b[tid];

        }




    }
}
