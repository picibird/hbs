// MathHelper.cs
// Date Created: 20.01.2016
// 
// Copyright (c) 2016, picibird GmbH 
// All rights reserved.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using picibits.core.export.instances;

namespace picibird.hbs.helper
{
    public static class MathHelper
    {
        /*
         * http://stackoverflow.com/questions/1106339/resize-image-to-fit-in-bounding-box
         */

        public static double ScaleSize(Size from, double? maxWidth, double? maxHeight)
        {
            if (!maxWidth.HasValue && !maxHeight.HasValue)
                throw new ArgumentException("At least one scale factor (toWidth or toHeight) must not be null.");
            if (from.Height == 0 || from.Width == 0) throw new ArgumentException("Cannot scale size from zero.");
            double? widthScale = null;
            double? heightScale = null;
            if (maxWidth.HasValue)
            {
                widthScale = maxWidth.Value/from.Width;
            }
            if (maxHeight.HasValue)
            {
                heightScale = maxHeight.Value/from.Height;
            }
            var scale = Math.Min((double) (widthScale ?? heightScale),
                (double) (heightScale ?? widthScale));
            return scale;
        }
    }
}