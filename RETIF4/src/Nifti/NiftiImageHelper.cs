/**
 * NiftiImageHelper class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RETIF4.Nifti {
    public static class NiftiImageHelper {

        // Get the number of slices on the X axis.
        public static int getNumberOfXSlices(NiftiImage nii, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null) return 0;


            // 
            if ((int)displayReorient >= 0 && (int)displayReorient <= 7) {
                // Flips & Rotate 180
                return nii.nx;

            } else if ((int)displayReorient >= 8 && (int)displayReorient <= 15) {
                // Rotate X 90
                return nii.nx;

            } else if ((int)displayReorient >= 16 && (int)displayReorient <= 23) {
                // Rotate Y 90
                return nii.nz;

            } else if ((int)displayReorient >= 24 && (int)displayReorient <= 31) {
                // Rotate Z 90
                return nii.ny;

            } else if ((int)displayReorient >= 32 && (int)displayReorient <= 39) {
                // Rotate X 90 + Y 90 | Rotate Y 90 + Z 90
                return nii.ny;

            } else if ((int)displayReorient >= 40 && (int)displayReorient <= 47) {
                // Rotate X 90 + Z 90
                return nii.nz;

            }
            
            return 0;

        }


        // Get the dimensions of a slice orthgonal to the X axis.
        public static Size getXSliceDimensions(NiftiImage nii, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null)        return new Size(0, 0);
            
            // 
            if ((int)displayReorient >= 0 && (int)displayReorient <= 7) {
                // Flips & Rotate 180
                return new Size(nii.ny, nii.nz);

            } else if ((int)displayReorient >= 8 && (int)displayReorient <= 15) {
                // Rotate X 90
                return new Size(nii.nz, nii.ny);

            } else if ((int)displayReorient >= 16 && (int)displayReorient <= 23) {
                // Rotate Y 90
                return new Size(nii.ny, nii.nx);

            } else if ((int)displayReorient >= 24 && (int)displayReorient <= 31) {
                // Rotate Z 90
                return new Size(nii.nx, nii.nz);

            } else if ((int)displayReorient >= 32 && (int)displayReorient <= 39) {
                // Rotate X 90 + Y 90 | Rotate Y 90 + Z 90
                return new Size(nii.nz, nii.nx);

            } else if ((int)displayReorient >= 40 && (int)displayReorient <= 47) {
                // Rotate X 90 + Z 90
                return new Size(nii.nx, nii.ny);

            }
            
            return new Size(0, 0);
        }


        // Get a slice orthogonal to the X axis. (in fact a RotX90CC + RotY90????? transformation)
        //
        // If (by data or using the 'displayReorient' parameter) the nifi is (re-)oriented correctly (top toward z=0, front toward y=0, left toward x=0) then
        // this would return the sagittal (aka medial) plane
        //
        public static Bitmap getXSliceAsBitmap24bit(NiftiImage nii, int slice, int displayMin, int displayMax, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null)        return null;
            /////////////////////////////////////if (slice >= nii.nx)   return null;

            // determine how much color difference every change in value is
            double displayDelta = 255.0 / (displayMax - displayMin);

            // calculate the z plane size
            int plane = nii.nx * nii.ny;


            int oldIndex = 0;
            Bitmap bmp = new Bitmap(nii.ny, nii.nz);
            BitmapData data = bmp.LockBits(new Rectangle(new Point(0), bmp.Size), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe {

                byte* ptr = (byte*)data.Scan0;
                byte color = 0;
                byte* ptrColor = &color;
                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte
                        
                    for (int y = 0; y < bmp.Height; y++) {
                        byte* ptr2 = ptr;
                        for (int x = bmp.Width - 1; x >= 0; x--) {
                            oldIndex = slice + x * nii.nx + y * plane;
                            *ptrColor = (byte)(nii.byteData[oldIndex] * displayDelta);
                            *(ptr2++) = *ptrColor;
                            *(ptr2++) = *ptrColor;
                            *(ptr2++) = *ptrColor;
                        }
                        ptr += data.Stride;
                    }

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                } else {
                    // int


                }

            }
            bmp.UnlockBits(data);
            



            return bmp;

        }




        // Get the number of slices on the Y axis.
        public static int getNumberOfYSlices(NiftiImage nii, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null) return 0;

            // 
            if ((int)displayReorient >= 0 && (int)displayReorient <= 7) {
                // Flips & Rotate 180
                return nii.ny;

            } else if ((int)displayReorient >= 8 && (int)displayReorient <= 15) {
                // Rotate X 90
                return nii.nz;

            } else if ((int)displayReorient >= 16 && (int)displayReorient <= 23) {
                // Rotate Y 90
                return nii.ny;
                
            } else if ((int)displayReorient >= 24 && (int)displayReorient <= 31) {
                // Rotate Z 90
                return nii.nx;

            } else if ((int)displayReorient >= 32 && (int)displayReorient <= 39) {
                // Rotate X 90 + Y 90 | Rotate Y 90 + Z 90
                return nii.nz;

            } else if ((int)displayReorient >= 40 && (int)displayReorient <= 47) {
                // Rotate X 90 + Z 90
                return nii.nx;

            }

            return 0;
            
        }

        // Get the dimensions of a slice orthgonal to the Y axis.
        public static Size getYSliceDimensions(NiftiImage nii, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null)        return new Size(0, 0);
            
            // 
            if ((int)displayReorient >= 0 && (int)displayReorient <= 7) {
                // Flips & Rotate 180
                return new Size(nii.nx, nii.nz);

            } else if ((int)displayReorient >= 8 && (int)displayReorient <= 15) {
                // Rotate X 90
                return new Size(nii.nx, nii.ny);

            } else if ((int)displayReorient >= 16 && (int)displayReorient <= 23) {
                // Rotate Y 90
                return new Size(nii.nz, nii.nx);

            } else if ((int)displayReorient >= 24 && (int)displayReorient <= 31) {
                // Rotate Z 90
                return new Size(nii.ny, nii.nz);

            } else if ((int)displayReorient >= 32 && (int)displayReorient <= 39) {
                // Rotate X 90 + Y 90 | Rotate Y 90 + Z 90
                return new Size(nii.ny, nii.nx);

            } else if ((int)displayReorient >= 40 && (int)displayReorient <= 47) {
                // Rotate X 90 + Z 90
                return new Size(nii.nz, nii.ny);

            }
            
            return new Size(0, 0);
        }


        // Get a slice orthogonal to the Y axis. (in fact a 90XCC rotated + flip Z transformation)
        //
        // If (by data or using the 'displayReorient' parameter) the nifi is (re-)oriented correctly (top toward z=0, front toward y=0, left toward x=0) then
        // this would return the coronal (aka frontal) plane
        //
        public static Bitmap getYSliceAsBitmap24bit(NiftiImage nii, int slice, int displayMin, int displayMax, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null)        return null;
            if (slice >= nii.ny)   return null;

            // determine how much color difference every change in value is
            double displayDelta = 255.0 / (displayMax - displayMin);

            // calculate the z plane size
            int plane = nii.nx * nii.ny;


            int oldIndex = 0;
            Bitmap bmp = new Bitmap(nii.nx, nii.nz);
            BitmapData data = bmp.LockBits(new Rectangle(new Point(0), bmp.Size), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe {

                byte* ptr = (byte*)data.Scan0;
                byte color = 0;
                byte* ptrColor = &color;
                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte
                        
                    for (int y = 0; y < bmp.Height; y++) {
                        byte* ptr2 = ptr;
                        for (int x = 0; x < bmp.Width; x++) {

                            oldIndex = x + slice * nii.nx + y * plane;
                            
                            //oldIndex = x + ySlice * nii.nx + ((nii.nz - 1 - y) * plane);
                            //oldIndex = x + (nii.ny - 1 - slice) * nii.nx + y * plane;
                            *ptrColor = (byte)(nii.byteData[oldIndex] * displayDelta);
                            *(ptr2++) = *ptrColor;
                            *(ptr2++) = *ptrColor;
                            *(ptr2++) = *ptrColor;
                        }
                        ptr += data.Stride;
                    }

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                } else {
                    // int


                }

            }
            bmp.UnlockBits(data);
            


            /*
            bmp = new Bitmap(nii.nx, nii.nz);
            //BitmapData data = bmp.LockBits(new Rectangle(new Point(0), bmp.Size), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int i = 0;
            for (int y = 0; y < nii.nz; y++) {
                for (int x = 0; x < nii.nx; x++) {
                    oldIndex = x + ySlice * nii.nx + ((nii.nz - 1 - y) * plane);

                    int value = nii.byteData[oldIndex];
                    //int value = nii.byteData[startpoint + i];
                    value = (int)(value * displayDelta);
                    bmp.SetPixel(x, y, Color.FromArgb(value, value, value));
                    
                    i++;
                }
            }
            */


            /*
            bmp = new Bitmap(nii.nx, nii.nz);
            //BitmapData data = bmp.LockBits(new Rectangle(new Point(0), bmp.Size), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int i = 0;
            for (int y = 0; y < nii.nz; y++) {
                for (int x = 0; x < nii.nx; x++) {
                    oldIndex = x + ySlice * nii.nx + ((nii.nz - 1 - y) * plane);

                    int value = nii.byteData[oldIndex];
                    //int value = nii.byteData[startpoint + i];
                    value = (int)(value * displayDelta);
                    bmp.SetPixel(x, y, Color.FromArgb(value, value, value));
                    
                    i++;
                }
            }
            */


            return bmp;

        }


        // Get the number of slices on the Z axis.
        public static int getNumberOfZSlices(NiftiImage nii, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null) return 0;

            // 
            if ((int)displayReorient >= 0 && (int)displayReorient <= 7) {
                // Flips & Rotate 180
                return nii.nz;

            } else if ((int)displayReorient >= 8 && (int)displayReorient <= 15) {
                // Rotate X 90
                return nii.ny;

            } else if ((int)displayReorient >= 16 && (int)displayReorient <= 23) {
                // Rotate Y 90
                return nii.nx;

            } else if ((int)displayReorient >= 24 && (int)displayReorient <= 31) {
                // Rotate Z 90
                return nii.nz;

            } else if ((int)displayReorient >= 32 && (int)displayReorient <= 39) {
                // Rotate X 90 + Y 90 | Rotate Y 90 + Z 90
                return nii.nx;

            } else if ((int)displayReorient >= 40 && (int)displayReorient <= 47) {
                // Rotate X 90 + Z 90
                return nii.ny;

            }

            return 0;

        }

        // Get the dimensions of a slice orthgonal to the Z axis.
        public static Size getZSliceDimensions(NiftiImage nii, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null)        return new Size(0, 0);
            
            // 
            if ((int)displayReorient >= 0 && (int)displayReorient <= 7) {
                // Flips & Rotate 180
                return new Size(nii.nx, nii.ny);

            } else if ((int)displayReorient >= 8 && (int)displayReorient <= 15) {
                // Rotate X 90
                return new Size(nii.nx, nii.nz);

            } else if ((int)displayReorient >= 16 && (int)displayReorient <= 23) {
                // Rotate Y 90
                return new Size(nii.nz, nii.ny);

            } else if ((int)displayReorient >= 24 && (int)displayReorient <= 31) {
                // Rotate Z 90
                return new Size(nii.ny, nii.nx);

            } else if ((int)displayReorient >= 32 && (int)displayReorient <= 39) {
                // Rotate X 90 + Y 90 | Rotate Y 90 + Z 90
                return new Size(nii.ny, nii.nz);

            } else if ((int)displayReorient >= 40 && (int)displayReorient <= 47) {
                // Rotate X 90 + Z 90
                return new Size(nii.nz, nii.nx);
            }
            
            return new Size(0, 0);
        }

        /**
         * Retrieve a 8-bit Indexed bitmap of a Z-slice
         * Only works if the datatype of the nifti is byte (getZSliceAsBitmap24bit uses this method automatically if possible)
         */
        private static Bitmap getZSliceAsBitmap8bitIndexed(NiftiImage nii, int zSlice, int displayMin, int displayMax) {
            if (nii == null)                                return null;
            if (nii.localDataType != NiftiImage.DT_BYTE)    return null;
            if (zSlice >= nii.nz)                           return null;
            if (displayMin < 0)                             return null;
            if (displayMax > 255)                           return null;
            if (displayMin > displayMax)                    return null;
            
            // calculate the starting point index
            int startpoint = nii.nx * nii.ny * zSlice;

            // create a 8bpp index bitmap
            Bitmap bmpGrey = new Bitmap(nii.nx, nii.ny, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bmpGrey.LockBits(new Rectangle(new Point(0), bmpGrey.Size), ImageLockMode.WriteOnly, bmpGrey.PixelFormat);
            for (int i = 0; i < nii.ny; i++) {
                int offset = startpoint + i * nii.nx;
                int scanOffset = i * bmpData.Stride;
                Marshal.Copy(nii.byteData, offset, new IntPtr(bmpData.Scan0.ToInt32() + scanOffset), nii.nx);
            }
            //Marshal.Copy(nii.data, startpoint, bmpData.Scan0, (nii.nx * nii.ny));     // gives a skewed image for some reaseon (wrong Stride?)
            bmpGrey.UnlockBits(bmpData);
            ColorPalette pal = bmpGrey.Palette;

            // determine the stepsize
            double displayDelta = 255.0 / (displayMax - displayMin);
            double displayColor = 0;
            for (int i = 0; i < displayMin; i++)    pal.Entries[i] = Color.Black;
            for (int i = displayMin; i < displayMax; i++) {
                pal.Entries[i] = Color.FromArgb((int)displayColor, (int)displayColor, (int)displayColor);
                displayColor += displayDelta;
            }
            for (int i = displayMax; i < 256; i++)  pal.Entries[i] = Color.White;
            bmpGrey.Palette = pal;

            return bmpGrey;

        }

        // Get a slice orthogonal to the Z axis.
        //
        // If (by data or using the 'displayReorient' parameter) the nifi is (re-)oriented correctly (top toward z=0, front toward y=0, left toward x=0) then
        // this would return the transverse (aka axial aka horizontal) plane
        //
        public static Bitmap getZSliceAsBitmap24bit(NiftiImage nii, int zSlice, int displayMin, int displayMax, NiftiImage.OrientationTransform displayReorient) {
            if (nii == null)        return null;
            if (zSlice >= nii.nz)   return null;

            // calculate the starting point index
            int startpoint = nii.nx * nii.ny * zSlice;

            Bitmap bmp = null;

            if (nii.localDataType == NiftiImage.DT_BYTE && displayMin >= 0 && displayMax <= 255 && displayReorient == NiftiImage.OrientationTransform.None) {
                // for nifti of the unsigned data type and a displayrange between 0 and 255 and no display reorientation to be applied, we have a faster method
                // by first creating a 8bpp index bitmap and then converting that to a 24-bit bitmap

                // create a 8bpp index bitmap and convert to 24-bit bitmap = 0,6ms
                Bitmap bmpGrey = getZSliceAsBitmap8bitIndexed(nii, zSlice, displayMin, displayMax);
                bmp = new Bitmap(bmpGrey.Width, bmpGrey.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(bmp);
                g.DrawImage(bmpGrey, 0, 0, bmpGrey.Width, bmpGrey.Height);
                g.Dispose();
                bmpGrey.Dispose();
                            
            } else {
                // for all other datatypes and/or if the range is not between 0 and 255, use this method

                // determine how much color difference every change in value is
                double displayDelta = 255.0 / (displayMax - displayMin);
                
                // create new 24 bitmap using unsafe pointer = 7ms
                bmp = new Bitmap(nii.nx, nii.ny);
                BitmapData data = bmp.LockBits(new Rectangle(new Point(0), bmp.Size), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                unsafe {

                    int i = startpoint;
                    byte* ptr = (byte*)data.Scan0;
                    byte color = 0;
                    byte* ptrColor = &color;

                    if (nii.localDataType == NiftiImage.DT_BYTE){
                        // byte
                        
                        for (int y = 0; y < bmp.Height; y++) {
                            byte* ptr2 = ptr;
                            for (int x = 0; x < bmp.Width; x++) {
                                *ptrColor = (byte)(nii.byteData[i] * displayDelta);
                                *(ptr2++) = *ptrColor;
                                *(ptr2++) = *ptrColor;
                                *(ptr2++) = *ptrColor;
                                i++;
                            }
                            ptr += data.Stride;
                        }

                    } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                        // ushort

                        for (int y = 0; y < bmp.Height; y++) {
                            byte* ptr2 = ptr;
                            for (int x = 0; x < bmp.Width; x++) {
                                *ptrColor = (byte)(nii.ushortData[i] * displayDelta);
                                *(ptr2++) = *ptrColor;
                                *(ptr2++) = *ptrColor;
                                *(ptr2++) = *ptrColor;
                                i++;
                            }
                            ptr += data.Stride;
                        }

                    } else {
                        // int

                        for (int y = 0; y < bmp.Height; y++) {
                            byte* ptr2 = ptr;
                            for (int x = 0; x < bmp.Width; x++) {
                                *ptrColor = (byte)(nii.intData[i] * displayDelta);
                                *(ptr2++) = *ptrColor;
                                *(ptr2++) = *ptrColor;
                                *(ptr2++) = *ptrColor;
                                i++;
                            }
                            ptr += data.Stride;
                        }

                    }

                } // end: unsafe
                bmp.UnlockBits(data);

            }

            return bmp;

        }



    }
}
