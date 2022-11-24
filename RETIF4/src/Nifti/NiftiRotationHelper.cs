/**
 * NiftiRotationHelper class
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
    public static class NiftiRotationHelper {

        public enum Rotations : int {
            NoRot = 0,          // do not rotate (along this axis)
            Rot90 = 1,          // rotate 90 degrees clockwise
            Rot90CC = 2         // rotate 90 degrees counter-clockwise
        };

        // no rotations, all flips
        // rotx

        /// <summary>
        /// Rotate and/or flip nifti data on one or more axis 
        /// First does 90 degrees rotations (clockwise or counterclockwise) on the X, Y and Z axis respectively and then applies X, Y and Z flips respectively.
        /// 
        /// about 6ms
        /// </summary>
        public static void RotateAndFlipNiftiData(NiftiImage nii, Rotations rotX, Rotations rotY, Rotations rotZ, bool flipX, bool flipY, bool flipZ) {

            // check if no rotations or flips have to be applied
            if (rotX == Rotations.NoRot && rotY == Rotations.NoRot && rotZ == Rotations.NoRot && !flipX && !flipY && !flipZ) return;

            // check if the data should only be flipped
            if (rotX == Rotations.NoRot && rotY == Rotations.NoRot && rotZ == Rotations.NoRot && (flipX || flipY || flipZ)) {
                FlipNiftiData(nii, flipX, flipY, flipZ);
            }

            // check if only the X should be rotated (including all flip possibilities)
            if ((rotX == Rotations.Rot90 || rotX == Rotations.Rot90CC) && rotY == Rotations.NoRot && rotZ == Rotations.NoRot) {
                RotateXNiftiData(nii, rotX, flipX, flipY, flipZ);
            }

            // check if only the Y should be rotated (including all flip possibilities)
            if (rotX == Rotations.NoRot && (rotY == Rotations.Rot90 || rotY == Rotations.Rot90CC) && rotZ == Rotations.NoRot) {
                RotateYNiftiData(nii, rotY, flipX, flipY, flipZ);
            }

            // check if only the Z should be rotated (including all flip possibilities)
            if (rotX == Rotations.NoRot && rotY == Rotations.NoRot && (rotZ == Rotations.Rot90 || rotZ == Rotations.Rot90CC)) {
                RotateZNiftiData(nii, rotZ, flipX, flipY, flipZ);
            }





            // TODO: RotX + RotY + RotZ not yet covered


        }

        private static void FlipNiftiData(NiftiImage nii, bool flipX, bool flipY, bool flipZ) {
            
            // return immediately if no flips have to be applied
            if (!flipX && !flipY && !flipZ)     return;

            // set the new dimensions for transformations (flips / rot 180)
            int newXSize = nii.nx;
            int newYSize = nii.ny;
            int newZSize = nii.nz;

            // variables for the loop
            int oldIndex = 0;
            int newYPos = 0;
            int newZPos = 0;
            int newIndex = 0;

            // calculate the plane size beforehand
            int zPlaneSize = newXSize * newYSize;

            // select the right loop to execute
            // (seperate loops because it is faster then re-evaluating 'if' statements during the loop)
            if (flipX && !flipY && !flipZ) {
                // Flip X

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (copy not necessary because the z is incremental)
                            oldIndex = newZPos * zPlaneSize + newYPos * newXSize;
                            Array.Reverse(nii.byteData, oldIndex, newXSize);
                        }
                    }

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (copy not necessary because the z is incremental)
                            oldIndex = newZPos * zPlaneSize + newYPos * newXSize;
                            Array.Reverse(nii.ushortData, oldIndex, newXSize);
                        }
                    }

                }

            } else if (!flipX && flipY && !flipZ) {
                // Flip Y
                
                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = newZPos * zPlaneSize + (newYSize - 1 - newYPos) * newXSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = newZPos * zPlaneSize + (newYSize - 1 - newYPos) * newXSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if (!flipX && !flipY && flipZ) {
                // Flip Z

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        // x and y are done as a block copy
                        oldIndex = (newZSize - 1 - newZPos) * zPlaneSize;
                        Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, zPlaneSize);
                        newIndex += zPlaneSize;
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        // x and y are done as a block copy
                        oldIndex = (newZSize - 1 - newZPos) * zPlaneSize;
                        Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, zPlaneSize);
                        newIndex += zPlaneSize;
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if (flipX && flipY && !flipZ) {
                // Flip X and Y

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        // x and y are done as a reverse (copy not necessary because the z is incremental)
                        oldIndex = newZPos * zPlaneSize;
                        Array.Reverse(nii.byteData, oldIndex, zPlaneSize);
                    }

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        // x and y are done as a reverse (copy not necessary because the z is incremental)
                        oldIndex = newZPos * zPlaneSize;
                        Array.Reverse(nii.ushortData, oldIndex, zPlaneSize);
                    }

                }

            } else if (flipX && !flipY && flipZ) {
                // Flip X and Z

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z is transposed)
                            oldIndex = (newZSize - 1 - newZPos) * zPlaneSize + newYPos * newXSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z is transposed)
                            oldIndex = (newZSize - 1 - newZPos) * zPlaneSize + newYPos * newXSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if (!flipX && flipY && flipZ) {
                // Flip Y and Z

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = (newZSize - 1 - newZPos) * zPlaneSize + (newYSize - 1 - newYPos) * newXSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = (newZSize - 1 - newZPos) * zPlaneSize + (newYSize - 1 - newYPos) * newXSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if (flipX && flipY && flipZ) {
                // Flip X, Y and Z

                Array.Reverse(nii.byteData);

            }
            /*
            for (newZPos = 0; newZPos < newZSize; newZPos++) {
                for (newYPos = 0; newYPos < newYSize; newYPos++) {
                    for (newXPos = 0; newXPos < newXSize; newXPos++) {
                        //Console.WriteLine(i2 + " -> newZPos: " + newZPos + "       newYPos: " + newYPos + "       newXPos: " + newXPos);



                        // RotX 90
                        //oldIndex = newZPos * newXSize + (newYSize - 1 - newYPos) * (newXSize * newZSize) + newXPos;

                        // RotY 90
                        //oldIndex = (newZSize - 1 - newZPos) + newYPos * newZSize + newXPos * (newYSize * newZSize);

                        // RotX 180
                        //oldIndex = newZPos * (newXSize * newYSize) + (newYSize - 1 - newYPos) * newXSize + (newXSize - 1 - newXPos);

                        // RotZ 90
                        //oldIndex = newZPos * (newXSize * newYSize) + newYPos + (newXSize - 1 - newXPos) * newYSize;

                        // RotX + RotY / RotY + RotZ
                        //oldIndex = (newZSize - 1 - newZPos) + (newYSize - 1 - newYPos) * (newXSize * newZSize) + newXPos * newZSize;

                        // RotX + RotZ
                        oldIndex = newZPos * newYSize + newYPos + newXPos * (newYSize * newZSize);





                        //Console.WriteLine(i2 + " -> oldIndex: " + oldIndex);
                        newBuffer[i2] = nii.byteData[oldIndex];

                        //Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, i2, newXSize);

                        i2++;
                    }
                }
            }
            */



        }

        private static void RotateXNiftiData(NiftiImage nii, Rotations rotX, bool flipX, bool flipY, bool flipZ) {
            
            // return immediately if no rotations have to be applied
            if (rotX == Rotations.NoRot)     return;
            
            // set the new dimensions for transformations
            int newXSize = nii.nx;
            int newYSize = nii.nz;
            int newZSize = nii.ny;

            // variables for the loop
            int oldIndex = 0;
            int newYPos = 0;
            int newZPos = 0;
            int newIndex = 0;

            // calculate the plane size beforehand
            int yPlaneSize = newXSize * newZSize;

            // select the right loop to execute
            // (seperate loops because it is faster then re-evaluating 'if' statements during the loop)
            if ((rotX == Rotations.Rot90CC && !flipX && !flipY && !flipZ) || (rotX == Rotations.Rot90 && !flipX && flipY && flipZ)) {
                // RotX 90 CC  &  RotX 90 + flip Y and Z
                
                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = (newZSize - 1 - newZPos) * newXSize + newYPos * yPlaneSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = (newZSize - 1 - newZPos) * newXSize + newYPos * yPlaneSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if ((rotX == Rotations.Rot90CC && flipX && !flipY && !flipZ) || (rotX == Rotations.Rot90 && flipX && flipY && flipZ)) {
                // RotX 90 CC + flip X  &  RotX 90 + flip X, Y and Z

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z and y are transposed)
                            oldIndex = (newZSize - 1 - newZPos) * newXSize + newYPos * yPlaneSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z and y are transposed)
                            oldIndex = (newZSize - 1 - newZPos) * newXSize + newYPos * yPlaneSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if ((rotX == Rotations.Rot90CC && !flipX && flipY && !flipZ) || (rotX == Rotations.Rot90 && !flipX && !flipY && flipZ)) {
                // RotX 90 CC + flip Y  &  RotX 90 + flip Z
                
                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = (newZSize - 1 - newZPos) * newXSize + (newYSize - 1 - newYPos) * yPlaneSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = (newZSize - 1 - newZPos) * newXSize + (newYSize - 1 - newYPos) * yPlaneSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if ((rotX == Rotations.Rot90CC && !flipX && !flipY && flipZ) || (rotX == Rotations.Rot90 && !flipX && flipY && !flipZ)) {
                // RotX 90 CC + flip Z  &  RotX 90 + flip Y

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = newZPos * newXSize + newYPos * yPlaneSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = newZPos * newXSize + newYPos * yPlaneSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if ((rotX == Rotations.Rot90CC && flipX && flipY && !flipZ) || (rotX == Rotations.Rot90 && flipX && !flipY && flipZ)) {
                // RotX 90 CC + flip X and Y  &  RotX 90 + flip X and Z

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z and y are transposed)
                            oldIndex = (newZSize - 1 - newZPos) * newXSize + (newYSize - 1 - newYPos) * yPlaneSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z and y are transposed)
                            oldIndex = (newZSize - 1 - newZPos) * newXSize + (newYSize - 1 - newYPos) * yPlaneSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if ((rotX == Rotations.Rot90CC && !flipX && flipY && flipZ) || (rotX == Rotations.Rot90 && !flipX && !flipY && !flipZ)) {
                // RotX 90 CC + flip Y and Z  &  RotX 90

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = newZPos * newXSize + (newYSize - 1 - newYPos) * yPlaneSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a block copy
                            oldIndex = newZPos * newXSize + (newYSize - 1 - newYPos) * yPlaneSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if ((rotX == Rotations.Rot90CC && flipX && !flipY && flipZ) || (rotX == Rotations.Rot90 && flipX && flipY && !flipZ)) {
                // RotX 90 CC + flip X and Z  &  RotX 90 + flip X and Y

                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte

                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z and y are transposed)
                            oldIndex = newZPos * newXSize + newYPos * yPlaneSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z and y are transposed)
                            oldIndex = newZPos * newXSize + newYPos * yPlaneSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            } else if ((rotX == Rotations.Rot90CC && flipX && flipY && flipZ) || (rotX == Rotations.Rot90 && flipX && !flipY && !flipZ)) {
                // RotX 90 CC + flip X, Y and Z  &  RotX 90 + flip X
                
                if (nii.localDataType == NiftiImage.DT_BYTE) {
                    // byte
                    
                    // transform
                    byte[] newBuffer = new byte[nii.byteData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z and y are transposed)
                            oldIndex = newZPos * newXSize + (newYSize - 1 - newYPos) * yPlaneSize;
                            Buffer.BlockCopy(nii.byteData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.byteData = null;
                    nii.byteData = newBuffer;

                } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                    // ushort

                    // transform
                    ushort[] newBuffer = new ushort[nii.ushortData.Length];
                    for (newZPos = 0; newZPos < newZSize; newZPos++) {
                        for (newYPos = 0; newYPos < newYSize; newYPos++) {
                            // x is done as a reverse (including blockcopy, since the z and y are transposed)
                            oldIndex = newZPos * newXSize + (newYSize - 1 - newYPos) * yPlaneSize;
                            Buffer.BlockCopy(nii.ushortData, oldIndex, newBuffer, newIndex, newXSize);
                            Array.Reverse(newBuffer, newIndex, newXSize);
                            newIndex += newXSize;
                        }
                    }
                    nii.ushortData = null;
                    nii.ushortData = newBuffer;

                }

            }

            // apply the new dimensions
            nii.ny = newYSize;
            nii.nz = newZSize;

        }


        private unsafe static void RotateYNiftiData(NiftiImage nii, Rotations rotY, bool flipX, bool flipY, bool flipZ) {
            
            // return immediately if no rotations have to be applied
            if (rotY == Rotations.NoRot)     return;
            
            // set the new dimensions for transformations
            int newXSize = nii.nz;
            int newYSize = nii.ny;
            int newZSize = nii.nx;

            // set and calculate the multiplication factors beforehand
            int xMultiplier = newYSize * newZSize;
            int yMultiplier = newZSize;
            int zMultiplier = 1;

            // select the right transformation
            if ((rotY == Rotations.Rot90CC && !flipX && !flipY && !flipZ) || (rotY == Rotations.Rot90 && flipX && !flipY && flipZ)) {
                // RotY 90 CC  &  RotY 90 + flip X and Z
    
                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, true, false, false);

            } else if ((rotY == Rotations.Rot90CC && flipX && !flipY && !flipZ) || (rotY == Rotations.Rot90 && !flipX && !flipY && flipZ)) {
                // RotY 90 CC + flip X & RotY 90 + flip Z

                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, false, false, false);

            } else if ((rotY == Rotations.Rot90CC && !flipX && flipY && !flipZ) || (rotY == Rotations.Rot90 && flipX && flipY && flipZ)) {
                // RotY 90 CC + flip Y & RotY 90 + flip X, Y and Z

                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, true, true, false);

            } else if ((rotY == Rotations.Rot90CC && !flipX && !flipY && flipZ) || (rotY == Rotations.Rot90 && flipX && !flipY && !flipZ)) {
                // RotY 90 CC + flip Z & RotY 90 + flip X

                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, true, false, true);

            } else if ((rotY == Rotations.Rot90CC && flipX && flipY && !flipZ) || (rotY == Rotations.Rot90 && !flipX && flipY && flipZ)) {
                // RotY 90 CC + flip X and Y & RotY 90 + flip Y and Z

                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, false, true, false);

            } else if ((rotY == Rotations.Rot90CC && !flipX && flipY && flipZ) || (rotY == Rotations.Rot90 && flipX && flipY && !flipZ)) {
                // RotY 90 CC + flip Y and Z & RotY 90 + flip X and Y

                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, true, true, true);

            } else if ((rotY == Rotations.Rot90CC && flipX && !flipY && flipZ) || (rotY == Rotations.Rot90 && !flipX && !flipY && !flipZ)) {
                // RotY 90 CC + flip X and Z & RotY 90

                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, false, false, true);

            } else if ((rotY == Rotations.Rot90CC && flipX && flipY && flipZ) || (rotY == Rotations.Rot90 && !flipX && flipY && !flipZ)) {
                // RotY 90 CC + flip X , Y and Z & RotY 90 + flip Y

                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, false, true, true);

            }

            // apply the new dimensions
            nii.nx = newXSize;
            nii.nz = newZSize;

        }


        private unsafe static void RotateZNiftiData(NiftiImage nii, Rotations rotZ, bool flipX, bool flipY, bool flipZ) {
            
            // return immediately if no rotations have to be applied
            if (rotZ == Rotations.NoRot)     return;
            
            // set the new dimensions for transformations
            int newXSize = nii.ny;
            int newYSize = nii.nx;
            int newZSize = nii.nz;

            // set and calculate the multiplication factors beforehand
            int xMultiplier = newYSize;
            int yMultiplier = 1;
            int zMultiplier = newXSize * newYSize;

            // select the right transformation
            if ((rotZ == Rotations.Rot90CC && !flipX && !flipY && !flipZ) || (rotZ == Rotations.Rot90 && flipX && flipY && !flipZ)) {
                // RotZ 90 CC  &  RotZ 90 + flip X and Y

                rotateData(nii, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, false, true, false);

            } else if ((rotZ == Rotations.Rot90CC && flipX && !flipY && !flipZ) || (rotZ == Rotations.Rot90 && !flipX && flipY && !flipZ)) {
                // RotZ 90 CC + flip X & RotZ 90 + flip Y
                



            }

            // apply the new dimensions
            nii.nx = newXSize;
            nii.ny = newYSize;

        }


        private static void rotateData(NiftiImage nii, int newXSize, int newYSize, int newZSize, int xMultiplier, int yMultiplier, int zMultiplier, bool revX, bool revY, bool revZ) {
            
            if (nii.localDataType == NiftiImage.DT_BYTE) {
                // byte

                // transform
                byte[] newBuffer = rotateLoop(nii.byteData, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, revX, revY, revZ);
                nii.byteData = null;
                nii.byteData = newBuffer;

            } else if (nii.localDataType == NiftiImage.DT_USHORT) {
                // ushort

                // transform
                ushort[] newBuffer = rotateLoop(nii.ushortData, newXSize, newYSize, newZSize, xMultiplier, yMultiplier, zMultiplier, revX, revY, revZ);
                nii.ushortData = null;
                nii.ushortData = newBuffer;

            }

        }
        
        private static byte[] rotateLoop(byte[] oldBuffer, int newXSize, int newYSize, int newZSize, int xMultiplier, int yMultiplier, int zMultiplier, bool revX, bool revY, bool revZ) {
            
            // variables for the loop
            int oldIndex = 0;
            int newXPos = 0;
            int newYPos = 0;
            int newZPos = 0;              

            byte[] newBuffer = new byte[oldBuffer.Length];

            // select the right loop to execute
            // (seperate loops because it is faster then re-evaluating 'if' statements during the loop)
            unsafe {
                fixed (byte* newBufferFixed = newBuffer) {
                    byte* newBufferPtr = newBufferFixed;

                    if (!revX && !revY && !revZ) {
                        // 

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = 0; newXPos < newXSize; newXPos++) {
                                    oldIndex = newXPos * xMultiplier + newYPos * yMultiplier + newZPos * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (revX && !revY && !revZ) {
                        
                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = newXSize - 1; newXPos > -1; newXPos--) {
                                    oldIndex = newXPos * xMultiplier + newYPos * yMultiplier + newZPos * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (!revX && revY && !revZ) {
                        // 

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = 0; newXPos < newXSize; newXPos++) {
                                    oldIndex = newXPos * xMultiplier + (newYSize - 1 - newYPos) * yMultiplier + newZPos * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (revX && revY && !revZ) {

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = newXSize - 1; newXPos > -1; newXPos--) {
                                    oldIndex = newXPos * xMultiplier + (newYSize - 1 - newYPos) * yMultiplier + newZPos * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (!revX && !revY && revZ) {
                        // 

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = 0; newXPos < newXSize; newXPos++) {
                                    oldIndex = newXPos * xMultiplier + newYPos * yMultiplier + (newZSize - 1 - newZPos) * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (revX && !revY && revZ) {
                        
                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = newXSize - 1; newXPos > -1; newXPos--) {
                                    oldIndex = newXPos * xMultiplier + newYPos * yMultiplier + (newZSize - 1 - newZPos) * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (!revX && revY && revZ) {
                        // 

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = 0; newXPos < newXSize; newXPos++) {
                                    oldIndex = newXPos * xMultiplier + (newYSize - 1 - newYPos) * yMultiplier + (newZSize - 1 - newZPos) * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (revX && revY && revZ) {

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = newXSize - 1; newXPos > -1; newXPos--) {
                                    oldIndex = newXPos * xMultiplier + (newYSize - 1 - newYPos) * yMultiplier + (newZSize - 1 - newZPos) * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }
                        
                    }
                    // end: select the right loop to execute

                }
            }
            // end: allow pointers

            return newBuffer;

        }


        
        private static ushort[] rotateLoop(ushort[] oldBuffer, int newXSize, int newYSize, int newZSize, int xMultiplier, int yMultiplier, int zMultiplier, bool revX, bool revY, bool revZ) {
            
            // variables for the loop
            int oldIndex = 0;
            int newXPos = 0;
            int newYPos = 0;
            int newZPos = 0;              

            ushort[] newBuffer = new ushort[oldBuffer.Length];

            // allow pointers
            unsafe {
                fixed (ushort* newBufferFixed = newBuffer) {
                    ushort* newBufferPtr = newBufferFixed;

                    // select the right loop to execute
                    // (seperate loops because it is faster then re-evaluating 'if' statements during the loop)
                    if (!revX && !revY && !revZ) {
                        // 

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = 0; newXPos < newXSize; newXPos++) {
                                    oldIndex = newXPos * xMultiplier + newYPos * yMultiplier + newZPos * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (revX && !revY && !revZ) {
                        
                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = newXSize - 1; newXPos > -1; newXPos--) {
                                    oldIndex = newXPos * xMultiplier + newYPos * yMultiplier + newZPos * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (!revX && revY && !revZ) {
                        // 

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = 0; newXPos < newXSize; newXPos++) {
                                    oldIndex = newXPos * xMultiplier + (newYSize - 1 - newYPos) * yMultiplier + newZPos * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (revX && revY && !revZ) {

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = newXSize - 1; newXPos > -1; newXPos--) {
                                    oldIndex = newXPos * xMultiplier + (newYSize - 1 - newYPos) * yMultiplier + newZPos * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (!revX && !revY && revZ) {
                        // 

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = 0; newXPos < newXSize; newXPos++) {
                                    oldIndex = newXPos * xMultiplier + newYPos * yMultiplier + (newZSize - 1 - newZPos) * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (revX && !revY && revZ) {
                        
                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = newXSize - 1; newXPos > -1; newXPos--) {
                                    oldIndex = newXPos * xMultiplier + newYPos * yMultiplier + (newZSize - 1 - newZPos) * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (!revX && revY && revZ) {
                        // 

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = 0; newXPos < newXSize; newXPos++) {
                                    oldIndex = newXPos * xMultiplier + (newYSize - 1 - newYPos) * yMultiplier + (newZSize - 1 - newZPos) * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }

                    } else if (revX && revY && revZ) {

                        for (newZPos = 0; newZPos < newZSize; newZPos++) {
                            for (newYPos = 0; newYPos < newYSize; newYPos++) {
                                for (newXPos = newXSize - 1; newXPos > -1; newXPos--) {
                                    oldIndex = newXPos * xMultiplier + (newYSize - 1 - newYPos) * yMultiplier + (newZSize - 1 - newZPos) * zMultiplier;
                                    *newBufferPtr = oldBuffer[oldIndex];
                                    newBufferPtr++;
                                }
                            }
                        }
                        
                    }
                    // end: select the right loop to execute

                }
            }
            // end: allow pointers

            return newBuffer;

        }




    }

}
