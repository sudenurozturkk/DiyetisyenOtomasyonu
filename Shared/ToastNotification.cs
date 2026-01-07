using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DiyetisyenOtomasyonu.Shared
{
    /// <summary>
    /// Modern Toast Bildirim Sistemi
    /// DevExpress tarzına uyumlu profesyonel tasarım
    /// </summary>
    public class ToastNotification : Form
    {
        private Timer animationTimer;
        private Timer closeTimer;
        private int targetY;
        private double opacity = 0;
        private bool isClosing = false;

        public enum ToastType
        {
            Success,
            Warning,
            Error,
            Info
        }

        private ToastNotification()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(350, 80);
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.Opacity = 0;
        }

        public static void Show(string message, ToastType type = ToastType.Info, int duration = 3000)
        {
            var toast = new ToastNotification();
            toast.SetupUI(message, type);
            toast.AnimateIn(duration);
        }

        public static void ShowSuccess(string message)
        {
            Show(message, ToastType.Success);
        }

        public static void ShowWarning(string message)
        {
            Show(message, ToastType.Warning);
        }

        public static void ShowError(string message)
        {
            Show(message, ToastType.Error);
        }

        public static void ShowInfo(string message)
        {
            Show(message, ToastType.Info);
        }

        private void SetupUI(string message, ToastType type)
        {
            Color accentColor;
            string icon;

            switch (type)
            {
                case ToastType.Success:
                    accentColor = Color.FromArgb(25, 135, 84);
                    icon = "✓";
                    break;
                case ToastType.Warning:
                    accentColor = Color.FromArgb(255, 193, 7);
                    icon = "⚠";
                    break;
                case ToastType.Error:
                    accentColor = Color.FromArgb(220, 53, 69);
                    icon = "✕";
                    break;
                default:
                    accentColor = Color.FromArgb(13, 110, 253);
                    icon = "ℹ";
                    break;
            }

            // Sol renk şeridi
            var colorStrip = new Panel
            {
                BackColor = accentColor,
                Size = new Size(6, this.Height),
                Location = new Point(0, 0),
                Dock = DockStyle.Left
            };
            this.Controls.Add(colorStrip);

            // İkon
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(20, 20),
                Size = new Size(40, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblIcon);

            // Mesaj
            var lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(65, 15),
                Size = new Size(250, 50),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblMessage);

            // Kapatma butonu
            var btnClose = new Label
            {
                Text = "×",
                Font = new Font("Segoe UI", 14F),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(this.Width - 30, 5),
                Size = new Size(25, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => AnimateOut();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(33, 37, 41);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.FromArgb(108, 117, 125);
            this.Controls.Add(btnClose);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Gölge efekti
            using (var path = new GraphicsPath())
            {
                var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                int radius = 8;
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                this.Region = new Region(path);
            }

            // Border
            using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private void AnimateIn(int displayDuration)
        {
            // Sağ alt köşeye konumla
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width - 20, workingArea.Bottom);
            targetY = workingArea.Bottom - this.Height - 20;

            this.Show();

            animationTimer = new Timer { Interval = 15 };
            animationTimer.Tick += (s, e) =>
            {
                if (this.Top > targetY)
                {
                    this.Top -= 8;
                    opacity += 0.1;
                    if (opacity > 1) opacity = 1;
                    this.Opacity = opacity;
                }
                else
                {
                    this.Top = targetY;
                    this.Opacity = 1;
                    animationTimer.Stop();
                    
                    // Görüntüleme süresinden sonra kapat
                    closeTimer = new Timer { Interval = displayDuration };
                    closeTimer.Tick += (s2, e2) =>
                    {
                        closeTimer.Stop();
                        AnimateOut();
                    };
                    closeTimer.Start();
                }
            };
            animationTimer.Start();
        }

        private void AnimateOut()
        {
            if (isClosing) return;
            isClosing = true;

            animationTimer = new Timer { Interval = 15 };
            animationTimer.Tick += (s, e) =>
            {
                opacity -= 0.1;
                if (opacity <= 0)
                {
                    animationTimer.Stop();
                    this.Close();
                    this.Dispose();
                }
                else
                {
                    this.Opacity = opacity;
                    this.Top += 5;
                }
            };
            animationTimer.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW - Taskbar'da gösterme
                return cp;
            }
        }
    }
}
