using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using PaintDotNet;
using PaintDotNet.Effects;
using OptionControls;
using OptionBased.Effects;
using ControlExtensions;

namespace GraphPaperEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author => base.GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public string Copyright => base.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public string DisplayName => base.GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("https://forums.getpaint.net/index.php?showtopic=106915");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Graph Paper")]
    public class GraphPaperEffectPlugin : OptionBasedEffect
    {
        private static readonly Image StaticIcon = new Bitmap(typeof(GraphPaperEffectPlugin), "GraphPaper.png");

        private int cellSize; // [2,100] Cell Size
        private int cellsPerGroup; // [1,10] Cells per Group (squared)
        private int cellsPerCluster; // [1,10] Groups per Cluster (squared)
        private GraphType graphType; // [1] Graph Type|Standard|Isometric
        private ColorBgra cellColor; // Cell Color
        private ColorBgra groupColor; // Group Color
        private ColorBgra clusterColor; // Cluster Color
        private ColorBgra isoVerColor; // IsoVer Color
        private ColorBgra backColor; // Background Color
        private LineStyle cellLineStyle; // Cell Dash Style
        private LineStyle groupLineStyle; // Group Dash Style
        private LineStyle clusterLineStyle; // Cluster Dash Style

        private Surface graphSurface;

        public GraphPaperEffectPlugin()
            : base(typeof(GraphPaperEffectPlugin), StaticIcon, EffectFlags.Configurable)
        {
        }

        #region Option Enums
        private enum OptionNames
        {
            GraphType,
            CellSize,
            GroupSize,
            ClusterSize,
            LineStylesBox,
            CellLineStyle,
            GroupLineStyle,
            ClusterLineStyle,
            ColorTabs,
            CellColorTab,
            GroupColorTab,
            ClusterColorTab,
            IsoVerColorTab,
            BgColorTab,
            CellColor,
            CellColorWheel,
            GroupColor,
            GroupColorWheel,
            ClusterColor,
            ClusterColorWheel,
            IsoVerColor,
            IsoVerColorWheel,
            BgColorWheel,
            BgColor
        }

        private enum GraphType
        {
            Standard,
            Isometric
        }

        private enum LineStyle
        {
            Solid,
            Dashed,
            Dotted
        }

        private enum CellColor
        {
            PrimaryColor,
            Custom
        }

        private enum GroupColor
        {
            CellColor,
            PrimaryColor,
            Custom
        }

        private enum ClusterColor
        {
            CellColor,
            PrimaryColor,
            Custom
        }

        private enum IsoVerColor
        {
            CellColor,
            PrimaryColor,
            Custom
        }

        private enum BackColor
        {
            None,
            SecondaryColor,
            Custom
        }
        #endregion

        protected override OptionControlList OnSetupOptions(OptionContext optContext)
        {
            return new OptionControlList
            {
                new OptionEnumRadioButtons<GraphType>(OptionNames.GraphType, optContext, GraphType.Standard),
                new OptionInt32Slider(OptionNames.CellSize, optContext, 10, 10, 100)
                {
                    NumericUnit = new NumericUnit("px\u00B2")
                },
                new OptionInt32Slider(OptionNames.GroupSize, optContext, 5, 1, 10)
                {
                    NumericUnit = new NumericUnit("\u00B2")
                },
                new OptionInt32Slider(OptionNames.ClusterSize, optContext, 2, 1, 10)
                {
                    NumericUnit = new NumericUnit("\u00B2")
                },
                new OptionPanelBox(OptionNames.LineStylesBox, optContext)
                {
                    new OptionEnumRadioButtons<LineStyle>(OptionNames.CellLineStyle, optContext, LineStyle.Dotted)
                    {
                        Packed = true
                    },
                    new OptionEnumRadioButtons<LineStyle>(OptionNames.GroupLineStyle, optContext, LineStyle.Dashed)
                    {
                        Packed = true
                    },
                    new OptionEnumRadioButtons<LineStyle>(OptionNames.ClusterLineStyle, optContext, LineStyle.Solid)
                    {
                        Packed = true
                    }
                },
                new OptionPanelPagesAsTabs(OptionNames.ColorTabs, optContext)
                {
                    new OptionPanelPage(OptionNames.CellColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<CellColor>(OptionNames.CellColor, optContext, CellColor.Custom)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.CellColorWheel, optContext, EnvironmentParameters.PrimaryColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette),
                    },
                    new OptionPanelPage(OptionNames.GroupColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<GroupColor>(OptionNames.GroupColor, optContext, GroupColor.CellColor)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.GroupColorWheel, optContext, EnvironmentParameters.PrimaryColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette),
                    },
                    new OptionPanelPage(OptionNames.ClusterColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<ClusterColor>(OptionNames.ClusterColor, optContext, ClusterColor.CellColor)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.ClusterColorWheel, optContext, EnvironmentParameters.PrimaryColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette),
                    },
                    new OptionPanelPage(OptionNames.IsoVerColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<IsoVerColor>(OptionNames.IsoVerColor, optContext, IsoVerColor.Custom)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.IsoVerColorWheel, optContext,
                                             ColorBgra.Blend(new ColorBgra[] { EnvironmentParameters.PrimaryColor, EnvironmentParameters.SecondaryColor }).ToColor(),
                                             ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette)
                    },
                    new OptionPanelPage(OptionNames.BgColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<BackColor>(OptionNames.BgColor, optContext, BackColor.Custom)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.BgColorWheel, optContext, EnvironmentParameters.SecondaryColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette)
                    }
                }
            };
        }

        #region UI Rules
        protected override void OnAdaptOptions()
        {
            Option(OptionNames.ColorTabs).SuppressTokenUpdate = true;

            Option(OptionNames.CellColor).ValueChanged += (sender, e) => CellColorWheel_Rule();
            Option(OptionNames.GroupColor).ValueChanged += (sender, e) => GroupColorWheel_Rule();
            Option(OptionNames.ClusterColor).ValueChanged += (sender, e) => ClusterColorWheel_Rule();
            Option(OptionNames.IsoVerColor).ValueChanged += (sender, e) => IsoVerColorWheel_Rule();
            Option(OptionNames.BgColor).ValueChanged += (sender, e) => BgColorWheel_Rule();
            Option(OptionNames.GraphType).ValueChanged += (sender, e) =>
            {
                IsoVerColorWheel_Rule();
                IsoVerColor_Rule();
            };

            CellColorWheel_Rule();
            GroupColorWheel_Rule();
            ClusterColorWheel_Rule();
            IsoVerColorWheel_Rule();
            BgColorWheel_Rule();
            IsoVerColor_Rule();


            void CellColorWheel_Rule()
            {
                Option(OptionNames.CellColorWheel).ReadOnly = ((OptionEnumRadioButtons<CellColor>)Option(OptionNames.CellColor)).Value != CellColor.Custom;
            }

            void GroupColorWheel_Rule()
            {
                Option(OptionNames.GroupColorWheel).ReadOnly = ((OptionEnumRadioButtons<GroupColor>)Option(OptionNames.GroupColor)).Value != GroupColor.Custom;
            }

            void ClusterColorWheel_Rule()
            {
                Option(OptionNames.ClusterColorWheel).ReadOnly = ((OptionEnumRadioButtons<ClusterColor>)Option(OptionNames.ClusterColor)).Value != ClusterColor.Custom;
            }

            void IsoVerColorWheel_Rule()
            {
                Option(OptionNames.IsoVerColorWheel).ReadOnly = ((((OptionEnumRadioButtons<IsoVerColor>)Option(OptionNames.IsoVerColor)).Value != IsoVerColor.Custom) || (((OptionEnumRadioButtons<GraphType>)Option(OptionNames.GraphType)).Value != GraphType.Isometric));
            }

            void BgColorWheel_Rule()
            {
                Option(OptionNames.BgColorWheel).ReadOnly = ((OptionEnumRadioButtons<BackColor>)Option(OptionNames.BgColor)).Value != BackColor.Custom;
            }

            void IsoVerColor_Rule()
            {
                Option(OptionNames.IsoVerColor).ReadOnly = ((OptionEnumRadioButtons<GraphType>)Option(OptionNames.GraphType)).Value != GraphType.Isometric;
            }
        }
        #endregion

        protected override void OnSetRenderInfo(OptionBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            #region Token Stuff
            cellSize = OptionTypeSlider<int>.GetOptionValue(OptionNames.CellSize, newToken.Items);
            cellsPerGroup = OptionTypeSlider<int>.GetOptionValue(OptionNames.GroupSize, newToken.Items);
            cellsPerCluster = OptionTypeSlider<int>.GetOptionValue(OptionNames.ClusterSize, newToken.Items);
            graphType = OptionEnumRadioButtons<GraphType>.GetOptionValue(OptionNames.GraphType, newToken.Items);
            switch (OptionEnumRadioButtons<CellColor>.GetOptionValue(OptionNames.CellColor, newToken.Items))
            {
                case CellColor.PrimaryColor:
                    cellColor = EnvironmentParameters.PrimaryColor;
                    break;
                case CellColor.Custom:
                    cellColor = OptionColorWheel.GetOptionValue(OptionNames.CellColorWheel, newToken.Items);
                    break;
            }
            switch (OptionEnumRadioButtons<GroupColor>.GetOptionValue(OptionNames.GroupColor, newToken.Items))
            {
                case GroupColor.CellColor:
                    groupColor = cellColor;
                    break;
                case GroupColor.PrimaryColor:
                    groupColor = EnvironmentParameters.PrimaryColor;
                    break;
                case GroupColor.Custom:
                    groupColor = OptionColorWheel.GetOptionValue(OptionNames.GroupColorWheel, newToken.Items);
                    break;
            }
            switch (OptionEnumRadioButtons<ClusterColor>.GetOptionValue(OptionNames.ClusterColor, newToken.Items))
            {
                case ClusterColor.CellColor:
                    clusterColor = cellColor;
                    break;
                case ClusterColor.PrimaryColor:
                    clusterColor = EnvironmentParameters.PrimaryColor;
                    break;
                case ClusterColor.Custom:
                    clusterColor = OptionColorWheel.GetOptionValue(OptionNames.ClusterColorWheel, newToken.Items);
                    break;
            }
            switch (OptionEnumRadioButtons<IsoVerColor>.GetOptionValue(OptionNames.IsoVerColor, newToken.Items))
            {
                case IsoVerColor.CellColor:
                    isoVerColor = cellColor;
                    break;
                case IsoVerColor.PrimaryColor:
                    isoVerColor = EnvironmentParameters.PrimaryColor;
                    break;
                case IsoVerColor.Custom:
                    isoVerColor = OptionColorWheel.GetOptionValue(OptionNames.IsoVerColorWheel, newToken.Items);
                    break;
            }
            switch (OptionEnumRadioButtons<BackColor>.GetOptionValue(OptionNames.BgColor, newToken.Items))
            {
                case BackColor.None:
                    backColor = Color.Transparent;
                    break;
                case BackColor.SecondaryColor:
                    backColor = EnvironmentParameters.SecondaryColor;
                    break;
                case BackColor.Custom:
                    backColor = OptionColorWheel.GetOptionValue(OptionNames.BgColorWheel, newToken.Items);
                    break;
            }
            cellLineStyle = OptionEnumRadioButtons<LineStyle>.GetOptionValue(OptionNames.CellLineStyle, newToken.Items);
            groupLineStyle = OptionEnumRadioButtons<LineStyle>.GetOptionValue(OptionNames.GroupLineStyle, newToken.Items);
            clusterLineStyle = OptionEnumRadioButtons<LineStyle>.GetOptionValue(OptionNames.ClusterLineStyle, newToken.Items);
            #endregion


            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();
            float centerX = selection.Width / 2f;
            float centerY = selection.Height / 2f;

            Bitmap graphBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics graphGraphics = Graphics.FromImage(graphBitmap);

            // Fill background
            Rectangle backgroundRect = new Rectangle(0, 0, selection.Width, selection.Height);
            using (SolidBrush bgBrush = new SolidBrush(backColor))
                graphGraphics.FillRectangle(bgBrush, backgroundRect);

            // Set Variables
            Pen gridPen = new Pen(Color.Black);
            int xLoops, yLoops;
            PointF start, end, start2, end2;

            // Draw Graph
            switch (graphType)
            {
                case GraphType.Standard:
                    #region
                    // Calculate the number of lines will fit in the selection
                    xLoops = (int)Math.Ceiling((double)selection.Height / cellSize / 2);
                    yLoops = (int)Math.Ceiling((double)selection.Width / cellSize / 2);

                    // Sets (Cell, Group, Cluster)
                    for (byte set = 0; set < 3; set++)
                    {
                        switch (set)
                        {
                            case 0: // Cells
                                gridPen.Width = 1;
                                gridPen.Color = cellColor;
                                gridPen.DashStyle = GetDashStyle(cellLineStyle);
                                break;
                            case 1: // Groups
                                gridPen.Width = 1;
                                gridPen.Color = groupColor;
                                gridPen.DashStyle = GetDashStyle(groupLineStyle);
                                break;
                            case 2: // Clusters
                                gridPen.Width = 2;
                                gridPen.Color = clusterColor;
                                gridPen.DashStyle = GetDashStyle(clusterLineStyle);
                                break;
                        }

                        // Draw Vertical Lines
                        for (int i = 0; i < yLoops; i++)
                        {
                            if ((set == 2) && (i % (cellsPerGroup * cellsPerCluster) != 0))
                                continue;
                            if ((set == 1) && ((i % cellsPerGroup != 0) || (i % (cellsPerGroup * cellsPerCluster) == 0)))
                                continue;
                            if ((set == 0) && (i % cellsPerGroup == 0))
                                continue;

                            if (i == 0)
                            {
                                start = new PointF(centerX, 0);
                                end = new PointF(centerX, selection.Height);
                                graphGraphics.DrawLine(gridPen, start, end);
                            }
                            else
                            {
                                start = new PointF(centerX + cellSize * i, 0);
                                end = new PointF(centerX + cellSize * i, selection.Height);

                                start2 = new PointF(centerX - cellSize * i, 0);
                                end2 = new PointF(centerX - cellSize * i, selection.Height);

                                graphGraphics.DrawLine(gridPen, start, end);
                                graphGraphics.DrawLine(gridPen, start2, end2);
                            }
                        }

                        // Draw Horizontal Lines
                        for (int i = 0; i < xLoops; i++)
                        {
                            if ((set == 2) && (i % (cellsPerGroup * cellsPerCluster) != 0))
                                continue;
                            if ((set == 1) && ((i % cellsPerGroup != 0) || (i % (cellsPerGroup * cellsPerCluster) == 0)))
                                continue;
                            if ((set == 0) && (i % cellsPerGroup == 0))
                                continue;

                            if (i == 0)
                            {
                                start = new PointF(0, centerY);
                                end = new PointF(selection.Width, centerY);
                                graphGraphics.DrawLine(gridPen, start, end);
                            }
                            else
                            {
                                start = new PointF(0, centerY + cellSize * i);
                                end = new PointF(selection.Width, centerY + cellSize * i);

                                start2 = new PointF(0, centerY - cellSize * i);
                                end2 = new PointF(selection.Width, centerY - cellSize * i);

                                graphGraphics.DrawLine(gridPen, start, end);
                                graphGraphics.DrawLine(gridPen, start2, end2);
                            }
                        }
                    }
                    #endregion

                    break;
                case GraphType.Isometric:
                    #region
                    const double rad30 = Math.PI / 180 * 30;
                    const double rad60 = Math.PI / 180 * 60;
                    float sineHelper = (float)(Math.Sin(rad60) / Math.Sin(rad30));

                    // Calculate the number of lines will fit in the selection
                    float adjustedHeight = (float)(selection.Height + selection.Width * Math.Sin(rad30) / Math.Sin(rad60));
                    xLoops = (int)Math.Ceiling(adjustedHeight / cellSize);
                    yLoops = (int)Math.Ceiling(selection.Width / (cellSize * sineHelper));

                    // Draw Vertical Lines
                    for (int i = 0; i < yLoops; i++)
                    {
                        if (i % (cellsPerGroup * cellsPerCluster) == 0)
                        {
                            gridPen.Width = 2;
                            gridPen.Color = isoVerColor;
                        }
                        else if (i % cellsPerGroup == 0)
                        {
                            gridPen.Width = 1;
                            gridPen.Color = isoVerColor;
                        }
                        else
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Color.FromArgb(85, isoVerColor);
                        }

                        if (i == 0)
                        {
                            start = new PointF(centerX, 0);
                            end = new PointF(centerX, selection.Height);
                            graphGraphics.DrawLine(gridPen, start, end);
                        }
                        else
                        {
                            start = new PointF(centerX + cellSize / 2f * sineHelper * i, 0);
                            end = new PointF(centerX + cellSize / 2f * sineHelper * i, selection.Height);

                            start2 = new PointF(centerX - cellSize / 2f * sineHelper * i, 0);
                            end2 = new PointF(centerX - cellSize / 2f * sineHelper * i, selection.Height);

                            graphGraphics.DrawLine(gridPen, start, end);
                            graphGraphics.DrawLine(gridPen, start2, end2);
                        }
                    }

                    graphGraphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Sets (Cell, Group, Cluster)
                    for (byte set = 0; set < 3; set++)
                    {
                        switch (set)
                        {
                            case 0: // Cells
                                gridPen.Width = 1;
                                gridPen.Color = cellColor;
                                gridPen.DashStyle = GetDashStyle(cellLineStyle);
                                break;
                            case 1: // Groups
                                gridPen.Width = 1;
                                gridPen.Color = groupColor;
                                gridPen.DashStyle = GetDashStyle(groupLineStyle);
                                break;
                            case 2: // Clusters
                                gridPen.Width = 1.6f;
                                gridPen.Color = clusterColor;
                                gridPen.DashStyle = GetDashStyle(clusterLineStyle);
                                break;
                        }

                        // Draw Isometric Grid Lines
                        for (int i = 1; i < xLoops; i++)
                        {
                            if ((set == 2) && (i % (cellsPerGroup * cellsPerCluster) != 0))
                                continue;
                            if ((set == 1) && ((i % cellsPerGroup != 0) || (i % (cellsPerGroup * cellsPerCluster) == 0)))
                                continue;
                            if ((set == 0) && (i % cellsPerGroup == 0))
                                continue;

                            start = new PointF(0, cellSize * i);
                            end = new PointF(cellSize * i * sineHelper, 0);

                            start2 = new PointF(selection.Width, cellSize * i);
                            end2 = new PointF(selection.Width - cellSize * i * sineHelper, 0);

                            graphGraphics.DrawLine(gridPen, start, end);
                            graphGraphics.DrawLine(gridPen, start2, end2);
                        }
                    }
                    #endregion

                    break;
            }

            gridPen.Dispose();

            graphSurface = Surface.CopyFromBitmap(graphBitmap);
            graphBitmap.Dispose();


            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        private DashStyle GetDashStyle(LineStyle style)
        {
            switch (style)
            {
                case LineStyle.Solid:
                    return DashStyle.Solid;
                case LineStyle.Dashed:
                    return DashStyle.Dash;
                case LineStyle.Dotted:
                    return DashStyle.Dot;
                default:
                    return DashStyle.Solid;
            }
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, renderRects[i]);
            }
        }

        private void Render(Surface dst, Surface src, Rectangle rect)
        {
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    dst[x, y] = graphSurface.GetBilinearSample(x - selection.Left, y - selection.Top);
                }
            }
        }

        protected override ConfigurationOfUI OnCustomizeUI()
        {
            return new ConfigurationOfUI()
            {
                PropertyBasedLook = true
            };
        }

        protected override ConfigurationOfDialog OnCustomizeDialog()
        {
            return new ConfigurationOfDialog();
        }
    }
}