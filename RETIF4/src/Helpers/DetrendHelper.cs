/**
 * DetrendHelper class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using RETIF4.Nifti;
using System;
using System.Diagnostics;


namespace RETIF4.Helpers {

    public static class DetrendHelper {


        /**
         * data, first dimension is time, second dimension the features
         **/
        public static double[] detrend(double[] data, bool full, int lambda) {

            // retrieve the number of timepoints
            int T = data.GetLength(0);
            
            // retrieve the matrix belonging to the amount of timepoints
            double[,] tm = getTime2DMatrix(T, lambda);

            //double[,] a = getTime2DMatrix(3, 200);
            //double[,] a2 = getTime2DMatrix(4, 200);
            //double[,] a3 = getTime2DMatrix(5, 200);
            //double[,] a4 = getTime2DMatrix(6, 200);
            //double[][] a5 = getTimeJaggedMatrix(400, 200);
            double[,] a5 = getTime2DMatrix(10, 200);

            try {

                Stopwatch sw = new Stopwatch();
                sw.Start();

                // create a managed buffer
                // half the byte size since we want to store them as ushort
                double[,] srcMat = a5;
                double[,] dd = new double[10, 10];
                //srcMat[0,0] = 11;

                
                //Console.WriteLine(iout);
                //Console.WriteLine(dstMat[0,0]);
                //dd = test(a5);
                /*
                for (int i = 0; i < 30; i++) {
                    //b5_crout = MatrixHelper.MatrixInverse_Crout(a5);
                    //b5_doolittle = MatrixHelper.MatrixInverse_Doolittle(a5);
                    //var inversematrix = Z.Inverse();

                    //mout = Z.Inverse();
                    int iout = NiftiDLL.detrendTest(srcMat, dd, srcMat.GetLength(0), srcMat.GetLength(1));
                }
                */

                sw.Stop();

                TimeSpan ts = sw.Elapsed;
                Console.WriteLine("RunTime s:" + ts.Seconds + "   ms:" + ts.Milliseconds + "     ticks:" + ts.Ticks);

                MatrixHelper.printMatrix(dd);

                /*
                MatrixHelper.print2DMatrix(a);
                Console.WriteLine("");
                MatrixHelper.print2DMatrix(a2);
                Console.WriteLine("");
                MatrixHelper.print2DMatrix(a3);
                Console.WriteLine("");
                MatrixHelper.print2DMatrix(a4);
                Console.WriteLine("");
                MatrixHelper.print2DMatrix(a5);

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");
                MatrixHelper.print2DMatrix(b5_crout);
                Console.WriteLine("");
                MatrixHelper.print2DMatrix(b5_doolittle);
                */


            } catch (Exception) {
                Console.WriteLine("fout");
            }


            //
            return new double[] { };
        }


        private static double[,] test(double[,] inmatrix) {
            /*
            double[][] outmatrix = new double[inmatrix.Length][];
            for (int i = 0; i < inmatrix.Length; i++)
                outmatrix[i] = new double[inmatrix[0].Length];
                */
            double[,] outmatrix = new double[inmatrix.GetLength(0), inmatrix.GetLength(0)];
            /*
            unsafe {
                int d = inmatrix.GetLength(0);
                fixed (double* S = &inmatrix[0,0]) {
                    fixed (double* D = &outmatrix[0, 0]) {

                        //void Crout(int d, double* S, double* D){
                        for (int k = 0; k < d; ++k) {
                            for (int i = k; i < d; ++i) {
                                double sum = 0.0;
                                for (int p = 0; p < k; ++p) sum += D[i * d + p] * D[p * d + k];
                                D[i * d + k] = S[i * d + k] - sum; // not dividing by diagonals
                            }
                            for (int j = k + 1; j < d; ++j) {
                                double sum = 0.0;
                                for (int p = 0; p < k; ++p) sum += D[k * d + p] * D[p * d + j];
                                D[k * d + j] = (S[k * d + j] - sum) / D[k * d + k];
                            }
                        }
                        
                    }
                }
            }
            */
            return outmatrix;

        }

        private static double[][] getTimeJaggedMatrix(int T, int lambda) {
            double lambdaMultiplier = Math.Pow(lambda, 2);
            double[][] outMatrix = null;
            if (T == 3 || T == 4 || T == 5) {

                if (T == 3) outMatrix = new double[][] {    new double[] { 1, -2,  1},
                                                            new double[] {-2,  4, -2},
                                                            new double[] { 1, -2,  1} };
                if (T == 4) outMatrix = new double[][] {    new double[] { 1, -2,  1,  0},
                                                            new double[] {-2,  5, -4,  1},
                                                            new double[] { 1, -4,  5, -2},
                                                            new double[] { 0,  1, -2,  1} };
                if (T == 5) outMatrix = new double[][] {    new double[] { 1, -2,  1,  0,  0},
                                                            new double[] {-2,  5, -4,  1,  0},
                                                            new double[] { 1, -4,  6, -4,  1},
                                                            new double[] { 0,  1, -4,  5, -2},
                                                            new double[] { 0,  0,  1, -2,  1} };

                int xLength = outMatrix.Length;
                int yLength = outMatrix[0].Length;
                for (int x = 0; x < xLength; x++) {
                    for (int y = 0; y < yLength; y++) {
                        if (outMatrix[x][y] != 0)
                            outMatrix[x][y] = outMatrix[x][y] * lambdaMultiplier;
                    }
                }
                for (int x = 0; x < xLength; x++) {
                    outMatrix[x][x] = outMatrix[x][x] + 1;
                }

            } else {

                // create the matrix and initialize to 0
                outMatrix = new double[T][];
                for (int i = 0; i < T; i++)
                    outMatrix[i] = new double[T];

                for (int i = 0; i < T; i++) {
                    if (i == 0 || i == T - 1) {

                        outMatrix[i][i] = 1 * lambdaMultiplier + 1;

                    } else if (i == 1) {

                        outMatrix[i][i] = 5 * lambdaMultiplier + 1;

                        outMatrix[i - 1][i    ] = -2 * lambdaMultiplier;
                        outMatrix[i    ][i - 1] = -2 * lambdaMultiplier;

                    } else if (i == T - 2) {

                        outMatrix[i][i] = 5 * lambdaMultiplier + 1;
                        
                        outMatrix[i + 1][i    ] = -2 * lambdaMultiplier;
                        outMatrix[i    ][i + 1] = -2 * lambdaMultiplier;

                    } else {

                        outMatrix[i][i] = 6 * lambdaMultiplier + 1;

                        outMatrix[i - 1][i    ] = -4 * lambdaMultiplier;
                        outMatrix[i    ][i - 1] = -4 * lambdaMultiplier;
                        outMatrix[i - 2][i    ] = 1 * lambdaMultiplier;
                        outMatrix[i    ][i - 2] = 1 * lambdaMultiplier;

                        outMatrix[i + 1][i    ] = -4 * lambdaMultiplier;
                        outMatrix[i    ][i + 1] = -4 * lambdaMultiplier;
                        outMatrix[i + 2][i    ] = 1 * lambdaMultiplier;
                        outMatrix[i    ][i + 2] = 1 * lambdaMultiplier;

                    }
                }
                
            }

            // 
            return outMatrix;

        }


        private static double[,] getTime2DMatrix(int T, int lambda) {
            double lambdaMultiplier = Math.Pow(lambda, 2);
            double[,] outMatrix = null;
            if (T == 3 || T == 4 || T == 5) {

                if (T == 3) outMatrix = new double[3, 3] {  { 1, -2,  1},
                                                            {-2,  4, -2},
                                                            { 1, -2,  1} };
                if (T == 4) outMatrix = new double[4, 4] {  { 1, -2,  1,  0},
                                                            {-2,  5, -4,  1},
                                                            { 1, -4,  5, -2},
                                                            { 0,  1, -2,  1} };
                if (T == 5) outMatrix = new double[5, 5] {  { 1, -2,  1,  0,  0},
                                                            {-2,  5, -4,  1,  0},
                                                            { 1, -4,  6, -4,  1},
                                                            { 0,  1, -4,  5, -2},
                                                            { 0,  0,  1, -2,  1} };

                int xLength = outMatrix.GetLength(0);
                int yLength = outMatrix.GetLength(1);
                for (int x = 0; x < xLength; x++) {
                    for (int y = 0; y < yLength; y++) {
                        if (outMatrix[x, y] != 0)
                            outMatrix[x, y] = outMatrix[x, y] * lambdaMultiplier;
                    }
                }
                for (int x = 0; x < xLength; x++) {
                    outMatrix[x, x] = outMatrix[x, x] + 1;
                }

            } else {

                outMatrix = new double[T, T];     // note that all elements are already initialized to 0

                outMatrix[0, 0] = 1;
                for (int i = 0; i < T; i++) {
                    if (i == 0 || i == T - 1) {

                        outMatrix[i, i] = 1 * lambdaMultiplier + 1;

                    } else if (i == 1) {

                        outMatrix[i, i] = 5 * lambdaMultiplier + 1;

                        outMatrix[i - 1, i] = -2 * lambdaMultiplier;
                        outMatrix[i, i - 1] = -2 * lambdaMultiplier;

                    } else if (i == T - 2) {

                        outMatrix[i, i] = 5 * lambdaMultiplier + 1;

                        outMatrix[i + 1, i] = -2 * lambdaMultiplier;
                        outMatrix[i, i + 1] = -2 * lambdaMultiplier;

                    } else {

                        outMatrix[i, i] = 6 * lambdaMultiplier + 1;

                        outMatrix[i - 1, i] = -4 * lambdaMultiplier;
                        outMatrix[i, i - 1] = -4 * lambdaMultiplier;
                        outMatrix[i - 2, i] = 1 * lambdaMultiplier;
                        outMatrix[i, i - 2] = 1 * lambdaMultiplier;

                        outMatrix[i + 1, i] = -4 * lambdaMultiplier;
                        outMatrix[i, i + 1] = -4 * lambdaMultiplier;
                        outMatrix[i + 2, i] = 1 * lambdaMultiplier;
                        outMatrix[i, i + 2] = 1 * lambdaMultiplier;

                    }
                }


            }



            return outMatrix;

        }
    }
}
