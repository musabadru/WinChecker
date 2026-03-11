using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;

namespace WinChecker.App.Converters;

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isInverted = parameter as string == "Inverted";
        int count = (int)value;
        bool isVisible = count > 0;

        if (isInverted)
        {
            isVisible = !isVisible;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
