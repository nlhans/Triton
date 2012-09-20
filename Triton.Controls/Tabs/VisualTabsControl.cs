using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.ComponentModel;
using System.Threading;

using System.Windows.Forms;

namespace Triton.Controls
{
    public enum VisualTab_DropdownStyle
    {
        Always,
        OnOverflow,
        Never
    }

    internal enum VisualTab_HitLocation
    {
        Tab,
        Close,
        Menu,
        Scroll_left,
        Scroll_right,
        None
    }

    internal class VisualTab_HitTest
    {
        public VisualTab_HitLocation Location;
        public int Tab;

        public VisualTab_HitTest()
        {
            Location = VisualTab_HitLocation.None;
            Tab = 0;
        }
    }

    public delegate void VisualTabPage_Delegate(VisualTabPage p);
    delegate void VisualTabPage_AddTab(VisualTabPage p, bool switchto);

    public sealed class VisualTabControl : Panel, ISupportInitialize, IDisposable
    {
        #region Fields

        public event VisualTabPage_Delegate TabStrip_Added;
        public event VisualTabPage_Delegate TabStrip_SelectedChanged;
        public event VisualTabPage_Delegate TabStrip_Removed;

        private VisualTabPage NoTabsAdded;
        private List<VisualTabPage> _Tabs = new List<VisualTabPage>();
        private bool _isInit = false;
        private int _SelectedTab_Index = 0;

        private bool _CustomFP;
        public VisualTabPage Frontpage
        {
            get
            {
                if (_CustomFP)
                    return NoTabsAdded;
                else
                    return null;
            }
            set
            {
                bool refresh = false;
                if (this._Tabs.Contains(NoTabsAdded))
                    refresh = true;
                _CustomFP = true;
                NoTabsAdded = value;

                if (refresh)
                    this._Tabs.RemoveAt(0);

                RefreshTab();
            }
        }


        private int _SelectedTab
        {
            get
            {
                return _SelectedTab_Index;
            }
            set
            {
                _PreviousTab = _SelectedTab_Index;
                _SelectedTab_Index = value;

                if (GetTab(value) != NoTabsAdded)
                {
                    ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OnTabChanged), GetTab(value));
                }
                RefreshTab();
            }
        }
        private int _PreviousTab = 0;

        private Point Mouse_Holding = new Point(0, 0);
        private Point Mouse_Moving = new Point(0, 0);
        #endregion

        #region Properties

        private Size _TabSize;
        private Size TabSize
        {
            get
            {
                return _TabSize;
            }
        }

        private Size _TabSizeMax = new Size(200, 30);
        private Size MaxTabSize
        {
            get
            {
                return _TabSizeMax;
            }
            set
            {
                _TabSizeMax = new Size(value.Width, Tab_Renderer.drawHeight);
            }
        }

        private Rectangle TabBarBounds
        {
            get
            {
                return new Rectangle(this.Tab_Renderer.Margins.Left, Tab_Renderer.Margins.Top, this.Width - this.Tab_Renderer.Margins.Horizontal, this.Padding.Top);
            }
        }

        private Rectangle TabBarBoundsWithButtons
        {
            get
            {
                int extra = this.Tab_Renderer.ScrollButtonWidth;
                if (Draw_Close)
                    extra += this.Tab_Renderer.CloseButtonWidth;
                return new Rectangle(this.Tab_Renderer.Margins.Left, Tab_Renderer.Margins.Top, this.Width - this.Tab_Renderer.Margins.Horizontal - extra, this.Padding.Top);
            }
        }

        public List<VisualTabPage> Tabs
        {
            get
            {
                return _Tabs;
            }
            set
            {
                _Tabs = value;
                RefreshTab();
            }
        }
        public bool Initializing
        {
            get
            {
                return _isInit;
            }
        }
        public int SelectedTabIndex
        {
            get
            {
                return this._SelectedTab;
            }
            set
            {
                this._SelectedTab = value;
                RefreshTab();
            }
        }

        public VisualTabPage SelectedTab
        {
            get
            {
                if (_Tabs.Count == 0)
                {
                    return null;
                }
                return GetTab(_SelectedTab);
            }
            set
            {
                _SelectedTab = GetIndex(value);
                RefreshTab();
            }
        }

        public bool Can_Close
        {
            get
            {
                return GetTab(_SelectedTab).IsCloseable;
            }
        }

        private bool _Draw_Close = true;
        public bool Draw_Close
        {
            get
            {
                return _Draw_Close;
            }
            set
            {
                _Draw_Close = value;
                RefreshTab();
            }
        }

        private VisualTab_DropdownStyle _Draw_Dropdown = VisualTab_DropdownStyle.OnOverflow;
        public VisualTab_DropdownStyle Draw_Dropdown
        {
            get
            {
                return _Draw_Dropdown;
            }
            set
            {
                _Draw_Dropdown = value;
                RefreshTab();
            }
        }

        private IVisualTabRenderer _tabRenderer = new VisualTabRenderer_VS2005();
        public IVisualTabRenderer Tab_Renderer
        {
            get
            {
                return _tabRenderer;
            }
            set
            {
                _tabRenderer = value;
                Padding pad = Tab_Renderer.Margins;
                pad.Top += Tab_Renderer.drawHeight;

                this.Padding = pad;
                RefreshTab();
            }
        }

        public bool WillDraw_Dropdown
        {
            get
            {
                if (Draw_Dropdown == VisualTab_DropdownStyle.Always)
                    return true;
                return false;
            }
        }

        private int _ScrollLocation = 0;
        public int ScrollLocation
        {
            get
            {
                if (Overflow)
                {
                    return _ScrollLocation;
                }
                return 0;
            }
            set
            {
                _ScrollLocation = value;
                RefreshTab();
            }
        }

        private int TabBarWorkspaceWidth
        {
            get
            {

                Rectangle LastTab = this.GetTabRectangle(this.Tabs.Count - 1, true);

                int extra = this.Tab_Renderer.ScrollButtonWidth;
                if (Draw_Close)
                    extra += this.Tab_Renderer.CloseButtonWidth;

                return (LastTab.X + LastTab.Width + extra);
            }
        }

        public bool Overflow
        {
            get
            {
                Rectangle TopBar = this.TabBarBounds;

                if (TabBarWorkspaceWidth > this.TabBarBounds.Width)
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region Constructor
        public VisualTabControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.Padding = Tab_Renderer.Margins;

            NoTabsAdded = new VisualTabPage();
            NoTabsAdded.Text = "(Empty)";

            Label l = new Label();
            l.Text = "No tabs are added to this control!";
            l.Dock = DockStyle.Fill;
            NoTabsAdded.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            NoTabsAdded.Controls.Add(l);

            this.SelectedTabIndex = 0;

        }

        private Rectangle GetTabRectangle(int index)
        {

            Rectangle r = new Rectangle(new Point((index - 1) * TabSize.Width - ScrollLocation, 0), TabSize);
            r = Tab_Renderer.GetOffsetBounds(r, index);
            r = new Rectangle(1 + Tab_Renderer.Margins.Left + r.X, 1 + Tab_Renderer.Margins.Top + r.Y, r.Width, r.Height);
            return r;
        }

        private Rectangle GetTabRectangle(int index, bool raw)
        {
            if (!raw) return GetTabRectangle(index);

            Rectangle r = new Rectangle(new Point((index - 1) * TabSize.Width, 0), TabSize);
            r = Tab_Renderer.GetOffsetBounds(r, index);
            r = new Rectangle(1 + Tab_Renderer.Margins.Left + r.X, 1 + Tab_Renderer.Margins.Top + r.Y, r.Width, r.Height);
            return r;
        }

        private VisualTab_HitTest HitTest(Point p)
        {
            VisualTab_HitTest test = new VisualTab_HitTest();

            for (int i = 0; i < this.Tabs.Count; i++)
            {
                Rectangle tab_re = this.GetTabRectangle(i);

                if (tab_re.Contains(p))
                {
                    test.Tab = i;
                    test.Location = VisualTab_HitLocation.Tab;
                    break;
                }
            }
            if (Tab_Renderer.ArrowLeft.Contains(p))
                test.Location = VisualTab_HitLocation.Scroll_left;
            if (Tab_Renderer.ArrowRight.Contains(p))
                test.Location = VisualTab_HitLocation.Scroll_right;
            if (Draw_Close)
            {
                if (Tab_Renderer.HitsCloseButton(p, TabBarBounds))
                    test.Location = VisualTab_HitLocation.Close;
            }
            if (WillDraw_Dropdown)
            {

            }

            return test;
        }
        #endregion

        #region Events
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Mouse_Moving = e.Location;
            this.Repaint();
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Mouse_Holding = e.Location;
            this.Repaint();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            Mouse_Moving = new Point(-1, -1);
            Mouse_Holding = new Point(-1, -1);
            Repaint();
        }



        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            Mouse_Holding = new Point(0, 0);
            //MessageBox.Show("Clicked at : " + location.ToString());
            VisualTab_HitTest HitAt = HitTest(e.Location);

            switch (HitAt.Location)
            {
                case VisualTab_HitLocation.Close:
                    if (Can_Close)
                        CloseTab(SelectedTab);
                    break;
                case VisualTab_HitLocation.Tab:

                    _SelectedTab = HitAt.Tab;
                    RefreshTab();
                    break;

                case VisualTab_HitLocation.Scroll_left:
                    int Loc = ScrollLocation;

                    Loc -= TabSize.Width + 20;
                    if (Loc < 0)
                        Loc = 0;
                    ScrollLocation = Loc;
                    break;

                case VisualTab_HitLocation.Scroll_right:
                    if (ScrollLocation + TabBarBounds.Width + TabSize.Width + 20 > TabBarWorkspaceWidth)
                        ScrollLocation = TabBarWorkspaceWidth - TabBarBounds.Width;
                    else
                        ScrollLocation += TabSize.Width + 20;
                    break;
            }
            this.Repaint();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            RefreshTabSize(g);

            if (_Tabs.Count > 0)
                _Draw_Tabs(g);
            // Draw close button.
            if (Draw_Close)
            {
                System.Windows.Forms.VisualStyles.PushButtonState state = System.Windows.Forms.VisualStyles.PushButtonState.Normal;
                if (Tab_Renderer.HitsCloseButton(Mouse_Moving, TabBarBounds))
                    state = System.Windows.Forms.VisualStyles.PushButtonState.Hot;
                if (Tab_Renderer.HitsCloseButton(Mouse_Holding, TabBarBounds))
                    state = System.Windows.Forms.VisualStyles.PushButtonState.Pressed;
                if (!Can_Close)
                    state = System.Windows.Forms.VisualStyles.PushButtonState.Disabled;
                Tab_Renderer.DrawCloseButton(g, TabBarBounds, state);
            }

            Tab_Renderer.DrawScrollButtons(g, TabBarBounds, Mouse_Moving, Mouse_Holding, (Draw_Close), Overflow);

            // Top
            g.FillRectangle(SystemBrushes.Control, new Rectangle(0, 0, this.Size.Width, Padding.Top - TabBarBounds.Height));

            // Left
            g.FillRectangle(SystemBrushes.Control, new Rectangle(0, 0, Padding.Left, this.Size.Height));

            // Right
            g.FillRectangle(SystemBrushes.Control, new Rectangle(this.Size.Width - Padding.Right, 0, this.Size.Width, this.Size.Height));

            // Bottom
            g.FillRectangle(SystemBrushes.Control, new Rectangle(0, this.Size.Height - Padding.Bottom, this.Size.Width, this.Size.Height));
        }

        private void Repaint()
        {
            this.Invalidate();
        }

        private void _Draw_Tabs(Graphics g)
        {
            int i = 0;
            Rectangle bounds_intern = GetTabRectangle(0);

            switch (Tab_Renderer.Mode)
            {
                case VisualTabRenderer_Mode.NO_PREFERENCE:

                    foreach (VisualTabPage p in this._Tabs)
                    {
                        bounds_intern = GetTabRectangle(i);

                        if (bounds_intern.X > TabBarBoundsWithButtons.Width + TabBarBoundsWithButtons.X)
                            break;
                        System.Windows.Forms.VisualStyles.TabItemState state = System.Windows.Forms.VisualStyles.TabItemState.Normal;

                        if (IsSelected(p))
                            state = System.Windows.Forms.VisualStyles.TabItemState.Selected;
                        else if (bounds_intern.Contains(Mouse_Moving))
                            state = System.Windows.Forms.VisualStyles.TabItemState.Hot;
                        Tab_Renderer.DrawTabItem(g, bounds_intern, state);
                        g.DrawString(p.Text, this.Font, SystemBrushes.ControlText, Tab_Renderer.GetTextBounds(bounds_intern));


                        i++;
                    }
                    break;

                case VisualTabRenderer_Mode.SELECTED_LAST:
                    foreach (VisualTabPage p in this._Tabs)
                    {
                        if (IsSelected(p))
                        {

                            i++;
                            continue;
                        }
                        bounds_intern = GetTabRectangle(i);

                        if (bounds_intern.X > TabBarBounds.Width + TabBarBounds.X)
                            break;
                        System.Windows.Forms.VisualStyles.TabItemState state = System.Windows.Forms.VisualStyles.TabItemState.Normal;

                        if (bounds_intern.Contains(Mouse_Moving))
                            state = System.Windows.Forms.VisualStyles.TabItemState.Hot;


                        Tab_Renderer.DrawTabItem(g, bounds_intern, state);
                        g.DrawString(p.Text, this.Font, SystemBrushes.ControlText, Tab_Renderer.GetTextBounds(bounds_intern));

                        i++;

                    }
                    i = 0;

                    foreach (VisualTabPage p in this._Tabs)
                    {
                        if (!IsSelected(p))
                        {
                            i++;

                            continue;
                        }
                        bounds_intern = GetTabRectangle(i);
                        if (bounds_intern.X > TabBarBounds.Width + TabBarBounds.X)
                            break;
                        System.Windows.Forms.VisualStyles.TabItemState state = System.Windows.Forms.VisualStyles.TabItemState.Selected;


                        Tab_Renderer.DrawTabItem(g, bounds_intern, state);
                        g.DrawString(p.Text, this.Font, SystemBrushes.ControlText, Tab_Renderer.GetTextBounds(bounds_intern));

                        break;

                    }
                    //MessageBox.Show("Selected tab: " + i + " , " + this.SelectedTabIndex);
                    break;

                case VisualTabRenderer_Mode.SELECTED_FIRST:

                    foreach (VisualTabPage p in this._Tabs)
                    {
                        bounds_intern = GetTabRectangle(i);
                        if (!IsSelected(p))
                        {

                            bounds_intern = Tab_Renderer.GetOffsetBounds(bounds_intern, i - 1);
                            i++;
                            continue;
                        }

                        System.Windows.Forms.VisualStyles.TabItemState state = System.Windows.Forms.VisualStyles.TabItemState.Selected;


                        Tab_Renderer.DrawTabItem(g, bounds_intern, state);

                        g.DrawString(p.Text, this.Font, SystemBrushes.ControlText, Tab_Renderer.GetTextBounds(bounds_intern));

                        i++;
                    }
                    i = 0;
                    bounds_intern = GetTabRectangle(0);
                    foreach (VisualTabPage p in this._Tabs)
                    {
                        if (IsSelected(p))
                        {
                            i++;
                            continue;
                        }
                        bounds_intern = GetTabRectangle(i);

                        System.Windows.Forms.VisualStyles.TabItemState state = System.Windows.Forms.VisualStyles.TabItemState.Normal;

                        if (bounds_intern.Contains(Mouse_Moving))
                            state = System.Windows.Forms.VisualStyles.TabItemState.Hot;

                        Tab_Renderer.DrawTabItem(g, bounds_intern, state);
                        g.DrawString(p.Text, this.Font, SystemBrushes.ControlText, Tab_Renderer.GetTextBounds(bounds_intern));

                        i++;

                    }
                    break;

            }
        }
        #endregion

        #region Methods
        public void CloseTab(VisualTabPage page)
        {
            if (this._Tabs.Contains(page))
            {
                page.Hide();
                this.Controls.Remove(page);
                this._Tabs.Remove(page);

                _SelectedTab = 0;

                RefreshTab();
                if (page != NoTabsAdded)
                {
                    ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OnTabRemoved), page);
                }
            }

        }
        private bool _Refreshing = false;
        private void RefreshTab()
        {

            if (_Tabs.Count > 0)
                for (int i = 0; i < Controls.Count; i++)
                {
                    VisualTabPage p = (VisualTabPage)Controls[i];
                    if (IsSelected(p))
                    {
                        this.Controls[i].Dock = DockStyle.Fill;
                        this.Controls[i].Show();
                    }
                    else
                    {
                        this.Controls[i].Dock = DockStyle.None;
                        this.Controls[i].Hide();
                    }
                }
            else
            {
                this.AddTab(NoTabsAdded, true);
            }
            //MessageBox.Show("Invalidating");
            this.Repaint();
        }

        public void AddTab(VisualTabPage p, bool switchto)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VisualTabPage_AddTab(AddTab), new object[2] { p, switchto });
                return;
            }
            if (p.Master != null && p.Master != this)
                throw new Exception("Please give a tab only 1 master control!");
            p.Master = this;
            p.Hide();
            this.Tabs.Add(p);
            this.Controls.Add(p);

            if (this.Tabs.Contains(NoTabsAdded) && NoTabsAdded != p)
            {
                this.Tabs.Remove(NoTabsAdded);
                this.Controls.Remove(NoTabsAdded);
                switchto = true;
            }
            if (switchto)
                _SelectedTab = Tabs.Count - 1;
            Repaint();

            if (p != NoTabsAdded)
            {
                ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OnTabAdded), p);
            }

        }


        private void OnTabAdded(object state)
        {
            VisualTabPage p = (VisualTabPage)state;
            if (TabStrip_Added != null)
                TabStrip_Added(p);
        }
        private void OnTabRemoved(object state)
        {
            VisualTabPage p = (VisualTabPage)state;
            if (TabStrip_Removed != null)
                TabStrip_Removed(p);
        }
        private void OnTabChanged(object state)
        {
            VisualTabPage p = (VisualTabPage)state;
            if (TabStrip_SelectedChanged != null)
                TabStrip_SelectedChanged(p);
        }

        public void RefreshTabSize(Graphics g)
        {
            int wi = 0;
            int max_wi = _TabSizeMax.Width;
            foreach (VisualTabPage p in Tabs)
            {
                string text = p.Text;
                /*StringFormat format = new StringFormat();
                format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, text.Length) });
                Region[] size = g.MeasureCharacterRanges(text, Font, new Rectangle(0, 0, 1000, 50), format);
                RectangleF r = size[0].GetBounds(g);
                wi = Math.Max(wi, r.Width);*/
                SizeF sf = g.MeasureString(text, Font);

                wi = Math.Max(wi, (int)Math.Ceiling(sf.Width));
                if (wi > max_wi)
                    break;

            }
            wi += 3;
            if (wi > max_wi)
                _TabSize = _TabSizeMax;
            else
                _TabSize = new Size(wi, Tab_Renderer.drawHeight);
        }

        public void AddTab(VisualTabPage p)
        {
            AddTab(p, false);
        }

        public bool IsSelected(VisualTabPage p)
        {
            return ((GetIndex(p) == this._SelectedTab) ? true : false);
        }

        private int GetIndex(VisualTabPage p)
        {
            int index = 0;
            foreach (VisualTabPage p_ in Tabs)
            {
                if (p_ == p)
                    return index;
                else
                    index++;
            }
            return -1;

        }
        private VisualTabPage GetTab(int index)
        {
            if (index == -1)
                return null;
            if (index < this.Tabs.Count)
                return this.Tabs[index];
            else
                return null;
        }

        public void BeginInit()
        {
            _isInit = true;
        }

        public void EndInit()
        {
            _isInit = false;

        }
        #endregion
    }
}