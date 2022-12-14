/**
 * DateHelper class
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

    public static class DateHelper {

	    public static string getDateTime() {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        public static string getDateTimeWithMs() {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff");
        }
    }

}
