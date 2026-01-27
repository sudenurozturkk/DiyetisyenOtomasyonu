using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using PatientEntity = DiyetisyenOtomasyonu.Domain.Patient;
using MessageEntity = DiyetisyenOtomasyonu.Domain.Message;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    public partial class FrmMessagesModern : XtraForm
    {
        private readonly PatientRepository _patientRepository;
        private readonly MessageService _messageService;
        private List<PatientEntity> _patients;
        private PatientEntity _selectedPatient;
        private System.Windows.Forms.Timer _refreshTimer;

        // UI Controls
        private Panel pnlPatientList;
        private Panel pnlChat;
        private Panel pnlChatMessages;
        private TextEdit txtMessage;
        private SimpleButton btnSend;
        private Label lblSelectedPatient;

        public FrmMessagesModern()
        {
            _patientRepository = new PatientRepository();
            _messageService = new MessageService();
            InitializeComponent();
            SetupUI();
            LoadPatients();
            StartAutoRefresh();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 700);
            this.Name = "FrmMessagesModern";
            this.Text = "Mesajlaşma";
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(20);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Sol Panel - Hasta Listesi
            pnlPatientList = CreatePatientListPanel();
            mainPanel.Controls.Add(pnlPatientList, 0, 0);

            // Sağ Panel - Mesajlaşma
            pnlChat = CreateChatPanel();
            mainPanel.Controls.Add(pnlChat, 1, 0);

            this.Controls.Add(mainPanel);
        }

        private Panel CreatePatientListPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 10, 0)
            };

            // Başlık
            var lblHeader = new Label
            {
                Text = "💬 HASTALAR",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 128, 128),
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(15, 15, 0, 0)
            };

            // Hasta listesi scroll panel
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(10)
            };
            scrollPanel.Tag = "patientScroll";

            panel.Controls.Add(scrollPanel);
            panel.Controls.Add(lblHeader);

            return panel;
        }

        private Panel CreateChatPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(10, 0, 0, 0)
            };

            // Üst - Seçili hasta başlık
            lblSelectedPatient = new Label
            {
                Text = "Bir hasta seçin",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 128, 128),
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(15, 15, 0, 0),
                BackColor = Color.FromArgb(240, 248, 255)
            };

            // Orta - Mesaj alanı
            pnlChatMessages = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10)
            };

            // Alt - Mesaj gönderme
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            txtMessage = new TextEdit
            {
                Location = new Point(10, 15),
                Size = new Size(600, 30)
            };
            txtMessage.Properties.NullValuePrompt = "Mesajınızı yazın...";
            txtMessage.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SendMessage(); };

            btnSend = new SimpleButton
            {
                Text = "Gönder ➤",
                Location = new Point(620, 10),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(0, 128, 128),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSend.Appearance.BackColor = Color.FromArgb(0, 128, 128);
            btnSend.Appearance.ForeColor = Color.White;
            btnSend.Click += (s, e) => SendMessage();

            bottomPanel.Controls.Add(txtMessage);
            bottomPanel.Controls.Add(btnSend);

            panel.Controls.Add(pnlChatMessages);
            panel.Controls.Add(lblSelectedPatient);
            panel.Controls.Add(bottomPanel);

            return panel;
        }

        private void LoadPatients()
        {
            try
            {
                _patients = _patientRepository.GetByDoctorId(AuthContext.UserId).ToList();
                var scrollPanel = pnlPatientList.Controls.OfType<Panel>().FirstOrDefault(p => p.Tag?.ToString() == "patientScroll");
                if (scrollPanel == null) return;

                scrollPanel.Controls.Clear();

                int yPos = 5;
                foreach (var patient in _patients)
                {
                    var card = CreatePatientCard(patient);
                    card.Location = new Point(5, yPos);
                    scrollPanel.Controls.Add(card);
                    yPos += card.Height + 5;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hastalar yüklenirken hata: {ex.Message}", "Hata");
            }
        }

        private Panel CreatePatientCard(PatientEntity patient)
        {
            var card = new Panel
            {
                Width = 260,
                Height = 60,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            var lblName = new Label
            {
                Text = $"👤 {patient.AdSoyad}",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var lblInfo = new Label
            {
                Text = $"{patient.Cinsiyet} | {patient.Yas} yaş",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.Gray,
                Location = new Point(10, 35),
                AutoSize = true
            };

            card.Controls.Add(lblName);
            card.Controls.Add(lblInfo);

            // Hover efekti
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(230, 245, 245);
            card.MouseLeave += (s, e) => card.BackColor = _selectedPatient?.Id == patient.Id ? Color.FromArgb(200, 230, 230) : Color.White;
            card.Click += (s, e) => SelectPatient(patient);
            lblName.Click += (s, e) => SelectPatient(patient);
            lblInfo.Click += (s, e) => SelectPatient(patient);

            return card;
        }

        private void SelectPatient(PatientEntity patient)
        {
            _selectedPatient = patient;
            lblSelectedPatient.Text = $"💬 {patient.AdSoyad} ile Mesajlaşma";
            LoadMessages();

            // Kartları güncelle
            var scrollPanel = pnlPatientList.Controls.OfType<Panel>().FirstOrDefault(p => p.Tag?.ToString() == "patientScroll");
            if (scrollPanel != null)
            {
                foreach (Panel card in scrollPanel.Controls.OfType<Panel>())
                {
                    var lbl = card.Controls.OfType<Label>().FirstOrDefault();
                    if (lbl != null && lbl.Text.Contains(patient.AdSoyad))
                        card.BackColor = Color.FromArgb(200, 230, 230);
                    else
                        card.BackColor = Color.White;
                }
            }
        }

        private void LoadMessages()
        {
            if (_selectedPatient == null) return;

            pnlChatMessages.Controls.Clear();

            try
            {
                var messages = _messageService.GetConversation(AuthContext.UserId, _selectedPatient.Id);

                int yPos = 10;
                foreach (var msg in messages.OrderBy(m => m.SentAt))
                {
                    var bubble = CreateMessageBubble(msg);
                    bubble.Location = new Point(msg.FromUserId == AuthContext.UserId ? pnlChatMessages.Width - bubble.Width - 30 : 10, yPos);
                    pnlChatMessages.Controls.Add(bubble);
                    yPos += bubble.Height + 10;
                }

                // En alta scroll
                pnlChatMessages.VerticalScroll.Value = pnlChatMessages.VerticalScroll.Maximum;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Mesajlar yüklenirken hata: {ex.Message}", "Hata");
            }
        }

        private Panel CreateMessageBubble(MessageEntity msg)
        {
            bool isMine = msg.FromUserId == AuthContext.UserId;

            var bubble = new Panel
            {
                Width = 350,
                AutoSize = true,
                MinimumSize = new Size(100, 50),
                BackColor = isMine ? Color.FromArgb(0, 128, 128) : Color.White,
                Padding = new Padding(10)
            };

            var lblContent = new Label
            {
                Text = msg.Content,
                Font = new Font("Segoe UI", 10F),
                ForeColor = isMine ? Color.White : Color.FromArgb(50, 50, 50),
                Location = new Point(10, 10),
                MaximumSize = new Size(320, 0),
                AutoSize = true
            };

            var lblTime = new Label
            {
                Text = msg.SentAt.ToString("HH:mm"),
                Font = new Font("Segoe UI", 8F),
                ForeColor = isMine ? Color.FromArgb(200, 255, 255) : Color.Gray,
                Location = new Point(10, lblContent.Bottom + 5),
                AutoSize = true
            };

            bubble.Controls.Add(lblContent);
            bubble.Controls.Add(lblTime);
            bubble.Height = lblTime.Bottom + 10;

            return bubble;
        }

        private void SendMessage()
        {
            if (_selectedPatient == null)
            {
                XtraMessageBox.Show("Lütfen bir hasta seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMessage.Text)) return;

            try
            {
                _messageService.SendMessage(AuthContext.UserId, _selectedPatient.Id, txtMessage.Text);
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
            _refreshTimer.Interval = 5000; // 5 saniye
            _refreshTimer.Tick += (s, e) => { if (_selectedPatient != null) LoadMessages(); };
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

