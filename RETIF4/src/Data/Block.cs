/**
 * Block class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;

namespace RETIF4.Data {

    [Serializable]
    public class Block {

        public int condition = 0;
        public int duration = 0;
        
        public double onset = 0;
        public double onset2 = 0;

        public Block() {    }

        public Block(int condition) {
            this.condition = condition;
        }

        public Block(int condition, int duration) {
            this.condition = condition;
            this.duration = duration;
        }

        public Block clone() {
            Block block = new Block();

            block.condition = condition;
            block.duration = duration;

            block.onset = onset;
            block.onset2 = onset2;

            return block;
        }
        
        public static string getLineAsLineHeaders() {
            string str = "";

            str += "duration" + "\t";
            str += "condition" + "\t";
            str += "onset" + "\t";
            str += "onset2";

            return str;
        }

        public static Block getBlockFromLine(string line) {

            try {
                string[] blockText = line.Split('\t');
                if (blockText.Length == 0)    return null;

                Block block = new Block();

                if (blockText.Length > 0)       block.duration = int.Parse(blockText[0]);
                if (blockText.Length > 1)       block.condition = int.Parse(blockText[1]);
                if (blockText.Length > 2)       block.onset = double.Parse(blockText[2]);
                if (blockText.Length > 3)       block.onset2 = double.Parse(blockText[3]);

                // return trial
                return block;

            } catch (Exception) {   }

            // return failure
            return null;

        }
        
        public string getAsLine() {
            string str = "";

            str += duration + "\t";
            str += condition + "\t";
            str += onset + "\t";
            str += onset2;

            return str;

        }

    }

}
