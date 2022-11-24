/**
 * MatrixHelper class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;

namespace RETIF4.Helpers {

    public static class MatrixHelper {

        public static void printMatrix(double[,] matrix) {
            int rowLength = matrix.GetLength(0);
            int colLength = matrix.GetLength(1);
            string arrayString = "";
            for (int i = 0; i < rowLength; i++) {
                for (int j = 0; j < colLength; j++) {
                    if (j > 0)              arrayString += "  ";
                    if (matrix[i, j] >= 0)  arrayString += " ";
                    arrayString += string.Format("{0:0.000}", matrix[i, j]);
                }
                arrayString += System.Environment.NewLine + System.Environment.NewLine;
            }
            Console.WriteLine(arrayString);
        }

        public static void printMatrix(double[][] matrix) {
            int rowLength = matrix.Length;
            int colLength = matrix[0].Length;
            string arrayString = "";
            for (int i = 0; i < rowLength; i++) {
                for (int j = 0; j < colLength; j++) {
                    if (j > 0)              arrayString += "  ";
                    if (matrix[i][j] >= 0)  arrayString += " ";
                    arrayString += string.Format("{0:0.000}", matrix[i][j]);
                }
                arrayString += System.Environment.NewLine + System.Environment.NewLine;
            }
            Console.WriteLine(arrayString);
        }



        
        private static double[][] copyMatrix(double[][] source) {
            int len = source.Length;
            double[][] dest = new double[len][];
            for (int x = 0; x < len; x++) {
                //double[] inner = source[x];
                //int ilen = inner.Length;
                //double[] newer = new double[ilen];
                //Array.Copy(inner, newer, ilen);
                //dest[x] = newer;
                dest[x] = (double[])source[x].Clone();
            }
            return dest;
        }
        
        public static double[][] MatrixInverse_Crout(double[][] matrix) {

            // assumes determinant is not 0
            // that is, the matrix does have an inverse
            int n = matrix.Length;
            double[][] result = copyMatrix(matrix);

            double[][] lum; // combined lower & upper
            int[] perm;
            int toggle;
            toggle = MatrixDecompose_Crout(matrix, out lum, out perm);

            // check if an inverse could not be computed (extra)
            double resultT = toggle;
            for (int i = 0; i < lum.Length; ++i)
                resultT *= lum[i][i];
            if (Math.Abs(resultT) < 1.0e-5)
                throw new Exception("Unable to compute inverse");


            double[] b = new double[n];
            for (int i = 0; i < n; ++i) {
                for (int j = 0; j < n; ++j)
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;

                double[] x = MatrixHelperSolve(lum, b); // 
                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }

            return result;
        } // MatrixInverse


        private static double[] MatrixHelperSolve(double[][] luMatrix, double[] b) {

            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix
            int n = luMatrix.Length;
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i) {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i) {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }

            return x;
        }

        private static int MatrixDecompose_Crout(double[][] m, out double[][] lum, out int[] perm) {
            // Crout's LU decomposition for matrix determinant and inverse
            // stores combined lower & upper in lum[][]
            // stores row permuations into perm[]
            // returns +1 or -1 according to even or odd number of row permutations
            // lower gets dummy 1.0s on diagonal (0.0s above)
            // upper gets lum values on diagonal (0.0s below)

            int toggle = +1; // even (+1) or odd (-1) row permutatuions
            int n = m.Length;

            // make a copy of m[][] into result lu[][]
            lum = copyMatrix(m);

            // make perm[]
            perm = new int[n];
            for (int i = 0; i < n; ++i)
                perm[i] = i;

            for (int j = 0; j < n - 1; ++j) {   // process by column. note n-1 
                double max = Math.Abs(lum[j][j]);
                int piv = j;

                for (int i = j + 1; i < n; ++i) {   // find pivot index
                    double xij = Math.Abs(lum[i][j]);
                    if (xij > max) {
                        max = xij;
                        piv = i;
                    }
                } 

                if (piv != j) {
                    double[] tmp = lum[piv]; // swap rows j, piv
                    lum[piv] = lum[j];
                    lum[j] = tmp;

                    int t = perm[piv]; // swap perm elements
                    perm[piv] = perm[j];
                    perm[j] = t;

                    toggle = -toggle;
                }

                double xjj = lum[j][j];
                if (xjj != 0.0) {
                    for (int i = j + 1; i < n; ++i) {
                        double xij = lum[i][j] / xjj;
                        lum[i][j] = xij;
                        for (int k = j + 1; k < n; ++k)
                            lum[i][k] -= xij * lum[j][k];
                    }
                }

            } // j

            return toggle;
        }


        public static double[][] MatrixInverse_Doolittle(double[][] matrix) {
            int n = matrix.Length;
            double[][] result = copyMatrix(matrix);

            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose_Doolittle(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");

            double[] b = new double[n];
            for (int i = 0; i < n; ++i) {
                for (int j = 0; j < n; ++j) {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }

                double[] x = MatrixHelperSolve(lum, b); // 

                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }

            return result;
        }


        private static double[][] MatrixDecompose_Doolittle(double[][] matrix, out int[] perm, out int toggle) {
            // Doolittle LUP decomposition with partial pivoting.
            // rerturns: result is L (with 1s on diagonal) and U;
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = matrix.Length;
            int cols = matrix[0].Length; // assume square
            if (rows != cols)
                throw new Exception("Attempt to decompose a non-square m");

            int n = rows; // convenience

            double[][] result = copyMatrix(matrix);

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; ++i) { perm[i] = i; }

            toggle = 1; // toggle tracks row swaps.
                        // +1 = even, -1 = odd. used by MatrixDeterminant

            for (int j = 0; j < n - 1; ++j) { // each column

                double colMax = Math.Abs(result[j][j]); // find largest val in col
                int pRow = j;
                //for (int i = j + 1; i < n; ++i) {
                //  if (result[i][j] > colMax) {
                //    colMax = result[i][j];
                //    pRow = i;
                //  }
                //}

                // reader Matt V needed this:
                for (int i = j + 1; i < n; ++i) {
                    if (Math.Abs(result[i][j]) > colMax) {
                        colMax = Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }
                // Not sure if this approach is needed always, or not.

                if (pRow != j) { // if largest value not on pivot, swap rows
                    double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // --------------------------------------------------
                // This part added later (not in original)
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j
                // --------------------------------------------------

                if (result[j][j] == 0.0) {
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row) {
                        if (result[row][j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    double[] rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }
                // --------------------------------------------------
                // if diagonal after swap is zero . .
                //if (Math.Abs(result[j][j]) < 1.0E-20) 
                //  return null; // consider a throw

                for (int i = j + 1; i < n; ++i) {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; ++k) {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }

            } // main j column loop

            return result;
        }

    }
}
