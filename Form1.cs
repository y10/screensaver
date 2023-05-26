using System.Runtime.InteropServices;

namespace Screensaver
{
    public partial class Form1 : Form
    {
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int MOUSEEVENTF_VIRTUALDESK = 0x4000;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private WindowsHook windowsHook;

        public Form1()
        {
            InitializeComponent();

            // add the exit menu item
            contextMenuStrip1.Items.Add("Exit").Click += Exit;

            // set the tray icon properties
            notifyIcon1.Text = "Screensaver";
            notifyIcon1.Visible = true;

            // set up the keyboard hook
            windowsHook = new WindowsHook();
            windowsHook.KeyDown += WindowsHook_KeyDown;
            windowsHook.MouseClick += WindowsHook_MouseClick;
            EnableRandomMoves();
        }

        private void WindowsHook_KeyDown(object sender, KeyEventArgs e)
        {
            DisableRandomMoves();
        }

        private void WindowsHook_MouseClick(object sender, EventArgs e)
        {
            DisableRandomMoves();
        }

        public void EnableRandomMoves()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(EnableRandomMoves));
            }
            else
            {
                timer1.Enabled = true;
                timer1.Start();
                Show();
            }
        }

        public void DisableRandomMoves()
        {
            timer1.Stop();
            timer1.Enabled = false;
            Hide();
        }

        private void Exit(object sender, EventArgs e)
        {
            DisableRandomMoves();
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Random random = new Random();

            // generate a random movement within the screen boundaries
            int x = random.Next(Screen.PrimaryScreen.Bounds.Width);
            int y = random.Next(Screen.PrimaryScreen.Bounds.Height);

            // move the mouse cursor
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK, x * 65535 / Screen.PrimaryScreen.Bounds.Width, y * 65535 / Screen.PrimaryScreen.Bounds.Height, 0, 0);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                DisableRandomMoves();
            }
            else
            { 
                EnableRandomMoves();
            }
        }
    }
}
