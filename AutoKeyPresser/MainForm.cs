using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoKeyPresser
{
    public partial class MainForm : Form
    {
        #region tray
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private ToolStripMenuItem trayMenuStart;
        private ToolStripMenuItem trayMenuStop;
        #endregion

        private Timer PressTimer = new Timer();
        private uint cycleTime = 1;
        private uint remainTime = 0;

        private bool isWaitingForKey = false;
        private Keys inputKey = Keys.Pause;

        public MainForm()
        {
            InitializeComponent();

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, OnOpen);
            trayMenu.Items.Add("START", null, button1_Click);
            trayMenu.Items.Add("STOP", null, button2_Click);
            trayMenu.Items.Add("Exit", null, OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "AutoKeyPresser";
            trayIcon.Icon = Icon;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += OnOpen;

            trayMenuStart = trayMenu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(item => item.Text == "START");
            trayMenuStop = trayMenu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(item => item.Text == "STOP");
            trayMenuStop.Checked = true;

            cycleTime = uint.Parse(textBox1.Text);

            PressTimer.Interval = 1000;
            PressTimer.Tick += (s, ev) =>
            {
                remainTime--;
                label3.Text = remainTime.ToString();

                if (remainTime < 1)
                {
                    PressKey();
                    remainTime = cycleTime;
                    label3.Text = remainTime.ToString();
                }
            };

            button3.Text = inputKey.ToString();

            button3.Click += button3_Click;
            KeyDown += Form1_KeyDown;
            textBox1.TextChanged += TextBox1_TextChanged;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            isWaitingForKey = true;
            button3.Text = "Press Key...";
            button3.BackColor = System.Drawing.Color.SkyBlue;
            button3.Focus();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isWaitingForKey)
            {
                isWaitingForKey = false;
                inputKey = e.KeyCode;
                button3.Text = $"{inputKey}";
                button3.BackColor = System.Drawing.Color.White;
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text.Trim();

            uint input = 0;
            if (!uint.TryParse(textBox1.Text, out input) || input > uint.MaxValue)
            {
                textBox1.Text = "";
            }
            else
            {
                cycleTime = input;
            }
        }

        private void OnOpen(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
            trayIcon.Visible = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (WindowState == FormWindowState.Minimized)
            {
                HideToTray();
            }
        }

        private void HideToTray()
        {
            trayIcon.Visible = true;
            Hide();
        }

        private void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

        private void PressKey()
        {
            string keyString;

            if ((inputKey >= Keys.D0 && inputKey <= Keys.D9))
            {
                keyString = inputKey.ToString().Replace("D", "");
            }
            else if (inputKey >= Keys.NumPad0 && inputKey <= Keys.NumPad1)
            {
                keyString = inputKey.ToString().Replace("NumPad", "");
            }
            else
            {
                switch (inputKey)
                {
                    case Keys.Pause: keyString = "{BREAK}"; break;
                    case Keys.Enter: keyString = "{ENTER}"; break;
                    case Keys.Back: keyString = "{BACKSPACE}"; break;
                    case Keys.Escape: keyString = "{ESC}"; break;
                    case Keys.Space: keyString = " "; break;
                    default:
                        keyString = inputKey.ToString();
                        if (keyString.Length > 1)
                            keyString = "{" + keyString.ToUpper() + "}";
                        break;
                }
            }

            SendKeys.SendWait(keyString);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                return;
            }

            label2.ForeColor = System.Drawing.Color.MediumSeaGreen;
            label2.Text = "START";
            textBox1.Enabled = false;
            trayMenuStart.Checked = true;
            trayMenuStop.Checked = false;

            StartTimer();
        }

        async void StartTimer()
        {
            await Task.Delay(1000);
            PressKey();

            remainTime = cycleTime;
            label3.Text = remainTime.ToString();

            PressTimer.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label2.ForeColor = System.Drawing.Color.Crimson;
            label2.Text = "STOP";
            label3.Text = "1";
            textBox1.Enabled = true;
            trayMenuStart.Checked = false;
            trayMenuStop.Checked = true;

            PressTimer.Stop();
        }
    }
}
