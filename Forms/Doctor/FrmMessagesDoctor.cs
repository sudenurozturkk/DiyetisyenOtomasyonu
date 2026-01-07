using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using PatientEntity = DiyetisyenOtomasyonu.Domain.Patient;
using MessageEntity = DiyetisyenOtomasyonu.Domain.Message;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    public partial class FrmMessagesDoctor : XtraForm
    {
        private readonly MessageService _messageService;
        private readonly PatientService _patientService;

        private GridControl gridPatients;
        private GridView viewPatients;
        private XtraScrollableControl scrollMessages;
        private FlowLayoutPanel flowMessages;
        private MemoEdit txtMessage;
        private LabelControl lblSelectedPatient;
        private LabelControl lblUnreadCount;
        private PanelControl emptyStatePanel;

        private int _selectedPatientId;
        private PatientEntity _selectedPatient;

        // Modern Renkler - Yeşil Tema (Mockup'a göre)
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
        private readonly Color DoctorMessageColor = Color.FromArgb(34, 197, 94);  // Yeşil - Doktor (sağda)
        private readonly Color PatientMessageColor = Color.White;                  // Beyaz - Hasta (solda)
        private readonly Color PatientMessageBorderColor = Color.FromArgb(226, 232, 240);

        public FrmMessagesDoctor()
        {
            InitializeComponent();
            _messageService = new MessageService();
            _patientService = new PatientService();
            InitializeUI();
            LoadPatients();
        }

        private void InitializeUI()
        {
            this.Text = "Mesajlar";
            this.BackColor = BackgroundColor;
            this.Padding = new Padding(15);

            var splitMain = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = true,
                SplitterPosition = 320,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            CreateLeftPanel(splitMain.Panel1);
            CreateRightPanel(splitMain.Panel2);

            this.Controls.Add(splitMain);
        }

        private void CreateLeftPanel(SplitGroupPanel panel)
        {
            panel.Padding = new Padding(0, 0, 10, 0);

            var grpPatients = new GroupControl
            {
                Text = "HASTALARIM",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                Appearance = { BackColor = CardColor }
            };

            var headerPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 50,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            lblUnreadCount = new LabelControl
            {
                Text = "0 okunmamış mesaj",
                Location = new Point(15, 15),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = DangerColor
            };
            headerPanel.Controls.Add(lblUnreadCount);

            grpPatients.Controls.Add(headerPanel);

            gridPatients = new GridControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };

            viewPatients = new GridView(gridPatients)
            {
                OptionsView = {
                    ShowGroupPanel = false,
                    ShowIndicator = false,
                    ColumnAutoWidth = true,
                    RowAutoHeight = true
                },
                OptionsBehavior = { Editable = false },
                RowHeight = 50
            };
            viewPatients.Appearance.Row.Font = new Font("Segoe UI", 10F);
            viewPatients.Appearance.SelectedRow.BackColor = Color.FromArgb(238, 242, 255);
            viewPatients.Appearance.SelectedRow.ForeColor = PrimaryColor;
            viewPatients.Appearance.FocusedRow.BackColor = Color.FromArgb(224, 231, 255);
            viewPatients.Appearance.FocusedRow.ForeColor = PrimaryColor;

            viewPatients.Columns.Clear();
            viewPatients.Columns.Add(new GridColumn { FieldName = "AdSoyad", Caption = "Hasta Adi", Visible = true, Width = 180 });
            viewPatients.Columns.Add(new GridColumn { FieldName = "BMIKategori", Caption = "Durum", Visible = true, Width = 100 });

            gridPatients.MainView = viewPatients;
            viewPatients.FocusedRowChanged += ViewPatients_FocusedRowChanged;

            grpPatients.Controls.Add(gridPatients);

            emptyStatePanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor,
                Visible = false
            };

            var lblEmptyIcon = new LabelControl
            {
                Text = "👥",
                Font = new Font("Segoe UI", 42F),
                ForeColor = TextSecondary,
                Location = new Point(100, 60),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(80, 70)
            };
            emptyStatePanel.Controls.Add(lblEmptyIcon);

            var lblEmptyText = new LabelControl
            {
                Text = "Henüz hasta yok",
                Font = new Font("Segoe UI", 12F),
                ForeColor = TextSecondary,
                Location = new Point(75, 140),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 25)
            };
            emptyStatePanel.Controls.Add(lblEmptyText);

            var lblEmptyHint = new LabelControl
            {
                Text = "Hastalarım sayfasından\nhasta ekleyin",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(70, 170),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 40)
            };
            emptyStatePanel.Controls.Add(lblEmptyHint);

            grpPatients.Controls.Add(emptyStatePanel);
            panel.Controls.Add(grpPatients);
        }

        private void CreateRightPanel(SplitGroupPanel panel)
        {
            panel.Padding = new Padding(10, 0, 0, 0);

            var grpChat = new GroupControl
            {
                Text = "MESAJLASMA",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                Appearance = { BackColor = CardColor }
            };

            var pnlHeader = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 55,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.FromArgb(238, 242, 255)
            };

            lblSelectedPatient = new LabelControl
            {
                Text = "Mesajlaşmak için hasta seçin...",
                Location = new Point(20, 15),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(600, 28)
            };
            pnlHeader.Controls.Add(lblSelectedPatient);

            grpChat.Controls.Add(pnlHeader);

            // Mesaj Yazma Alani (EN ALTTA)
            var pnlInput = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 130,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.FromArgb(248, 250, 252),
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

            grpChat.Controls.Add(pnlInput);

            // Mesaj Gecmisi Container (ORTADA - Scroll edilebilir)
            scrollMessages = new XtraScrollableControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(15, 0, 15, 0), // Header'ın altından başlaması için üst padding yok
                AutoScroll = true
            };
            scrollMessages.AutoScrollMargin = new Size(0, 20); // Alt/üst boşluk

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
            grpChat.Controls.Add(scrollMessages);

            panel.Controls.Add(grpChat);
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                BtnSend_Click(sender, e);
            }
        }

        private void LoadPatients()
        {
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                
                bool isEmpty = patients == null || patients.Count == 0;
                emptyStatePanel.Visible = isEmpty;
                gridPatients.Visible = !isEmpty;

                if (!isEmpty)
                {
                    gridPatients.DataSource = patients;
                    
                    int unreadCount = _messageService.GetUnreadCount(AuthContext.UserId);
                    lblUnreadCount.Text = unreadCount > 0 
                        ? $"🔴 {unreadCount} okunmamis mesaj" 
                        : "✓ Tum mesajlar okundu";
                    lblUnreadCount.ForeColor = unreadCount > 0 ? DangerColor : SuccessColor;
                }
                else
                {
                    lblUnreadCount.Text = "Hasta ekleyin";
                    lblUnreadCount.ForeColor = WarningColor;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hastalar yuklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewPatients_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var selected = viewPatients.GetFocusedRow() as PatientEntity;
            if (selected != null)
            {
                _selectedPatientId = selected.Id;
                _selectedPatient = selected;
                lblSelectedPatient.Text = "Konuşma: " + selected.AdSoyad;
                LoadMessages();
            }
        }

        private void LoadMessages()
        {
            if (_selectedPatientId == 0) return;

            try
            {
                flowMessages.SuspendLayout();
                flowMessages.Controls.Clear();

                var messages = _messageService.GetConversation(AuthContext.UserId, _selectedPatientId);
                
                if (messages.Count == 0)
                {
                    var emptyLabel = new Label
                    {
                        Text = "Henüz mesaj yok.\nAşağıdan mesaj gönderin.",
                        Font = new Font("Segoe UI", 11F),
                        ForeColor = TextSecondary,
                        AutoSize = false,
                        Size = new Size(400, 60),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Margin = new Padding(100, 50, 10, 10)
                    };
                    flowMessages.Controls.Add(emptyLabel);
                }
                else
                {
                    var sortedMessages = messages.OrderBy(m => m.SentAt).ToList();
                    
                    foreach (var msg in sortedMessages)
                    {
                        bool isDoctor = msg.FromUserId == AuthContext.UserId;
                        var messageBubble = CreateMessageBubble(msg, isDoctor);
                        flowMessages.Controls.Add(messageBubble);

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
                            
                            // Scroll pozisyonunu minimum'a ayarla
                            scrollMessages.HorizontalScroll.Value = 0;
                            if (scrollMessages.VerticalScroll.Visible)
                            {
                                scrollMessages.VerticalScroll.Value = scrollMessages.VerticalScroll.Minimum;
                            }
                            
                            // AutoScrollPosition ile scroll pozisyonunu ayarla
                            scrollMessages.AutoScrollPosition = new Point(0, 0);
                            
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
                
                // Okunmamis sayiyi guncelle
                int unreadCount = _messageService.GetUnreadCount(AuthContext.UserId);
                lblUnreadCount.Text = unreadCount > 0 
                    ? $"🔴 {unreadCount} okunmamış mesaj" 
                    : "✓ Tüm mesajlar okundu";
                lblUnreadCount.ForeColor = unreadCount > 0 ? DangerColor : SuccessColor;
            }
            catch (Exception ex)
            {
                var errorLabel = new Label
                {
                    Text = "Mesajlar yüklenirken hata: " + ex.Message,
                    ForeColor = DangerColor,
                    AutoSize = true
                };
                flowMessages.Controls.Add(errorLabel);
            }
        }

        private Panel CreateMessageBubble(MessageEntity msg, bool isDoctor)
        {
            // Container genişliği - flowMessages genişliğini kullan
            int containerWidth = flowMessages != null ? flowMessages.Width - 25 : 500;
            if (containerWidth < 200) containerWidth = 200;
            
            var container = new Panel
            {
                Width = containerWidth,
                Height = 0, // AutoSize için başlangıçta 0
                AutoSize = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 5, 0, 5),
                Padding = new Padding(0)
            };

            // Balon içeriği
            int maxBubbleWidth = (int)(containerWidth * 0.70); // %70 genişlik sınırı
            if (maxBubbleWidth < 150) maxBubbleWidth = 150;

            var bubble = new Panel
            {
                AutoSize = false,
                BackColor = isDoctor ? DoctorMessageColor : PatientMessageColor,
                Padding = new Padding(10, 8, 10, 8),
                Tag = isDoctor // Tag ile doktor/hasta bilgisini sakla
            };

            // Mesaj metni
            var lblContent = new Label
            {
                Text = msg.Content,
                Font = new Font("Segoe UI", 10F),
                ForeColor = isDoctor ? Color.White : TextPrimary,
                AutoSize = true,
                MaximumSize = new Size(maxBubbleWidth - 20, 0), // Padding payı
                BackColor = Color.Transparent,
                Location = new Point(10, 8)
            };
            bubble.Controls.Add(lblContent);

            // Zaman ve Durum
            string timeText = msg.SentAt.ToString("HH:mm");
            if (isDoctor) timeText += (msg.IsRead ? " ✓✓" : " ✓");

            var lblTime = new Label
            {
                Text = timeText,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = isDoctor ? Color.FromArgb(220, 255, 255, 255) : TextSecondary,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            
            // Zaman etiketini ekle
            bubble.Controls.Add(lblTime);
            
            // Boyutları hesapla
            // Label AutoSize olduğu için boyutu otomatik alacak
            // Ancak bazen hemen hesaplanmayabilir, bu yüzden PerformLayout çağırabiliriz ama
            // WinForms'da label boyutu genellikle hemen güncellenir.
            
            // Balon boyutunu ayarla
            int contentWidth = lblContent.PreferredWidth;
            int contentHeight = lblContent.PreferredHeight;
            int timeWidth = lblTime.PreferredWidth;
            int timeHeight = lblTime.PreferredHeight;

            // Balon genişliği: İçerik genişliği veya zaman genişliği + padding
            // Zaman etiketi sağ altta olacak
            
            // Eğer içerik tek satırsa ve kısa ise, zamanı yanına koyabiliriz (yer varsa)
            // Ama basitlik için her zaman altına koyalım veya sağa yaslayalım
            
            int bubbleWidth = Math.Max(contentWidth, timeWidth) + 25;
            if (bubbleWidth > maxBubbleWidth) bubbleWidth = maxBubbleWidth;
            if (bubbleWidth < 80) bubbleWidth = 80; // Minimum genişlik

            // Zaman etiketi konumu
            // İçerik bittikten sonra biraz boşluk bırakıp sağa yasla
            
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
            if (isDoctor)
            {
                var menuDeleteForEveryone = new ToolStripMenuItem("⚠ Herkesten Sil");
                // ...
            }
            */

            // Balona sağ tık menüsü ata
            bubble.ContextMenuStrip = contextMenu;
            lblContent.ContextMenuStrip = contextMenu;
            lblTime.ContextMenuStrip = contextMenu;
            
            // Yuvarlak köşeler
            bubble.Paint += (s, e) =>
            {
                var rect = bubble.ClientRectangle;
                rect.Width -= 1; rect.Height -= 1;
                using (var path = GetRoundedRectPath(rect, 12))
                using (var brush = new SolidBrush(bubble.BackColor))
                using (var pen = new Pen(isDoctor ? DoctorMessageColor : PatientMessageBorderColor, 1))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                    if (!isDoctor) e.Graphics.DrawPath(pen, path);
                }
            };

            // Konumlandırma
            if (isDoctor)
            {
                // Sağ tarafa yasla
                container.Controls.Add(bubble);
                bubble.Location = new Point(containerWidth - bubble.Width - 15, 0);
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            else
            {
                // Sol tarafa yasla
                container.Controls.Add(bubble);
                bubble.Location = new Point(5, 0);
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }

            // Container yüksekliğini ayarla
            container.Height = bubble.Height + 10;

            // Container resize olduğunda bubble'ı yeniden konumlandır
            container.Resize += (s, e) =>
            {
                if (isDoctor)
                {
                    bubble.Location = new Point(container.Width - bubble.Width - 15, 0);
                }
                else
                {
                    bubble.Location = new Point(5, 0);
                }
            };

            return container;
        }

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
            if (_selectedPatientId == 0)
            {
                XtraMessageBox.Show("Lütfen hasta seçin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMessage.Text)) return;

            try
            {
                _messageService.SendMessage(AuthContext.UserId, _selectedPatientId, txtMessage.Text.Trim());
                txtMessage.Text = string.Empty;
                LoadMessages();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Mesaj gönderilemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Name = "FrmMessagesDoctor";
            this.ResumeLayout(false);
        }
    }
}
