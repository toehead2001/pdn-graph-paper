using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Collections.Generic;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

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
    public class GraphPaperEffectPlugin : PropertyBasedEffect
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
                return null;
                //return new Bitmap(typeof(GraphPaperEffectPlugin), "GraphPaper.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Render;  // Programmer's chosen default
            }
        }

        public GraphPaperEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuName, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4,
            Amount5,
            Amount6
        }

        public enum Amount4Options
        {
            Amount4Option1,
            Amount4Option2
        }


        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Amount1, 10, 10, 100));
            props.Add(new Int32Property(PropertyNames.Amount2, 5, 1, 10));
            props.Add(new Int32Property(PropertyNames.Amount3, 2, 1, 10));
            props.Add(StaticListChoiceProperty.CreateForEnum<Amount4Options>(PropertyNames.Amount4, 0, false));
            props.Add(new Int32Property(PropertyNames.Amount5, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff));
            props.Add(new Int32Property(PropertyNames.Amount6, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff));

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>();
            propRules.Add(new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.Amount6, PropertyNames.Amount4, Amount4Options.Amount4Option1, false));

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.DisplayName, "Cell Size");
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.DisplayName, "Cells per Group (squared)");
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.DisplayName, "Groups per Cluster (squared)");
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.DisplayName, "Graph Type");
            configUI.SetPropertyControlType(PropertyNames.Amount4, PropertyControlType.RadioButton);
            PropertyControlInfo Amount4Control = configUI.FindControlForPropertyName(PropertyNames.Amount4);
            Amount4Control.SetValueDisplayName(Amount4Options.Amount4Option1, "Standard");
            Amount4Control.SetValueDisplayName(Amount4Options.Amount4Option2, "Isometric");
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.DisplayName, "Line Color");
            configUI.SetPropertyControlType(PropertyNames.Amount5, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.DisplayName, "Secondary Line Color");
            configUI.SetPropertyControlType(PropertyNames.Amount6, PropertyControlType.ColorWheel);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Amount1).Value;
            Amount2 = newToken.GetProperty<Int32Property>(PropertyNames.Amount2).Value;
            Amount3 = newToken.GetProperty<Int32Property>(PropertyNames.Amount3).Value;
            Amount4 = (byte)((int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Amount4).Value);
            Amount5 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount5).Value);
            Amount6 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount6).Value);


            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();
            float centerX = selection.Width / 2f;
            float centerY = selection.Height / 2f;

            Bitmap graphBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics graphGraphics = Graphics.FromImage(graphBitmap);

            // Fill background
            Rectangle backgroundRect = new Rectangle(0, 0, selection.Width, selection.Height);
            graphGraphics.FillRectangle(new SolidBrush(Color.White), backgroundRect);

            // Set Variables
            Pen gridPen = new Pen(Color.Black);
            int xLoops, yLoops;
            PointF start, end, start2, end2;

            switch (Amount4)
            {
                case 0: // Standard
                    #region
                    // Calculate the number of lines will fit in the selection
                    xLoops = (int)Math.Ceiling((double)selection.Height / Amount1 / 2);
                    yLoops = (int)Math.Ceiling((double)selection.Width / Amount1 / 2);

                    // Draw Vertical Lines
                    for (int i = 0; i < yLoops; i++)
                    {
                        if (i % (Amount2 * Amount3) == 0)
                        {
                            gridPen.Width = 2;
                            gridPen.Color = Amount5;
                        }
                        else if (i % Amount2 == 0)
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Amount5;
                        }
                        else
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Color.FromArgb(85, Amount5);
                        }

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
                        if (i % (Amount2 * Amount3) == 0)
                        {
                            gridPen.Width = 2;
                            gridPen.Color = Amount5;
                        }
                        else if (i % Amount2 == 0)
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Amount5;
                        }
                        else
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Color.FromArgb(85, Amount5);
                        }

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
                    #endregion

                    break;
                case 1: // Isometric
                    #region
                    double rad30 = Math.PI / 180 * 30;
                    double rad60 = Math.PI / 180 * 60;
                    float sineHelper = (float)(Math.Sin(rad60) / Math.Sin(rad30));

                    float adjustedHeight = (float)(selection.Height + selection.Width * Math.Sin(rad30) / Math.Sin(rad60));
                    xLoops = (int)Math.Ceiling(adjustedHeight / Amount1);
                    yLoops = (int)Math.Ceiling(selection.Width / (Amount1 * sineHelper));

                    // Draw Vertical Lines
                    for (int i = 0; i < yLoops; i++)
                    {
                        if (i % (Amount2 * Amount3) == 0)
                        {
                            gridPen.Width = 2;
                            gridPen.Color = Amount6;
                        }
                        else if (i % Amount2 == 0)
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Amount6;
                        }
                        else
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Color.FromArgb(85, Amount6);
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

                    // Draw Grid Lines
                    for (int i = 1; i < xLoops; i++)
                    {
                        if (i % (Amount2 * Amount3) == 0)
                        {
                            gridPen.Width = 1.6f;
                            gridPen.Color = Amount5;
                        }
                        else if (i % Amount2 == 0)
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Amount5;
                        }
                        else
                        {
                            gridPen.Width = 1;
                            gridPen.Color = Color.FromArgb(85, Amount5);
                        }

                        start = new PointF(0, Amount1 * i);
                        end = new PointF(Amount1 * i * sineHelper, 0);

                        start2 = new PointF(selection.Width, Amount1 * i);
                        end2 = new PointF(selection.Width - Amount1 * i * sineHelper, 0);

                        graphGraphics.DrawLine(gridPen, start, end);
                        graphGraphics.DrawLine(gridPen, start2, end2);
                    }
                    #endregion

                    break;
            }

            gridPen.Dispose();

            graphSurface = Surface.CopyFromBitmap(graphBitmap);
            graphBitmap.Dispose();


            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
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
        int Amount1 = 10; // [2,100] Cell Size
        int Amount2 = 5; // [1,10] Cells per Group (squared)
        int Amount3 = 2; // [1,10] Groups per Cluster (squared)
        byte Amount4 = 0; // [1] Graph Type|Standard|Isometric
        ColorBgra Amount5 = ColorBgra.FromBgr(0, 0, 0); // Color
        ColorBgra Amount6 = ColorBgra.FromBgr(0, 0, 0); // Secondary Color
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

    }
}