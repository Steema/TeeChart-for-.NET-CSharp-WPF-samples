using System.Windows;
using System.Windows.Media.Animation;

namespace SeriesRatings.Controls
{
    /// <summary>
    ///     Interaction logic for LoadingSpinner.xaml
    /// </summary>
    public partial class LoadingSpinner
    {
        public LoadingSpinner()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((Storyboard) Resources["Rotation"]).Begin();
        }
    }
}