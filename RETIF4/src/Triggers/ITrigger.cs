/**
 * ITrigger interface
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using RETIF4.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RETIF4.Triggers {

    public interface ITrigger {

        event EventHandler<TriggerEventArgs> triggerHandler;

        // 
        bool start();
        void stop();
        bool isListening();

        void destroy();

    }

}
