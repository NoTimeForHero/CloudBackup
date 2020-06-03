using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinClient
{
    public partial class PanelCompleted : UserControl
    {
        public PanelCompleted(bool needShutdown)
        {
            InitializeComponent();
            lblShudownHelper.Visible = needShutdown;
            lblTimer.Visible = needShutdown;
            if (!needShutdown)
            {
                lblStatus.Height = btnClose.Location.Y - lblStatus.Location.Y;
            }
        }
    }
}
