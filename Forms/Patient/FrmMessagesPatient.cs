using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmMessagesPatient : XtraForm
    {
        private readonly MessageService _messageService;
        private readonly PatientRepository _patientRepo;
        private DiyetisyenOtomasyonu.Domain.Patient _currentPatient;
        private System.Windows.Forms.Timer _refreshTimer;

        // UI Controls
        private Panel pnlChatMessages;
        private TextEdit txtMessage;
        private SimpleButton btnSend;
        private Label lblDoctorName;

        // Colors
        private Color PrimaryColor { get { return UiStyles.PrimaryColor; } } // Teal
        private Color BackgroundColor { get { return Color.FromArgb(245, 247, 250); } }
        private Color BubbleMyColor { get { return PrimaryColor; } }
        private Color BubbleOtherColor { get { return Color.White; } }

        public FrmMessagesPatient()
        {
            _messageService = new MessageService();
            _patientRepo = new PatientRepository();
            
            InitializeComponent();
            SetupUI();
            LoadPatientData();
            LoadMessages();
            StartAutoRefresh();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1000, 600);
            this.Name = "FrmMessagesPatient";
            this.Text = "Mesajlarım";
            this.FormBorderStyle = FormBorderStyle.None;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.BackColor = BackgroundColor;
            this.Padding = new Padding(20);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            // Rounded corners for main panel
            mainPanel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, mainPanel.Width - 1, mainPanel.Height - 1);
                }
            };

            // 1. Header (Doktor Bilgisi)
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(240, 248, 255), // Light AliceBlue
                Padding = new Padding(20, 0, 20, 0)
            };

            lblDoctorName = new Label
            {
                Text = "Doktorunuz: Yükleniyor...",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                AutoSize = true,
                Location = new Point(20, 18)
            };
            headerPanel.Controls.Add(lblDoctorName);
            mainPanel.Controls.Add(headerPanel);

            // 3. Bottom (Mesaj Gönderme)
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };
            // Top border for bottom panel
            bottomPanel.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                    e.Graphics.DrawLine(pen, 0, 0, bottomPanel.Width, 0);
            };

            btnSend = new SimpleButton
            {
                Text = "Gönder ➤",
                Dock = DockStyle.Right,
                Width = 100,
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) }
            };
            btnSend.Click += (s, e) => SendMessage();

            txtMessage = new TextEdit
            {
                Dock = DockStyle.Fill,
                Properties = { NullValuePrompt = "Mesajınızı yazın...", AutoHeight = false }
            };
            txtMessage.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            txtMessage.Properties.Padding = new Padding(5);
            txtMessage.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SendMessage(); };

            // Spacer between text and button
            var spacer = new Panel { Dock = DockStyle.Right, Width = 10 };

            bottomPanel.Controls.Add(txtMessage);
            bottomPanel.Controls.Add(spacer);
            bottomPanel.Controls.Add(btnSend);
            mainPanel.Controls.Add(bottomPanel);

            // 2. Chat Area (Middle)
            pnlChatMessages = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250), // Hafif gri arka plan
                Padding = new Padding(20)
            };
            mainPanel.Controls.Add(pnlChatMessages);
            pnlChatMessages.BringToFront(); // Ensure it's not covered

            this.Controls.Add(mainPanel);
        }

        private void LoadPatientData()
        {
            _currentPatient = _patientRepo.GetById(AuthContext.UserId);
            if (_currentPatient != null)
            {
                // Doktor adını bulmak için user repo gerekebilir ama şimdilik basitçe
                // Repository'de GetDoctorName gibi bir metod yoksa, User tablosundan çekmek lazım
                // PatientRepository.GetFullPatientById zaten user bilgisini getiriyor ama doktor adını getirmiyor
                // Şimdilik "Dr. Diyetisyen" yazalım veya doktor ID'den çekelim
                lblDoctorName.Text = $"👨‍⚕️ Doktorunuzla Sohbet";
            }
        }

        private void LoadMessages()
        {
            if (_currentPatient == null) return;

            pnlChatMessages.Controls.Clear();
            try
            {
                var messages = _messageService.GetConversation(AuthContext.UserId, _currentPatient.DoctorId);
                
                int yPos = 20;
                foreach (var msg in messages.OrderBy(m => m.SentAt))
                {
                    var bubble = CreateMessageBubble(msg);
                    
                    // Konumlandırma
                    if (msg.FromUserId == AuthContext.UserId)
                    {
                        // Benim mesajım - Sağda
                        bubble.Location = new Point(pnlChatMessages.Width - bubble.Width - 40, yPos); // Scrollbar payı
                    }
                    else
                    {
                        // Doktor mesajı - Solda
                        bubble.Location = new Point(20, yPos);
                    }

                    pnlChatMessages.Controls.Add(bubble);
                    yPos += bubble.Height + 15; // Baloncuklar arası boşluk
                }

                // En alta scroll
                pnlChatMessages.VerticalScroll.Value = pnlChatMessages.VerticalScroll.Maximum;
            }
            catch (Exception ex)
            {
                // Sessiz hata veya log
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private Panel CreateMessageBubble(DiyetisyenOtomasyonu.Domain.Message msg)
        {
            bool isMine = msg.FromUserId == AuthContext.UserId;

            var bubble = new Panel
            {
                Width = 400, // Sabit genişlik yerine içeriğe göre değişebilir ama basitlik için max width
                AutoSize = true,
                MinimumSize = new Size(100, 50),
                BackColor = isMine ? BubbleMyColor : BubbleOtherColor,
                Padding = new Padding(15)
            };
            
            // Rounded corners logic would go here in Paint event if needed
            // For now, simple colored panels

            var lblContent = new Label
            {
                Text = msg.Content,
                Font = new Font("Segoe UI", 10F),
                ForeColor = isMine ? Color.White : Color.FromArgb(50, 50, 50),
                Location = new Point(15, 15),
                MaximumSize = new Size(370, 0), // Wrap text
                AutoSize = true
            };

            var lblTime = new Label
            {
                Text = msg.SentAt.ToString("HH:mm"),
                Font = new Font("Segoe UI", 8F),
                ForeColor = isMine ? Color.FromArgb(200, 255, 255) : Color.Gray,
                Location = new Point(15, lblContent.Bottom + 5),
                AutoSize = true
            };

            bubble.Controls.Add(lblContent);
            bubble.Controls.Add(lblTime);
            
            // AutoSize panel height calculation workaround
            bubble.Height = lblTime.Bottom + 15;

            return bubble;
        }

        private void SendMessage()
        {
            if (_currentPatient == null || string.IsNullOrWhiteSpace(txtMessage.Text)) return;

            try
            {
                _messageService.SendMessage(AuthContext.UserId, _currentPatient.DoctorId, txtMessage.Text);
                txtMessage.Text = "";
                LoadMessages();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Mesaj gönderilemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartAutoRefresh()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 5000;
            _refreshTimer.Tick += (s, e) => LoadMessages();
            _refreshTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
