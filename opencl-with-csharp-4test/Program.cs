using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace opencl_with_csharp_4test
{

    /*
    便利なURL
    https://www.khronos.org/registry/cl/api/1.2/cl.h
    */

    class Program
    {
       

        static void Main(string[] args)
        {
            var obj = new OpenCLExecuter();
            obj.ExecuteSample();      






            Console.ReadLine();

        }
    }
}
