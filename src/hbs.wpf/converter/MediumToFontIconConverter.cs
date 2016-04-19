// MediumToFontIconConverter.cs
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
using picibits.core.helper;

namespace picibird.hbs.wpf.converter
{
    public class MediumToFontIconConverter : MarkupExtension, IValueConverter
    {
        private static MediumToFontIconConverter m_converter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var medium = value as string;
                medium = StringHelper.RemoveWhitespace(medium);
                switch (medium)
                {
                    case "book":
                        return "k";
                    case "bookreview":
                        return "a";
                    case "bookchapter":
                        return "#";
                    case "proceeding":
                        return ">";
                    case "datamed":
                        return "m";
                    case "ebook":
                        return "k";
                    case "diss":
                        return "J";
                    case "journal":
                        return "L";
                    case "ejournal":
                        return "L";
                    case "article":
                        return "8";
                    case "video":
                        return "M";
                    case "audio":
                        return "<";
                    case "map":
                        return "O";
                    case "other":
                        return "S";
                }
                ;
                //'book': 'Buch',
                //  'bookreview': 'Buchbesprechung',
                //  'bookchapter': 'Buchkapitel',
                //  'proceeding': 'Konferenzbericht',
                //  'datamed': 'Datenträger',
                //  'ebook': 'eBook',
                //  'diss': 'Hochschulschrift',
                //  'journal': 'Zeitschrift',
                //  'ejournal': 'eJournal',
                //  'article': 'Artikel',
                //  'video': 'Film',
                //  'audio': 'Tonträger',
                //  'map': 'Karten/Bildmaterial',
                //  'other': 'Sonstiges',
            }
            return "S";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (m_converter == null)
                m_converter = new MediumToFontIconConverter();
            return m_converter;
        }
    }
}