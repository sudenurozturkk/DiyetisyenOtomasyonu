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
using PatientEntity = DiyetisyenOtomasyonu.Domain.Patient;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    public partial class FrmPatients : XtraForm
    {
        private readonly PatientRepository _patientRepository;
        private readonly PatientService _patientService;
        private List<PatientEntity> _patients;
        private List<PatientEntity> _filteredPatients;
        private PatientEntity _selectedPatient;

        // Controls
        private TextEdit txtAdSoyad, txtKullaniciAdi, txtSifre, txtSearch;
        private MemoEdit txtKronikHastalik, txtIlaclar, txtAlerjiler;
        private ComboBoxEdit cmbCinsiyet, cmbLifestyle, cmbActivity;
        private SpinEdit spnYas, spnBoy, spnKilo;
        private SimpleButton btnKaydet, btnTemizle, btnYenile, btnSil;
        private Panel pnlBMI, pnlKalori, pnlPatientList;
        private Label lblToplam;

        // Colors
        private readonly Color Primary = Color.FromArgb(16, 185, 129);
        private readonly Color PrimaryDark = Color.FromArgb(5, 150, 105);
        private readonly Color PrimaryLight = Color.FromArgb(220, 252, 231);
        private readonly Color Secondary = Color.FromArgb(99, 102, 241);
        private readonly Color Accent = Color.FromArgb(251, 146, 60);
        private readonly Color Success = Color.FromArgb(34, 197, 94);
        private readonly Color Danger = Color.FromArgb(239, 68, 68);
        private readonly Color Warning = Color.FromArgb(251, 191, 36);
        private readonly Color Info = Color.FromArgb(59, 130, 246);
        private readonly Color Pink = Color.FromArgb(236, 72, 153);
        private readonly Color Purple = Color.FromArgb(139, 92, 246);
        private readonly Color Cyan = Color.FromArgb(6, 182, 212);
        private readonly Color CardBg = Color.White;
        private readonly Color BgColor = Color.FromArgb(248, 250, 252);
        private readonly Color TextDark = Color.FromArgb(30, 41, 59);
        private readonly Color TextGray = Color.FromArgb(100, 116, 139);
        private readonly Color Border = Color.FromArgb(226, 232, 240);

        public FrmPatients()
        {
            _patientRepository = new PatientRepository();
            _patientService = new PatientService();
            InitializeComponent();
            BuildUI();
            this.Load += (s, e) => LoadPatients();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1300, 800);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BgColor;
            this.ResumeLayout(false);
        }

        private void BuildUI()
        {
            // Main layout
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15),
                BackColor = Color.Transparent
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            main.Controls.Add(BuildLeftPanel(), 0, 0);
            main.Controls.Add(BuildRightPanel(), 1, 0);

            this.Controls.Add(main);
        }

        private Panel BuildLeftPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardBg, Margin = new Padding(0, 0, 8, 0) };
            panel.Paint += (s, e) => PaintCard(e.Graphics, panel);

            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = CardBg, AutoScroll = true };

            int y = 0;

            // Title
            var title = new Label
            {
                Text = "➕ YENİ HASTA KAYDI",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Primary,
                Location = new Point(0, y),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            content.Controls.Add(title);
            y += 45;

            // Fields
            AddLabel(content, "Ad Soyad *", y); y += 22;
            txtAdSoyad = AddTextBox(content, "Hasta adı soyadı", y); y += 45;

            AddLabel(content, "Kullanıcı Adı *", y); y += 22;
            txtKullaniciAdi = AddTextBox(content, "kullaniciadi", y); y += 45;

            AddLabel(content, "Şifre *", y); y += 22;
            txtSifre = AddTextBox(content, "••••••••", y, true); y += 45;

            // Cinsiyet + Yaş
            AddLabel(content, "Cinsiyet", y);
            content.Controls.Add(new Label { Text = "Yaş", Font = new Font("Segoe UI", 9), ForeColor = TextGray, Location = new Point(175, y), AutoSize = true, BackColor = Color.Transparent });
            y += 22;
            cmbCinsiyet = AddCombo(content, new[] { "Erkek", "Kadın" }, 0, y, 160);
            spnYas = AddSpin(content, 1, 120, 30, 175, y, 160);
            y += 45;

            // Boy + Kilo
            AddLabel(content, "Boy (cm)", y);
            content.Controls.Add(new Label { Text = "Kilo (kg)", Font = new Font("Segoe UI", 9), ForeColor = TextGray, Location = new Point(175, y), AutoSize = true, BackColor = Color.Transparent });
            y += 22;
            spnBoy = AddSpin(content, 100, 220, 170, 0, y, 160);
            spnKilo = AddSpin(content, 30, 250, 70, 175, y, 160);
            y += 45;

            // Lifestyle + Activity
            AddLabel(content, "Yaşam Tarzı", y);
            content.Controls.Add(new Label { Text = "Aktivite", Font = new Font("Segoe UI", 9), ForeColor = TextGray, Location = new Point(175, y), AutoSize = true, BackColor = Color.Transparent });
            y += 22;
            cmbLifestyle = AddCombo(content, new[] { "Ofis Çalışanı", "Öğrenci", "Ev Hanımı", "Serbest", "Sporcu", "Emekli" }, 0, y, 160);
            cmbActivity = AddCombo(content, new[] { "Hareketsiz", "Az Hareketli", "Orta", "Aktif", "Çok Aktif" }, 2, y, 160);
            cmbActivity.Location = new Point(175, y);
            y += 45;

            // Kronik Hastalıklar
            AddLabel(content, "Kronik Hastalıklar", y); y += 22;
            txtKronikHastalik = new MemoEdit { Location = new Point(0, y), Size = new Size(335, 50) };
            txtKronikHastalik.Properties.NullValuePrompt = "Diyabet, tansiyon, kalp vb.";
            content.Controls.Add(txtKronikHastalik);
            y += 55;

            // Kullanılan İlaçlar
            AddLabel(content, "Kullanılan İlaçlar", y); y += 22;
            txtIlaclar = new MemoEdit { Location = new Point(0, y), Size = new Size(335, 50) };
            txtIlaclar.Properties.NullValuePrompt = "Metformin, Aspirin vb.";
            content.Controls.Add(txtIlaclar);
            y += 55;

            // Alerjiler
            AddLabel(content, "Alerjiler", y); y += 22;
            txtAlerjiler = new MemoEdit { Location = new Point(0, y), Size = new Size(335, 50) };
            txtAlerjiler.Properties.NullValuePrompt = "Fıstık, gluten, laktoz vb.";
            content.Controls.Add(txtAlerjiler);
            y += 60;

            // BMI + Kalori cards
            pnlBMI = CreateCard("BMI", "24.2", "Normal", Primary, 0, y);
            content.Controls.Add(pnlBMI);
            pnlKalori = CreateCard("KALORİ", "2000", "kcal/gün", Accent, 175, y);
            content.Controls.Add(pnlKalori);
            y += 100;

            // Buttons
            btnKaydet = CreateBtn("💾 KAYDET", Primary, 0, y, 160);
            btnKaydet.Click += BtnKaydet_Click;
            content.Controls.Add(btnKaydet);

            btnTemizle = CreateBtn("🔄 TEMİZLE", Secondary, 170, y, 160);
            btnTemizle.Click += (s, e) => ClearForm();
            content.Controls.Add(btnTemizle);

            // Events
            cmbCinsiyet.SelectedIndexChanged += (s, e) => CalcMetrics();
            spnYas.EditValueChanged += (s, e) => CalcMetrics();
            spnBoy.EditValueChanged += (s, e) => CalcMetrics();
            spnKilo.EditValueChanged += (s, e) => CalcMetrics();
            cmbActivity.SelectedIndexChanged += (s, e) => CalcMetrics();

            panel.Controls.Add(content);
            CalcMetrics();
            return panel;
        }

        private Panel BuildRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardBg, Margin = new Padding(8, 0, 0, 0) };
            panel.Paint += (s, e) => PaintCard(e.Graphics, panel);

            // Patient list - ÖNCE EKLE (Fill olduğu için en son eklenmeli ama önce tanımla)
            pnlPatientList = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = CardBg };

            // Column headers
            var headers = new Panel { Dock = DockStyle.Top, Height = 35, BackColor = Color.FromArgb(248, 250, 252) };
            headers.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Border), 0, 34, headers.Width, 34);
            
            int[] cols = { 15, 180, 290, 360, 410, 470 };
            string[] titles = { "HASTA", "KULLANICI", "CİNSİYET", "YAŞ", "KİLO", "BMI" };
            for (int i = 0; i < titles.Length; i++)
            {
                headers.Controls.Add(new Label
                {
                    Text = titles[i],
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = TextGray,
                    Location = new Point(cols[i], 10),
                    AutoSize = true,
                    BackColor = Color.Transparent
                });
            }

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = CardBg, Padding = new Padding(15, 8, 15, 8) };

            lblToplam = new Label
            {
                Text = "Toplam: 0 hasta",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbar.Controls.Add(lblToplam);

            txtSearch = new TextEdit { Location = new Point(180, 10), Size = new Size(200, 30) };
            txtSearch.Properties.NullValuePrompt = "🔍 Ara...";
            txtSearch.EditValueChanged += (s, e) => FilterPatients();
            toolbar.Controls.Add(txtSearch);

            btnYenile = CreateBtn("🔄", Info, 395, 8, 40);
            btnYenile.Click += (s, e) => LoadPatients();
            toolbar.Controls.Add(btnYenile);

            btnSil = CreateBtn("🗑️", Danger, 445, 8, 40);
            btnSil.Click += BtnSil_Click;
            toolbar.Controls.Add(btnSil);

            // Title bar
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Primary };
            titleBar.Controls.Add(new Label
            {
                Text = "📋 HASTA LİSTESİ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            });

            // EKLEME SIRASI ÖNEMLİ - Dock.Fill en son eklenmeli
            panel.Controls.Add(pnlPatientList);  // Fill - en son
            panel.Controls.Add(headers);          // Top
            panel.Controls.Add(toolbar);          // Top
            panel.Controls.Add(titleBar);         // Top - en üstte

            return panel;
        }

        private void LoadPatients()
        {
            try
            {
                _patients = _patientRepository.GetByDoctorId(AuthContext.UserId).ToList();
                _filteredPatients = new List<PatientEntity>(_patients);
                lblToplam.Text = $"Toplam: {_patients.Count} hasta";
                RenderList();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hata: {ex.Message}", "Veritabanı Hatası");
            }
        }

        private void FilterPatients()
        {
            var s = txtSearch.Text?.Trim().ToLower() ?? "";
            _filteredPatients = string.IsNullOrEmpty(s) ? new List<PatientEntity>(_patients) :
                _patients.Where(p => (p.AdSoyad?.ToLower().Contains(s) ?? false) || (p.KullaniciAdi?.ToLower().Contains(s) ?? false) || p.Id.ToString().Contains(s)).ToList();
            RenderList();
        }

        private void RenderList()
        {
            pnlPatientList.Controls.Clear();

            if (_filteredPatients == null || _filteredPatients.Count == 0)
            {
                pnlPatientList.Controls.Add(new Label { Text = "Hasta bulunamadı", Font = new Font("Segoe UI", 11), ForeColor = TextGray, Location = new Point(180, 50), AutoSize = true });
                return;
            }

            int y = 5;
            foreach (var p in _filteredPatients)
            {
                var row = CreateRow(p, y);
                pnlPatientList.Controls.Add(row);
                y += 55;
            }
        }

        private Panel CreateRow(PatientEntity p, int y)
        {
            bool sel = _selectedPatient?.Id == p.Id;
            var color = GetColor(p.Id);

            var row = new Panel
            {
                Location = new Point(5, y),
                Size = new Size(pnlPatientList.Width - 25, 50),
                BackColor = sel ? PrimaryLight : CardBg,
                Cursor = Cursors.Hand,
                Tag = p
            };
            row.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(sel ? Primary : Border, sel ? 2 : 1))
                using (var path = RRect(new Rectangle(0, 0, row.Width - 1, row.Height - 1), 8))
                    e.Graphics.DrawPath(pen, path);
            };

            // Avatar
            var av = new Panel { Location = new Point(10, 5), Size = new Size(40, 40) };
            av.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(color), 0, 0, 39, 39);
                var init = GetInit(p.AdSoyad);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(init, new Font("Segoe UI", 11, FontStyle.Bold), Brushes.White, new RectangleF(0, 0, 40, 40), sf);
            };
            row.Controls.Add(av);

            // Name
            row.Controls.Add(new Label { Text = p.AdSoyad ?? "-", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = TextDark, Location = new Point(58, 7), AutoSize = true, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = $"#{p.Id}", Font = new Font("Segoe UI", 8), ForeColor = TextGray, Location = new Point(58, 26), AutoSize = true, BackColor = Color.Transparent });

            // Username
            row.Controls.Add(new Label { Text = $"@{p.KullaniciAdi ?? "?"}", Font = new Font("Segoe UI", 9), ForeColor = TextGray, Location = new Point(175, 16), AutoSize = true, BackColor = Color.Transparent });

            // Gender
            bool m = p.Cinsiyet == "Erkek";
            var badge = new Panel { Location = new Point(285, 13), Size = new Size(55, 24) };
            badge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var c = m ? Info : Pink;
                using (var brush = new SolidBrush(Color.FromArgb(40, c)))
                using (var path = RRect(new Rectangle(0, 0, 54, 23), 10))
                    e.Graphics.FillPath(brush, path);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(m ? "Erkek" : "Kadın", new Font("Segoe UI", 8, FontStyle.Bold), new SolidBrush(c), badge.ClientRectangle, sf);
            };
            row.Controls.Add(badge);

            // Age
            row.Controls.Add(new Label { Text = p.Yas.ToString(), Font = new Font("Segoe UI", 10), ForeColor = TextDark, Location = new Point(355, 16), AutoSize = true, BackColor = Color.Transparent });

            // Weight
            row.Controls.Add(new Label { Text = $"{p.GuncelKilo:F0}kg", Font = new Font("Segoe UI", 10), ForeColor = TextDark, Location = new Point(405, 16), AutoSize = true, BackColor = Color.Transparent });

            // BMI
            double bmi = p.BMI;
            Color bc = bmi < 18.5 ? Info : bmi < 25 ? Success : bmi < 30 ? Warning : Danger;
            row.Controls.Add(new Label { Text = $"{bmi:F1}", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = bc, Location = new Point(465, 16), AutoSize = true, BackColor = Color.Transparent });

            // Events
            EventHandler click = (s, e) => Select(p);
            row.Click += click;
            foreach (Control c in row.Controls) { c.Click += click; c.Cursor = Cursors.Hand; }
            row.MouseEnter += (s, e) => { if (!sel) row.BackColor = Color.FromArgb(248, 250, 252); };
            row.MouseLeave += (s, e) => { row.BackColor = (_selectedPatient?.Id == p.Id) ? PrimaryLight : CardBg; };

            return row;
        }

        private void Select(PatientEntity p)
        {
            _selectedPatient = p;
            txtAdSoyad.Text = p.AdSoyad;
            txtKullaniciAdi.Text = p.KullaniciAdi;
            txtSifre.Text = "";
            cmbCinsiyet.SelectedIndex = p.Cinsiyet == "Erkek" ? 0 : 1;
            spnYas.Value = p.Yas;
            spnBoy.Value = (decimal)p.Boy;
            spnKilo.Value = (decimal)p.GuncelKilo;
            cmbLifestyle.SelectedIndex = Math.Min((int)p.LifestyleType, 5);
            cmbActivity.SelectedIndex = Math.Min((int)p.ActivityLevel, 4);
            txtKronikHastalik.Text = p.MedicalHistory ?? "";
            txtIlaclar.Text = p.Medications ?? "";
            txtAlerjiler.Text = p.AllergiesText ?? "";
            btnKaydet.Text = "💾 GÜNCELLE";
            CalcMetrics();
            RenderList();
        }

        private void ClearForm()
        {
            _selectedPatient = null;
            txtAdSoyad.Text = txtKullaniciAdi.Text = txtSifre.Text = "";
            txtKronikHastalik.Text = txtIlaclar.Text = txtAlerjiler.Text = "";
            cmbCinsiyet.SelectedIndex = 0;
            spnYas.Value = 30; spnBoy.Value = 170; spnKilo.Value = 70;
            cmbLifestyle.SelectedIndex = 0; cmbActivity.SelectedIndex = 2;
            btnKaydet.Text = "💾 KAYDET";
            CalcMetrics();
            RenderList();
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text) || string.IsNullOrWhiteSpace(txtKullaniciAdi.Text))
            {
                XtraMessageBox.Show("Ad Soyad ve Kullanıcı Adı zorunlu!", "Uyarı");
                return;
            }

            try
            {
                if (_selectedPatient == null)
                {
                    if (string.IsNullOrWhiteSpace(txtSifre.Text) || txtSifre.Text.Length < 6)
                    {
                        XtraMessageBox.Show("Şifre en az 6 karakter!", "Uyarı");
                        return;
                    }

                    var np = new PatientEntity
                    {
                        AdSoyad = txtAdSoyad.Text.Trim(),
                        KullaniciAdi = txtKullaniciAdi.Text.Trim().ToLower(),
                        ParolaHash = PasswordHasher.HashPassword(txtSifre.Text),
                        Role = UserRole.Patient,
                        DoctorId = AuthContext.UserId,
                        Cinsiyet = cmbCinsiyet.SelectedIndex == 0 ? "Erkek" : "Kadın",
                        Yas = (int)spnYas.Value,
                        Boy = (double)spnBoy.Value,
                        BaslangicKilosu = (double)spnKilo.Value,
                        GuncelKilo = (double)spnKilo.Value,
                        LifestyleType = (LifestyleType)cmbLifestyle.SelectedIndex,
                        ActivityLevel = (ActivityLevel)cmbActivity.SelectedIndex,
                        MedicalHistory = txtKronikHastalik.Text?.Trim(),
                        Medications = txtIlaclar.Text?.Trim(),
                        AllergiesText = txtAlerjiler.Text?.Trim(),
                        KayitTarihi = DateTime.Now,
                        AktifMi = true
                    };
                    _patientService.AddPatient(np);
                    XtraMessageBox.Show("Hasta kaydedildi!", "Başarılı");
                }
                else
                {
                    _selectedPatient.AdSoyad = txtAdSoyad.Text.Trim();
                    _selectedPatient.KullaniciAdi = txtKullaniciAdi.Text.Trim().ToLower();
                    _selectedPatient.Cinsiyet = cmbCinsiyet.SelectedIndex == 0 ? "Erkek" : "Kadın";
                    _selectedPatient.Yas = (int)spnYas.Value;
                    _selectedPatient.Boy = (double)spnBoy.Value;
                    _selectedPatient.GuncelKilo = (double)spnKilo.Value;
                    _selectedPatient.LifestyleType = (LifestyleType)cmbLifestyle.SelectedIndex;
                    _selectedPatient.ActivityLevel = (ActivityLevel)cmbActivity.SelectedIndex;
                    _selectedPatient.MedicalHistory = txtKronikHastalik.Text?.Trim();
                    _selectedPatient.Medications = txtIlaclar.Text?.Trim();
                    _selectedPatient.AllergiesText = txtAlerjiler.Text?.Trim();
                    if (!string.IsNullOrWhiteSpace(txtSifre.Text))
                        _selectedPatient.ParolaHash = PasswordHasher.HashPassword(txtSifre.Text);
                    _patientService.UpdatePatient(_selectedPatient);
                    XtraMessageBox.Show("Hasta güncellendi!", "Başarılı");
                }
                LoadPatients();
                ClearForm();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata");
            }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (_selectedPatient == null) { XtraMessageBox.Show("Önce hasta seçin!", "Uyarı"); return; }
            if (XtraMessageBox.Show($"{_selectedPatient.AdSoyad} silinsin mi?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _patientService.DeletePatient(_selectedPatient.Id);
                    XtraMessageBox.Show("Silindi!", "Başarılı");
                    LoadPatients();
                    ClearForm();
                }
                catch (Exception ex) { XtraMessageBox.Show($"Hata: {ex.Message}", "Hata"); }
            }
        }

        private void CalcMetrics()
        {
            try
            {
                double h = (double)spnBoy.Value / 100;
                double w = (double)spnKilo.Value;
                double bmi = w / (h * h);
                string st; Color c;
                if (bmi < 18.5) { st = "Zayıf"; c = Info; }
                else if (bmi < 25) { st = "Normal"; c = Success; }
                else if (bmi < 30) { st = "Kilolu"; c = Warning; }
                else { st = "Obez"; c = Danger; }
                UpdateCard(pnlBMI, $"{bmi:F1}", st, c);

                int age = (int)spnYas.Value;
                bool m = cmbCinsiyet.SelectedIndex == 0;
                double bmr = m ? 88.362 + 13.397 * w + 4.799 * (double)spnBoy.Value - 5.677 * age
                              : 447.593 + 9.247 * w + 3.098 * (double)spnBoy.Value - 4.330 * age;
                double[] f = { 1.2, 1.375, 1.55, 1.725, 1.9 };
                double tdee = bmr * f[Math.Min(cmbActivity.SelectedIndex, 4)];
                UpdateCard(pnlKalori, $"{tdee:N0}", "kcal/gün", Accent);
            }
            catch { }
        }

        #region Helpers
        private void AddLabel(Panel p, string t, int y)
        {
            // Zorunlu alanlar için kırmızı, diğerleri için canlı renkler
            Color labelColor;
            if (t.Contains("*"))
            {
                labelColor = Color.FromArgb(220, 53, 69); // Kırmızı - zorunlu alan
            }
            else if (t.Contains("Ad Soyad") || t.Contains("Kullanıcı") || t.Contains("Şifre"))
            {
                labelColor = Primary; // Yeşil - önemli alanlar
            }
            else if (t.Contains("Boy") || t.Contains("Kilo") || t.Contains("Yaş"))
            {
                labelColor = Info; // Mavi - fiziksel özellikler
            }
            else if (t.Contains("Yaşam") || t.Contains("Aktivite"))
            {
                labelColor = Secondary; // Mor - yaşam tarzı
            }
            else if (t.Contains("Kronik") || t.Contains("İlaç") || t.Contains("Alerji"))
            {
                labelColor = Warning; // Turuncu - sağlık bilgileri
            }
            else
            {
                labelColor = Color.FromArgb(99, 102, 241); // Mor - varsayılan
            }
            
            var lbl = new Label 
            { 
                Text = t, 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), 
                ForeColor = labelColor, 
                Location = new Point(0, y), 
                AutoSize = true, 
                BackColor = Color.Transparent 
            };
            p.Controls.Add(lbl);
        }
        private TextEdit AddTextBox(Panel p, string ph, int y, bool pwd = false)
        {
            var t = new TextEdit { Location = new Point(0, y), Size = new Size(335, 32) };
            t.Properties.NullValuePrompt = ph;
            if (pwd) t.Properties.UseSystemPasswordChar = true;
            p.Controls.Add(t);
            return t;
        }
        private ComboBoxEdit AddCombo(Panel p, string[] items, int sel, int y, int w)
        {
            var c = new ComboBoxEdit { Location = new Point(0, y), Size = new Size(w, 32) };
            c.Properties.Items.AddRange(items);
            c.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            c.SelectedIndex = sel;
            p.Controls.Add(c);
            return c;
        }
        private SpinEdit AddSpin(Panel p, decimal min, decimal max, decimal val, int x, int y, int w)
        {
            var s = new SpinEdit { Location = new Point(x, y), Size = new Size(w, 32) };
            s.Properties.MinValue = min; s.Properties.MaxValue = max; s.Value = val;
            p.Controls.Add(s);
            return s;
        }
        private Panel CreateCard(string title, string val, string st, Color c, int x, int y)
        {
            var p = new Panel { Location = new Point(x, y), Size = new Size(160, 90) };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(25, c)))
                using (var path = RRect(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10))
                    e.Graphics.FillPath(brush, path);
            };
            p.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = c, Location = new Point(12, 8), AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(new Label { Text = val, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = c, Location = new Point(12, 28), AutoSize = true, BackColor = Color.Transparent, Tag = "v" });
            p.Controls.Add(new Label { Text = st, Font = new Font("Segoe UI", 9), ForeColor = c, Location = new Point(12, 60), AutoSize = true, BackColor = Color.Transparent, Tag = "s" });
            return p;
        }
        private void UpdateCard(Panel p, string v, string s, Color c)
        {
            foreach (Control ctrl in p.Controls) { ctrl.ForeColor = c; if (ctrl.Tag?.ToString() == "v") ctrl.Text = v; if (ctrl.Tag?.ToString() == "s") ctrl.Text = s; }
            p.Invalidate();
        }
        private SimpleButton CreateBtn(string t, Color c, int x, int y, int w) => new SimpleButton { Text = t, Location = new Point(x, y), Size = new Size(w, 38), Font = new Font("Segoe UI", 10, FontStyle.Bold), Appearance = { BackColor = c, ForeColor = Color.White } };
        private void PaintCard(Graphics g, Panel p)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RRect(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 12))
            {
                g.FillPath(new SolidBrush(CardBg), path);
                g.DrawPath(new Pen(Border), path);
            }
        }
        private GraphicsPath RRect(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad, rad, 180, 90);
            p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90);
            p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90);
            p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure();
            return p;
        }
        private string GetInit(string n) => string.IsNullOrEmpty(n) ? "?" : n.Split(' ').Length >= 2 ? $"{n.Split(' ')[0][0]}{n.Split(' ')[1][0]}".ToUpper() : n.Substring(0, Math.Min(2, n.Length)).ToUpper();
        private Color GetColor(int id) => new[] { Primary, Secondary, Accent, Success, Pink, Purple, Cyan, Info }[Math.Abs(id) % 8];
        #endregion
    }
}
