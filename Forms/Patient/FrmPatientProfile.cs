using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    /// <summary>
    /// Hasta Profil Formu - Hasta kendi bilgilerini görüp düzenleyebilir
    /// DevExpress bileşenleri ile modern tasarım
    /// </summary>
    public partial class FrmPatientProfile : DevExpress.XtraEditors.XtraForm
    {
        // Repositories
        private readonly PatientRepository _patientRepo;
        private readonly UserRepository _userRepo;
        private readonly WeightEntryRepository _weightRepo;
        private readonly BodyMeasurementRepository _bodyRepo;
        private readonly GoalRepository _goalRepo;
        
        // Data
        private DiyetisyenOtomasyonu.Domain.Patient _patient;
        private int _patientId;
        
        // Colors
        private Color PrimaryColor { get { return UiStyles.PrimaryColor; } }
        private Color SuccessColor { get { return UiStyles.SuccessColor; } }
        private Color WarningColor { get { return UiStyles.WarningColor; } }
        private Color CardBg { get { return Color.White; } }
        private Color BackgroundColor { get { return UiStyles.BackgroundColor; } }
        private Color TextPrimary { get { return UiStyles.TextPrimary; } }
        private Color TextSecondary { get { return UiStyles.TextSecondary; } }

        // UI Controls
        private PanelControl mainPanel;
        private PanelControl headerPanel;
        private PictureEdit picProfile;
        private TextEdit txtAdSoyad;
        private TextEdit txtBoy;
        private TextEdit txtKilo;
        private TextEdit txtHedefKilo;
        private SpinEdit spinYas;
        private ComboBoxEdit cmbCinsiyet;
        private MemoEdit txtNotlar;
        private SimpleButton btnSave;
        private SimpleButton btnUploadPhoto;
        private GridControl gridMeasurements;
        private GridView gridViewMeasurements;

        public FrmPatientProfile(int patientId)
        {
            _patientId = patientId;
            _patientRepo = new PatientRepository();
            _userRepo = new UserRepository();
            _weightRepo = new WeightEntryRepository();
            _bodyRepo = new BodyMeasurementRepository();
            _goalRepo = new GoalRepository();
            
            InitializeComponent();
            SetupUI();
            LoadPatientData();
        }

        private void InitializeComponent()
        {
            this.Text = "Profilim";
            this.Size = new Size(1200, 800);
            this.BackColor = BackgroundColor;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetupUI()
        {
            // Main Panel
            mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor,
                Padding = new Padding(20)
            };
            this.Controls.Add(mainPanel);

            // Header
            headerPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 60,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            headerPanel.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, headerPanel.Width, headerPanel.Height),
                    PrimaryColor, Color.FromArgb(34, 197, 94), LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, headerPanel.Width, headerPanel.Height);
                }
            };

            var lblTitle = new LabelControl
            {
                Text = "👤 Profilim",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(headerPanel);

            // Content Panel - Two columns
            var contentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainPanel.Controls.Add(contentPanel);
            contentPanel.BringToFront();

            // Left Column - Profile Info
            var leftPanel = CreateProfileInfoPanel();
            contentPanel.Controls.Add(leftPanel, 0, 0);

            // Right Column - Measurements & Goals
            var rightPanel = CreateMeasurementsPanel();
            contentPanel.Controls.Add(rightPanel, 1, 0);
        }

        private PanelControl CreateProfileInfoPanel()
        {
            var panel = new PanelControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 0),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardBg
            };

            // Header
            var header = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 40,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            header.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, header.Width, header.Height),
                    PrimaryColor, Color.FromArgb(34, 197, 94), LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, header.Width, header.Height);
                }
            };
            var lblHeader = new LabelControl
            {
                Text = "📋 Kişisel Bilgiler",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblHeader);
            panel.Controls.Add(header);

            // Content
            var content = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardBg,
                AutoScroll = true
            };

            int y = 20;
            int labelWidth = 120;
            int inputWidth = 200;
            int leftMargin = 20;

            // Profile Photo
            picProfile = new PictureEdit
            {
                Location = new Point(leftMargin, y),
                Size = new Size(120, 120),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple
            };
            picProfile.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            picProfile.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            content.Controls.Add(picProfile);

            btnUploadPhoto = new SimpleButton
            {
                Text = "📷 Resim Yükle",
                Location = new Point(leftMargin + 140, y + 40),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 9F)
            };
            btnUploadPhoto.Appearance.BackColor = PrimaryColor;
            btnUploadPhoto.Appearance.ForeColor = Color.White;
            btnUploadPhoto.Appearance.Options.UseBackColor = true;
            btnUploadPhoto.Appearance.Options.UseForeColor = true;
            btnUploadPhoto.Click += BtnUploadPhoto_Click;
            content.Controls.Add(btnUploadPhoto);

            y += 140;

            // Ad Soyad
            AddLabel(content, "Ad Soyad:", leftMargin, y);
            txtAdSoyad = new TextEdit
            {
                Location = new Point(leftMargin + labelWidth, y - 3),
                Size = new Size(inputWidth, 28),
                Font = new Font("Segoe UI", 10F)
            };
            content.Controls.Add(txtAdSoyad);
            y += 40;

            // Yaş
            AddLabel(content, "Yaş:", leftMargin, y);
            spinYas = new SpinEdit
            {
                Location = new Point(leftMargin + labelWidth, y - 3),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 10F)
            };
            spinYas.Properties.MinValue = 1;
            spinYas.Properties.MaxValue = 120;
            content.Controls.Add(spinYas);
            y += 40;

            // Cinsiyet
            AddLabel(content, "Cinsiyet:", leftMargin, y);
            cmbCinsiyet = new ComboBoxEdit
            {
                Location = new Point(leftMargin + labelWidth, y - 3),
                Size = new Size(inputWidth, 28),
                Font = new Font("Segoe UI", 10F)
            };
            cmbCinsiyet.Properties.Items.AddRange(new[] { "Erkek", "Kadın" });
            cmbCinsiyet.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            content.Controls.Add(cmbCinsiyet);
            y += 40;

            // Boy
            AddLabel(content, "Boy (cm):", leftMargin, y);
            txtBoy = new TextEdit
            {
                Location = new Point(leftMargin + labelWidth, y - 3),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 10F)
            };
            content.Controls.Add(txtBoy);
            y += 40;

            // Güncel Kilo
            AddLabel(content, "Güncel Kilo (kg):", leftMargin, y);
            txtKilo = new TextEdit
            {
                Location = new Point(leftMargin + labelWidth, y - 3),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 10F)
            };
            content.Controls.Add(txtKilo);
            y += 40;

            // Hedef Kilo
            AddLabel(content, "Hedef Kilo (kg):", leftMargin, y);
            txtHedefKilo = new TextEdit
            {
                Location = new Point(leftMargin + labelWidth, y - 3),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 10F)
            };
            content.Controls.Add(txtHedefKilo);
            y += 40;

            // Notlar
            AddLabel(content, "Notlar:", leftMargin, y);
            txtNotlar = new MemoEdit
            {
                Location = new Point(leftMargin + labelWidth, y - 3),
                Size = new Size(inputWidth, 80),
                Font = new Font("Segoe UI", 10F)
            };
            content.Controls.Add(txtNotlar);
            y += 100;

            // Save Button
            btnSave = new SimpleButton
            {
                Text = "💾 Kaydet",
                Location = new Point(leftMargin, y),
                Size = new Size(150, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            btnSave.Appearance.BackColor = SuccessColor;
            btnSave.Appearance.ForeColor = Color.White;
            btnSave.Appearance.Options.UseBackColor = true;
            btnSave.Appearance.Options.UseForeColor = true;
            btnSave.Click += BtnSave_Click;
            content.Controls.Add(btnSave);

            panel.Controls.Add(content);
            content.BringToFront();

            return panel;
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            var lbl = new LabelControl
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(x, y)
            };
            parent.Controls.Add(lbl);
        }

        private PanelControl CreateMeasurementsPanel()
        {
            var panel = new PanelControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 0, 0, 0),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardBg
            };

            // Header
            var header = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 40,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            header.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, header.Width, header.Height),
                    WarningColor, Color.FromArgb(251, 191, 36), LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, header.Width, header.Height);
                }
            };
            var lblHeader = new LabelControl
            {
                Text = "📏 Vücut Ölçülerim",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblHeader);
            panel.Controls.Add(header);

            // Add Measurement Button
            var btnAdd = new SimpleButton
            {
                Text = "➕ Yeni Ölçüm Ekle",
                Location = new Point(header.Width - 160, 5),
                Size = new Size(150, 30),
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAdd.Appearance.BackColor = Color.White;
            btnAdd.Appearance.ForeColor = WarningColor;
            btnAdd.Appearance.Options.UseBackColor = true;
            btnAdd.Appearance.Options.UseForeColor = true;
            btnAdd.Click += BtnAddMeasurement_Click;
            header.Controls.Add(btnAdd);

            // Grid
            gridMeasurements = new GridControl
            {
                Dock = DockStyle.Fill
            };
            gridViewMeasurements = new GridView(gridMeasurements)
            {
                OptionsView = { ShowGroupPanel = false }
            };
            gridMeasurements.MainView = gridViewMeasurements;
            panel.Controls.Add(gridMeasurements);
            gridMeasurements.BringToFront();

            return panel;
        }

        private void LoadPatientData()
        {
            try
            {
                // GetFullPatientById kullan - ProfilePhoto dahil tüm bilgileri getirir
                _patient = _patientRepo.GetFullPatientById(_patientId);
                if (_patient == null)
                {
                    XtraMessageBox.Show("Hasta bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Fill form
                txtAdSoyad.Text = _patient.AdSoyad;
                spinYas.Value = _patient.Yas;
                cmbCinsiyet.Text = _patient.Cinsiyet;
                txtBoy.Text = _patient.Boy.ToString();
                txtKilo.Text = _patient.GuncelKilo.ToString();
                txtNotlar.Text = _patient.Notlar;

                // Load target weight from goals
                var goals = _goalRepo.GetByPatientId(_patientId);
                var weightGoal = goals.FirstOrDefault(g => g.GoalType == GoalType.Weight && g.IsActive);
                if (weightGoal != null)
                {
                    txtHedefKilo.Text = weightGoal.TargetValue.ToString();
                }

                // Load profile photo
                if (!string.IsNullOrEmpty(_patient.ProfilePhoto) && File.Exists(_patient.ProfilePhoto))
                {
                    try
                    {
                        picProfile.Image = Image.FromFile(_patient.ProfilePhoto);
                    }
                    catch { }
                }

                // Load measurements
                LoadMeasurements();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Veri yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMeasurements()
        {
            var measurements = _bodyRepo.GetByPatient(_patientId);
            gridMeasurements.DataSource = measurements.Select(m => new
            {
                Tarih = m.Date.ToString("dd.MM.yyyy"),
                Göğüs = m.Chest + " cm",
                Bel = m.Waist + " cm",
                Kalça = m.Hip + " cm",
                Kol = m.Arm + " cm",
                Bacak = m.Thigh + " cm"
            }).ToList();
        }

        private void BtnUploadPhoto_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Profil Fotoğrafı Seçin";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Copy to app folder
                        string profileDir = Path.Combine(Application.StartupPath, "ProfilePhotos");
                        if (!Directory.Exists(profileDir))
                            Directory.CreateDirectory(profileDir);

                        string destPath = Path.Combine(profileDir, _patientId + Path.GetExtension(ofd.FileName));
                        File.Copy(ofd.FileName, destPath, true);

                        // Update UI
                        picProfile.Image = Image.FromFile(destPath);
                        _patient.ProfilePhoto = destPath;

                        XtraMessageBox.Show("Fotoğraf yüklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Fotoğraf yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtAdSoyad.Text))
                {
                    XtraMessageBox.Show("Ad Soyad boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _patient.AdSoyad = txtAdSoyad.Text.Trim();
                _patient.Yas = (int)spinYas.Value;
                _patient.Cinsiyet = cmbCinsiyet.Text;
                
                double boy;
                if (double.TryParse(txtBoy.Text, out boy))
                    _patient.Boy = boy;
                    
                double kilo;
                if (double.TryParse(txtKilo.Text, out kilo))
                {
                    // Also add weight entry
                    if (Math.Abs(_patient.GuncelKilo - kilo) > 0.1)
                    {
                        var entry = new WeightEntry
                        {
                            PatientId = _patientId,
                            Date = DateTime.Now,
                            Weight = kilo,
                            Notes = "Profil güncellemesi"
                        };
                        _weightRepo.Add(entry);
                    }
                    _patient.GuncelKilo = kilo;
                }
                
                _patient.Notlar = txtNotlar.Text;

                // Update Patients table (ProfilePhoto dahil)
                bool patientUpdated = _patientRepo.Update(_patient);
                
                if (!patientUpdated)
                {
                    XtraMessageBox.Show("Hasta bilgileri kaydedilemedi!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // CRITICAL: Also update Users table for AdSoyad and ProfilePhoto
                var user = _userRepo.GetById(_patientId);
                if (user != null)
                {
                    user.AdSoyad = _patient.AdSoyad;
                    // ProfilePhoto'yu güncelle (null değilse veya boş değilse)
                    if (_patient.ProfilePhoto != null)
                    {
                        user.ProfilePhoto = _patient.ProfilePhoto;
                    }
                    bool userUpdated = _userRepo.Update(user);
                    
                    if (!userUpdated)
                    {
                        XtraMessageBox.Show("Kullanıcı bilgileri kaydedilemedi!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    XtraMessageBox.Show("Kullanıcı bilgisi bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                XtraMessageBox.Show("Bilgileriniz başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // UI'ı yenile
                LoadPatientData();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Kayıt hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddMeasurement_Click(object sender, EventArgs e)
        {
            using (var form = new XtraForm())
            {
                form.Text = "Yeni Vücut Ölçümü";
                form.Size = new Size(400, 350);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = BackgroundColor;

                int y = 20;
                int leftMargin = 20;

                var lblTitle = new LabelControl
                {
                    Text = "📏 Yeni Ölçüm Girin",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = PrimaryColor,
                    Location = new Point(leftMargin, y)
                };
                form.Controls.Add(lblTitle);
                y += 40;

                var measurements = new[] { "Göğüs", "Bel", "Kalça", "Kol", "Bacak", "Boyun" };
                var inputs = new TextEdit[measurements.Length];

                for (int i = 0; i < measurements.Length; i++)
                {
                    var lbl = new LabelControl
                    {
                        Text = measurements[i] + " (cm):",
                        Location = new Point(leftMargin, y + 3),
                        Font = new Font("Segoe UI", 10F)
                    };
                    form.Controls.Add(lbl);

                    inputs[i] = new TextEdit
                    {
                        Location = new Point(leftMargin + 120, y),
                        Size = new Size(80, 28)
                    };
                    form.Controls.Add(inputs[i]);
                    y += 35;
                }

                var btnSaveMeasurement = new SimpleButton
                {
                    Text = "💾 Kaydet",
                    Location = new Point(leftMargin, y + 10),
                    Size = new Size(120, 40),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                btnSaveMeasurement.Appearance.BackColor = SuccessColor;
                btnSaveMeasurement.Appearance.ForeColor = Color.White;
                btnSaveMeasurement.Appearance.Options.UseBackColor = true;
                btnSaveMeasurement.Appearance.Options.UseForeColor = true;
                btnSaveMeasurement.Click += (s, args) =>
                {
                    try
                    {
                        var m = new BodyMeasurement
                        {
                            PatientId = _patientId,
                            Date = DateTime.Now
                        };

                        double val;
                        if (double.TryParse(inputs[0].Text, out val)) m.Chest = val;
                        if (double.TryParse(inputs[1].Text, out val)) m.Waist = val;
                        if (double.TryParse(inputs[2].Text, out val)) m.Hip = val;
                        if (double.TryParse(inputs[3].Text, out val)) m.Arm = val;
                        if (double.TryParse(inputs[4].Text, out val)) m.Thigh = val;
                        if (double.TryParse(inputs[5].Text, out val)) m.Neck = val;

                        _bodyRepo.Add(m);
                        LoadMeasurements();
                        form.Close();
                        XtraMessageBox.Show("Ölçüm kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                form.Controls.Add(btnSaveMeasurement);

                form.ShowDialog(this);
            }
        }
    }
}
