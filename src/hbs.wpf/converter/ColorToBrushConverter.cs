// ColorToBrushConverter.cs
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
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace picibird.hbs.wpf.converter
{
    /// <summary>
    ///     An <see cref="IValueConverter" /> which converts a <see cref="Color" /> value to a <see cref="SolidColorBrush" />
    ///     value.
    /// </summary>
    public class ColorToBrushConverter : MarkupExtension, IValueConverter
    {
        private static ColorToBrushConverter m_converter;

        /// <summary>
        ///     Converts a <see cref="Color" /> value to a <see cref="SolidColorBrush" /> value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                try
                {
                    var color = FromHex((string) value);
                    return new SolidColorBrush(color);
                }
                catch
                {
                }
            }

            if (value == null || !(value is Color))
            {
                return null;
            }

            return new SolidColorBrush((Color) value);
        }

        /// <summary>
        ///     Converts a <see cref="SolidColorBrush" /> value to a <see cref="Color" /> value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as SolidColorBrush;

            if (brush == null)
            {
                return null;
            }

            return brush.Color;
        }

        private static Color FromHex(string argbHEX)
        {
            var argb = int.Parse(argbHEX.Replace("#", ""), NumberStyles.HexNumber);
            return Color.FromArgb((byte) (argb >> 24),
                (byte) (argb >> 16),
                (byte) (argb >> 8),
                (byte) argb);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (m_converter == null)
                m_converter = new ColorToBrushConverter();
            return m_converter;
        }
    }
}