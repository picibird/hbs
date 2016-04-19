// ResourcesConverter.cs
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
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

using picibits.core;

namespace picibird.hbs.wpf.converter
{
    public class ResourcesConverter : MarkupExtension, IValueConverter
    {
        private static ResourcesConverter m_converter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object resource = null;

            resource = Pici.Resources.Find(value.ToString());
            if (resource != null) return resource;

            resource = Application.Current.TryFindResource(value.ToString());
            if (resource != null) return resource;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (m_converter == null)
                m_converter = new ResourcesConverter();
            return m_converter;
        }
    }
}