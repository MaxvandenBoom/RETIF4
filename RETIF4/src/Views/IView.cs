/**
 * IView interface
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
namespace RETIF4.Views {

    public interface IView {

        // window/control/view functions
        void start();
        void stop();

        int getWindowX();
        int getWindowY();
        void setWindowLocation(int x, int y);

        int getWindowWidth();
        int getWindowHeight();
        void setWindowSize(int width, int height);
        void setWindowLocationAndSize(int x, int y, int width, int height);

        int getContentWidth();
        int getContentHeight();
        void setContentSize(int width, int height);

        bool hasBorder();
        void setBorder(bool border);

        void setBackgroundColor(float red, float green, float blue);

        bool isStarted();

        void drawLine(float x1, float y1, float x2, float y2, float lineWidth, bool dashed, float colorR, float colorG, float colorB);
        void drawRectangle(float x1, float y1, float x2, float y2, float lineWidth, float colorR, float colorG, float colorB);
        void drawTorus(float cx, float cy, double inner, double outer, int num_segments, float colorR, float colorG, float colorB);

    }
}
