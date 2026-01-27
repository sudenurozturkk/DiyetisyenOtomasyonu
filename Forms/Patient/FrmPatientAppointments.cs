using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmPatientAppointments : XtraForm
    {
        private readonly AppointmentService _appointmentService;
        private readonly PatientRepository _patientRepo;
        private XtraScrollableControl pnlAppointments;
        private ComboBoxEdit cmbFilter;

        // Modern Renkler - UiStyles
        private Color PrimaryColor => UiStyles.PrimaryColor;
        private Color SuccessGreen => UiStyles.SuccessColor;
        private Color InfoBlue => UiStyles.InfoColor;
        private Color WarningOrange => UiStyles.WarningColor;
        private Color DangerRed => UiStyles.DangerColor;
        private Color CardColor => Color.White;
        private Color BackgroundColor => Color.FromArgb(245, 247, 250);
        private Color TextPrimary => UiStyles.TextPrimary;
        private Color TextSecondary => UiStyles.TextSecondary;

        public FrmPatientAppointments()
        {
            InitializeComponent();
            _appointmentService = new AppointmentService();
            _patientRepo = new PatientRepository();
            SetupUI();
            LoadAppointments();
        }

        private void SetupUI()
        {
            this.Text = "Randevularım";
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(20);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // List

            // 1. Header
            var headerPanel = CreateHeaderPanel();
            mainLayout.Controls.Add(headerPanel, 0, 0);

            // 2. Appointments List
            pnlAppointments = new XtraScrollableControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            mainLayout.Controls.Add(pnlAppointments, 0, 1);

            this.Controls.Add(mainLayout);
        }

        private PanelControl CreateHeaderPanel()
        {
            var panel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            var lblTitle = new LabelControl
            {
                Text = "📅 Randevularım",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 10),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            // Randevu Talep Et Butonu
            var btnRequest = new SimpleButton
            {
                Text = "➕ Randevu Talep Et",
                Location = new Point(panel.Width - 360, 12),
                Size = new Size(150, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnRequest.Appearance.BackColor = SuccessGreen;
            btnRequest.Appearance.ForeColor = Color.White;
            btnRequest.Appearance.Options.UseBackColor = true;
            btnRequest.Appearance.Options.UseForeColor = true;
            btnRequest.Click += BtnRequest_Click;
            panel.Controls.Add(btnRequest);

            cmbFilter = new ComboBoxEdit
            {
                Location = new Point(panel.Width - 200, 13),
                Size = new Size(190, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            cmbFilter.Properties.Items.AddRange(new[] { "Gelecek Randevular", "Geçmiş Randevular", "Tümü" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => LoadAppointments();
            panel.Controls.Add(cmbFilter);

            return panel;
        }

        private void BtnRequest_Click(object sender, EventArgs e)
        {
            using (var form = new XtraForm())
            {
                form.Text = "Randevu Talep Et";
                form.Size = new Size(400, 350);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = BackgroundColor;

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20),
                    RowCount = 5,
                    ColumnCount = 1,
                    BackColor = Color.Transparent
                };
                
                // Tarih Seçimi
                layout.Controls.Add(new LabelControl { Text = "Tarih ve Saat:", Font = new Font("Segoe UI", 10F, FontStyle.Bold) });
                var dtDate = new DateEdit { Properties = { CalendarView = DevExpress.XtraEditors.Repository.CalendarView.TouchUI, Mask = { EditMask = "g", UseMaskAsDisplayFormat = true } } };
                dtDate.DateTime = DateTime.Now.AddDays(1).Date.AddHours(9); // Yarın sabah 9
                dtDate.Dock = DockStyle.Top;
                layout.Controls.Add(dtDate);

                // Tür Seçimi
                layout.Controls.Add(new LabelControl { Text = "Randevu Türü:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Padding = new Padding(0, 10, 0, 0) });
                var rgType = new RadioGroup();
                rgType.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(AppointmentType.Online, "Online Görüşme"));
                rgType.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(AppointmentType.Clinic, "Klinik Muayene"));
                rgType.SelectedIndex = 0;
                rgType.Dock = DockStyle.Top;
                rgType.Height = 60;
                layout.Controls.Add(rgType);

                // Not
                layout.Controls.Add(new LabelControl { Text = "Notunuz (Opsiyonel):", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Padding = new Padding(0, 10, 0, 0) });
                var txtNote = new MemoEdit { Height = 60, Dock = DockStyle.Top };
                layout.Controls.Add(txtNote);

                // Buton
                var btnSend = new SimpleButton
                {
                    Text = "Talebi Gönder",
                    Height = 40,
                    Dock = DockStyle.Bottom,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                btnSend.Appearance.BackColor = PrimaryColor;
                btnSend.Appearance.ForeColor = Color.White;
                btnSend.Appearance.Options.UseBackColor = true;
                btnSend.Appearance.Options.UseForeColor = true;
                btnSend.Click += (s, args) =>
                {
                    if (dtDate.DateTime <= DateTime.Now)
                    {
                        XtraMessageBox.Show("Geçmiş bir tarihe randevu alamazsınız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        // Hastanın doktorunu bul
                        var patient = _patientRepo.GetById(AuthContext.UserId);
                        if (patient == null || patient.DoctorId <= 0)
                        {
                            XtraMessageBox.Show("Size atanmış bir doktor bulunamadı. Lütfen doktorunuzla iletişime geçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        var app = new Appointment
                        {
                            PatientId = AuthContext.UserId,
                            DoctorId = patient.DoctorId, // Hastanın doktoru
                            DateTime = dtDate.DateTime,
                            Type = (AppointmentType)rgType.EditValue,
                            Status = AppointmentStatus.Pending,
                            Notes = txtNote.Text,
                            CreatedAt = DateTime.Now
                        };

                        var repo = new DiyetisyenOtomasyonu.Infrastructure.Repositories.AppointmentRepository();
                        repo.Add(app);

                        XtraMessageBox.Show("Randevu talebiniz iletildi! Doktorunuz onayladığında bildirim alacaksınız.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        form.Close();
                        LoadAppointments();
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                layout.Controls.Add(btnSend);

                form.Controls.Add(layout);
                form.ShowDialog();
            }
        }

        private void LoadAppointments()
        {
            pnlAppointments.Controls.Clear();
            var appointments = _appointmentService.GetPatientAppointments(AuthContext.UserId);

            // Filter
            if (cmbFilter.SelectedIndex == 0) // Gelecek
                appointments = appointments.Where(a => a.DateTime >= DateTime.Now).ToList();
            else if (cmbFilter.SelectedIndex == 1) // Geçmiş
                appointments = appointments.Where(a => a.DateTime < DateTime.Now).ToList();

            if (!appointments.Any())
            {
                var lbl = new LabelControl { Text = "Randevu bulunamadı.", Location = new Point(20, 20) };
                pnlAppointments.Controls.Add(lbl);
                return;
            }

            int y = 0;
            foreach (var app in appointments.OrderBy(a => a.DateTime))
            {
                var card = CreateAppointmentCard(app);
                card.Location = new Point(0, y);
                card.Width = pnlAppointments.Width - 20;
                pnlAppointments.Controls.Add(card);
                y += card.Height + 15;
            }
        }

        private PanelControl CreateAppointmentCard(Appointment app)
        {
            var card = new PanelControl
            {
                Height = 100,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor,
                Padding = new Padding(0),
                Margin = new Padding(0, 0, 0, 15)
            };

            // Sol kenar çubuğu
            var leftBar = new PanelControl
            {
                Dock = DockStyle.Left,
                Width = 6,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = GetStatusColor(app.Status)
            };
            card.Controls.Add(leftBar);

            // Layout
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };
            // Tarih (20%), Saat/Tip (30%), Doktor/Durum (30%), Aksiyon (20%)
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            // 1. Tarih Kutusu
            var datePanel = new PanelControl { BorderStyle = BorderStyles.NoBorder, BackColor = Color.Transparent, Dock = DockStyle.Fill };
            var lblDay = new LabelControl
            {
                Text = app.DateTime.Day.ToString(),
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = GetStatusColor(app.Status),
                Dock = DockStyle.Top,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            };
            var lblMonth = new LabelControl
            {
                Text = app.DateTime.ToString("MMM").ToUpper(),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            };
            datePanel.Controls.Add(lblMonth);
            datePanel.Controls.Add(lblDay);
            layout.Controls.Add(datePanel, 0, 0);

            // 2. Saat ve Tip
            var timePanel = new PanelControl { BorderStyle = BorderStyles.NoBorder, BackColor = Color.Transparent, Dock = DockStyle.Fill };
            var lblTime = new LabelControl
            {
                Text = app.DateTime.ToString("HH:mm"),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 10)
            };
            var lblType = new LabelControl
            {
                Text = app.Type == AppointmentType.Online ? "🖥️ Online" : "🏥 Klinik",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextSecondary,
                Location = new Point(0, 40)
            };
            timePanel.Controls.Add(lblTime);
            timePanel.Controls.Add(lblType);
            layout.Controls.Add(timePanel, 1, 0);

            // 3. Doktor ve Durum
            var statusPanel = new PanelControl { BorderStyle = BorderStyles.NoBorder, BackColor = Color.Transparent, Dock = DockStyle.Fill };
            var lblDoc = new LabelControl
            {
                Text = "Dr. Diyetisyen",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 15)
            };
            var lblStatus = new LabelControl
            {
                Text = GetStatusText(app.Status),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Appearance = { BackColor = GetStatusColor(app.Status) },
                Padding = new Padding(5, 2, 5, 2),
                Location = new Point(0, 40)
            };
            statusPanel.Controls.Add(lblDoc);
            statusPanel.Controls.Add(lblStatus);
            layout.Controls.Add(statusPanel, 2, 0);

            // 4. Aksiyon Butonu
            if (app.DateTime > DateTime.Now && app.Status == AppointmentStatus.Scheduled)
            {
                var btnCancel = new SimpleButton
                {
                    Text = "İptal",
                    Size = new Size(80, 35),
                    Anchor = AnchorStyles.Right,
                    Font = new Font("Segoe UI", 9F)
                };
                btnCancel.Appearance.BackColor = DangerRed;
                btnCancel.Appearance.ForeColor = Color.White;
                btnCancel.Appearance.Options.UseBackColor = true;
                btnCancel.Appearance.Options.UseForeColor = true;
                btnCancel.Click += (s, e) => {
                    if (XtraMessageBox.Show("Randevuyu iptal etmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _appointmentService.UpdateStatus(app.Id, AppointmentStatus.Cancelled);
                        LoadAppointments();
                    }
                };
                layout.Controls.Add(btnCancel, 3, 0);
            }
            else if (app.Status == AppointmentStatus.Pending)
            {
                var lblPending = new LabelControl
                {
                    Text = "⏳ Onay Bekliyor",
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = WarningOrange,
                    Anchor = AnchorStyles.Right
                };
                layout.Controls.Add(lblPending, 3, 0);
            }

            card.Controls.Add(layout);

            // Alt çizgi
            var line = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            card.Controls.Add(line);

            return card;
        }

        private string GetStatusText(AppointmentStatus status)
        {
            switch (status)
            {
                case AppointmentStatus.Scheduled: return "Planlandı";
                case AppointmentStatus.Completed: return "Tamamlandı";
                case AppointmentStatus.Cancelled: return "İptal Edildi";
                default: return status.ToString();
            }
        }

        private Color GetStatusColor(AppointmentStatus status)
        {
            switch (status)
            {
                case AppointmentStatus.Scheduled: return InfoBlue;
                case AppointmentStatus.Completed: return SuccessGreen;
                case AppointmentStatus.Cancelled: return DangerRed;
                default: return Color.Gray;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Name = "FrmPatientAppointments";
            this.ResumeLayout(false);
        }
    }
}
