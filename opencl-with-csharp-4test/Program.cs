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
        public const string OpenCL = "OpenCL.dll";

        //対象のDLLを読み込んで、DLL中のメソッドを実行可能にします。
        //ここではDLL中の'clGetPlatformIDs'メソッドを利用します。
        [DllImport(OpenCL)]
        public static extern int clGetPlatformIDs
            (uint num_entries,
            IntPtr[] platforms,
            out uint num_platforms);


        [DllImport(OpenCL)]
        public static extern int clGetPlatformInfo
            (IntPtr platform,
            int param_name,
            uint param_value_size,
            StringBuilder param_value,
            out IntPtr param_value_size_ret);

        [DllImport(OpenCL)]
        public static extern int clGetDeviceInfo
            (IntPtr device,
            int param_name,
            uint param_value_size,
            StringBuilder param_value,
            out IntPtr param_value_size_ret);

        [DllImport(OpenCL)]
        public static extern int clGetDeviceIDs
            (IntPtr platform,
            uint device_type,
            uint num_entries,
            IntPtr[] devices,
            out uint num_devices);


        public const int CL_SUCCESS = 0;
        public const int CL_PLATFORM_PROFILE = 0x0900;
        public const int CL_PLATFORM_VERSION = 0x0901;
        public const int CL_DEVICE_NAME = 0x102B;
        public const int CL_DEVICE_VENDOR = 0x102C;
        public const int CL_DEVICE_TYPE_DEFAULT = (1 << 0);

        static void Main(string[] args)
        {

            uint platformCount;

            int errcode = clGetPlatformIDs(0u, null, out platformCount);

            if (errcode != CL_SUCCESS)
                Console.WriteLine("Error at clGetPlatformIDs : " + errcode);
            else
                Console.WriteLine("Number of OpenCL Platforms : " + platformCount.ToString());


            IntPtr[] platforms = new IntPtr[platformCount];
            clGetPlatformIDs(platformCount, platforms, out platformCount);

            foreach (IntPtr platform in platforms)
            {
                
                IntPtr valueSize;
                StringBuilder value = new StringBuilder();

                //[1]プラットフォームの情報を取得します。
                //データサイズを取得します
                errcode = clGetPlatformInfo(platform, CL_PLATFORM_PROFILE, 0, null, out valueSize);
                if (errcode != CL_SUCCESS)
                    throw new Exception("Error at clGetPlatformInfo : " + errcode);
                //データを取得します
                errcode = clGetPlatformInfo(platform, CL_PLATFORM_PROFILE, (uint)valueSize.ToInt32(), value, out valueSize);
                Console.WriteLine("Platform Profile   : " + value);

                //[2]OpenCLプラットフォームのバージョンを取得します。
                //データサイズを取得します
                errcode = clGetPlatformInfo(platform, CL_PLATFORM_VERSION, 0, null, out valueSize);
                if (errcode != CL_SUCCESS)
                    throw new Exception("Error at clGetPlatformInfo : " + errcode);
                //データを取得します
                errcode = clGetPlatformInfo(platform, CL_PLATFORM_VERSION, (uint)valueSize.ToInt32(), value, out valueSize);
                Console.WriteLine("Platform Version   : " + value);


                //[3]デバイスの情報を取得します。
                //デバイスの数を取得します。
                uint deviceCount;
                errcode = clGetDeviceIDs(platform, CL_DEVICE_TYPE_DEFAULT, 0, null, out deviceCount);
                if (errcode != CL_SUCCESS)
                    throw new Exception("Error at clGetDeviceIDs  : " + errcode);
                //デバイスのIDを取得します。
                IntPtr[] devices = new IntPtr[deviceCount];
                errcode = clGetDeviceIDs
                              (platform, CL_DEVICE_TYPE_DEFAULT, deviceCount, devices, out deviceCount);

                foreach (IntPtr device in devices)
                {
                    //デバイスの提供元を取得します。
                    errcode = clGetDeviceInfo(device, CL_DEVICE_VENDOR, 0, null, out valueSize);
                    if (errcode != CL_SUCCESS)
                        throw new Exception("Error at clGetDeviceInfo  : " + errcode);
                    clGetDeviceInfo(device, CL_DEVICE_VENDOR, (uint)valueSize.ToInt32(), value, out valueSize);
                    Console.WriteLine(" - Device Vendor   : " + value);

                    //デバイスの情報(名前)を取得します。
                    errcode = clGetDeviceInfo(device, CL_DEVICE_NAME, 0, null, out valueSize);
                    if (errcode != CL_SUCCESS)
                        throw new Exception("Error at clGetDeviceInfo  : " + errcode);
                    clGetDeviceInfo(device, CL_DEVICE_NAME, (uint)valueSize.ToInt32(), value, out valueSize);
                    Console.WriteLine(" - Device Name     : " + value);
                }


            }






            Console.ReadLine();

        }
    }
}
