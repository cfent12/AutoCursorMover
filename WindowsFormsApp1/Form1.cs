using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;
using System.Drawing;

namespace AutoCursorMover
{
    public partial class Form1 : Form
    {
        #region mouse input
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const uint INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        #endregion

        #region tray
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private ToolStripMenuItem trayMenuStart;
        private ToolStripMenuItem trayMenuStop;
        #endregion

        private int screenWidth = 0;
        private int screenHeight = 0;

        private Timer moveTimer = new Timer();
        private Random rand = new Random();
        private uint cycleTime = 0;
        private uint remainTime = 0;

        public Form1()
        {
            InitializeComponent();

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, OnOpen);
            trayMenu.Items.Add("START", null, button1_Click);
            trayMenu.Items.Add("STOP", null, button2_Click);
            trayMenu.Items.Add("Exit", null, OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "AutoCursorMover";
            trayIcon.Icon = Icon;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += OnOpen;

            trayMenuStart = trayMenu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(item => item.Text == "START");
            trayMenuStop = trayMenu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(item => item.Text == "STOP");
            trayMenuStop.Checked = true;

            screenWidth = Screen.PrimaryScreen.Bounds.Width;
            screenHeight = Screen.PrimaryScreen.Bounds.Height;

            cycleTime = uint.Parse(textBox1.Text);

            moveTimer.Interval = 1000;
            moveTimer.Tick += (s, ev) =>
            {
                remainTime--;
                label3.Text = remainTime.ToString();

                if (remainTime < 1)
                {
                    MoveCursor();
                    remainTime = cycleTime;
                    label3.Text = remainTime.ToString();
                }
            };

            KeyDown += Form1_KeyDown;
            textBox1.TextChanged += TextBox1_TextChanged;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                AboutForm aboutForm = new AboutForm();
                aboutForm.Location = new Point(Location.X + Size.Width, Location.Y + (Size.Height/2 - aboutForm.Height/2));
                aboutForm.Show();
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

        private void MoveCursor()
        {
            int x = rand.Next(0, screenWidth);
            int y = rand.Next(0, screenHeight);

            int dx = (x * 65535 / screenWidth);
            int dy = (y * 65535 / screenHeight);

            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mi.dx = dx;
            inputs[0].mi.dy = dy;
            inputs[0].mi.mouseData = 0;
            inputs[0].mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;
            inputs[0].mi.time = 0;
            inputs[0].mi.dwExtraInfo = IntPtr.Zero;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "")
            {
                return;
            }

            label2.ForeColor = System.Drawing.Color.MediumSeaGreen;
            label2.Text = "START";
            textBox1.Enabled = false;
            trayMenuStart.Checked = true;
            trayMenuStop.Checked = false;

            StartMove();
        }

        async void StartMove()
        {
            await Task.Delay(1000);
            MoveCursor();

            remainTime = cycleTime;
            label3.Text = remainTime.ToString();

            moveTimer.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label2.ForeColor = System.Drawing.Color.Crimson;
            label2.Text = "STOP";
            label3.Text = "1";
            textBox1.Enabled = true;
            trayMenuStart.Checked = false;
            trayMenuStop.Checked = true;

            moveTimer.Stop();
        }
    }
}
