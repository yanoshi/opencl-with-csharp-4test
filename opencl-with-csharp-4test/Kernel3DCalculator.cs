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
    public class Kernel3DCalculator
    {
        public const int SIZEX = 12;
        public const int SIZEY = 12;
        public const int SIZEZ = 12;

        public const int KERNEL_SIZEX = 3;
        public const int KERNEL_SIZEY = 3;
        public const int KERNEL_SIZEZ = 3;

        [Cudafy]
        public static ushort[,,] SourceMemory = new ushort[SIZEX, SIZEY, SIZEZ];

        [Cudafy]
        public static float[,,] KernelMemory = new float[KERNEL_SIZEX, KERNEL_SIZEY, KERNEL_SIZEZ];

        public static void Execute()
        {
            CudafyModes.Language = eLanguage.OpenCL;

            CudafyModule km = CudafyTranslator.Cudafy(eArchitecture.OpenCL11);


            GPGPU gpu = CudafyHost.GetDevice(eGPUType.OpenCL, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            ushort[,,] hostSource = new ushort[SIZEX,SIZEY,SIZEZ];
            float[,,] hostKernel = new float[KERNEL_SIZEX, KERNEL_SIZEY, KERNEL_SIZEZ];
            ushort[,][] output = new ushort[SIZEY,SIZEZ][];

            ushort[] dev_outputtemp = gpu.Allocate<ushort>(SIZEX);
            for (int z = 0; z < hostSource.GetLength(2); z++)
                for (int y = 0; y < hostSource.GetLength(1); y++)
                    for (int x = 0; x < hostSource.GetLength(0); x++)
                        hostSource[x, y, z] = (ushort)((z * SIZEX * SIZEY + y * SIZEX + x) % 128);

            for (int z = 0; z < hostKernel.GetLength(2); z++)
                for (int y = 0; y < hostKernel.GetLength(1); y++)
                    for (int x = 0; x < hostKernel.GetLength(0); x++)
                        hostKernel[x, y,z] = 1.0f / (KERNEL_SIZEX * KERNEL_SIZEY * KERNEL_SIZEZ);



            gpu.CopyToConstantMemory(hostSource, SourceMemory);
            gpu.CopyToConstantMemory(hostKernel, KernelMemory);


            for(int z = 0;z<SIZEZ;z++)
                for(int y = 0;y<SIZEY;y++)
                {
                    ushort[] outputTemp = new ushort[SIZEX];

                    gpu.Launch(SIZEZ*SIZEY, 1).ApplyKernel(z,dev_outputtemp);
                    
                    gpu.CopyFromDevice(dev_outputtemp, outputTemp);

                    output[y, z] = outputTemp;
                }
            

            gpu.Free(dev_outputtemp);
            Console.WriteLine("Success!!!!!");
        }





        [Cudafy]
        public static void ApplyKernel(GThread thread, int z, ushort[] outputData)
        {
            int x = 0;
            int y = thread.blockIdx.x % SIZEY;
            float value = 0;

            while(x < SIZEX)
            {
                for(int kernelZ = KERNEL_SIZEZ / -2; kernelZ <= KERNEL_SIZEZ /2; kernelZ++)
                    for (int kernelY = KERNEL_SIZEY / -2; kernelY <= KERNEL_SIZEY / 2; kernelY++)
                        for (int kernelX = KERNEL_SIZEX / -2; kernelX <= KERNEL_SIZEX / 2; kernelX++)
                        {
                            int tempX = x + kernelX;
                            int tempY = y + kernelY;
                            int tempZ = z + kernelZ;

                            if (tempX >= 0 && tempX < SIZEX &&
                                tempY >= 0 && tempY < SIZEY &&
                                tempZ >= 0 && tempZ < SIZEZ)
                                value +=
                                    KernelMemory[
                                        kernelX + KERNEL_SIZEX / 2,
                                        kernelY + KERNEL_SIZEY / 2,
                                        kernelZ + KERNEL_SIZEZ / 2] *
                                    SourceMemory[tempX, tempY, tempZ];                   
                        }

                outputData[x] = (ushort)value;

                x++;
            }

        }
    }
}
