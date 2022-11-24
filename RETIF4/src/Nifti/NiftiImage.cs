/**
 * NiftiImage class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;

namespace RETIF4.Nifti {

    public class NiftiImage {

        // The reorientation options that can be applied to the data to make the brain
        // oriented in a way that the top of the brain is pointing towards z=0, the front is pointing to y=0
        // and the left is pointing towards x=0
        // 
        //    1 / z
        //     /
        //    /
        //  0/_________ x      
        //   |        1
        //   |
        //   |
        //   |
        //  1| y
        // 
        //
        public enum OrientationTransform : int {

            // Flips & Rotate 180
            None = 0,
            RotX180_FlipYZ = 0,
            RotY180_FlipXZ = 0,
            RotZ180_FlipXY = 0,

            RotX180_FlipXYZ = 1,
            RotY180_FlipZ = 1,
            RotZ180_FlipY = 1,
            FlipX = 1,

            RotX180_FlipZ = 2,
            RotY180_FlipXYZ = 2,
            RotZ180_FlipX = 2,
            FlipY = 2,

            RotX180_FlipY = 3,
            RotY180_FlipX = 3,
            RotZ180_FlipXYZ = 3,
            FlipZ = 3,

            RotX180_FlipXZ = 4,
            RotY180_FlipYZ = 4,
            RotZ180 = 4,
            FlipXY = 4,

            RotX180 = 5,
            RotY180_FlipXY = 5,
            RotZ180_FlipXZ = 5,
            FlipYZ = 5,

            RotX180_FlipXY = 6,
            RotY180 = 6,
            RotZ180_FlipYZ = 6,
            FlipXZ = 6,

            RotX180_FlipX = 7,
            RotY180_FlipY = 7,
            RotZ180_FlipZ = 7,
            FlipXYZ = 7,


            // Rotate X 90
            RotX90CC_FlipYZ = 8,
            RotX90 = 8,

            RotX90CC_FlipXYZ = 9,
            RotX90_FlipX = 9,

            RotX90CC_FlipZ = 10,
            RotX90_FlipY = 10,

            RotX90CC_FlipY = 11,
            RotX90_FlipZ = 11,

            RotX90CC_FlipXZ = 12,
            RotX90_FlipXY = 12,

            RotX90CC = 13,
            RotX90_FlipYZ = 13,

            RotX90CC_FlipXY = 14,
            RotX90_FlipXZ = 14,

            RotX90CC_FlipX = 15,
            RotX90_FlipXYZ = 15,


            // Rotate Y 90
            RotY90CC_FlipXZ = 16,
            RotY90 = 16,

            RotY90CC_FlipZ = 17,
            RotY90_FlipX = 17,

            RotY90CC_FlipXYZ = 18,
            RotY90_FlipY = 18,

            RotY90CC_FlipX = 19,
            RotY90_FlipZ = 19,

            RotY90CC_FlipYZ = 20,
            RotY90_FlipXY = 20,

            RotY90CC_FlipXY = 21,
            RotY90_FlipYZ = 21,

            RotYCC90 = 22,
            RotY90_FlipXZ = 22,

            RotY90CC_FlipY = 23,
            RotY90_FlipXYZ = 23,


            // Rotate Z 90
            RotZ90CC_FlipXY = 24,
            RotZ90 = 24,

            RotZ90CC_FlipY = 25,
            RotZ90_FlipX = 25,

            RotZ90CC_FlipX = 26,
            RotZ90_FlipY = 26,

            RotZ90CC_FlipXYZ = 27,
            RotZ90_FlipZ = 27,

            RotZCC90 = 28,
            RotZ90_FlipXY = 28,

            RotZ90CC_FlipXZ = 29,
            RotZ90_FlipYZ = 29,
                        
            RotZ90CC_FlipYZ = 30,
            RotZ90_FlipXZ = 30,

            RotZ90CC_FlipZ = 31,
            RotZ90_FlipXYZ = 31,


            // Rotate X 90 + Y 90 | Rotate Y 90 + Z 90
            RotX90_RotY90 = 32,
            RotX90CC_RotY90_FlipXY = 32,
            RotX90CC_RotY90CC_FlipYZ = 32,
            RotX90_RotY90CC_FlipXZ = 32,
            RotY90_RotZ90CC = 32,
            RotY90CC_RotZ90_FlipXZ = 32,
            RotY90CC_RotZ90CC_FlipYZ = 32,
            RotY90_RotZ90_FlipXY = 32,

            RotX90CC_RotY90_FlipY = 33,
            RotX90CC_RotY90CC_FlipXYZ = 33,
            RotX90_RotY90_FlipX = 33,
            RotX90_RotY90CC_FlipZ = 33,
            RotY90CC_RotZ90_FlipZ = 33,
            RotY90CC_RotZ90CC_FlipXYZ = 33,
            RotY90_RotZ90_FlipY = 33,
            RotY90_RotZ90CC_FlipX = 33,

            RotX90CC_RotY90_FlipX = 34,
            RotX90CC_RotY90CC_FlipZ = 34,
            RotX90_RotY90_FlipY = 34,
            RotX90_RotY90CC_FlipXYZ = 34,
            RotY90CC_RotZ90_FlipXYZ = 34,
            RotY90CC_RotZ90CC_FlipZ = 34,
            RotY90_RotZ90_FlipX = 34,
            RotY90_RotZ90CC_FlipY = 34,

            RotX90CC_RotY90_FlipXYZ = 35,
            RotX90CC_RotY90CC_FlipY = 35,
            RotX90_RotY90_FlipZ = 35,
            RotX90_RotY90CC_FlipX = 35,
            RotY90CC_RotZ90_FlipX = 35,
            RotY90CC_RotZ90CC_FlipY = 35,
            RotY90_RotZ90_FlipXYZ = 35,
            RotY90_RotZ90CC_FlipZ = 35,

            RotX90CC_RotY90 = 36,
            RotX90CC_RotY90CC_FlipXZ = 36,
            RotX90_RotY90_FlipXY = 36,
            RotX90_RotY90CC_FlipYZ = 36,
            RotY90_RotZ90 = 36,
            RotY90CC_RotZ90_FlipYZ = 36,
            RotY90CC_RotZ90CC_FlipXZ = 36,
            RotY90_RotZ90CC_FlipXY = 36,

            RotX90CC_RotY90CC = 37,
            RotX90CC_RotY90_FlipXZ = 37,
            RotX90_RotY90_FlipYZ = 37,
            RotX90_RotY90CC_FlipXY = 37,
            RotY90CC_RotZ90CC = 37,
            RotY90CC_RotZ90_FlipXY = 37,
            RotY90_RotZ90_FlipXZ = 37,
            RotY90_RotZ90CC_FlipYZ = 37,
            
            RotX90_RotY90CC = 38,
            RotX90CC_RotY90_FlipYZ = 38,
            RotX90CC_RotY90CC_FlipXY = 38,
            RotX90_RotY90_FlipXZ = 38,
            RotY90CC_RotZ90 = 38,
            RotY90CC_RotZ90CC_FlipXY = 38,
            RotY90_RotZ90_FlipYZ = 38,
            RotY90_RotZ90CC_FlipXZ = 38,
            
            RotX90CC_RotY90_FlipZ = 39,
            RotX90CC_RotY90CC_FlipX = 39,
            RotX90_RotY90_FlipXYZ = 39,
            RotX90_RotY90CC_FlipY = 39,
            RotY90CC_RotZ90_FlipY = 39,
            RotY90CC_RotZ90CC_FlipX = 39,
            RotY90_RotZ90_FlipZ = 39,
            RotY90_RotZ90CC_FlipXYZ = 39,


            // Rotate X 90 + Z 90
            RotX90_RotZ90 = 40,
            RotX90CC_RotZ90_FlipXZ = 40,
            RotX90CC_RotZ90CC_FlipYZ = 40,
            RotX90_RotZ90CC_FlipXY = 40,

            RotX90CC_RotZ90_FlipZ = 41,
            RotX90CC_RotZ90CC_FlipXYZ = 41,
            RotX90_RotZ90_FlipX = 41,
            RotX90_RotZ90CC_FlipY = 41,

            RotX90CC_RotZ90_FlipXYZ = 42,
            RotX90CC_RotZ90CC_FlipZ = 42,
            RotX90_RotZ90_FlipY = 42,
            RotX90_RotZ90CC_FlipX = 42,

            RotX90CC_RotZ90_FlipX = 43,
            RotX90CC_RotZ90CC_FlipY = 43,
            RotX90_RotZ90_FlipZ = 43,
            RotX90_RotZ90CC_FlipXYZ = 43,

            RotX90_RotZ90CC = 44,
            RotX90CC_RotZ90_FlipYZ = 44,
            RotX90CC_RotZ90CC_FlipXZ = 44,
            RotX90_RotZ90_FlipXY = 44,

            RotX90CC_RotZ90CC = 45,
            RotX90CC_RotZ90_FlipXY = 45,
            RotX90_RotZ90_FlipYZ = 45,
            RotX90_RotZ90CC_FlipXZ = 45,

            RotX90CC_RotZ90 = 46,
            RotX90CC_RotZ90CC_FlipXY = 46,
            RotX90_RotZ90_FlipXZ = 46,
            RotX90_RotZ90CC_FlipYZ = 46,

            RotX90CC_RotZ90_FlipY = 47,
            RotX90CC_RotZ90CC_FlipX = 47,
            RotX90_RotZ90_FlipXYZ = 47,
            RotX90_RotZ90CC_FlipZ = 47,

            
        };


        // possible local data types in which the nifti data can be stored
        public const int DT_BYTE = 0;       // 1 byte per voxel, so a byte (0 - 255)
        public const int DT_USHORT = 1;     // 2 byte per voxel, so an ushort (0 - 65535)

        // information retrieved from the nifti (corresponds with the nifti1_io)
        public int datatype = 0;                /*!< type of data in voxels: DT_* code   (from the nifti class, not this class)   */
        public int nx = 0;                      /*!< dimensions of grid array                                                     */
        public int ny = 0;                      /*!< dimensions of grid array                                                     */
        public int nz = 0;                      /*!< dimensions of grid array                                                     */
        public long nvox = 0;                   /*!< number of voxels = nx*ny*nz*...*nw                                           */
        public int nbyper = 0;                  /*!< bytes per voxel, matches datatype                                            */
        public int ndim = 3;


        // The reorientation that should be applied for display in order to make the top of the brain point
        // towards z=0, the front point to y=0 and the left point towards x=0
        public OrientationTransform displayReorientation = OrientationTransform.None;

        public int localDataType = DT_BYTE;
        public byte[] byteData = null;
        public ushort[] ushortData = null;
        public int[] intData = null;

        public double[] affine = new double[16];

        
        //bool DisplayRangeIsSet = false;                  // variable to check whether the range of values to produce a bitmap were determined
        //public int displayHighest = 0;                   // the lowest value possible in the bitmap (
        //public int displayLowest = 0;

        bool niftiRangeIsSet = false;                   // variable to check whether the range of values in the nifti were determined
        private int niftiHighest = 0;                         // highest value in the nifti data
        private int niftiLowest = 0;                          // lowest value in the nifti data

        public void determineNiftiRange() {
            
            // 20ms
            if (localDataType == DT_BYTE) {
                // byte data

                niftiLowest = byteData[0];
                niftiHighest = byteData[0];
                int length = byteData.Length;
                unsafe {
                    fixed (byte* ptrByteData = byteData) {
                        byte* ptr = ptrByteData;
                        int remaining = byteData.Length;
                        while (remaining-- > 0) {
                            if (*ptr < niftiLowest)     niftiLowest = *ptr;
                            if (*ptr > niftiHighest)    niftiHighest = *ptr;
                            ptr++;
                        }
                    }
                }
                
            } else if (localDataType == DT_USHORT) {
                // ushort data

                niftiLowest = ushortData[0];
                niftiHighest = ushortData[0];
                int length = ushortData.Length;
                unsafe {
                    fixed (ushort* ptrByteData = ushortData) {
                        ushort* ptr = ptrByteData;
                        int remaining = ushortData.Length;
                        while (remaining-- > 0) {
                            if (*ptr < niftiLowest)     niftiLowest = *ptr;
                            if (*ptr > niftiHighest)    niftiHighest = *ptr;
                            ptr++;
                        }
                    }
                }

            } else {
                // assume int

                niftiLowest = intData[0];
                niftiHighest = intData[0];
                int length = intData.Length;
                unsafe {
                    fixed (int* ptrByteData = intData) {
                        int* ptr = ptrByteData;
                        int remaining = intData.Length;
                        while (remaining-- > 0) {
                            if (*ptr < niftiLowest)     niftiLowest = *ptr;
                            if (*ptr > niftiHighest)    niftiHighest = *ptr;
                            ptr++;
                        }
                    }
                }

            }

            /*
            // 45 ms
            int highest = 0;
            int lowest = 65000;
            for (int i = 0; i < image.data.Length; i++) {
                if (image.data[i] > highest)   highest = image.data[i];
                if (image.data[i] < lowest)    lowest = image.data[i];
            }
            */

            // flag that the range was set
            niftiRangeIsSet = true;

        }
        

        public void setNiftiRange(int lowest, int highest) {
            
            // make sure lowest is lower or equal to highest
            if (lowest > highest)   return;

            // check lower range is within the data format
            if ((localDataType == DT_BYTE || localDataType == DT_USHORT) && lowest < 0)     return;

            // check higher range is within the data format
            if ((localDataType == DT_BYTE) && highest > 255)        return;
            if ((localDataType == DT_USHORT) && highest > 65535)    return;

            // set the values in the object
            niftiLowest = lowest;
            niftiHighest = highest;

            // flag the range as set
            niftiRangeIsSet = true;

        }

        public int getNiftiLowest() {
            if (!niftiRangeIsSet)   determineNiftiRange();
            return niftiLowest;
        }

        public int getNiftiHighest() {
            if (!niftiRangeIsSet)   determineNiftiRange();
            return niftiHighest;
        }

    }

}
