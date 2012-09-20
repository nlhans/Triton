using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Triton.Controls
{
    public class VisualListDetails : ListView
    {
        public VisualListDetails(bool doDock)
        {
            if (doDock)
                Dock = DockStyle.Fill;

            FullRowSelect = true;
            MultiSelect = true;
            View = System.Windows.Forms.View.Details;
        }
    }
}
