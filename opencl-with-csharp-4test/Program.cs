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
        public const string OpenCL = "OpenCL.dll";

        //対象のDLLを読み込んで、DLL中のメソッドを実行可能にします。
        //ここではDLL中の'clGetPlatformIDs'メソッドを利用します。
        [DllImport(OpenCL)]
        public static extern int clGetPlatformIDs
            (uint num_entries,
            IntPtr[] platforms,
            out uint num_platforms);

        public const int CL_SUCCESS = 0;

        static void Main(string[] args)
        {

            uint platformCount;

            int errcode = clGetPlatformIDs(0u, null, out platformCount);

            if (errcode != CL_SUCCESS)
                Console.WriteLine("Error at clGetPlatformIDs : " + errcode);
            else
                Console.WriteLine("Number of OpenCL Platforms : " + platformCount.ToString());

            Console.ReadLine();

        }
    }
}
