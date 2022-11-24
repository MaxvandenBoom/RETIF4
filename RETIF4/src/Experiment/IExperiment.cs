/**
 * IExperiment interface
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using RETIF4.Data;
using RETIF4.Events;
using System.Collections.Generic;

namespace RETIF4.Experiment {

    public interface IExperiment {

        void init();

        string getWorkDirectory();

        string getViewClass();
        string getCollectorClass();
        string getTriggerClass();

        List<Phase> getPhases();

        /**
         * Callback function, called at the start of an experimental phase by Mainthread
         * 
         * Note that this is the last thing called. At this
         * point, the view, the volume collection and trigger catching have already started
         **/
        void startPhase(ref Phase phase);

        /**
         * Callback function, called when the end of an experimental phase (as defined per configuration) was reached
         * 
         * This function is called first before the task's nextPhase (as defined by the configuration) is
         * initiated and a next phase/view task is started
         **/
        void stopPhase(ref Phase phase);

        /**
         * Callback function, called for every new incoming volume
         * 
         * (after the Mainthread has finished auto-processing with it)
         **/
        void processVolume(ref Phase phase, Volume volume);

        /**
         * Callback function, called for every incoming trigger
         * 
         * (after the Mainthread is finished with it)
         **/
        void processTrigger(ref Phase phase, TriggerEventArgs e);

        /**
         * Callback function, called at the start of each trial in the view
         * 
         * Initiated by the View, forwarded to the MainThread, which forwards it here
         **/
        void processViewTrialStart(ref Phase phase, double condition);

        /**
         * Get the most recent feedback value from the feedback task
         * 
         * Most likely to be requested by the view from the mainthread, which in it's turn
         * requests it from the experiment (here).
         **/
        double getFeedbackValue(ref Phase phase);

        /**
         * Get the most recent feedback values from the feedback task
         * 
         * Most likely to be requested by the view from the mainthread, which in it's turn
         * requests it from the experiment (here).
         **/
        double[] getFeedbackValues(ref Phase phase);
    }

}
