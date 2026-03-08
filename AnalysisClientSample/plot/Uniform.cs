using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AutoMetalPlot
{
    public static class UniformPlot
    {
        // 默认配置
        // 默认配置
        private static readonly GridLayoutConfig DefaultConfig = new GridLayoutConfig
        {
            Rows = 2,
            Columns = 2,
            CellPadding = 1,
            BackgroundColor = Color.White,
            BorderStyle = TableLayoutPanelCellBorderStyle.Single
        };

        public static void CreateUniformGrid(
            TableLayoutPanel container,
            IEnumerable<PlotModel> plotModels,
            GridLayoutConfig config = null)
        {
            if (config == null) throw new ArgumentException(nameof(container));
            if (plotModels == null) throw new ArgumentNullException(nameof(plotModels));

            var actualConfig = config ?? DefaultConfig;

            // 配置容器
            ConfigureContainer(container, actualConfig);

            // 填充PlotView
            PopulatePlotViews(container, plotModels, actualConfig);
            
        }


        /// <summary>
        /// 创建示例图表模型集合
        /// </summary>
        public static List<PlotModel> CreateSamplePlotModels(int count)
        {
            // 这里设置多个热力图展示情况
            var models = new List<PlotModel>();
            var rnd = new Random();

            // 传递灰度级数

            // 传递几个灰度矩阵

            for (int i = 0; i < count; i++)
            {
                var model = new PlotModel { Title = "Cakes per weekday" };
                
                
                model.Axes.Add(new CategoryAxis
                {
                    Position = AxisPosition.Bottom,

                    Key = "weekdayAxis",

                    ItemsSource = Enumerable.Range(1, 7)
                        .Select(k => k.ToString())
                        .ToArray()
                });

                model.Axes.Add(new CategoryAxis
                {
                    Position = AxisPosition.Left,

                    Key = "CakeAxis",

                    ItemsSource = Enumerable.Range(1, 7)
                        .Select(k => k.ToString())
                        .ToArray()
                });

                model.Axes.Add(new LinearColorAxis
                {
                    Palette = OxyPalettes.Hot(200)
                });

                var rand = new Random();
                var data = new double[7, 7];
                
                for(int x = 0; x < 7; ++x)
                {
                    for(int y = 0; y < 7;++y)
                    {
                        data[y, x] = rand.Next(0, 200) * (0.13 * (y + 1));
                    }
                }

                var heatMapSeries = new HeatMapSeries
                {
                    X0 = 0,
                    X1 = 6,
                    Y0 = 0,
                    Y1 = 4,
                    XAxisKey = "weekdayAxis",
                    YAxisKey = "CakeAxis",
                    RenderMethod = HeatMapRenderMethod.Rectangles,
                    LabelFontSize = 0.2,
                    Data = data
                };

                model.Series.Add(heatMapSeries);

                models.Add(model);

                //var model = new PlotModel { Title = $"热力图 {i + 1}" };

                //model.Axes.Add(new LinearColorAxis
                //{
                //    Palette = OxyPalettes.Rainbow(100)
                //});

                //var singleData = new double[100];

                //for(int x = 0; x < 100; ++x)
                //{
                //    singleData[x] = Math.Exp((-1.0 / 2.0) * Math.Pow(((double)x - 50.0) / 20.0,2.0));
                //}

                //var data = new double[100, 100];

                //for(int x = 0;x < 100; ++x)
                //{
                //    for(int y = 0; y < 100; ++y)
                //    {
                //        data[y,x] = singleData[x] * singleData[(y + 30) % 100] * 100;
                //    }
                //}

                //var heatMapSeries = new HeatMapSeries
                //{
                //    X0 = 0,
                //    X1 = 99,
                //    Y0 = 0,
                //    Y1 = 99,
                //    Interpolate = true,
                //    RenderMethod= HeatMapRenderMethod.Bitmap,
                //    Data = data
                //};

                //model.Series.Add(heatMapSeries);

                //models.Add(model);
            }

            return models;
        }

        private static void PopulatePlotViews(
    TableLayoutPanel container,
    IEnumerable<PlotModel> plotModels,
    GridLayoutConfig config)
        {
            int index = 0;
            var enumerator = plotModels.GetEnumerator();

            for (int row = 0; row < config.Rows; row++)
            {
                for (int col = 0; col < config.Columns; col++)
                {
                    if (!enumerator.MoveNext()) return;

                    var plotView = new PlotView
                    {
                        Dock = DockStyle.Fill,
                        Model = enumerator.Current,
                        BackColor = config.BackgroundColor,
                        Margin = new Padding(config.CellPadding)
                    };

                    container.Controls.Add(plotView, col, row);
                    index++;
                }
            }
        }

        private static void ConfigureContainer(TableLayoutPanel container, GridLayoutConfig config)
        {
            container.Controls.Clear();
            container.ColumnCount = config.Columns;
            container.RowCount = config.Rows;
            container.CellBorderStyle = config.BorderStyle;
            container.Padding = new Padding(config.CellPadding);
            container.Margin = new Padding(config.CellPadding);

            // 设置均匀分布的行列
            for (int i = 0; i < config.Columns; i++)
            {
                container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / config.Columns));
            }

            for (int i = 0; i < config.Rows; i++)
            {
                container.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / config.Rows));
            }
        }


        /// <summary>
        /// 网格布局配置
        /// </summary>
        public class GridLayoutConfig
        {
            public int Rows { get; set; } = 3;
            public int Columns { get; set; } = 3;
            public int CellPadding { get; set; } = 5;
            public Color BackgroundColor { get; set; } = Color.White;
            public TableLayoutPanelCellBorderStyle BorderStyle { get; set; } = TableLayoutPanelCellBorderStyle.Single;
        }

    }
}
