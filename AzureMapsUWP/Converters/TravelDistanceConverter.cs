using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AzureMapsUWP.Converters
{
    public class TravelDistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = "N/A";

            if (value == null)
                return result;

            double distance = System.Convert.ToDouble(value);

            if (distance >= 500)
            {
                distance = distance / 1000;
                result = distance.ToString("0.0 km");
            }
            else
                result = distance.ToString("0 m");

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
