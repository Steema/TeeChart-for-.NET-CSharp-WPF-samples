using Steema.TeeChart.WPF;
using Steema.TeeChart.WPF.Drawing;
using Steema.TeeChart.WPF.Tools;
using Steema.TeeChart.WPF.Styles;
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

namespace WpfExampleApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Steema.TeeChart.WPF.Styles.FastLine ExFastLine1;
		private Steema.TeeChart.WPF.Styles.FastLine ExFastLine2;

		System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

		public MainWindow()
		{
			InitializeComponent();

			btnFill.Click += btnFill_ClickAsync;
			btnScroll.Click += btnScroll_Click;

			dispatcherTimer.Tick += dispatcherTimer_Tick;

			modifyChart();

		}

		public void modifyChart()
		{

			Chart.Visibility = Visibility.Visible;

			Chart.Aspect.View3D = false;
			Chart.CurrentTheme = ThemeType.Report;

			Chart.Header.Font.Size = 16;
			Chart.Header.Text="FastLine Example";

			Chart.Axes.Left.SetMinMax(0, 8100);
			Chart.Axes.Left.Increment = 1000;

			Chart.Legend.Alignment = LegendAlignments.Bottom;
			Chart.Legend.Brush.Gradient.Visible = false;
			Chart.Legend.Brush.Visible = false;

			Chart.Panel.Gradient.Visible = false;
			// If we set the panel visibility to false, all the mouse events go through the chart, so we set it to fully transparent instead.
			Chart.Panel.Brush.Transparency = 100;
			Chart.Panel.Bevel.Inner = BevelStyles.None;
			Chart.Panel.Bevel.Outer = BevelStyles.None;

			ExFastLine1 = new Steema.TeeChart.WPF.Styles.FastLine(Chart.Chart)
			{
				Title = "Example FastLine 1",
				Color = Color.FromRgb(0xfa, 0x68, 0x00),
			};
			ExFastLine1.Marks.Arrow.Visible = false;
			ExFastLine1.Marks.Brush.Visible = false;
			ExFastLine1.Marks.Pen.Visible = false;

			ExFastLine2 = new Steema.TeeChart.WPF.Styles.FastLine(Chart.Chart)
			{
				Title = "Example FastLine 1",
				Color = Color.FromRgb(0x11, 0x9e, 0xda),
			};

			Chart.Chart.ToolTip.InitialDelay = 500;
			Chart.Chart.ToolTip.BackColor = Color.FromRgb(0x25, 0x25, 0x25);
			Chart.Chart.ToolTip.ForeColor = Color.FromRgb(0xFF, 0xFF, 0xFF);

		}

		bool isScrolling = false;

		private void btnScroll_Click(object sender, RoutedEventArgs e)
		{
			if (!isScrolling)
			{
				btnScroll.Content = "Stop";
				isScrolling = true;
				btnFill.IsEnabled = false;
				dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
				dispatcherTimer.Start();
			}
			else
			{
				btnScroll.Content = "Scroller";
				isScrolling = false;
				btnFill.IsEnabled = true;
				dispatcherTimer.Stop();
			}
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			LoadScrollData();
		}

		private async void btnFill_ClickAsync(object sender, RoutedEventArgs e)
		{
			LoadData();
		}

		Random rand = new Random();
		private double getValue(double last)
		{
			if (last > 7000)
				return 0 - rand.Next(100);
			else if (last < 100)
				return rand.Next(100);
			else
			  return 50 - rand.Next(100);
		}

		private void LoadData()
		{
			//ExFastLine1.Clear();
			//ExFastLine2.Clear();

			int valueCount = 10000;
			int firstPoint = ExFastLine1.Count;

			for (int i = firstPoint; i < firstPoint+valueCount; i++)
			{
				if (i > 0)
				{
					ExFastLine1.Add(i, ExFastLine1.YValues.Last + getValue(ExFastLine1.YValues.Last));
					ExFastLine2.Add(i, ExFastLine2.YValues.Last + getValue(ExFastLine2.YValues.Last));
				}
				else
				{
					ExFastLine1.Add(i, rand.Next(5000));
					ExFastLine2.Add(i, rand.Next(5000));
				}
			}
		}

		private void LoadScrollData()
		{
			//ExFastLine1.Clear();
			//ExFastLine2.Clear();

			int valueCount = 10;
			int firstPoint = (ExFastLine1.Count > 0) ? (int)ExFastLine1.XValues.Last : 0;

			for (int i = firstPoint; i < firstPoint + valueCount; i++)
			{
				if (i > 0)
				{
					ExFastLine1.Add(i, ExFastLine1.YValues.Last + getValue(ExFastLine1.YValues.Last));
					ExFastLine2.Add(i, ExFastLine2.YValues.Last + getValue(ExFastLine2.YValues.Last));
				}
				else
				{
					ExFastLine1.Add(i, rand.Next(5000));
					ExFastLine2.Add(i, rand.Next(5000));
				}
			}

			if (ExFastLine1.Count>500)
			{
				Chart.Axes.Bottom.SetMinMax(ExFastLine1.XValues.Last - 500, ExFastLine1.XValues.Last);
			}

			if ((ExFastLine1.Count > 2000) && (ExFastLine1.Count % 500 == 0))
			{
				ExFastLine1.Delete(0, 200);
				ExFastLine2.Delete(0, 200);
			}

		}
	}
}
