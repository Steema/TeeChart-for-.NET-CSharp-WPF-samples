using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SeriesRatings.Data;
using Steema.TeeChart.WPF;
using Steema.TeeChart.WPF.Drawing;
using Steema.TeeChart.WPF.Styles;
using Steema.TeeChart.WPF.Tools;

namespace SeriesRatings
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _adjustMinMaxRatings;

        private Line _seasonSeries;
        private Line _episodeSeries;

        private CancellationTokenSource _searchCancellationTokenSource;
        private CancellationTokenSource _chartCancellationTokenSource;

        private List<SearchSeriesData> _defaultSeries;

        public MainWindow()
        {
            InitializeComponent();

            // TODO: Refactor & Clean-up code
            // TODO: Document code
        }

        private void InitializeChart()
        {
            Chart.Visibility = Visibility.Visible;

            Chart.Aspect.View3D = false;
            Chart.CurrentTheme = ThemeType.BlackIsBack;

            Chart.Header.Font.Size = 16;

            Chart.Legend.Alignment = LegendAlignments.Bottom;
            Chart.Legend.Brush.Gradient.Visible = false;
            Chart.Legend.Brush.Visible = false;

            Chart.Panel.Gradient.Visible = false;
            // If we set the panel visibility to false, all the mouse events go through the chart, so we set it to fully transparent instead.
            Chart.Panel.Brush.Transparency = 100;
            Chart.Panel.Bevel.Inner = BevelStyles.None;
            Chart.Panel.Bevel.Outer = BevelStyles.None;

            var dashStyle = new DashStyle(new double[] {1, 5}, 0);

            var leftAxis = Chart.Axes.Left;
            leftAxis.Automatic = false;
            leftAxis.SetMinMax(0, 10);
            leftAxis.Grid.Width = 0.5;
            leftAxis.Grid.Style = dashStyle;

            var bottomAxis = Chart.Axes.Bottom;
            bottomAxis.Grid.Width = 0.5;
            bottomAxis.Grid.Style = dashStyle;
            bottomAxis.Grid.DrawEvery = 1;
            bottomAxis.MinAxisIncrement = 1;
            bottomAxis.Increment = 1;
            bottomAxis.Labels.Style = AxisLabelStyle.Value;
            bottomAxis.Labels.OnAxis = true;

            var topAxis = Chart.Axes.Top;
            topAxis.Grid.Color = Color.FromRgb(0x11, 0x9e, 0xda);

            var rightAxis = Chart.Axes.Right;
            rightAxis.Grid.Color = Color.FromRgb(0xe5, 0x14, 0x00);
            rightAxis.Grid.Width = 2;
            rightAxis.Grid.Transparency = 0;
            rightAxis.Ticks.Visible = false;

            _seasonSeries = new Line(Chart.Chart)
            {
                Title = "Average Season Rating",
                Color = Color.FromRgb(0xfa, 0x68, 0x00),
                LinePen =
                {
                    Width = 2
                },
                Marks =
                {
                    Visible = true,
                    Style = MarksStyles.Value,
                    FontSeriesColor = true,
                    DrawEvery = 2,
                    OnTop = true
                }
            };
            _seasonSeries.Marks.Arrow.Visible = false;
            _seasonSeries.Marks.Brush.Visible = false;
            _seasonSeries.Marks.Pen.Visible = false;

            _episodeSeries = new Line(Chart.Chart)
            {
                Title = "Individual Episode Ratings",
                Color = Color.FromRgb(0x11, 0x9e, 0xda),
                LinePen =
                {
                    Width = 3
                },
                Pointer =
                {
                    Visible = true,
                    Style = PointerStyles.Circle,
                    Pen =
                    {
                        Color = Color.FromRgb(0xff, 0xff, 0xff),
                        Width = 2
                    },
                    HorizSize = 5,
                    VertSize = 5
                },
                HorizAxis = HorizontalAxis.Both,
                VertAxis = VerticalAxis.Both
            };

            new MarksTip
            {
                Chart = Chart.Chart,
                MouseAction = MarksTipMouseAction.Move,
                Active = true,
                Style = MarksStyles.Label,
                Series = _episodeSeries
            };

            Chart.Chart.ToolTip.InitialDelay = 500;
            Chart.Chart.ToolTip.BackColor = Color.FromRgb(0x25, 0x25, 0x25);
            Chart.Chart.ToolTip.ForeColor = Color.FromRgb(0xFF, 0xFF, 0xFF);
        }

        private async Task LoadSeries(string imdbId)
        {
            CancelPreviousOperation(ref _chartCancellationTokenSource);
            ShowLoadingFeedback(true, ChartLoadingSpinner, Chart);

            var series = await SeriesData.GetSeriesData(imdbId, _chartCancellationTokenSource.Token);

            FinishOperation(ref _chartCancellationTokenSource);
            ShowLoadingFeedback(false, ChartLoadingSpinner, Chart);

            if (series != null) // Series Selection Async NOT Cancelled
            {
                if (Chart.Series.Count < 1) InitializeChart();

                ClearSeasonSelector();
                SetChartToSeries(series);
            }
        }

        private void SetChartToSeries(SeriesData seriesData)
        {
            Chart.Zoom.Undo();

            Chart.Header.Text = $"{seriesData.Title} Episode Ratings";

            _episodeSeries.Clear();
            _seasonSeries.Clear();

            var topAxis = Chart.Axes.Top;
            topAxis.Labels.Items.Clear();
            topAxis.Labels.Items.Add(1, " ");

            var rightAxis = Chart.Axes.Right;
            rightAxis.Labels.Items.Clear();
            rightAxis.Labels.Items.Add(seriesData.Rating);
            rightAxis.Labels.Items[0].Font.Color = rightAxis.Grid.Color;
            
            var episodes = 1;
            foreach (var season in seriesData.Seasons)
            {
                AddSeason(season, ref episodes);
            }

            var lastEpisode = episodes - 1;

            Chart.Axes.Bottom.SetMinMax(1, lastEpisode);
            topAxis.SetMinMax(1, lastEpisode);

            if (_adjustMinMaxRatings) AdjustMinMaxRatings();
            else UnadjustMinMaxRatings();
        }

        private void AddSeason(SeasonData season, ref int episodes)
        {
            var firstEpisode = Math.Max(1, episodes - 1);
            var seasonNumber = _seasonSeries.Count / 2 + 1;

            double ratingAverage = 0;
            var ratedEpisodes = 0;
            foreach (var episode in season.Episodes)
            {
                if (double.IsNaN(episode.Rating)) break;

                _episodeSeries.Add(episodes, episode.Rating, GetEpisodeLabel(episode, seasonNumber));
                ratingAverage += episode.Rating;

                episodes++;
                ratedEpisodes++;
            }
            ratingAverage /= ratedEpisodes;

            _seasonSeries.Add(firstEpisode, ratingAverage);
            _seasonSeries.Add(episodes - 1, ratingAverage);

            episodes += season.Episodes.Count - ratedEpisodes;

            var label = Chart.Axes.Top.Labels.Items.Add(episodes - 1, $"Season {seasonNumber}");
            label.Font.Color = Color.FromRgb(0xFF, 0xFF, 0xFF);
            //TODO: Center season labeling

            SeasonSelector.Items.Add($"Season {seasonNumber}");
        }

        private static string GetEpisodeLabel(EpisodeData episode, int season)
        {
            return $"{season}x{episode.Episode:D2}: {episode.Title}\n{episode.Rating:#0.0}";
        }

        private void ClearSeasonSelector()
        {
            var allSeasons = SeasonSelector.Items[0];
            SeasonSelector.Items.Clear();
            SeasonSelector.Items.Add(allSeasons);
            SeasonSelector.SelectedIndex = 0;
        }

        private async Task Search(string query)
        {
            CancelPreviousOperation(ref _searchCancellationTokenSource);
            ShowLoadingFeedback(true, SearchLoadingSpinner, SearchResultsList);

            List<SearchSeriesData> results;
            if (string.IsNullOrWhiteSpace(query))
            {
                if (_defaultSeries == null)
                    _defaultSeries = await SeriesData.SearchDefaults(_searchCancellationTokenSource.Token);
                results = _defaultSeries;
            }
            else
            {
                results = await SeriesData.Search(query, _searchCancellationTokenSource.Token);
            }

            if (results != null) // Search Async Cancelled
            {
                SearchResultsList.ItemsSource = results;
            }

            ShowLoadingFeedback(false, SearchLoadingSpinner, SearchResultsList);
            FinishOperation(ref _searchCancellationTokenSource);
        }

        private static void ShowLoadingFeedback(bool loading, UIElement spinner, UIElement control)
        {
            spinner.Visibility = loading ? Visibility.Visible : Visibility.Hidden;
            control.Opacity = loading ? 0.5 : 1;
        }

        private static void CancelPreviousOperation(ref CancellationTokenSource tokenSource)
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
            tokenSource = new CancellationTokenSource();
        }

        private static void FinishOperation(ref CancellationTokenSource tokenSource)
        {
            tokenSource?.Dispose();
            tokenSource = null;
        }

        private async void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var query = ((TextBox) sender).Text;
                await Search(query);
            }
        }

        private async void OnSelectSeries(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView) sender;
            var series = (SearchSeriesData) listView.SelectedItem;
            if (series == null) return;

            await LoadSeries(series.ImdbId);
        }

        private void OnAdjustMinMaxRatings(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox) sender;

            _adjustMinMaxRatings = checkbox.IsChecked.HasValue && checkbox.IsChecked.Value;

            if (_adjustMinMaxRatings) AdjustMinMaxRatings();
            else UnadjustMinMaxRatings();
        }

        private void AdjustMinMaxRatings()
        {
            Chart.Axes.Left.Automatic = true;
            Chart.Axes.Right.Automatic = true;
            Chart.Invalidate();
        }

        private void UnadjustMinMaxRatings()
        {
            Chart.Axes.Left.SetMinMax(0, 10);
            Chart.Axes.Right.SetMinMax(0, 10);
            Chart.Invalidate();
        }

        private void OnSelectSeason(object sender, SelectionChangedEventArgs e)
        {
            if (Chart == null || Chart.Series.Count < 1) return;

            var comboBox = (ComboBox) sender;
            var seasonSelected = comboBox.SelectedIndex;

            if (seasonSelected <= 0)
            {
                UndoZoom();
            }
            else
            {
                ZoomInSeason(seasonSelected);
            }
        }

        private void ZoomInSeason(int season)
        {
            var leftAxis = Chart.Axes.Left;

            // Every season has two points, one for the start and one for the end, so to get the starting point we multiply the
            // season by 2.
            var seasonStartIndex = (season - 1) * 2;
            var lastEpisode = _episodeSeries.Count - 1;

            var startY = 10.0;
            var endY = 0.0;
            if (_adjustMinMaxRatings)
            {
                startY = leftAxis.MaxYValue;
                endY = leftAxis.MinYValue;
            }
            startY = leftAxis.CalcYPosValue(startY);
            endY = leftAxis.CalcYPosValue(endY);

            var startX = Math.Floor(_seasonSeries.CalcXPos(seasonStartIndex) - 1);
            var endX = seasonStartIndex + 1 >= _seasonSeries.Count - 1 // Is this the last season?
                // The position of where the season would end even if all the episodes aren't there yet.
                // We use this to show the last season fully even if it hasn't ended, and thus all its episodes aren't rated yet.
                ? Chart.Axes.Bottom.CalcXPosValue(lastEpisode)
                // The position of the last episode of the season.
                : _seasonSeries.CalcXPos(seasonStartIndex + 1);

            Chart.Zoom.ZoomRect(new Rect(new Point(startX, startY), new Point(endX, endY)));
        }

        private void UndoZoom()
        {
            Chart.Zoom.Undo();
            Chart.Axes.Left.Scroll(0, true);
            Chart.Axes.Bottom.Scroll(0, true);

            if (_adjustMinMaxRatings)
            {
                AdjustMinMaxRatings();
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await Search(""); // We do an empty search when the program starts to get the default series.
        }

        private void OnScroll(object sender, EventArgs e)
        {
            var leftAxis = Chart.Axes.Left;

            var minLeft = _adjustMinMaxRatings ? leftAxis.MinYValue : 0;
            var maxLeft = _adjustMinMaxRatings ? leftAxis.MaxYValue : 10;

            var lastEpisode = _episodeSeries.Count - 1;

            KeepAxisInBounds(leftAxis, minLeft, maxLeft);
            KeepAxisInBounds(Chart.Axes.Right, minLeft, maxLeft);
            KeepAxisInBounds(Chart.Axes.Bottom, 1, lastEpisode);
            KeepAxisInBounds(Chart.Axes.Top, 1, lastEpisode);
        }

        private static void KeepAxisInBounds(Axis axis, double minimum, double maximum)
        {
            // We check how much the axis overshoots its bounds and add that to the minimum and maximum to get it back in place.
            var diff = 0.0;
            if (axis.Minimum < minimum) diff = minimum - axis.Minimum;
            else if (axis.Maximum > maximum) diff = maximum - axis.Maximum;
            axis.Maximum += diff;
            axis.Minimum += diff;
        }
    }
}