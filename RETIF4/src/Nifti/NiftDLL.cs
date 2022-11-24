/**
 * NiftiDLL class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using itk.simple;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RETIF4.Nifti {

    public static class NiftiDLL {

                                    /*--- the original ANALYZE 7.5 type codes ---*/
        public const int DT_NONE            = 0;
        public const int DT_UNKNOWN         = 0;
        public const int DT_BINARY          = 1;      /* binary (1 bit/voxel)         */
        public const int DT_UNSIGNED_CHAR   = 2;      /* unsigned char (8 bits/voxel) */
        public const int DT_SIGNED_SHORT    = 4;      /* signed short (16 bits/voxel) */
        public const int DT_SIGNED_INT      = 8;      /* signed int (32 bits/voxel)   */
        public const int DT_FLOAT           = 16;     /* float (32 bits/voxel)        */
        public const int DT_COMPLEX         = 32;     /* complex (64 bits/voxel)      */
        public const int DT_DOUBLE          = 64;     /* double (64 bits/voxel)       */
        public const int DT_RGB             = 128;    /* RGB triple (24 bits/voxel)   */
        public const int DT_ALL             = 255;    /* not very useful (?)          */

                                    /*----- another set of names for the same ---*/
        public const int DT_UINT8            = 2;
        public const int DT_INT16            = 4;
        public const int DT_INT32            = 8;
        public const int DT_FLOAT32          = 16;
        public const int DT_COMPLEX64        = 32;
        public const int DT_FLOAT64          = 64;
        public const int DT_RGB24            = 128;

                                    /*------------------- new codes for NIFTI ---*/
        public const int DT_INT8             = 256;   /* signed char (8 bits)         */
        public const int DT_UINT16           = 512;   /* unsigned short (16 bits)     */
        public const int DT_UINT32           = 768;   /* unsigned int (32 bits)       */
        public const int DT_INT64            = 1024;  /* long long (64 bits)          */
        public const int DT_UINT64           = 1280;  /* unsigned long long (64 bits) */
        public const int DT_FLOAT128         = 1536;  /* long double (128 bits)       */
        public const int DT_COMPLEX128       = 1792;  /* double pair (128 bits)       */
        public const int DT_COMPLEX256       = 2048;  /* long double pair (256 bits)  */
        public const int DT_RGBA32           = 2304;  /* 4 byte RGBA (32 bits/voxel)  */
        
        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);

        //private static Assembly itkAssembly = null;

        static NiftiDLL() {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            path = Path.Combine(path, Environment.Is64BitProcess ? "win64" : "win32");
            bool ok = SetDllDirectory(path);
            if (!ok) {
                var result = MessageBox.Show("Could not set DLL directory ('" + path + "')", "DLL error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            string dllPath = path + Path.DirectorySeparatorChar + "nifti.dll";
            IntPtr pDll = LoadLibrary(dllPath);
            if (pDll == IntPtr.Zero) {
                MessageBox.Show("LoadLibrary failed to load nifti dll ('" + dllPath + "') with error: " + GetLastError() + ".\n\nThis is probably because the dlls (nifti.dll, blas_winxx_MT.dll and lapack_winxx_MT.dll) are dependent on VS runtime dlls.\n\nMake sure that the Visual C++ Redistributables (2010, 2012) are installed on your system. For debug executables, make sure the debug runtime dlls msvcp110d.dll and msvcr110d.dll from the VS runtime are available.", "DLL error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // load the ITK C# dll
            //itkAssembly = Assembly.LoadFile(path + Path.DirectorySeparatorChar + "SimpleITKCSharpManaged.dll");

        }


        [DllImport("nifti.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int n_GetBitVersion();

        [DllImport("nifti.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int n_ReadNifti(string filename);

        [DllImport("nifti.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool n_ReadNifti_Info(string filename, ref int nx, ref int ny, ref int nz, ref long nvox, ref int nbyper, ref int ndim, ref int datatype, double[] affine);

        [DllImport("nifti.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool n_ReadNifti_Data_char(string filename, byte[] dstDataBuffer, long bufferLength);

        [DllImport("nifti.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool n_ReadNifti_Data_ushort(string filename, ushort[] dstDataBuffer, long bufferLength);

        [DllImport("nifti.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int n_test();

        [DllImport("nifti.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int detrendTest(double[,] srcMatrix, double[,] dstMatrix, int lenDim0, int lenDim1);


        public static void ITKTest() {
            /*
            Console.WriteLine("aaa");
            //Type type = itkAssembly.GetType("SimpleITKCSharpManaged.itk.simple.Image");
            Type type = itkAssembly.GetType("itk.simple.SimpleITK");
            MethodInfo info = type.GetMethod("ReadImage", new Type[] { typeof(System.String) });
            object result = info.Invoke(null, new object[] { "D:\\G3\\FUNC_bos-0001.nii" });
            */

            //itk.simple.Image input = SimpleITK.ReadImage("D:\\G3\\FUNC_bos-0001.nii");

            // Cast to we know the the pixel type
            //input = SimpleITK.Cast(input, itk.simple.PixelIDValueEnum.sitkFloat32);




            ImageFileReader reader = new ImageFileReader();
            reader.SetOutputPixelType(PixelIDValueEnum.sitkFloat32);

            reader.SetFileName("D:\\G3\\FUNC_bos-0001.nii");
            Image fixedImage = reader.Execute();

            reader.SetFileName("D:\\G3\\FUNC_bos-0002.nii");
            Image movingImage = reader.Execute();



            itk.simple.VectorUInt32 index = new itk.simple.VectorUInt32();
            index.Add(20);
            index.Add(20);
            index.Add(20);
            Console.WriteLine("vf " + fixedImage.GetPixelAsFloat(index));
            Console.WriteLine("vm " + movingImage.GetPixelAsFloat(index));



            
            ImageRegistrationMethod R = new ImageRegistrationMethod();
            R.SetMetricAsMeanSquares();
            //R.SetMetricAsMattesMutualInformation(50); //numberof histogrambins
            //R.SetMetricSamplingPercentage(3.0); //Sampling %
            double maxStep = 4.0;
            double minStep = 0.01;
            //uint numberOfIterations = 200;
            uint numberOfIterations = 100;
            double relaxationFactor = 0.5;
            R.SetOptimizerAsRegularStepGradientDescent(maxStep,
                                                        minStep,
                                                        numberOfIterations,
                                                        relaxationFactor);
            R.SetInitialTransform(new TranslationTransform(fixedImage.GetDimension()));
            R.SetInterpolator(InterpolatorEnum.sitkLinear);

            //IterationUpdate cmd = new IterationUpdate(R);
            itk.simple.Command cmd = new Command();
            R.AddCommand(EventEnum.sitkIterationEvent, cmd);



            Console.WriteLine("bbb");
            
            Transform transform_final = R.Execute(fixedImage, movingImage);
            double metrica = R.GetMetricValue();
            
            Console.WriteLine("ccc");
            

            Image resampled = SimpleITK.Resample(movingImage, fixedImage, transform_final, InterpolatorEnum.sitkLinear, 0.0, movingImage.GetPixelID());
            SimpleITK.WriteImage(resampled, "D:\\G3\\output.nii");

            Console.WriteLine("vf " + fixedImage.GetPixelAsFloat(index));
            Console.WriteLine("vm " + movingImage.GetPixelAsFloat(index));
            Console.WriteLine("vre " + resampled.GetPixelAsFloat(index));

        }

        public static void ITKRealign(string refImageFilepath, string movingImageFilepath, string outputImageFilepath) {

            // 
            ImageFileReader reader = new ImageFileReader();
            
            reader.SetOutputPixelType(PixelIDValueEnum.sitkFloat32);

            // read the reference image
            reader.SetFileName(refImageFilepath);
            Image fixedImage = reader.Execute();

            // read the moving image
            reader.SetFileName(movingImageFilepath);
            Image movingImage = reader.Execute();


            // set registration method and parameters
            ImageRegistrationMethod R = new ImageRegistrationMethod();
            R.SetMetricAsMeanSquares();
            R.SetMetricAsMattesMutualInformation(50); //numberof histogrambins
            R.SetMetricSamplingPercentage(1.0); //Sampling %
            R.SetMetricSamplingStrategy(ImageRegistrationMethod.MetricSamplingStrategyType.RANDOM);
            double maxStep = 4.0;
            double minStep = 0.01;
            //uint numberOfIterations = 200;
            uint numberOfIterations = 100;
            double relaxationFactor = 0.5;
            //R.set
            R.SetOptimizerAsRegularStepGradientDescent(maxStep,
                                                        minStep,
                                                        numberOfIterations,
                                                        relaxationFactor);
                                                        
            //R.SetInitialTransform(new TranslationTransform(fixedImage.GetDimension()));
            R.SetInitialTransform(new TranslationTransform(3));
            //R.SetInitialTransformAsBSpline(new BSplineTransform(3));
            R.SetInterpolator(InterpolatorEnum.sitkLinear);
            //R.SetInterpolator(InterpolatorEnum.sitkBSplineResamplerOrder4);
            itk.simple.Command cmd = new Command();
            R.AddCommand(EventEnum.sitkIterationEvent, cmd);

            // execute the 
            Transform transform_final = R.Execute(fixedImage, movingImage);
            
            // apply the transformation matrix
            Image resampled = SimpleITK.Resample(movingImage, fixedImage, transform_final, InterpolatorEnum.sitkLinear, 0.0, movingImage.GetPixelID());
            SimpleITK.WriteImage(resampled, outputImageFilepath);

        }

        /**
         * Function to read an image (information and data) into a managed class 
         * 
         * function performance:
         * - just readNifti = 9.1 ms (1000 reps)
         * - allocate managed memory in c# and cpymem the image data into it in c++ = 14 ms (1000 reps)
         * 
         * return: NiftiImage object on success, null on failure
         **/
        public static NiftiImage n_ReadNifti_Safe(string filename) {

            // create a NiftiImage object to store the information and data in
            NiftiImage image = new NiftiImage();

            // retrieve the nifti information
            // (this is needed to create a managed buffer of the right size to hold the data)
            bool ret = n_ReadNifti_Info(filename, ref image.nx, ref image.ny, ref image.nz, ref image.nvox, ref image.nbyper, ref image.ndim, ref image.datatype, image.affine);
            
            // check if the nifti was readable and the information was retrieved
            if (ret) {
                // read succesfully and information was retrieved

                // read the nifti, the dll will copy the data from the image into the managed buffer 
                //ret = n_ReadNifti_Data(filename, image.data, bufferLength);

                if (image.nbyper == DT_UINT8) {
                    // 2 bytes per voxel
                    
                    // calculate how big the data (byte) buffer should be
                    long bufferLength = image.nbyper * (int)image.nvox;

                    // create a managed buffer
                    // half the byte size since we want to store them as ushort
                    image.ushortData = new ushort[bufferLength / 2];

                    // retrieve the data as ushort
                    ret = n_ReadNifti_Data_ushort(filename, image.ushortData, bufferLength);

                    // set the local data type as ushort
                    image.localDataType = NiftiImage.DT_USHORT;

                    // check if the nifti was readable and the data was retrieved
                    if (ret) {

                        // return the NiftiImage object
                        return image;

                    }

                } else if (image.nbyper == DT_BINARY) {
                    // 1 byte per voxel

                    // calculate how big the data (byte) buffer should be
                    long bufferLength = image.nbyper * (int)image.nvox;

                    // create a managed buffer
                    image.byteData = new byte[bufferLength];
                    
                    // retrieve the data as ushort
                    ret = n_ReadNifti_Data_char(filename, image.byteData, bufferLength);
                    
                    // set the local data type as ushort
                    image.localDataType = NiftiImage.DT_BYTE;

                    // check if the nifti was readable and the data was retrieved
                    if (ret) {

                        // return the NiftiImage object
                        return image;

                    }

                } else {

                    // return null, image was found but format is not supported
                    return null;

                }

            }

            // return null, image was not found or could not be read
            return null;

        }

        class IterationUpdate : Command {

            private ImageRegistrationMethod m_Method;

            public IterationUpdate(ImageRegistrationMethod m) {
                m_Method = m;
            }

            public override void Execute() {
                VectorDouble pos = m_Method.GetOptimizerPosition();
                Console.WriteLine("{0:3} = {1:10.5} : [{2}, {3}]",
                                  m_Method.GetOptimizerIteration(),
                                  m_Method.GetMetricValue(),
                                  pos[0], pos[1]);
            }
        }


    }

}
