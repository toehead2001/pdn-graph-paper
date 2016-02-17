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
        public string Author
        {
            get
            {
                return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            }
        }
        public string Copyright
        {
            get
            {
                return ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            }
        }

        public string DisplayName
        {
            get
            {
                return ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            }
        }

        public Version Version
        {
            get
            {
                return base.GetType().Assembly.GetName().Version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return new Uri("http://www.getpaint.net/redirect/plugins.html");
            }
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Graph Paper")]
    public class GraphPaperEffectPlugin : OptionBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return "Graph Paper";
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return new Bitmap(typeof(GraphPaperEffectPlugin), "GraphPaper.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Render;
            }
        }

        public GraphPaperEffectPlugin()
            : base(typeof(GraphPaperEffectPlugin), StaticIcon, EffectFlags.Configurable)
        {
        }

        #region Option Enums
        enum OptionNames
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

        enum GraphTypeEnum
        {
            Standard,
            Isometric
        }

        enum CellLineStyleEnum
        {
            Solid,
            Dashed,
            Dotted
        }

        enum GroupLineStyleEnum
        {
            Solid,
            Dashed,
            Dotted
        }

        enum ClusterLineStyleEnum
        {
            Solid,
            Dashed,
            Dotted
        }

        enum CellColorEnum
        {
            PrimaryColor,
            Custom
        }

        enum GroupColorEnum
        {
            CellColor,
            PrimaryColor,
            Custom
        }

        enum ClusterColorEnum
        {
            CellColor,
            PrimaryColor,
            Custom
        }

        enum IsoVerColorEnum
        {
            CellColor,
            PrimaryColor,
            Custom
        }

        enum BgColorEnum
        {
            None,
            SecondaryColor,
            Custom
        }
        #endregion


        protected override OptionControlList OnSetupOptions(OptionContext optContext)
        {
            OptionInt32Slider cellSize = new OptionInt32Slider(OptionNames.CellSize, optContext, 10, 10, 100);
            cellSize.NumericUnit = new NumericUnit("px\u00B2");
            OptionInt32Slider groupSize = new OptionInt32Slider(OptionNames.GroupSize, optContext, 5, 1, 10);
            groupSize.NumericUnit = new NumericUnit("\u00B2");
            OptionInt32Slider clusterSize = new OptionInt32Slider(OptionNames.ClusterSize, optContext, 2, 1, 10);
            clusterSize.NumericUnit = new NumericUnit("\u00B2");
            Color mixedColor = Color.FromArgb((EnvironmentParameters.PrimaryColor.A + EnvironmentParameters.SecondaryColor.A) / 2, (EnvironmentParameters.PrimaryColor.R + EnvironmentParameters.SecondaryColor.R) / 2,
                (EnvironmentParameters.PrimaryColor.G + EnvironmentParameters.SecondaryColor.G) / 2, (EnvironmentParameters.PrimaryColor.B + EnvironmentParameters.SecondaryColor.B) / 2);

            OptionControlList options = new OptionControlList
            {
                new OptionEnumRadioButtons<GraphTypeEnum>(OptionNames.GraphType, optContext, GraphTypeEnum.Standard),
                cellSize,
                groupSize,
                clusterSize,
                new OptionPanelBox(OptionNames.LineStylesBox, optContext)
                {
                    new OptionEnumRadioButtons<CellLineStyleEnum>(OptionNames.CellLineStyle, optContext, CellLineStyleEnum.Dotted)
                    {
                        Packed = true
                    },
                    new OptionEnumRadioButtons<GroupLineStyleEnum>(OptionNames.GroupLineStyle, optContext, GroupLineStyleEnum.Dashed)
                    {
                        Packed = true
                    },
                    new OptionEnumRadioButtons<ClusterLineStyleEnum>(OptionNames.ClusterLineStyle, optContext, ClusterLineStyleEnum.Solid)
                    {
                        Packed = true
                    }
                },
                new OptionPanelPagesAsTabs(OptionNames.ColorTabs, optContext)
                {
                    new OptionPanelPage(OptionNames.CellColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<CellColorEnum>(OptionNames.CellColor, optContext, CellColorEnum.Custom)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.CellColorWheel, optContext, EnvironmentParameters.PrimaryColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette),
                    },
                    new OptionPanelPage(OptionNames.GroupColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<GroupColorEnum>(OptionNames.GroupColor, optContext, GroupColorEnum.CellColor)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.GroupColorWheel, optContext, EnvironmentParameters.PrimaryColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette),
                    },
                    new OptionPanelPage(OptionNames.ClusterColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<ClusterColorEnum>(OptionNames.ClusterColor, optContext, ClusterColorEnum.CellColor)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.ClusterColorWheel, optContext, EnvironmentParameters.PrimaryColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette),
                    },
                    new OptionPanelPage(OptionNames.IsoVerColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<IsoVerColorEnum>(OptionNames.IsoVerColor, optContext, IsoVerColorEnum.Custom)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.IsoVerColorWheel, optContext, mixedColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette)
                    },
                    new OptionPanelPage(OptionNames.BgColorTab, optContext)
                    {
                        new OptionEnumRadioButtons<BgColorEnum>(OptionNames.BgColor, optContext, BgColorEnum.Custom)
                        {
                            Packed = true
                        },
                        new OptionColorWheel(OptionNames.BgColorWheel, optContext, EnvironmentParameters.SecondaryColor, ColorWheelEnum.AddAlpha | ColorWheelEnum.AddPalette)
                    }
                }
            };
            return options;
        }

        #region UI Rules
        protected override void OnAdaptOptions()
        {
            Option(OptionNames.ColorTabs).SuppressTokenUpdate = true;

            Option(OptionNames.CellColor).ValueChanged += new OptionValueEventHandler(CellColorRadio_ValueChanged);
            Option(OptionNames.GroupColor).ValueChanged += new OptionValueEventHandler(GroupColorRadio_ValueChanged);
            Option(OptionNames.ClusterColor).ValueChanged += new OptionValueEventHandler(ClusterColorRadio_ValueChanged);
            Option(OptionNames.IsoVerColor).ValueChanged += new OptionValueEventHandler(IsoVerColorRadio_ValueChanged);
            Option(OptionNames.BgColor).ValueChanged += new OptionValueEventHandler(BgColorRadio_ValueChanged);

            Option(OptionNames.GraphType).ValueChanged += new OptionValueEventHandler(GraphTypeRadio_ValueChanged);

            CellColorWheel_Rule();
            GroupColorWheel_Rule();
            ClusterColorWheel_Rule();
            IsoVerColorWheel_Rule();
            BgColorWheel_Rule();
            IsoVerColor_Rule();
        }

        private void CellColorRadio_ValueChanged(object sender, EventArgs e)
        {
            CellColorWheel_Rule();
        }
        private void CellColorWheel_Rule()
        {
            Option(OptionNames.CellColorWheel).ReadOnly = ((OptionEnumRadioButtons<CellColorEnum>)Option(OptionNames.CellColor)).Value != CellColorEnum.Custom;
        }

        private void GroupColorRadio_ValueChanged(object sender, EventArgs e)
        {
            GroupColorWheel_Rule();
        }
        private void GroupColorWheel_Rule()
        {
            Option(OptionNames.GroupColorWheel).ReadOnly = ((OptionEnumRadioButtons<GroupColorEnum>)Option(OptionNames.GroupColor)).Value != GroupColorEnum.Custom;
        }

        private void ClusterColorRadio_ValueChanged(object sender, EventArgs e)
        {
            ClusterColorWheel_Rule();
        }
        private void ClusterColorWheel_Rule()
        {
            Option(OptionNames.ClusterColorWheel).ReadOnly = ((OptionEnumRadioButtons<ClusterColorEnum>)Option(OptionNames.ClusterColor)).Value != ClusterColorEnum.Custom;
        }

        private void IsoVerColorRadio_ValueChanged(object sender, EventArgs e)
        {
            IsoVerColorWheel_Rule();
        }
        private void IsoVerColorWheel_Rule()
        {
            Option(OptionNames.IsoVerColorWheel).ReadOnly = ((((OptionEnumRadioButtons<IsoVerColorEnum>)Option(OptionNames.IsoVerColor)).Value != IsoVerColorEnum.Custom) || (((OptionEnumRadioButtons<GraphTypeEnum>)Option(OptionNames.GraphType)).Value != GraphTypeEnum.Isometric));
        }

        private void BgColorRadio_ValueChanged(object sender, EventArgs e)
        {
            BgColorWheel_Rule();
        }
        private void BgColorWheel_Rule()
        {
            Option(OptionNames.BgColorWheel).ReadOnly = ((OptionEnumRadioButtons<BgColorEnum>)Option(OptionNames.BgColor)).Value != BgColorEnum.Custom;
        }

        private void GraphTypeRadio_ValueChanged(object sender, EventArgs e)
        {
            IsoVerColorWheel_Rule();
            IsoVerColor_Rule();
        }
        private void IsoVerColor_Rule()
        {
            Option(OptionNames.IsoVerColor).ReadOnly = ((OptionEnumRadioButtons<GraphTypeEnum>)Option(OptionNames.GraphType)).Value != GraphTypeEnum.Isometric;
        }
        #endregion

        protected override void OnSetRenderInfo(OptionBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            #region Token Stuff
            byte cellColorRadio = (byte)OptionEnumRadioButtons<CellColorEnum>.GetOptionValue(OptionNames.CellColor, newToken.Items);
            byte groupColorRadio = (byte)OptionEnumRadioButtons<GroupColorEnum>.GetOptionValue(OptionNames.GroupColor, newToken.Items);
            byte clusterColorRadio = (byte)OptionEnumRadioButtons<ClusterColorEnum>.GetOptionValue(OptionNames.ClusterColor, newToken.Items);
            byte isoVerColorRadio = (byte)OptionEnumRadioButtons<IsoVerColorEnum>.GetOptionValue(OptionNames.IsoVerColor, newToken.Items);
            byte bgColorRadio = (byte)OptionEnumRadioButtons<BgColorEnum>.GetOptionValue(OptionNames.BgColor, newToken.Items);

            Amount1 = OptionTypeSlider<int>.GetOptionValue(OptionNames.CellSize, newToken.Items);
            Amount2 = OptionTypeSlider<int>.GetOptionValue(OptionNames.GroupSize, newToken.Items);
            Amount3 = OptionTypeSlider<int>.GetOptionValue(OptionNames.ClusterSize, newToken.Items);
            Amount4 = (byte)OptionEnumRadioButtons<GraphTypeEnum>.GetOptionValue(OptionNames.GraphType, newToken.Items);
            switch (cellColorRadio)
            {
                case 0: // Primary Color
                    Amount5 = EnvironmentParameters.PrimaryColor;
                    break;
                case 1: // Custom
                    Amount5 = OptionColorWheel.GetOptionValue(OptionNames.CellColorWheel, newToken.Items);
                    break;
            }
            switch (groupColorRadio)
            {
                case 0: // Cell Color
                    Amount6 = Amount5;
                    break;
                case 1: // Primary Color
                    Amount6 = EnvironmentParameters.PrimaryColor;
                    break;
                case 2: // Custom
                    Amount6 = OptionColorWheel.GetOptionValue(OptionNames.GroupColorWheel, newToken.Items);
                    break;
            }
            switch (clusterColorRadio)
            {
                case 0: // Cell Color
                    Amount7 = Amount5;
                    break;
                case 1: // Primary Color
                    Amount7 = EnvironmentParameters.PrimaryColor;
                    break;
                case 2: // Custom
                    Amount7 = OptionColorWheel.GetOptionValue(OptionNames.ClusterColorWheel, newToken.Items);
                    break;
            }
            switch (isoVerColorRadio)
            {
                case 0: // Cell Color
                    Amount8 = Amount5;
                    break;
                case 1: // Primary Color
                    Amount8 = EnvironmentParameters.PrimaryColor;
                    break;
                case 2: // Custom
                    Amount8 = OptionColorWheel.GetOptionValue(OptionNames.IsoVerColorWheel, newToken.Items);
                    break;
            }
            switch (bgColorRadio)
            {
                case 0: // None
                    Amount9 = Color.Transparent;
                    break;
                case 1: // Secondary Color
                    Amount9 = EnvironmentParameters.SecondaryColor;
                    break;
                case 2: // Custom
                    Amount9 = OptionColorWheel.GetOptionValue(OptionNames.BgColorWheel, newToken.Items);
                    break;
            }
            Amount10 = (byte)OptionEnumDropDown<CellLineStyleEnum>.GetOptionValue(OptionNames.CellLineStyle, newToken.Items);
            Amount11 = (byte)OptionEnumDropDown<GroupLineStyleEnum>.GetOptionValue(OptionNames.GroupLineStyle, newToken.Items);
            Amount12 = (byte)OptionEnumDropDown<ClusterLineStyleEnum>.GetOptionValue(OptionNames.ClusterLineStyle, newToken.Items);
            #endregion


            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();
            float centerX = selection.Width / 2f;
            float centerY = selection.Height / 2f;

            Bitmap graphBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics graphGraphics = Graphics.FromImage(graphBitmap);

            // Fill background
            Rectangle backgroundRect = new Rectangle(0, 0, selection.Width, selection.Height);
            using (SolidBrush bgBrush = new SolidBrush(Amount9))
                graphGraphics.FillRectangle(bgBrush, backgroundRect);

            // Set Variables
            Pen gridPen = new Pen(Color.Black);
            int xLoops, yLoops;
            PointF start, end, start2, end2;

            // Draw Graph
            switch (Amount4)
            {
                case 0: // Standard
                    #region
                    // Calculate the number of lines will fit in the selection
                    xLoops = (int)Math.Ceiling((double)selection.Height / Amount1 / 2);
                    yLoops = (int)Math.Ceiling((double)selection.Width / Amount1 / 2);

                    // Sets (Cell, Group, Cluster)
                    for (byte set = 0; set < 3; set++)
                    {
                        switch (set)
                        {
                            case 0: // Cells
                                gridPen.Width = 1;
                                gridPen.Color = Amount5;
                                gridPen.DashStyle = getDashStyle(Amount10);
                                break;
                            case 1: // Groups
                                gridPen.Width = 1;
                                gridPen.Color = Amount6;
                                gridPen.DashStyle = getDashStyle(Amount11);
                                break;
                            case 2: // Clusters
                                gridPen.Width = 2;
                                gridPen.Color = Amount7;
                                gridPen.DashStyle = getDashStyle(Amount12);
                                break;
                        }

                        // Draw Vertical Lines
                        for (int i = 0; i < yLoops; i++)
                        {
                            if ((set == 2) && (i % (Amount2 * Amount3) != 0))
                                continue;
                            else if ((set == 1) && ((i % Amount2 != 0) || (i % (Amount2 * Amount3) == 0)))
                                continue;
                            else if ((set == 0) && (i % Amount2 == 0))
                                continue;

                            if (i == 0)
                            {
                                start = new PointF(centerX, 0);
                                end = new PointF(centerX, selection.Height);
                                graphGraphics.DrawLine(gridPen, start, end);
                            }
                            else
                            {
                                start = new PointF(centerX + Amount1 * i, 0);
                                end = new PointF(centerX + Amount1 * i, selection.Height);

                                start2 = new PointF(centerX - Amount1 * i, 0);
                                end2 = new PointF(centerX - Amount1 * i, selection.Height);

                                graphGraphics.DrawLine(gridPen, start, end);
                                graphGraphics.DrawLine(gridPen, start2, end2);
                            }
                        }

                        // Draw Horizontal Lines
                        for (int i = 0; i < yLoops; i++)
                        {
                            if ((set == 2) && (i % (Amount2 * Amount3) != 0))
                                continue;
                            else if ((set == 1) && ((i % Amount2 != 0) || (i % (Amount2 * Amount3) == 0)))
                                continue;
                            else if ((set == 0) && (i % Amount2 == 0))
                                continue;

                            if (i == 0)
                            {
                                start = new PointF(0, centerY);
                                end = new PointF(selection.Width, centerY);
                                graphGraphics.DrawLine(gridPen, start, end);
                            }
                            else
                            {
                                start = new PointF(0, centerY + Amount1 * i);
                                end = new PointF(selection.Width, centerY + Amount1 * i);

                                start2 = new PointF(0, centerY - Amount1 * i);
                                end2 = new PointF(selection.Width, centerY - Amount1 * i);

                                graphGraphics.DrawLine(gridPen, start, end);
                                graphGraphics.DrawLine(gridPen, start2, end2);
                            }
                        }
                    }
                    #endregion

                    break;
                case 1: // Isometric
                    #region
                    double rad30 = Math.PI / 180 * 30;
                    double rad60 = Math.PI / 180 * 60;
                    float sineHelper = (float)(Math.Sin(rad60) / Math.Sin(rad30));

                    // Calculate the number of lines will fit in the selection
                    float adjustedHeight = (float)(selection.Height + selection.Width * Math.Sin(rad30) / Math.Sin(rad60));
                    xLoops = (int)Math.Ceiling(adjustedHeight / Amount1);
                    yLoops = (int)Math.Ceiling(selection.Width / (Amount1 * sineHelper));

                    // Draw Vertical Lines
                    for (int i = 0; i < yLoops; i++)
                    {
                        if (i % (Amount2 * Amount3) == 0)
                        {
                            gridPen.Width = 2;
                            gridPen.Color = Amount8;
                        }
                        else if (i % Amount2 == 0)
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Amount8;
                        }
                        else
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Color.FromArgb(85, Amount8);
                        }

                        if (i == 0)
                        {
                            start = new PointF(centerX, 0);
                            end = new PointF(centerX, selection.Height);
                            graphGraphics.DrawLine(gridPen, start, end);
                        }
                        else
                        {
                            start = new PointF(centerX + Amount1 / 2f * sineHelper * i, 0);
                            end = new PointF(centerX + Amount1 / 2f * sineHelper * i, selection.Height);

                            start2 = new PointF(centerX - Amount1 / 2f * sineHelper * i, 0);
                            end2 = new PointF(centerX - Amount1 / 2f * sineHelper * i, selection.Height);

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
                                gridPen.Color = Amount5;
                                gridPen.DashStyle = getDashStyle(Amount10);
                                break;
                            case 1: // Groups
                                gridPen.Width = 1;
                                gridPen.Color = Amount6;
                                gridPen.DashStyle = getDashStyle(Amount11);
                                break;
                            case 2: // Clusters
                                gridPen.Width = 1.6f;
                                gridPen.Color = Amount7;
                                gridPen.DashStyle = getDashStyle(Amount12);
                                break;
                        }

                        // Draw Isometric Grid Lines
                        for (int i = 1; i < xLoops; i++)
                        {
                            if ((set == 2) && (i % (Amount2 * Amount3) != 0))
                                continue;
                            else if ((set == 1) && ((i % Amount2 != 0) || (i % (Amount2 * Amount3) == 0)))
                                continue;
                            else if ((set == 0) && (i % Amount2 == 0))
                                continue;

                            start = new PointF(0, Amount1 * i);
                            end = new PointF(Amount1 * i * sineHelper, 0);

                            start2 = new PointF(selection.Width, Amount1 * i);
                            end2 = new PointF(selection.Width - Amount1 * i * sineHelper, 0);

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

        // Fetch Dash Styles
        DashStyle getDashStyle(int set)
        {
            DashStyle dashStyleOfSet;
            switch (set)
            {
                case 0: // Solid
                    dashStyleOfSet = DashStyle.Solid;
                    break;
                case 1: // Dashed
                    dashStyleOfSet = DashStyle.Dash;
                    break;
                case 2: // Dotted
                    dashStyleOfSet = DashStyle.Dot;
                    break;
                default:
                    dashStyleOfSet = DashStyle.Solid;
                    break;
            }
            return dashStyleOfSet;
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, renderRects[i]);
            }
        }

        #region CodeLab
        int Amount1; // [2,100] Cell Size
        int Amount2; // [1,10] Cells per Group (squared)
        int Amount3; // [1,10] Groups per Cluster (squared)
        byte Amount4; // [1] Graph Type|Standard|Isometric
        ColorBgra Amount5; // Cell Color
        ColorBgra Amount6; // Group Color
        ColorBgra Amount7; // Cluster Color
        ColorBgra Amount8; // IsoVer Color
        ColorBgra Amount9; // Background Color
        byte Amount10; // Cell Dash Style
        byte Amount11; // Group Dash Style
        byte Amount12; // Cluster Dash Style
        #endregion

        Surface graphSurface;

        void Render(Surface dst, Surface src, Rectangle rect)
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