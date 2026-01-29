using System;
using System.Windows.Forms;

namespace AutoCursorMover
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            Opacity = 0;

            Shown += AboutForm_Shown;
        }

        private void AboutForm_Shown(object sender, EventArgs e)
        {
            Timer showTimer = new Timer();
            showTimer.Interval = 50;
            showTimer.Tick += (s, ev) =>
            {
                if (Opacity < 1)
                {
                    Opacity += 0.05;
                }
                else
                {
                    showTimer.Stop();
                    Timer hideTimer = new Timer();
                    hideTimer.Interval = 50;
                    hideTimer.Tick += (ss, ee) =>
                    {
                        if (Opacity > 0)
                        {
                            Opacity -= 0.05;
                        }
                        else
                        {
                            hideTimer.Stop();
                            Close();
                        }
                    };
                    hideTimer.Start();
                }
            };
            showTimer.Start();
        }
    }
}
