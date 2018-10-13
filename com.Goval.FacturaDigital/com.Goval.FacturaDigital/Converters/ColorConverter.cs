using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Converters
{
    public class BillStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringStatus = (string)value;

            switch (Enum.Parse(typeof(Model.BillStatus), stringStatus))
            {
                case Model.BillStatus.Done:
                    return Color.FromHex("#64a338");
                case Model.BillStatus.Error:
                    return Color.FromHex("#e03b24");
                case Model.BillStatus.Processing:
                    return Color.FromHex("#ffcc00");
                case Model.BillStatus.Rejected:
                    return Color.FromHex("#ffcc00");
                default:
                    return Color.FromHex("#87a2c7");
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
