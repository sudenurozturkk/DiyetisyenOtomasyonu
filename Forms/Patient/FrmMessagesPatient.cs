using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using MessageEntity = DiyetisyenOtomasyonu.Domain.Message;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmMessagesPatient : XtraForm
    {
        private readonly MessageService _messageService;
        private readonly PatientService _patientService;
        private XtraScrollableControl scrollMessages;
        private FlowLayoutPanel flowMessages;
        private MemoEdit txtMessage;
        private LabelControl lblDoctorName;
        private int _doctorId;

        // Modern Renkler - YEŞİL TEMA (Mockup'a göre)
        private readonly Color PrimaryColor = Color.FromArgb(13, 148, 136);       // Teal/Yeşil
        private readonly Color SecondaryColor = Color.FromArgb(20, 184, 166);     // Açık Teal
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);        // Yeşil
        private readonly Color DangerColor = Color.FromArgb(239, 68, 68);
        private readonly Color WarningColor = Color.FromArgb(245, 158, 11);
        private readonly Color CardColor = Color.White;
        private readonly Color BackgroundColor = Color.FromArgb(248, 250, 252);
        private readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        
        // Mesaj Balonu Renkleri - YEŞİL TEMA (Mockup'a göre)
        private readonly Color PatientMessageColor = Color.FromArgb(34, 197, 94);  // Yeşil - Hasta (sağda - kendi mesajları)
        private readonly Color DoctorMessageColor = Color.White;                    // Beyaz - Doktor (solda)
        private readonly Color DoctorMessageBorderColor = Color.FromArgb(226, 232, 240);

        public FrmMessagesPatient()
        {
            InitializeComponent();
            _messageService = new MessageService();
            _patientService = new PatientService();
            InitializeUI();
            GetDoctorId();
            LoadMessages();
        }

        private void InitializeUI()
        {
            this.Text = "Mesajlarım";
            this.BackColor = BackgroundColor;
            this.Padding = new Padding(15);

            // Ana kart - Doktor sayfasıyla aynı stil
            var mainCard = new GroupControl
            {
                Dock = DockStyle.Fill,
                Text = "MESAJLASMA",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                Appearance = { BackColor = CardColor }
            };

            // Header - Doktor bilgisi
            var pnlHeader = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 55,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.FromArgb(238, 242, 255)
            };

            lblDoctorName = new LabelControl
            {
                Text = "Doktor: Yukleniyor...",
                Location = new Point(20, 15),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(600, 28)
            };
            pnlHeader.Controls.Add(lblDoctorName);

            mainCard.Controls.Add(pnlHeader);

            // Mesaj yazma alani (EN ALTTA)
            var pnlInput = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 130,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15)
            };

            var lblNewMsg = new LabelControl
            {
                Text = "Yeni Mesaj Yaz (Enter ile gönder):",
                Location = new Point(10, 5),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            pnlInput.Controls.Add(lblNewMsg);

            txtMessage = new MemoEdit
            {
                Location = new Point(10, 28),
                Size = new Size(580, 50),
                Properties = {
                    NullText = "Mesajınızı yazın ve Enter'a basın...",
                    Appearance = { Font = new Font("Segoe UI", 11F) },
                    ScrollBars = ScrollBars.None
                }
            };
            txtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMessage.KeyDown += TxtMessage_KeyDown;
            pnlInput.Controls.Add(txtMessage);

            var btnSend = new SimpleButton
            {
                Text = "Gönder",
                Location = new Point(10, 85),
                Size = new Size(130, 38),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Appearance = { BackColor = SuccessColor, ForeColor = Color.White },
                AllowFocus = false
            };
            btnSend.Click += BtnSend_Click;
            pnlInput.Controls.Add(btnSend);

            mainCard.Controls.Add(pnlInput);

            // Mesaj Geçmişi - Scrollable (kırpılmayı önler) - Doktor sayfasıyla aynı
            scrollMessages = new XtraScrollableControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(15, 0, 15, 0), // Header'ın altından başlaması için üst padding yok
                AutoScroll = true
            };
            scrollMessages.AutoScrollMargin = new Size(0, 20);

            flowMessages = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 70, 0, 20), // Header'dan sonra 70px boşluk - ilk mesaj görünsün
                Margin = new Padding(0),
                Width = scrollMessages.ClientSize.Width - 30
            };

            // FlowLayoutPanel genişliğini scrollMessages ile senkronize et
            scrollMessages.Resize += (s, e) =>
            {
                if (flowMessages != null && scrollMessages.ClientSize.Width > 0)
                {
                    flowMessages.Width = scrollMessages.ClientSize.Width - 30;
                }
            };

            scrollMessages.Controls.Add(flowMessages);
            mainCard.Controls.Add(scrollMessages);

            this.Controls.Add(mainCard);
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter ile mesaj gönder (Shift+Enter yeni satır)
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                BtnSend_Click(sender, e);
            }
        }

        private void GetDoctorId()
        {
            var patient = _patientService.GetPatientById(AuthContext.UserId);
            if (patient != null)
            {
                _doctorId = patient.DoctorId;
                lblDoctorName.Text = "👨‍⚕️ Doktorunuz: Dr. " + (patient.DoctorName ?? "Diyetisyen");
            }
        }

        private void LoadMessages()
        {
            flowMessages.SuspendLayout();
            flowMessages.Controls.Clear();

            var messages = _messageService.GetConversation(AuthContext.UserId, _doctorId);

            if (messages.Count == 0)
            {
                flowMessages.Controls.Add(new Label
                {
                    Text = "Henüz mesaj yok.\nDoktorunuza aşağıdan mesaj gönderebilirsiniz.",
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = TextSecondary,
                    AutoSize = true,
                    Margin = new Padding(10, 20, 10, 10)
                });
            }
            else
            {
                var sortedMessages = messages.OrderBy(m => m.SentAt).ToList();

                foreach (var msg in sortedMessages)
                {
                    bool isPatient = msg.FromUserId == AuthContext.UserId;
                    var bubble = CreateMessageBubble(msg, isPatient);
                    flowMessages.Controls.Add(bubble);

                    if (msg.ToUserId == AuthContext.UserId && !msg.IsRead)
                    {
                        _messageService.MarkAsRead(msg.Id);
                    }
                }
            }

            flowMessages.ResumeLayout(true);
            flowMessages.PerformLayout();

            // FlowLayoutPanel genişliğini güncelle
            flowMessages.Width = scrollMessages.ClientSize.Width - 30;
            
            // AutoSize ile yükseklik otomatik ayarlanacak
            flowMessages.PerformLayout();

            // Scroll pozisyonunu EN ALTA ayarla - en son mesajlar görünsün (WhatsApp gibi)
            // Handle oluşturulduktan sonra çalıştır
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // Önce layout'u tamamla
                        scrollMessages.PerformLayout();
                        flowMessages.PerformLayout();
                        
                        // Scroll'u EN ALTA ayarla - en son mesaj görünsün
                        if (flowMessages.Controls.Count > 0)
                        {
                            var lastControl = flowMessages.Controls[flowMessages.Controls.Count - 1];
                            scrollMessages.ScrollControlIntoView(lastControl);
                        }
                        else if (scrollMessages.VerticalScroll.Visible)
                        {
                            scrollMessages.VerticalScroll.Value = scrollMessages.VerticalScroll.Maximum;
                        }
                        
                        // Son layout ve yeniden çizim
                        scrollMessages.PerformLayout();
                        flowMessages.PerformLayout();
                        flowMessages.Invalidate();
                        scrollMessages.Invalidate();
                        scrollMessages.Refresh();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Scroll hatası: " + ex.Message);
                    }
                }));
            }
            else
            {
                // Handle henüz oluşturulmadıysa, Load event'inde çalıştır
                this.Load += (s, e) =>
                {
                    try
                    {
                        scrollMessages.HorizontalScroll.Value = 0;
                        if (scrollMessages.VerticalScroll.Visible)
                        {
                            scrollMessages.VerticalScroll.Value = scrollMessages.VerticalScroll.Minimum;
                        }
                        scrollMessages.AutoScrollPosition = new Point(0, 0);
                        if (flowMessages.Controls.Count > 0)
                        {
                            scrollMessages.ScrollControlIntoView(flowMessages.Controls[0]);
                        }
                    }
                    catch { }
                };
            }
        }

        /// <summary>
        /// WhatsApp tarzi mesaj balonu olusturur
        /// </summary>
        private Panel CreateMessageBubble(MessageEntity msg, bool isPatient)
        {
            // Container genişliği - flowMessages genişliğini kullan
            int containerWidth = flowMessages != null ? flowMessages.Width - 25 : 500;
            if (containerWidth < 200) containerWidth = 200;

            // Ana container - tum genisligi kaplar
            var container = new Panel
            {
                Width = containerWidth,
                Height = 0, // AutoSize için başlangıçta 0
                AutoSize = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 5, 0, 5),
                Padding = new Padding(0)
            };

            // Mesaj balonu - Hasta kendi mesajlari sag (mavi/yeşil), doktor mesajlari sol (gri/beyaz)
            int maxBubbleWidth = (int)(containerWidth * 0.70); // %70 genişlik sınırı
            if (maxBubbleWidth < 150) maxBubbleWidth = 150;

            var bubble = new Panel
            {
                AutoSize = false,
                BackColor = isPatient ? PatientMessageColor : DoctorMessageColor,
                Padding = new Padding(10, 8, 10, 8),
                Tag = isPatient // Tag ile hasta/doktor bilgisini sakla
            };

            // Mesaj metni - Doktor sayfasıyla aynı stil
            var lblContent = new Label
            {
                Text = msg.Content,
                Font = new Font("Segoe UI", 10F),
                ForeColor = isPatient ? Color.White : TextPrimary,
                AutoSize = true,
                MaximumSize = new Size(maxBubbleWidth - 20, 0), // Padding payı
                BackColor = Color.Transparent,
                Location = new Point(10, 8)
            };
            bubble.Controls.Add(lblContent);

            // Zaman ve Durum - Doktor sayfasıyla aynı stil
            string timeText = msg.SentAt.ToString("HH:mm");
            if (isPatient) timeText += (msg.IsRead ? " ✓✓" : " ✓");

            var lblTime = new Label
            {
                Text = timeText,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = isPatient ? Color.FromArgb(220, 255, 255, 255) : TextSecondary,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            
            // Zaman etiketini ekle
            bubble.Controls.Add(lblTime);
            
            // Boyutları hesapla
            int contentWidth = lblContent.PreferredWidth;
            int contentHeight = lblContent.PreferredHeight;
            int timeWidth = lblTime.PreferredWidth;
            int timeHeight = lblTime.PreferredHeight;

            // Balon genişliği
            int bubbleWidth = Math.Max(contentWidth, timeWidth) + 25;
            if (bubbleWidth > maxBubbleWidth) bubbleWidth = maxBubbleWidth;
            if (bubbleWidth < 80) bubbleWidth = 80; // Minimum genişlik

            int bubbleHeight = contentHeight + timeHeight + 15; // 15px padding toplam
            
            // Zamanı sağ alt köşeye yerleştir
            lblTime.Location = new Point(bubbleWidth - timeWidth - 8, bubbleHeight - timeHeight - 5);

            bubble.Size = new Size(bubbleWidth, bubbleHeight);

            // Sağ tıklama menüsü (WhatsApp tarzı silme seçenekleri)
            var contextMenu = new ContextMenuStrip();
            contextMenu.Font = new Font("Segoe UI", 10F);
            
            // Benden Sil seçeneği (herkes için)
            var menuDeleteForMe = new ToolStripMenuItem("🗑 Benden Sil");
            menuDeleteForMe.Click += (s, e) =>
            {
                var result = XtraMessageBox.Show(
                    "Bu mesaj sadece sizin görünümünüzden silinecek. Devam etmek istiyor musunuz?", 
                    "Benden Sil",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _messageService.DeleteMessageForMe(msg.Id, AuthContext.UserId);
                        LoadMessages();
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
            contextMenu.Items.Add(menuDeleteForMe);

            // Herkesten Sil seçeneği KALDIRILDI (Veritabanı desteği yok)
            /*
            if (isPatient)
            {
                var menuDeleteForEveryone = new ToolStripMenuItem("⚠ Herkesten Sil");
                // ...
            }
            */

            // Balona sağ tık menüsü ata
            bubble.ContextMenuStrip = contextMenu;
            lblContent.ContextMenuStrip = contextMenu;
            lblTime.ContextMenuStrip = contextMenu;

            // Yuvarlak köşeler - Doktor sayfasıyla aynı stil
            bubble.Paint += (s, e) =>
            {
                var rect = bubble.ClientRectangle;
                rect.Width -= 1; rect.Height -= 1;
                using (var path = GetRoundedRectPath(rect, 12))
                using (var brush = new SolidBrush(bubble.BackColor))
                using (var pen = new Pen(isPatient ? PatientMessageColor : DoctorMessageBorderColor, 1))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                    if (!isPatient) e.Graphics.DrawPath(pen, path);
                }
            };

            // Hasta mesaji sagda, doktor mesaji solda
            if (isPatient)
            {
                container.Controls.Add(bubble);
                bubble.Location = new Point(containerWidth - bubble.Width - 15, 0);
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            else
            {
                container.Controls.Add(bubble);
                bubble.Location = new Point(5, 0);
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }

            container.Height = bubble.Height + 10;

            // Container resize olduğunda bubble'ı yeniden konumlandır
            container.Resize += (s, e) =>
            {
                if (isPatient)
                    bubble.Location = new Point(container.Width - bubble.Width - 15, 0);
                else
                    bubble.Location = new Point(5, 0);
            };

            return container;
        }

        /// <summary>
        /// Yuvarlatilmis dikdortgen path olusturur
        /// </summary>
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            
            return path;
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            // Boş mesaj gönderme
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
                return;

            _messageService.SendMessage(AuthContext.UserId, _doctorId, txtMessage.Text.Trim());
            txtMessage.Text = string.Empty;
            LoadMessages();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Name = "FrmMessagesPatient";
            this.ResumeLayout(false);
        }
    }
}
