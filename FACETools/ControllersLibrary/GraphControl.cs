using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Sample = System.Collections.Generic.KeyValuePair<long, float>;

namespace ControllersLibrary
{
    public partial class GraphControl : UserControl
    {
        public class DataSamples
        {
            private List<Sample> data;
            public List<Sample> Data
            {
                get { return data; }
                set { data = value; }
            }

            private int count;
            private long lastTime;

            public DataSamples()
            {
                data = new List<Sample>();
                count = 0;
                lastTime = 0;
            }
            public Sample Last
            {
                get
                {
                    return new Sample(lastTime, data[data.Count - 1].Value);
                }
            }
            /// <summary>
            /// Add a sample annotated with a temporal mark.
            /// NOTE: this method assumes that samples come in order with respect to time!
            /// Samples in the past will be discarded.
            /// </summary>
            /// <param name="s">Sample to add</param>
            public void AddSample(Sample s)
            {
                    count++;
                    if (data.Count == 0)
                    {
                        lastTime = s.Key;
                        data.Add(s);
                        return;
                    }

                    // Discard data in the past
                    if (Last.Key > s.Key) return;

                    //if (Last.Value != s.Value)
                    //{
                    if (data[data.Count - 1].Key != lastTime)
                        data.Add(Last);
                    data.Add(s);
                    //}
                    lastTime = s.Key;
            }

            public int Count
            {
                get { return count; }
            }
            /// <summary>
            /// Beware! this is O(log(n))!
            /// </summary>
            /// <param name="time"></param>
            /// <returns></returns>
            public float this[long time]
            {
                get
                {
                        if (data.Count == 0)
                            throw new IndexOutOfRangeException("There are no data samples");
                        if (lastTime < time)
                            throw new IndexOutOfRangeException("Time is in the future!");

                        int s = 0, e = data.Count - 1, p = (s + e) / 2;
                        while ((e - s) > 1)
                        {
                            if (data[p].Key == time)
                                return data[p].Value;

                            if (data[p].Key < time)
                                s = p;
                            else
                                e = p;
                            p = (s + e) / 2;
                        }

                        if (data[s].Key == time)
                            return data[s].Value;
                        if (e > s && data[e].Key == time)
                            return data[e].Value;
                        if (data[e].Key < time) s = e;

                        // Interpolate
                        long next = s + 1 < data.Count ? data[s + 1].Key : time;
                        float value = s + 1 < data.Count ? data[s + 1].Value : Last.Value;
                        double span = next - data[s].Key;
                        double spanv = value - data[s].Value;
                        return (float)((((time - data[s].Key) / span) * spanv) + data[s].Value);
                    }
            }

            /// <summary>
            /// Find the minimum and maximum value in the interval. It uses interpolation,
            /// so a careful choice for start end and samplefreq may help in reducing
            /// the calculations.
            /// </summary>
            /// <param name="samplefreq"></param>
            /// <param name="start"></param>
            /// <param name="end"></param>
            /// <param name="min">The initial value for min. Use the lower bound you intend.</param>
            /// <param name="max">The initial value for max. Use the upper bound you intend.</param>
            public void FindMinMax(long samplefreq, long start, long end, ref float min, ref float max)
            {
                if (data.Count == 0) return;

                long i = Math.Max(start, 0L);
                end = Math.Min(end, lastTime);
                for (; i <= end; i += samplefreq)
                {
                    if (this[i] < min) min = this[i];
                    if (this[i] > max) max = this[i];
                }
                if (max == min)
                {
                    float delta = min == 0 ? 0.01f : 0.01f * Math.Abs(min);
                    max += delta;
                    min -= delta;
                }
            }

        }

        public class Style
        {
            public Style()
            {
            }

            public Style(GraphControl c)
            {
                GraphBackColor = c.GraphBackColor.ToArgb();
                LineColor = c.LineColor.ToArgb();
                AxisColor = c.AxisColor.ToArgb();
                ZeroColor = c.ZeroColor.ToArgb();
                BeginColor = c.BeginColor.ToArgb();

                labelVisible = c.labelVisible;
                boxVisible = c.boxVisible;

                leftTopMargin = c.leftTopMargin;
                rightBottomMargin = c.rightBottomMargin;

                MinVisibleValue = c.MinVisibleValue;
                MaxVisibleValue = c.MaxVisibleValue;

                verticalLines = c.verticalLines;
                verticalLabelFormat = c.VerticalLabelFormat;

                startTime = c.startTime;

                title = c.Text;
                titleFontFamily = c.titleFont.FontFamily.Name;
                titleFontSize = c.titleFont.Size;
                titleColor = c.TitleColor.ToArgb();
                titleDock = c.titleDock;

                visibleSamples = c.visibleSamples;

                showTooltip = c.showTooltip;

                timeScale = c.timeScale;
                timeFormat = c.timeFormat;
            }

            public int AxisColor;
            public int ZeroColor;
            public int BeginColor;
            public int LineColor;
            public int GraphBackColor;

            public bool labelVisible;
            public bool boxVisible;

            public Size leftTopMargin;
            public Size rightBottomMargin;

            public float MinVisibleValue;
            public float MaxVisibleValue;

            public int verticalLines;
            public string verticalLabelFormat;

            public long startTime;

            public string title;
            public string titleFontFamily;
            public float titleFontSize;
            public int titleColor;
            public DockStyle titleDock;

            public int visibleSamples;

            public bool showTooltip;

            public int timeScale;
            public string timeFormat;
        }


        private DataSamples data;
        public DataSamples DataSample
        {
            get { return data; }
            set { data = value; }
        }


        #region Config properties
        private Color axisColor;

        private static Color DefaultAxisColor = Color.Black;
        //private static Color DefaultLineColor = Color.LimeGreen;
        //private static Color DefaultGraphBackColor = Color.DarkBlue;
        private static Color DefaultLineColor = Color.Black;
        private static Color DefaultGraphBackColor = Color.LightGray;
        private static Color DefaultZeroColor = Color.Black;
        private static Color DefaultBeginColor = Color.Red;
        private const bool DefaultLabelVisible = true;
        private const bool DefaultBoxVisible = true;
        private static Size DefaultLeftTopMargin = new Size(10, 10);
        private static Size DefaultRightBottomMargin = new Size(10, 10);
        private const int DefaultVerticalLines = 0;
        private static string DefaultVerticalLabelFormat = "{0:F2}";
        private const long DefaultStartTime = 0;
        private const string DefaultTitle = "";
        private static Font DefaultTitleFont = new Font("Arial", 32);
        private static Color DefaultTitleColor = Color.FromArgb(40, Color.White);
        private const DockStyle DefaultTitleDock = DockStyle.None;
        private const int DefaultVisibleSamples = 6000; //before 600
        private const bool DefaultShowTooltip = false;
        private const int DefaultTimeScale = 10000000; // In 100-nanoseconds
        private const string DefaultTimeFormat = "{0:T}";

        [Category("Graph Style")]
        public Color AxisColor
        {
            get
            {
                return axisColor;
            }
            set
            {
                axisColor = value;
                Invalidate();
            }
        }

        private Color zeroColor;

        [Category("Graph Style")]
        public Color ZeroColor
        {
            get
            {
                return zeroColor;
            }
            set
            {
                zeroColor = value;
                Invalidate();
            }
        }

        private Color beginColor;

        [Category("Graph Style")]
        public Color BeginColor
        {
            get
            {
                return beginColor;
            }
            set
            {
                beginColor = value;
                Invalidate();
            }
        }

        private float minVisibleValue;

        [Category("Graph Style")]
        public float MinVisibleValue
        {
            get
            {
                return minVisibleValue;
            }
            set
            {
                minVisibleValue = value;
                lastMin = value;
                Invalidate();
            }
        }

        private float maxVisibleValue;

        [Category("Graph Style")]
        public float MaxVisibleValue
        {
            get
            {
                return maxVisibleValue;
            }
            set
            {
                maxVisibleValue = value;
                lastMax = value;
                Invalidate();
            }
        }

        private Size leftTopMargin;

        [Category("Graph Style")]
        public Size LeftTopMargin
        {
            get
            {
                return leftTopMargin;
            }
            set
            {
                leftTopMargin = value;
                Invalidate();
            }
        }

        private Size rightBottomMargin;

        [Category("Graph Style")]
        public Size RightBottomMargin
        {
            get
            {
                return rightBottomMargin;
            }
            set
            {
                rightBottomMargin = value;
                Invalidate();
            }
        }

        private bool labelVisible;

        [Category("Graph Style")]
        public bool LabelVisible
        {
            get
            {
                return labelVisible;
            }
            set
            {
                labelVisible = value;
                Invalidate();
            }
        }

        private bool boxVisible;

        [Category("Graph Style")]
        public bool BoxVisible
        {
            get
            {
                return boxVisible;
            }
            set
            {
                boxVisible = value;
                Invalidate();
            }
        }

        private int verticalLines;

        [Category("Graph Style")]
        public int VerticalLines
        {
            get
            {
                return verticalLines;
            }
            set
            {
                verticalLines = value;
                Invalidate();
            }
        }

        [Category("Graph Style")]
        public Color GraphBackColor
        {
            get
            {
                return BackColor;
            }
            set
            {
                BackColor = value;
                Invalidate();
            }
        }

        [Category("Graph Style")]
        public Color LineColor
        {
            get
            {
                return ForeColor;
            }
            set
            {
                ForeColor = value;
                Invalidate();
            }
        }

        private string verticalLabelFormat;
        [Category("Graph Style")]
        public string VerticalLabelFormat
        {
            get
            {
                return verticalLabelFormat;
            }
            set
            {
                verticalLabelFormat = value;
                Invalidate();
            }
        }

        private long startTime;
        [Category("Graph Style")]
        [Description("Set the time (in microseconds) assumed the 0 time of the experiment")]
        public long StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = value;
                Invalidate();
            }
        }

        [Category("Graph Style")]
        [Description("Set the title of the graph")]
        public string Title
        {
            get
            {
                return Text;
            }
            set
            {
                Text = value;
                Invalidate();
            }
        }

        private Font titleFont;
        [Category("Graph Style")]
        [Description("The font used for the title of the Graph")]
        public Font TitleFont
        {
            get
            {
                return titleFont;
            }
            set
            {
                titleFont = value;
                Invalidate();
            }
        }

        private Color titleColor;
        [Category("Graph Style")]
        [Description("The color used for the title of the Graph")]
        public Color TitleColor
        {
            get
            {
                return titleColor;
            }
            set
            {
                titleColor = value;
                Invalidate();
            }
        }

        private DockStyle titleDock;
        [Category("Graph Style")]
        public DockStyle TitleDock
        {
            get
            {
                return titleDock;
            }
            set
            {
                titleDock = value;
                Invalidate();
            }
        }

        // Call it before scrolling, so far is not supported
        [Category("Graph Style")]
        public int VisibleSamples
        {
            get
            {
                return visibleSamples;
            }
            set
            {
                visibleSamples = value;
                initView = startTime - visibleSamples;
                Invalidate();
            }
        }

        private bool showTooltip;
        [Category("Graph Style")]
        public bool ShowTooltip
        {
            get
            {
                return showTooltip;
            }
            set
            {
                showTooltip = value;
                Invalidate();
            }
        }

        private int timeScale;
        [Category("Graph Style")]
        public int TimeScale
        {
            get
            {
                return timeScale;
            }
            set
            {
                timeScale = value;
                Invalidate();
            }
        }

        private string timeFormat;
        [Category("Graph Style")]
        public string TimeFormat
        {
            get
            {
                return timeFormat;
            }
            set
            {
                timeFormat = value;
                Invalidate();
            }
        }

        #endregion

        public Style GetGraphStyle()
        {
            return new Style(this);
        }
        public void SetGraphStyle(Style v)
        {
            CopyStyle(v);
        }

        public void CopyStyle(Style c)
        {
            GraphBackColor = Color.FromArgb(c.GraphBackColor);
            LineColor = Color.FromArgb(c.LineColor);
            AxisColor = Color.FromArgb(c.AxisColor);
            ZeroColor = Color.FromArgb(c.ZeroColor);
            BeginColor = Color.FromArgb(c.BeginColor);

            labelVisible = c.labelVisible;
            boxVisible = c.boxVisible;

            leftTopMargin = c.leftTopMargin;
            rightBottomMargin = c.rightBottomMargin;
            minVisibleValue = c.MinVisibleValue;
            maxVisibleValue = c.MaxVisibleValue;

            verticalLines = c.verticalLines;
            verticalLabelFormat = c.verticalLabelFormat;

            startTime = c.startTime;

            Text = c.title;
            titleFont = new Font(c.titleFontFamily, c.titleFontSize);
            titleColor = Color.FromArgb(c.titleColor);
            titleDock = c.titleDock;

            visibleSamples = c.visibleSamples;

            showTooltip = c.showTooltip;

            timeScale = c.timeScale;
            timeFormat = c.timeFormat;
        }

        public void CopyStyle(GraphControl c)
        {
            BackColor = c.BackColor;
            ForeColor = c.ForeColor;
            AxisColor = c.AxisColor;
            ZeroColor = c.ZeroColor;
            BeginColor = c.BeginColor;

            labelVisible = c.labelVisible;
            boxVisible = c.boxVisible;

            leftTopMargin = c.leftTopMargin;
            rightBottomMargin = c.rightBottomMargin;

            minVisibleValue = c.minVisibleValue;
            maxVisibleValue = c.maxVisibleValue;

            verticalLines = c.verticalLines;
            verticalLabelFormat = c.verticalLabelFormat;

            startTime = c.startTime;

            Text = c.Text;
            titleFont = c.titleFont;
            titleColor = c.titleColor;
            titleDock = c.titleDock;

            visibleSamples = c.visibleSamples;

            showTooltip = c.showTooltip;

            timeScale = c.timeScale;
            timeFormat = c.timeFormat;
        }

        protected long initView;

        public int visibleSamples;

        private float absMin;
        private float absMax;
        private float lastMin;
        private float lastMax;

        private bool autoupdate = true;
        public bool UpdateViewOnSample
        {
            get { return autoupdate; }
            set { autoupdate = value; }
        }

        public void SetView(long firstsample)
        {
            if (autoupdate) return; // Ignore the set view
            initView = firstsample;
            if (data.Last.Key < initView + visibleSamples) initView = data.Last.Key - visibleSamples;
            Invalidate();
        }

        public GraphControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //InitializeComponent();

            data = new DataSamples();

            minVisibleValue = float.MaxValue;
            maxVisibleValue = float.MinValue;

            absMax = float.MinValue;
            absMin = float.MaxValue;
            lastMin = minVisibleValue;
            lastMax = maxVisibleValue;

            rightBottomMargin = DefaultRightBottomMargin;
            leftTopMargin = DefaultLeftTopMargin;

            verticalLines = DefaultVerticalLines;

            labelVisible = DefaultLabelVisible;
            boxVisible = DefaultBoxVisible;

            axisColor = DefaultAxisColor;
            zeroColor = DefaultZeroColor;
            beginColor = DefaultBeginColor;
            LineColor = DefaultLineColor;
            GraphBackColor = DefaultGraphBackColor;

            verticalLabelFormat = DefaultVerticalLabelFormat;

            startTime = DefaultStartTime;
            Text = DefaultTitle;
            titleFont = DefaultTitleFont;
            titleColor = DefaultTitleColor;
            titleDock = DefaultTitleDock;
            visibleSamples = DefaultVisibleSamples;
            showTooltip = DefaultShowTooltip;

            timeScale = DefaultTimeScale;
            timeFormat = DefaultTimeFormat;

            initView = startTime - visibleSamples;
        }

        public void FillWithSampleData(int count)
        {
            Random rnd = new Random();
            int i = 0;
            AddSample(i++, 49);
            for (; i < count - 1; i++)
            {
                float v = (float)(48 + 2 * rnd.NextDouble());
                AddSample(i, v);
            }
            AddSample(count - 1, 48);
            AddSample(count++, 49);
            AddSample(count++, 49);
            AddSample(count++, 49);
            AddSample(count++, 49);
            AddSample(count++, 49);
        }

        private void Zoom(int amount)
        {
            int newVisibleSamples = Math.Max(5, visibleSamples + amount);
            if (initView - startTime < 0)
                initView = startTime - newVisibleSamples + (initView + visibleSamples);
            visibleSamples = newVisibleSamples;
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int newVisibleSamples = Math.Max(5, visibleSamples + e.Delta);
            if (initView - startTime < 0)
                initView = startTime - newVisibleSamples + (initView + visibleSamples);
            visibleSamples = newVisibleSamples;
            Invalidate();
        }

        ToolTip valueTip = new ToolTip();
        // Used to approximate the "visible" property of the tooltip that is missing. Otherwise it would go in an infinite loop of repaint!
        Point lastShow = new Point();
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (showTooltip)
            {
                if (screenMargins.Contains(e.Location))
                {
                    long time = initView + (long)(visibleSamples * ((e.X - screenMargins.Left) / (float)screenMargins.Width));
                    if (lastShow != e.Location && time >= 0 && time <= data.Last.Key)
                    {
                        float v = lastMin + (lastMax - lastMin) * (1 - ((e.Y - screenMargins.Top) / (float)screenMargins.Height));
                        if (Math.Abs(v - data[time]) / (lastMax - lastMin) < 0.02f)
                        {
                            valueTip.Show(string.Format(verticalLabelFormat, data[time]), this, e.X, e.Y - 22, 2000);
                            lastShow = e.Location;
                        }
                    }
                }
            }
        }

        public void AddSample(long time, float value)
        {
            if (value < absMin) absMin = value;
            if (value > absMax) absMax = value;

            if (autoupdate && data.Count > 0)
                initView += time - data.Last.Key;

            data.AddSample(new Sample(time, value));

            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        private Rectangle screenMargins = new Rectangle();

        private DateTime GetTime(long time)
        {
            return new DateTime(time * timeScale);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                // Non sono sicuro serva
                int sampleLen = 1;
                Graphics g = e.Graphics;
                SizeF minbox = g.MeasureString(string.Format(verticalLabelFormat, lastMin), Font);
                SizeF maxbox = g.MeasureString(string.Format(verticalLabelFormat, lastMax), Font);
                SizeF valuebox = new SizeF(Math.Max(minbox.Width, maxbox.Width), Math.Max(minbox.Height, maxbox.Height));
                SizeF timebox = g.MeasureString(string.Format(timeFormat, GetTime(initView + visibleSamples)), Font);

                Size ltm = leftTopMargin;
                Size rbm = rightBottomMargin;

                if (labelVisible)
                {
                    ltm.Width = (int)Math.Max(ltm.Width, valuebox.Width + 5);
                    ltm.Height = (int)Math.Max(ltm.Height, (valuebox.Height / 2) + 2);
                    rbm.Height = (int)Math.Max(rbm.Height, timebox.Height + 5);
                }

                // Note: since we invert y axis we have to use Top instead of Bottom and vice versa
                Rectangle margins = new Rectangle(ltm.Width, rbm.Height, Width - ltm.Width - rbm.Width, Height - ltm.Height - rbm.Height);
                screenMargins = new Rectangle(ltm.Width, ltm.Height, Width - ltm.Width - rbm.Width, Height - ltm.Height - rbm.Height);

                g.TranslateTransform(margins.Left, Height - margins.Top);
                g.ScaleTransform(1, -1);

                float timexunit = Math.Max(1.0f, (visibleSamples * sampleLen) / (float)margins.Width);
                float pixelxunit = Math.Max(1.0f, margins.Width / (float)(visibleSamples * sampleLen));

                float min = minVisibleValue, max = maxVisibleValue;
                float pos = initView;
                data.FindMinMax((long)timexunit, initView, initView + visibleSamples * sampleLen, ref min, ref max);
                lastMin = min;
                lastMax = max;
                using (Pen linePen = new Pen(ForeColor), axisPen = new Pen(axisColor), zeroPen = new Pen(zeroColor), beginPen = new Pen(beginColor), gridPen = new Pen(axisColor))
                {
                    if (Text != null && Text != string.Empty && titleDock != DockStyle.None)
                    {
                        System.Drawing.Drawing2D.GraphicsState gs = g.Save();
                        g.ResetTransform();

                        SizeF sz = g.MeasureString(Text, TitleFont);
                        PointF p = new PointF();
                        switch (titleDock)
                        {
                            case DockStyle.Fill:
                                p = new PointF(margins.Left + (margins.Width - sz.Width) / 2, Height - margins.Top - margins.Height + (margins.Height - sz.Height) / 2);
                                break;
                            case DockStyle.Bottom:
                                p = new PointF(margins.Left + (margins.Width - sz.Width) / 2, Height - margins.Top - sz.Height);
                                break;
                            case DockStyle.Top:
                                p = new PointF(margins.Left + (margins.Width - sz.Width) / 2, margins.Bottom - margins.Height);
                                break;
                            case DockStyle.Left:
                                p = new PointF(margins.Left, Height - margins.Top - sz.Height);
                                break;
                            case DockStyle.Right:
                                p = new PointF(margins.Right - sz.Width, Height - margins.Top - sz.Height);
                                break;
                        }
                        g.DrawString(Text, TitleFont, new SolidBrush(titleColor), p);

                        g.Restore(gs);
                    }

                    g.DrawLine(axisPen, 0, 0, 0, margins.Height);
                    g.DrawLine(axisPen, 0, 0, margins.Width, 0);
                    if (boxVisible)
                    {
                        g.DrawLine(axisPen, margins.Width, 0, margins.Width, margins.Height);
                        g.DrawLine(axisPen, 0, margins.Height, margins.Width, margins.Height);
                    }

                    if (labelVisible || verticalLines > 0)
                    {
                        gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        gridPen.Color = Color.FromArgb(127, axisColor);
                    }

                    if (verticalLines > 0)
                    {
                        int px = margins.Width / (verticalLines + 1);
                        for (int i = 1; i <= verticalLines; i++)
                            g.DrawLine(gridPen, i * px, 0, i * px, margins.Height);
                    }

                    if (initView - startTime <= 0)
                    {
                        int sx = (int)((Math.Abs(StartTime - initView) / timexunit) * pixelxunit);
                        g.DrawLine(beginPen, sx, 0, sx, margins.Height);
                    }

                    if (labelVisible)
                    {
                        int nly = (int)((margins.Height / valuebox.Height) / 3);
                        int nlx = (int)((margins.Width / timebox.Width) / 3);
                        int pxly = margins.Height / Math.Max(nly, 1);
                        int pxlx = margins.Width / Math.Max(nlx, 1);
                        float dvy = (max - min) / nly;
                        float dvx = visibleSamples / (float)nlx;
                        System.Drawing.Drawing2D.GraphicsState gs = g.Save();
                        g.ResetTransform();

                        using (SolidBrush fontColor = new SolidBrush(axisColor))
                        {
                            // y labels
                            for (int i = 0; i <= nly; i++)
                            {
                                float liney = i * pxly + valuebox.Height / 2 + 2;
                                g.DrawString(string.Format(verticalLabelFormat, i * dvy + min), Font, fontColor, margins.Left - valuebox.Width, Height - margins.Top - (valuebox.Height / 2) - i * pxly);
                                if ((i == 0 && !boxVisible) || ((i > 0) && (i < nly)))
                                    g.DrawLine(gridPen, margins.Left, liney, margins.Right, liney);
                            }

                            // x labels
                            for (int i = 0; i <= nlx; i++)
                            {
                                float linex = i * pxlx + timebox.Width / 2 + 2;
                                long time = (long)(i * dvx + initView);
                                if (time > 0)
                                    g.DrawString(string.Format(timeFormat, GetTime(time)), Font, fontColor, margins.Left + (timebox.Width / 2) + i * pxlx, Height - margins.Top + 2);
                            }
                        }
                        g.Restore(gs);
                    }

                    if (max != min && min < 0)
                    {
                        int sy = (int)((margins.Height / (max - min)) * (0 - min));
                        g.DrawLine(zeroPen, 0, sy, margins.Width, sy);
                    }
                    for (int i = 0; i < (margins.Width / pixelxunit) && pos <= (initView + (visibleSamples * sampleLen) - timexunit); i++, pos += timexunit)
                    {
                        if (pos < 0) continue;
                        int sx = (int)(pixelxunit * i);
                        int dx = (int)(pixelxunit * (i + 1));
                        int sy = (int)((margins.Height / (max - min)) * (data[(long)pos] - min));
                        int dy = (int)((margins.Height / (max - min)) * (data[(long)(pos + timexunit)] - min));
                        g.DrawLine(linePen, sx, sy, dx, dy);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            //base.OnGotFocus(e);
            //Refresh();
        }
    }
}