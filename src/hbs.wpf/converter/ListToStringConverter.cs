using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace picibird.hbs.wpf.converter
{
    public class ListToStringConverter : MarkupExtension, IValueConverter
    {
        private static ListToStringConverter m_converter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a String");


            return String.Join(", ", ((IEnumerable<string>)value).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (m_converter == null)
                m_converter = new ListToStringConverter();
            return m_converter;
        }
    }
}
