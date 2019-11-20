using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AzureMapsUWP.Converters
{
    public class TravelModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            string result = "";

            switch (value.ToString())
            {
                case "car":
                    result = "\uE804;";
                    break;

                case "walk":
                    result = "\uE806;";
                    break;

                case "bus":
                    result = "\uE806;";
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
