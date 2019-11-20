using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AzureMapsUWP.Converters
{
    public class TravelTimeLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            double seconds = System.Convert.ToDouble(value);

            double minutes = seconds / 60;

            string result = "Mins";

            if (minutes >= 60)
                result = "Hrs";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
