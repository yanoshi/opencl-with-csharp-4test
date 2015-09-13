using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace opencl_with_csharp_4test
{

    public class OpenCLExecuter
    {
        #region DLLImport
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


        [DllImport(OpenCL)]
        public static extern IntPtr clCreateContext
            (IntPtr[] properties,
            uint num_devices,
            IntPtr[] devices,
            CL_CALLBACK_clCreateContext pfn_notify,
            IntPtr user_data,
            out int errcode_ret);



        [DllImport(OpenCL)]
        public static extern IntPtr clCreateProgramWithSource
            (IntPtr context,
            uint count,
            string[] strings,
            IntPtr[] lengths,
            out int errcode_ret);


        [DllImport(OpenCL)]
        public static extern int clBuildProgram
            (IntPtr program,
            uint num_devices,
            IntPtr[] device_list,
            string options,
            object pfn_notify,
            IntPtr user_data);



        public delegate void CL_CALLBACK_clCreateContext(string errinfo, IntPtr private_info, int cb, IntPtr user_data);

        public delegate void CL_CALLBACK_clBuildProgram(IntPtr program, IntPtr user_data);
        #endregion



        #region 定数
        public const int CL_SUCCESS = 0;
        public const int CL_PLATFORM_PROFILE = 0x0900;
        public const int CL_PLATFORM_VERSION = 0x0901;
        public const int CL_DEVICE_NAME = 0x102B;
        public const int CL_DEVICE_VENDOR = 0x102C;
        public const int CL_DEVICE_TYPE_DEFAULT = (1 << 0);

        public const string OpenCL = "OpenCL.dll";
        #endregion



        public OpenCLExecuter()
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

                this.Platform = platform;
                this.Devices = devices;
            }
        }

        #region プロパティ
        public IntPtr Platform { get; }

        public IntPtr[] Devices { get; }
        #endregion




        /// <summary>
        /// サンプルコードを実行する
        /// http://neareal.net/index.php?Programming%2FOpenCL%2FOpenCLSharp%2FTutorial%2F4_SimpleOpenCLTask-1
        /// </summary>
        public void ExecuteSample()
        {
            //演算するデータの用意
            float[] xDataArray = new float[1920];
            float[] yDataArray = new float[1080];
            float[] rDataArray = new float[1920 * 1080];

            for (int i = 0; i < xDataArray.Length; i++)
                xDataArray[i] = 3.1415f;
            for (int i = 0; i < yDataArray.Length; i++)
                yDataArray[i] = 3.1415f;

            int errcode;



            //OpenCLCによって記述されるソースコード
            string[] sourceCode;

            using (var stream = new System.IO.StreamReader("kernel.cl"))
            {
                sourceCode = new string[] { stream.ReadToEnd() };
            }


            //コンテキストの生成
            IntPtr context = clCreateContext
                (null, 1, this.Devices, null, IntPtr.Zero, out errcode);
            if (errcode != CL_SUCCESS)
                throw new Exception("Error at clCreateContext : " + errcode);



            //プログラムの読み込み
            IntPtr program = clCreateProgramWithSource
                (context, (uint)sourceCode.Length, sourceCode, null, out errcode);
            if (errcode != CL_SUCCESS)
                throw new Exception("Error at clCreateProgramWithSource : " + errcode);

            //プログラムのビルド
            errcode = clBuildProgram(program, 1, Devices, null, null, IntPtr.Zero);
            if (errcode != CL_SUCCESS)
                throw new Exception("Error at clBuildProgram : " + errcode);
        }

    }
}
