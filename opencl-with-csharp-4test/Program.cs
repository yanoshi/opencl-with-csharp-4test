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
            OpenCLExecuter.Execute();
            Console.ReadKey();
        }
    }

}