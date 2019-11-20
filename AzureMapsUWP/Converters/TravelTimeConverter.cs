using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AzureMapsUWP.Converters
{
    public class TravelTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            double seconds = System.Convert.ToDouble(value);

            double minutes = seconds / 60;

            string result = Math.Round(minutes).ToString("0");

            if (minutes >= 60)
                result = (minutes / 60).ToString("0.0");
            
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
