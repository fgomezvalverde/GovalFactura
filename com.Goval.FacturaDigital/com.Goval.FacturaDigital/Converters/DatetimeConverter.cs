using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Converters
{
    public class DatetimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            string result = "invalidadDateTimeToConvert";
            if (date != null)
            {
                if (date.Date == DateTime.Now.Date)
                {
                    result = "Hoy";
                }
                else if (date.Date == DateTime.Now.AddDays(-1).Date)
                {
                    result = "Ayer";
                }
                else
                {
                    result = date.ToString("d", new CultureInfo("pt-BR"));
                }
                
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
