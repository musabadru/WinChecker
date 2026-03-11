using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System.Collections;

namespace WinChecker.App.Converters;

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isInverted = parameter?.ToString() == "Inverted";
        bool isVisible = false;

        if (value is int count)
        {
            isVisible = count > 0;
        }
        else if (value is bool b)
        {
            isVisible = b;
        }
        else if (value is string s)
        {
            isVisible = !string.IsNullOrEmpty(s);
        }
        else if (value is IEnumerable list)
        {
            isVisible = list.GetEnumerator().MoveNext();
        }
        else
        {
            isVisible = value != null;
        }

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
