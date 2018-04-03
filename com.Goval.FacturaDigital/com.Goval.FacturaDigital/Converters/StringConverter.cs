using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Converters
{
    public class MoneyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            try
            {
                decimal money = (decimal)value;
                var parsedMoney = Utils.Utils.FormatNumericToString(money);
                return "₡" + parsedMoney;
            }
            catch (Exception)
            {
                return "MoneyConvert parse ERROR";
            }

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
