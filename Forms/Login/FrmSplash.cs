using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DiyetisyenOtomasyonu.Forms.Login
{
    /// <summary>
    /// Modern profesyonel Splash ekranı
    /// Gradient arka plan, logo animasyonu ve progress bar
    /// </summary>
    public class FrmSplash : Form
    {
        private Timer progressTimer;
        private Timer fadeTimer;
        private int progressValue = 0;
        private Label lblProgress;
        private Panel progressBar;
        private Panel progressFill;

        // Gradient renkleri
        private readonly Color GradientStart = Color.FromArgb(79, 70, 229);
        private readonly Color GradientEnd = Color.FromArgb(124, 58, 237);

        public FrmSplash()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Form ayarları
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(500, 350);
            this.DoubleBuffered = true;
            this.BackColor = GradientStart;

            // Logo ve Başlık
            var lblLogo = new Label
            {
                Text = "🥗",
                Font = new Font("Segoe UI Emoji", 48F),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(0, 50),
                Size = new Size(500, 70),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblLogo);

            var lblAppName = new Label
            {
                Text = "DiyetPro",
                Font = new Font("Segoe UI", 36F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(0, 120),
                Size = new Size(500, 55),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblAppName);

            var lblTagline = new Label
            {
                Text = "Profesyonel Diyetisyen Hasta Takip Sistemi",
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(220, 255, 255, 255),
                BackColor = Color.Transparent,
                Location = new Point(0, 175),
                Size = new Size(500, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTagline);

            // Progress Bar Container
            progressBar = new Panel
            {
                Location = new Point(75, 240),
                Size = new Size(350, 8),
                BackColor = Color.FromArgb(100, 255, 255, 255)
            };
            MakeRounded(progressBar, 4);
            this.Controls.Add(progressBar);

            // Progress Bar Fill
            progressFill = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(0, 8),
                BackColor = Color.White
            };
            progressBar.Controls.Add(progressFill);

            // Progress Text
            lblProgress = new Label
            {
                Text = "Yükleniyor...",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                BackColor = Color.Transparent,
                Location = new Point(0, 260),
                Size = new Size(500, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblProgress);

            // Version
            var lblVersion = new Label
            {
                Text = "v1.0.0",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(150, 255, 255, 255),
                BackColor = Color.Transparent,
                Location = new Point(0, 320),
                Size = new Size(500, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblVersion);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Gradient arka plan
            using (var brush = new LinearGradientBrush(
                this.ClientRectangle,
                GradientStart,
                GradientEnd,
                45F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            // Yumuşak kenarlar
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StartLoading();
        }

        private void StartLoading()
        {
            string[] loadingSteps = {
                "Veritabanı bağlantısı kuruluyor...",
                "Kullanıcı ayarları yükleniyor...",
                "Modüller hazırlanıyor...",
                "Tema uygulanıyor...",
                "Son kontroller yapılıyor...",
                "Hazır!"
            };

            int stepIndex = 0;

            progressTimer = new Timer { Interval = 50 };
            progressTimer.Tick += (s, ev) =>
            {
                progressValue += 2;
                
                // Progress bar'ı güncelle
                int width = (int)((progressValue / 100.0) * progressBar.Width);
                progressFill.Size = new Size(width, progressFill.Height);

                // Loading metin güncelle
                int newStep = (int)(progressValue / (100.0 / loadingSteps.Length));
                if (newStep < loadingSteps.Length && newStep != stepIndex)
                {
                    stepIndex = newStep;
                    lblProgress.Text = loadingSteps[stepIndex];
                }

                if (progressValue >= 100)
                {
                    progressTimer.Stop();
                    
                    // Fade out ve kapat
                    fadeTimer = new Timer { Interval = 30 };
                    fadeTimer.Tick += (s2, ev2) =>
                    {
                        this.Opacity -= 0.05;
                        if (this.Opacity <= 0)
                        {
                            fadeTimer.Stop();
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    };
                    fadeTimer.Start();
                }
            };
            progressTimer.Start();
        }

        private void MakeRounded(Control control, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(control.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(control.Width - radius * 2, control.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, control.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                control.Region = new Region(path);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED - Flicker önleme
                return cp;
            }
        }
    }
}
