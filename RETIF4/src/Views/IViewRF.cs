/**
 * IViewRF interface
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using RETIF4.Data;

namespace RETIF4.Views {

    public interface IViewRF : IView {

        int getCondition();
        string getGUIViewInfo();

        void startPreScene(int scene);              // is called when the phase is started but before the actual scene is started (gives the opportunity to show something before a task actually begins (with creating a new phase that preceeds the phase)
        void startScene(int scene);

        Block[] getTrialSequence();

    }

}
