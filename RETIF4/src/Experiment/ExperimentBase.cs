/**
 * ExperimentBase class
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
using System.Text;

namespace RETIF4.Experiment {

    public class ExperimentBase {

        protected string CONFIG_VIEW_CLASS = "";
        protected string CONFIG_COLLECTOR_CLASS = "";
        protected string CONFIG_TRIGGER_CLASS = "";
        protected Object configurationLock = new Object();                              // lock object to lock the volume que

        protected List<Phase> phases = new List<Phase>();                              // list of all the possible phases in the experiment

        public string getViewClass() {
            lock (configurationLock) {
                return CONFIG_VIEW_CLASS;
            }
        }

        public string getCollectorClass() {
            lock (configurationLock) {
                return CONFIG_COLLECTOR_CLASS;
            }
        }

        public string getTriggerClass() {
            lock (configurationLock) {
                return CONFIG_TRIGGER_CLASS;
            }
        }

        public List<Phase> getPhases() {
            lock (configurationLock) {
                return phases;
            }
        }

        public Phase getPhaseById(int id) {
            lock (configurationLock) {
                for (int i = 0; i < phases.Count; i++) {
                    if (phases[i].phaseID == id)    return phases[i];
                }
            }
            return null;
        }

    }

}
