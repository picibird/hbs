// CollapsedIfNoDataConverter.cs
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
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace picibird.hbs.wpf.converter
{
    public class CollapsedIfNoDataConverter : MarkupExtension, IValueConverter
    {
        private static CollapsedIfNoDataConverter m_converter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            if (value is string)
            {
                if (string.IsNullOrEmpty(value as string))
                    return Visibility.Collapsed;
            }
            if (value is bool)
            {
                var boolValue = (bool) value;
                if (!boolValue)
                    return Visibility.Collapsed;
            }
            if (value is Array)
            {
                var arrayValue = (Array)value;
                if (arrayValue.Length == 0)
                    return Visibility.Collapsed;
            }
            if (value is IList)
            {
                var listValue = (IList)value;
                if (listValue.Count == 0)
                    return Visibility.Collapsed;
            }
            if (value is ICollection)
            {
                var collValue = (ICollection)value;
                if (collValue.Count == 0)
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (m_converter == null)
                m_converter = new CollapsedIfNoDataConverter();
            return m_converter;
        }
    }
}