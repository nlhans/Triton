using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Triton.Controls
{
    public enum VisualTabRenderer_Mode
    {
        SELECTED_FIRST,
        SELECTED_LAST,
        NO_PREFERENCE
    };


    public interface IVisualTabRenderer
    {
        void DrawTabItem(Graphics g, Rectangle bounds, TabItemState state);
        void DrawTabPage(Graphics g, Rectangle bounds);
        Rectangle GetModifiedBounds(Rectangle bounds);
        Rectangle GetOffsetBounds(Rectangle bounds, int index);
        Rectangle GetTextBounds(Rectangle bounds);
        void DrawCloseButton(Graphics g, Rectangle bounds, PushButtonState state);
        void DrawScrollButtons(Graphics g, Rectangle bounds, Point MouseLocation, Point MouseClicking, bool Cross, bool Enabled);
        bool HitsCloseButton(Point p, Rectangle bounds);

        Padding Margins { get; }

        int CloseButtonWidth { get; }
        int ScrollButtonWidth { get; }
        int drawHeight { get; }
        Rectangle ArrowLeft { get; }
        Rectangle ArrowRight { get; }

        VisualTabRenderer_Mode Mode { get; }
    }

    public class VisualTabRenderer_Windows : IVisualTabRenderer
    {
        public Padding Margins
        {
            get
            {
                return new Padding(20, 20, 15, 15);
            }
        }

        public int CloseButtonWidth
        {
            get
            {
                return 20;
            }
        }

        public int ScrollButtonWidth
        {
            get
            {
                return 56;
            }
        }
        public Rectangle GetModifiedBounds(Rectangle bounds)
        {
            return new Rectangle(bounds.X, bounds.Y + 3, bounds.Width, bounds.Height - 3);
        }

        public Rectangle GetOffsetBounds(Rectangle bounds, int index)
        {
            return new Rectangle(bounds.X + bounds.Width - index - 1, bounds.Y, bounds.Width, bounds.Height);
        }

        public Rectangle GetTextBounds(Rectangle bounds)
        {
            return new Rectangle(bounds.X + 3, bounds.Y + 6, bounds.Width - 6, bounds.Height - 6);
        }

        public void DrawCloseButton(Graphics g, Rectangle bounds, PushButtonState state)
        {
            if (TabRenderer.IsSupported == false)
                (new VisualTabRenderer_VS2008()).DrawCloseButton(g, bounds, state);
            //bounds = new Rectangle(bounds.X + Margins.Left, bounds.Y, bounds.Width - Margins.Horizontal, bounds.Height);
            //MessageBox.Show(bounds.ToString());
            //bounds.Offset(Margins.Left, Margins.Top);
            //bounds.Inflate(Margins.Horizontal, Margins.Vertical);

            Rectangle cross = new Rectangle(new Point(bounds.X + bounds.Width - 28, bounds.Y + 2), new Size(20, 21));

            g.FillRectangle(SystemBrushes.Control, new Rectangle(cross.X - 5, cross.Y - 2, cross.Width + 40, drawHeight));
            ButtonRenderer.DrawButton(g, cross, state);
            g.DrawString("X", SystemFonts.DialogFont, SystemBrushes.ControlText, new PointF(cross.X + 4, cross.Y + 4));

        }

        public bool HitsCloseButton(Point p, Rectangle bounds)
        {
            Rectangle cross = new Rectangle(new Point(bounds.X + bounds.Width - 28, bounds.Y + 2), new Size(20, 21));

            if (cross.Contains(p))
                return true;
            return false;

        }


        private Rectangle arrow_left = new Rectangle(0, 0, 1, 1);
        private Rectangle arrow_right = new Rectangle(0, 0, 1, 1);

        public Rectangle ArrowLeft
        {
            get
            {
                return arrow_left;
            }
        }
        public Rectangle ArrowRight
        {
            get
            {
                return arrow_right;
            }
        }

        private Rectangle GenerateArrowBounds(Rectangle bounds, bool Cross)
        {
            Rectangle arrows = new Rectangle(bounds.X + bounds.Width - 56 - ((Cross) ? 20 : 0), bounds.Y, 45, drawHeight);
            arrow_left = new Rectangle(arrows.X + 5, arrows.Y + (arrows.Height - 20) / 2, 18, 20);
            arrow_right = new Rectangle(arrows.X + 22, arrows.Y + (arrows.Height - 20) / 2, 18, 20);

            return arrows;
        }

        public void DrawScrollButtons(Graphics g, Rectangle bounds, Point Mouselocation, Point MouseClicking, bool Cross, bool Enabled)
        {
            Rectangle Arrows = GenerateArrowBounds(bounds, Cross);
            Arrows = new Rectangle(Arrows.X, Arrows.Y, Arrows.Width, bounds.Height);
            g.FillRectangle(SystemBrushes.Control, Arrows);

            PushButtonState state_left = PushButtonState.Normal;
            PushButtonState state_right = PushButtonState.Normal;
            if (ArrowLeft.Contains(Mouselocation))
            {
                state_left = PushButtonState.Hot;
            }
            if (ArrowRight.Contains(Mouselocation))
            {
                state_right = PushButtonState.Hot;
            }
            if (ArrowLeft.Contains(MouseClicking))
            {
                state_left = PushButtonState.Pressed;
            }
            if (ArrowRight.Contains(MouseClicking))
            {
                state_right = PushButtonState.Pressed;
            }
            if (!Enabled)
            {
                state_left = PushButtonState.Disabled;
                state_right = PushButtonState.Disabled;
            }
            ButtonRenderer.DrawButton(g, ArrowLeft, state_left);
            ButtonRenderer.DrawButton(g, ArrowRight, state_right);
            g.DrawString("<", SystemFonts.DialogFont, SystemBrushes.ControlText, new Point(arrow_left.X + 3, arrow_left.Y + 3));
            g.DrawString(">", SystemFonts.DialogFont, SystemBrushes.ControlText, new Point(arrow_right.X + 3, arrow_right.Y + 3));
        }

        private void DrawCloseCross(Graphics g, Rectangle bounds)
        {
            Pen dark = new Pen(SystemBrushes.ControlDarkDark, 2f);

            g.DrawLine(dark, new Point(bounds.X + 1, bounds.Y + 1), new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            g.DrawLine(dark, new Point(bounds.X + bounds.Width, bounds.Y + 1), new Point(bounds.X + 1, bounds.Y + bounds.Height));
        }

        public void DrawTabItem(Graphics g, Rectangle bounds, TabItemState state)
        {

            if (TabRenderer.IsSupported == false)
                (new VisualTabRenderer_VS2008()).DrawTabItem(g, bounds, state);
            bounds = GetModifiedBounds(bounds);
            TabRenderer.DrawTabItem(g, bounds, state);

        }
        public void DrawTabPage(Graphics g, Rectangle bounds)
        {

            if (TabRenderer.IsSupported == false)
                (new VisualTabRenderer_VS2008()).DrawTabPage(g, bounds);
            TabRenderer.DrawTabPage(g, bounds);

        }

        public VisualTabRenderer_Mode Mode
        {
            get
            {
                return VisualTabRenderer_Mode.NO_PREFERENCE;
            }
        }

        public int drawHeight
        {
            get
            {
                return 25;
            }
        }
    }

    public class VisualTabRenderer_VS2008 : IVisualTabRenderer
    {
        public Padding Margins
        {
            get
            {
                return new Padding(20, 20, 15, 15);
            }
        }
        private static int Tab_SlopeWidth = 15;
        private static int Tab_Height = 18;
        public int CloseButtonWidth
        {
            get
            {
                return 30;
            }
        }

        public int ScrollButtonWidth
        {
            get
            {
                return 28;
            }
        }


        public int drawHeight
        {
            get
            {
                return Tab_Height;
            }
        }

        public Rectangle GetModifiedBounds(Rectangle bounds)
        {
            return new Rectangle(bounds.X, bounds.Y, bounds.Width + Tab_SlopeWidth, Tab_Height);
        }

        public Rectangle GetOffsetBounds(Rectangle bounds, int index)
        {
            return new Rectangle(bounds.X + bounds.Width, bounds.Y, bounds.Width, bounds.Height);
        }

        public Rectangle GetTextBounds(Rectangle bounds)
        {
            return new Rectangle(bounds.X + Tab_SlopeWidth + 2, bounds.Y + 1, bounds.Width - Tab_SlopeWidth, Tab_Height);
        }

        public void DrawTabItem(Graphics g, Rectangle bounds, TabItemState state)
        {
            int offset = bounds.Height - Tab_Height;

            SmoothingMode _Old = SmoothingMode.None;
            bool _ModifiedSmoothMode = false;
            if (g.SmoothingMode != SmoothingMode.AntiAlias && g.SmoothingMode != SmoothingMode.HighQuality)
            {
                _Old = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                _ModifiedSmoothMode = true;
            }
            bounds = GetModifiedBounds(bounds);
            GraphicsPath path = new GraphicsPath();

            path.AddLine(bounds.X, bounds.Y + Tab_Height, bounds.X + Tab_SlopeWidth - 4, bounds.Y + 2);
            path.AddLine(bounds.X + Tab_SlopeWidth - 4, bounds.Y + 2, bounds.X + Tab_SlopeWidth, bounds.Y);
            path.AddLine(bounds.X + Tab_SlopeWidth, bounds.Y, bounds.X + bounds.Width, bounds.Y);
            path.AddLine(bounds.X + bounds.Width, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height);
            path.CloseFigure();

            LinearGradientBrush background = new LinearGradientBrush(bounds, SystemColors.ControlLightLight, SystemColors.Control, LinearGradientMode.Vertical);

            LinearGradientBrush background_active = new LinearGradientBrush(bounds, SystemColors.Control, SystemColors.GradientInactiveCaption, LinearGradientMode.Vertical);

            if (state == TabItemState.Selected)
            {
                g.FillPath(background, path);
                g.DrawPath(SystemPens.ControlDark, path);
            }
            else if (state == TabItemState.Normal)
            {
                g.FillPath(background_active, path);
                g.DrawPath(SystemPens.InactiveCaption, path);
            }

            if (_ModifiedSmoothMode)
                g.SmoothingMode = _Old;
        }

        public void DrawTabPage(Graphics g, Rectangle bounds)
        {

            g.DrawRectangle(SystemPens.ControlDark, new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 4, bounds.Height - 3));


        }
        public VisualTabRenderer_Mode Mode
        {
            get
            {
                return VisualTabRenderer_Mode.SELECTED_LAST;
            }
        }

        public bool HitsCloseButton(Point p, Rectangle bounds)
        {
            Rectangle cross = new Rectangle(new Point(bounds.Width - 14 + bounds.X, bounds.Y + 4), new Size(9, 9));

            if (cross.Contains(p))
                return true;
            return false;

        }


        private Rectangle arrow_left = new Rectangle(0, 0, 1, 1);
        private Rectangle arrow_right = new Rectangle(0, 0, 1, 1);

        public Rectangle ArrowLeft
        {
            get
            {
                return arrow_left;
            }
        }
        public Rectangle ArrowRight
        {
            get
            {
                return arrow_right;
            }
        }

        private Rectangle GenerateArrowBounds(Rectangle bounds, bool Cross)
        {
            Rectangle arrows = new Rectangle(bounds.X + bounds.Width - 25 - ((Cross) ? 20 : 0), bounds.Y, 28, drawHeight);
            arrow_left = new Rectangle(arrows.X + 1, arrows.Y - 9 + (bounds.Height - 20) / 2, 12, 15);
            arrow_right = new Rectangle(arrows.X + 13, arrows.Y - 9 + (bounds.Height - 20) / 2, 12, 15);

            return arrows;
        }

        public void DrawScrollButtons(Graphics g, Rectangle bounds, Point MouseLocation, Point MouseClicking, bool Cross, bool Enabled)
        {
            Rectangle arrows = GenerateArrowBounds(bounds, Cross);

            arrows = new Rectangle(arrows.X, arrows.Y, arrows.Width, arrows.Height);
            g.FillRectangle(SystemBrushes.Control, new Rectangle(arrows.X, arrows.Y, arrows.Width, arrows.Height));

            if (arrow_left.Contains(MouseLocation) && Enabled)
            {
                g.FillRectangle(new SolidBrush(_render.ColorTable.ButtonPressedHighlight), arrow_left);
                g.DrawRectangle(new Pen(_render.ColorTable.ButtonPressedHighlightBorder, 1f), arrow_left);
            }

            if (arrow_right.Contains(MouseLocation) && Enabled)
            {
                g.FillRectangle(new SolidBrush(_render.ColorTable.ButtonPressedHighlight), arrow_right);
                g.DrawRectangle(new Pen(_render.ColorTable.ButtonPressedHighlightBorder, 1f), arrow_right);
            }
            arrows = new Rectangle(arrows.X, arrows.Y - 2 + (bounds.Height - 30) / 2, arrows.Width, 11);
            DrawScrollArrows(g, arrows, Enabled);
        }
        private void DrawScrollArrows(Graphics g, Rectangle bounds, bool Enabled)
        {
            int Height_2 = Convert.ToInt32(Math.Floor(bounds.Height / 2.0));
            int Size = Height_2 * 2;
            Brush Color = SystemBrushes.ControlDarkDark;
            if (!Enabled)
                Color = SystemBrushes.ControlLight;
            GraphicsPath p = new GraphicsPath();
            Point top, bottom, outside;

            // Right button
            top = new Point(bounds.X + 7 + Size, bounds.Y);
            bottom = new Point(bounds.X + 7 + Size, bounds.Y + bounds.Height + 1);
            outside = new Point(bounds.X + 8 + Size * 2 - Height_2, bounds.Y + 1 + Height_2);
            //MessageBox.Show(top.ToString() + "\n" + bottom.ToString() + "\n" + outside.ToString() + "\n" + bounds.ToString());
            p.AddLine(top, outside);
            p.AddLine(outside, bottom);
            p.AddLine(bottom, top);

            p.CloseFigure();

            g.FillPath(Color, p);

            p = new GraphicsPath();
            // Left button
            top = new Point(bounds.X + 9, bounds.Y);
            bottom = new Point(bounds.X + 9, bounds.Y + bounds.Height + 1);
            outside = new Point(bounds.X + 8 - Height_2, bounds.Y + 1 + Height_2);
            p.AddLine(top, outside);
            p.AddLine(outside, bottom);
            p.AddLine(bottom, top);
            g.FillPath(Color, p);


        }

        private ToolStripProfessionalRenderer _render = new ToolStripProfessionalRenderer();

        public void DrawCloseButton(Graphics g, Rectangle bounds, PushButtonState state)
        {
            if (state == PushButtonState.Disabled)
                return;


            Rectangle cross = new Rectangle(new Point(bounds.Width - 14 + bounds.X, bounds.Y + 4), new Size(9, 9));

            g.FillRectangle(SystemBrushes.Control, new Rectangle(bounds.Width - 20 + bounds.X, bounds.Y, 50, 50));


            // Draw normal cross.
            if (state == PushButtonState.Normal || state == PushButtonState.Default)
                DrawCloseCross(g, cross, state);
            else
            {
                Rectangle cross_out = new Rectangle(cross.X - 2, cross.Y - 2, cross.Width + 6, cross.Height + 6);
                g.FillRectangle(new SolidBrush(_render.ColorTable.ButtonPressedHighlight), cross_out);
                g.DrawRectangle(new Pen(_render.ColorTable.ButtonPressedHighlightBorder, 1f), cross_out);
                DrawCloseCross(g, cross, state);

            }

        }


        private void DrawCloseCross(Graphics g, Rectangle bounds, PushButtonState state)
        {
            Pen dark = new Pen(((state == PushButtonState.Hot) ? Color.Black : ProfessionalColors.GripDark), 2f);

            g.DrawLine(dark, new Point(bounds.X + 1, bounds.Y + 1), new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            g.DrawLine(dark, new Point(bounds.X + bounds.Width, bounds.Y + 1), new Point(bounds.X + 1, bounds.Y + bounds.Height));
        }
    }

    public class VisualTabRenderer_VS2005 : IVisualTabRenderer
    {
        public Padding Margins
        {
            get
            {
                return new Padding(20, 20, 15, 15);
            }
        }
        private static int Tab_Height = 24;

        public int CloseButtonWidth
        {
            get
            {
                return 30;
            }
        }

        public int ScrollButtonWidth
        {
            get
            {
                return 28;
            }
        }

        public int drawHeight
        {
            get
            {
                return Tab_Height;
            }
        }

        public bool HitsCloseButton(Point p, Rectangle bounds)
        {
            Rectangle cross = new Rectangle(new Point(bounds.Width - 15 + bounds.X, bounds.Y + 6), new Size(10, 10));

            if (cross.Contains(p))

                return true;

            return false;

        }

        private ToolStripProfessionalRenderer _render = new ToolStripProfessionalRenderer();

        public void DrawCloseButton(Graphics g, Rectangle bounds, PushButtonState state)
        {
            if (state == PushButtonState.Disabled)
                return;

            Rectangle cross = new Rectangle(new Point(bounds.Width - 15 + bounds.X, bounds.Y + 6), new Size(10, 10));

            g.FillRectangle(SystemBrushes.Control, new Rectangle(bounds.Width - 20 + bounds.X, bounds.Y, 50, 50));


            // Draw normal cross.
            if (state == PushButtonState.Normal || state == PushButtonState.Default)
                DrawCloseCross(g, cross, state);
            else
            {
                Rectangle cross_out = new Rectangle(cross.X - 2, cross.Y - 2, cross.Width + 6, cross.Height + 6);
                g.FillRectangle(new SolidBrush(_render.ColorTable.ButtonPressedHighlight), cross_out);
                g.DrawRectangle(new Pen(_render.ColorTable.ButtonPressedHighlightBorder, 1f), cross_out);
                DrawCloseCross(g, cross, state);

            }

        }

        private Rectangle arrow_left = new Rectangle(0, 0, 1, 1);
        private Rectangle arrow_right = new Rectangle(0, 0, 1, 1);

        public Rectangle ArrowLeft
        {
            get
            {
                return arrow_left;
            }
        }
        public Rectangle ArrowRight
        {
            get
            {
                return arrow_right;
            }
        }

        private Rectangle GenerateArrowBounds(Rectangle bounds, bool Cross)
        {
            Rectangle arrows = new Rectangle(bounds.X + bounds.Width - 25 - ((Cross) ? 20 : 0), bounds.Y, 28, drawHeight);
            arrow_left = new Rectangle(arrows.X + 1, arrows.Y - 9 + (bounds.Height - 20) / 2, 12, 15);
            arrow_right = new Rectangle(arrows.X + 13, arrows.Y - 9 + (bounds.Height - 20) / 2, 12, 15);

            return arrows;
        }

        public void DrawScrollButtons(Graphics g, Rectangle bounds, Point MouseLocation, Point MouseClicking, bool Cross, bool Enabled)
        {
            Rectangle arrows = GenerateArrowBounds(bounds, Cross);

            arrows = new Rectangle(arrows.X, arrows.Y, arrows.Width, arrows.Height);
            g.FillRectangle(SystemBrushes.Control, new Rectangle(arrows.X, arrows.Y, arrows.Width, arrows.Height));

            if (arrow_left.Contains(MouseLocation) && Enabled)
            {
                g.FillRectangle(new SolidBrush(_render.ColorTable.ButtonPressedHighlight), arrow_left);
                g.DrawRectangle(new Pen(_render.ColorTable.ButtonPressedHighlightBorder, 1f), arrow_left);
            }

            if (arrow_right.Contains(MouseLocation) && Enabled)
            {
                g.FillRectangle(new SolidBrush(_render.ColorTable.ButtonPressedHighlight), arrow_right);
                g.DrawRectangle(new Pen(_render.ColorTable.ButtonPressedHighlightBorder, 1f), arrow_right);
            }
            arrows = new Rectangle(arrows.X, arrows.Y - 2 + (bounds.Height - 30) / 2, arrows.Width, 11);
            DrawScrollArrows(g, arrows, Enabled);
        }
        private void DrawScrollArrows(Graphics g, Rectangle bounds, bool Enabled)
        {
            int Height_2 = Convert.ToInt32(Math.Floor(bounds.Height / 2.0));
            int Size = Height_2 * 2;
            Brush Color = SystemBrushes.ControlDarkDark;
            if (!Enabled)
                Color = SystemBrushes.ControlLight;
            GraphicsPath p = new GraphicsPath();
            Point top, bottom, outside;

            // Right button
            top = new Point(bounds.X + 7 + Size, bounds.Y);
            bottom = new Point(bounds.X + 7 + Size, bounds.Y + bounds.Height + 1);
            outside = new Point(bounds.X + 8 + Size * 2 - Height_2, bounds.Y + 1 + Height_2);
            //MessageBox.Show(top.ToString() + "\n" + bottom.ToString() + "\n" + outside.ToString() + "\n" + bounds.ToString());
            p.AddLine(top, outside);
            p.AddLine(outside, bottom);
            p.AddLine(bottom, top);

            p.CloseFigure();

            g.FillPath(Color, p);

            p = new GraphicsPath();
            // Left button
            top = new Point(bounds.X + 9, bounds.Y);
            bottom = new Point(bounds.X + 9, bounds.Y + bounds.Height + 1);
            outside = new Point(bounds.X + 8 - Height_2, bounds.Y + 1 + Height_2);
            p.AddLine(top, outside);
            p.AddLine(outside, bottom);
            p.AddLine(bottom, top);
            g.FillPath(Color, p);


        }

        private void DrawCloseCross(Graphics g, Rectangle bounds, PushButtonState state)
        {
            Pen dark = new Pen(((state == PushButtonState.Hot) ? Color.Black : ProfessionalColors.GripDark), 2f);

            g.DrawLine(dark, new Point(bounds.X + 1, bounds.Y + 1), new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            g.DrawLine(dark, new Point(bounds.X + bounds.Width, bounds.Y + 1), new Point(bounds.X + 1, bounds.Y + bounds.Height));
        }

        public Rectangle GetModifiedBounds(Rectangle bounds)
        {
            return new Rectangle(bounds.X, bounds.Y + 2, bounds.Width, Tab_Height - 3);
        }

        public Rectangle GetOffsetBounds(Rectangle bounds, int index)
        {
            return new Rectangle(bounds.X + bounds.Width, bounds.Y, bounds.Width, bounds.Height);
        }

        public Rectangle GetTextBounds(Rectangle bounds)
        {
            return new Rectangle(bounds.X + 2, bounds.Y + 4, bounds.Width, Tab_Height);
        }

        public void DrawTabItem(Graphics g, Rectangle bounds, TabItemState state)
        {
            SmoothingMode _Old = SmoothingMode.None;
            bool _ModifiedSmoothMode = false;
            if (g.SmoothingMode != SmoothingMode.AntiAlias && g.SmoothingMode != SmoothingMode.HighQuality)
            {
                _Old = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                _ModifiedSmoothMode = true;
            }
            bounds = GetModifiedBounds(bounds);

            if (state == TabItemState.Selected)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddLine(new Point(bounds.X, bounds.Height + bounds.Y), new Point(bounds.X, bounds.Y));
                path.AddLine(new Point(bounds.X, bounds.Y), new Point(bounds.Width + bounds.X, bounds.Y));
                path.AddLine(new Point(bounds.Width + bounds.X, bounds.Y), new Point(bounds.X + bounds.Width, bounds.Height + bounds.Y));

                g.FillPath(SystemBrushes.ControlLightLight, path);
                g.DrawPath(SystemPens.ControlDark, path);
            }
            else if (state == TabItemState.Normal)
            {
                g.DrawLine(SystemPens.ControlDark, new Point(bounds.X + bounds.Width, bounds.Y + 4), new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height - 3));
                g.DrawLine(SystemPens.ControlDark, new Point(bounds.X, bounds.Y + bounds.Height), new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height));

            }
            g.DrawLine(SystemPens.ControlLightLight, new Point(bounds.X, bounds.Y + bounds.Height + 1), new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height + 1));


            if (_ModifiedSmoothMode)
                g.SmoothingMode = _Old;
        }

        public void DrawTabPage(Graphics g, Rectangle bounds)
        {
            //g.FillRectangle(SystemBrushes.ControlLightLight, bounds);
            g.DrawRectangle(SystemPens.ControlDark, new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 4, bounds.Height - 3));

        }
        public VisualTabRenderer_Mode Mode
        {
            get
            {
                return VisualTabRenderer_Mode.SELECTED_FIRST;
            }
        }
    }
}
