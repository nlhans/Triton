using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.ComponentModel;

using System.Windows.Forms;


namespace Triton.Controls
{
    public class VisualTabPage : Panel
    {
        private string _Text = "TabPage";
        public virtual string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                this._Text = value;
            }
        }

        private VisualTabControl _Master;
        internal VisualTabControl Master
        {
            get
            {
                return this._Master;
            }
            set
            {
                _Master = value;
            }
        }

        internal IVisualTabRenderer Tab_Renderer
        {
            get
            {
                if (Master == null)
                    return new VisualTabRenderer_Windows();

                return Master.Tab_Renderer;
            }
        }

        private bool _IsCloseable = true;
        public virtual bool IsCloseable
        {
            get
            {
                return _IsCloseable;
            }
            set
            {
                _IsCloseable = value;
            }
        }

        public VisualTabPage()
            : base()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.BackColor = SystemColors.ControlLightLight;

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Tab_Renderer.DrawTabPage(e.Graphics, new Rectangle(new Point(0, -1), new Size(this.Size.Width + 2, this.Size.Height + 2)));

        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control.GetType() == (new Button()).GetType())
            {
                ((Button)e.Control).UseVisualStyleBackColor = true;
            }
        }
    }

}
