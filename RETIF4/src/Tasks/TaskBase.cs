/**
 * TaskBase class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using NLog;
using System.IO;

namespace RETIF4.Tasks {

    public class TaskBase {

        private static Logger logger = LogManager.GetLogger("TaskBase");

        protected string outputDirectory = null;                      // the directory where the processing steps be stored in (will be set by the constructor)
        protected bool finished = false;

        public TaskBase(string workDirectory) {

            // check the working directory
            if (string.IsNullOrEmpty(workDirectory.Trim())) {

                // message
                logger.Error("No valid working directory was given (empty)");
                this.outputDirectory = null;

            } else {

                // build the output directory path
                string outputDirectory = workDirectory + "Output";

                // if the directory does not exists then create the directory
                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

                // message
                logger.Info("Output path set to \'" + outputDirectory + "\'");

                // set the path
                this.outputDirectory = outputDirectory;

            }
        }

        public bool isTaskFinished() {
            return finished;
        }

    }

}
