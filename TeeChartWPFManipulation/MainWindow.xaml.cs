using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeeChartWPFManipulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TChart1.Header.Text += " IsManipulationEnabled";

            TChart1.IsManipulationEnabled = true;

            TChart1.Series.Add(typeof(Steema.TeeChart.WPF.Styles.Bar)).FillSampleValues();

            TChart1.ClickLegend += TChart1_ClickLegend;
        }

        private void TChart1_ClickLegend(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Legend");
        }

        private void rbScroll_Click(object sender, RoutedEventArgs e)
        {
            TChart1.Panning.Allow = Steema.TeeChart.WPF.ScrollModes.Both;
        }

        private void rbZoom_Click(object sender, RoutedEventArgs e)
        {
            TChart1.Panning.Allow = Steema.TeeChart.WPF.ScrollModes.None;
        }
    }

    public enum ZoomScroll
    {
        Zoom,
        Scroll
    }
}
