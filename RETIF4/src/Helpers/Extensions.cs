/**
 * Type extensions
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RETIF4.Helpers {

    static class ThreadSafeRandom {

        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }

    }

    public static class Extensions {

        public static void Swap<T>(this IList<T> list, int indexA, int indexB) {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        public static void Shuffle<T>(this IList<T> list) {

            int n = list.Count;
            while (n > 1) {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

        }

        public static double Average(this IEnumerable<int> array) {
            long sum = 0;
            long count = 0;
            foreach (int v in array) {
                sum += v;
                count++;
            }
            if (count > 0)  return (double)sum / count;
            return 0;
        }

        public static double Average(this IEnumerable<double> array) {
            double sum = 0.0;
            long count = 0;
            foreach (double v in array) {
                sum += v;
                count++;
            }
            if (count > 0) return sum / count;
            return 0.0;
        }

        
        /**
         * Extends a sequence by shuffling and concatination
         * 
         * allowSubsequent - whether the last condition of the former sequence is allowed to be the same at the first condition of the following sequence
         **/
        public static int[] ExtendSequenceWithShuffledRepeat(this IEnumerable<int> initialSequence, int targetLength, bool allowSubsequent) {
        //private int[] (int[] initialSequence, int targetLength, bool allowSubsequent) {
            if (initialSequence == null || initialSequence.Count() < 1 || targetLength == 0)     return new int[0];

            int[] outSequence = new int[targetLength];
            int cursor = 0;

            // store the initial sequence for shuffling
            int[] inSequence = initialSequence.ToArray();
            
            // 
            int maxLoops = 100000;
            int loopCounter = 0;

            // loop until the target length was reached
            while (cursor < targetLength) {
                if (loopCounter > maxLoops) {
                    //logger.Error("Error on randomization, check code, returning empty array");
                    return new int[0];
                }

                // shuffle the array
                inSequence.Shuffle();

                // check if we disallow subsequents and whether the first of the shuffled input
                // sequence is the same as the last in the output sequence up till now
                while (!allowSubsequent && cursor != 0 && inSequence[0] == outSequence[cursor - 1]) {
                    if (loopCounter > maxLoops) {
                        //logger.Error("Error on randomization, check code, returning empty array");
                        return new int[0];
                    }

                    // reshuffle the array
                    inSequence.Shuffle();

                    loopCounter++;  // safety
                }
                

                // transfer the sequence and highten the cursor
                for (int i = 0; i < inSequence.Length && cursor < outSequence.Length; i++) {
                    outSequence[cursor] = inSequence[i];
                    cursor++;
                }

                
                loopCounter++;      // safety
            }

            // return the resulting sequence
            return outSequence;
        }



    }

}
