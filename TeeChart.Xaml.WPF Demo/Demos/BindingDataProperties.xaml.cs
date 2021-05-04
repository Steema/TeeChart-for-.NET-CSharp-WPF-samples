using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace XamlWPFDemo.Demos
{
    public partial class BindingDataProperties
    {
        public BindingDataProperties()
        {
            InitializeComponent();

            var internalChart = LogicalTreeHelper.GetChildren(tChart1).OfType<Steema.TeeChart.WPF.TChart>().First();

            var internalColorGrid = (Steema.TeeChart.WPF.Styles.ColorGrid)internalChart[0];
            internalColorGrid.Pen.Color = Colors.White;
            internalColorGrid.Pen.Width = 2;
        }
    }
}
