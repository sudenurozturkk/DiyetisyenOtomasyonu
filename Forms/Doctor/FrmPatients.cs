using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;
using PatientEntity = DiyetisyenOtomasyonu.Domain.Patient;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Hasta Yönetimi - Professional Modern Tasarım
    /// Sol: Hasta Listesi | Sağ: Kayıt Formu
    /// </summary>
    public partial class FrmPatients : XtraForm
    {
        private readonly PatientService _patientService;
        private List<PatientEntity> _patients;
        private PatientEntity _selectedPatient;

        #region Colors
        private readonly Color PrimaryGreen = ColorTranslator.FromHtml("#0D9488");
        private readonly Color DarkGreen = ColorTranslator.FromHtml("#0F766E");
        private readonly Color LightGreen = ColorTranslator.FromHtml("#CCFBF1");
        private readonly Color SuccessGreen = ColorTranslator.FromHtml("#22C55E");
        private readonly Color SuccessBg = ColorTranslator.FromHtml("#DCFCE7");
        private readonly Color DangerRed = ColorTranslator.FromHtml("#EF4444");
        private readonly Color DangerBg = ColorTranslator.FromHtml("#FEE2E2");
        private readonly Color WarningOrange = ColorTranslator.FromHtml("#F97316");
        private readonly Color WarningBg = ColorTranslator.FromHtml("#FED7AA");
        private readonly Color WarningYellow = ColorTranslator.FromHtml("#EAB308");
        private readonly Color WarningYellowBg = ColorTranslator.FromHtml("#FEF9C3");
        private readonly Color PurpleColor = ColorTranslator.FromHtml("#9333EA");
        private readonly Color BackgroundLight = ColorTranslator.FromHtml("#F8FAFC");
        private readonly Color CardWhite = Color.White;
        private readonly Color BorderGray = ColorTranslator.FromHtml("#E2E8F0");
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextMedium = ColorTranslator.FromHtml("#64748B");
        private readonly Color TextLight = ColorTranslator.FromHtml("#94A3B8");
        #endregion

        #region Controls
        private Panel pnlPatientList;
        private TextEdit txtSearch;
        private XtraTabControl tabControl;
        private TextEdit txtAdSoyad, txtKullaniciAdi, txtParola;
        private RadioButton rbErkek, rbKadin;
        private SpinEdit spnYas, spnBoy, spnKilo, spnHedefKilo;
        private ComboBoxEdit cmbLifestyle, cmbActivity;
        private Label lblTotalCount, lblOverweightCount, lblNormalCount, lblRiskCount;
        private Label lblTdeeValue, lblTdeeHedef;
        #endregion

        public FrmPatients()
        {
            InitializeComponent();
            _patientService = new PatientService();
            SetupUI();
            LoadPatients();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1200, 750);
            this.Name = "FrmPatients";
            this.Text = "Hasta Yönetimi";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BackgroundLight;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.Padding = new Padding(25);

            // Header
            CreateHeader();

            // Summary Cards
            CreateSummaryCards();

            // Toolbar
            CreateToolbar();

            // Main Content
            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel2,
                SplitterDistance = 600,
                SplitterWidth = 25,
                BackColor = BackgroundLight
            };
            mainSplit.Panel1.Padding = new Padding(0, 0, 10, 0);
            mainSplit.Panel2.Padding = new Padding(10, 0, 0, 0);

            CreatePatientListPanel(mainSplit.Panel1);
            CreateRightFormPanel(mainSplit.Panel2);

            this.Controls.Add(mainSplit);
        }

        private void CreateHeader()
        {
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = CardWhite, Padding = new Padding(20) };
            pnlHeader.Paint += (s, e) => DrawRoundedBorder(e.Graphics, pnlHeader.Width, pnlHeader.Height, 15);

            var lblIcon = new Label { Text = "👥", Font = new Font("Segoe UI", 24F), Location = new Point(20, 15), AutoSize = true };
            pnlHeader.Controls.Add(lblIcon);

            var lblTitle = new Label { Text = "Hasta Yönetimi", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = PrimaryGreen, Location = new Point(70, 12), AutoSize = true };
            pnlHeader.Controls.Add(lblTitle);

            var lblSubtitle = new Label { Text = "Hastalarınızı yönetin, yeni kayıt ekleyin ve bilgilerini güncelleyin.", Font = new Font("Segoe UI", 10F), ForeColor = TextMedium, Location = new Point(70, 42), AutoSize = true };
            pnlHeader.Controls.Add(lblSubtitle);

            this.Controls.Add(pnlHeader);
        }

        private void CreateSummaryCards()
        {
            var pnlCards = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                ColumnCount = 4,
                Padding = new Padding(0, 15, 0, 15)
            };
            for (int i = 0; i < 4; i++) pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            var c1 = CreateSummaryCard("Toplam Hasta", "0", "👥", LightGreen, PrimaryGreen);
            lblTotalCount = (Label)c1.Controls[2];
            pnlCards.Controls.Add(c1, 0, 0);

            var c2 = CreateSummaryCard("Fazla Kilolu", "0", "⚠️", DangerBg, DangerRed);
            lblOverweightCount = (Label)c2.Controls[2];
            pnlCards.Controls.Add(c2, 1, 0);

            var c3 = CreateSummaryCard("Normal Kilo", "0", "✅", SuccessBg, SuccessGreen);
            lblNormalCount = (Label)c3.Controls[2];
            pnlCards.Controls.Add(c3, 2, 0);

            var c4 = CreateSummaryCard("Riskli (BMI>30)", "0", "⚡", WarningYellowBg, WarningYellow);
            lblRiskCount = (Label)c4.Controls[2];
            pnlCards.Controls.Add(c4, 3, 0);

            this.Controls.Add(pnlCards);
        }

        private Panel CreateSummaryCard(string title, string value, string icon, Color bg, Color fg)
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5), BackColor = CardWhite, Cursor = Cursors.Hand };
            pnl.Paint += (s, e) =>
            {
                DrawRoundedBorder(e.Graphics, pnl.Width, pnl.Height, 12);
                // Left color bar
                using (var brush = new SolidBrush(fg))
                    e.Graphics.FillRectangle(brush, 0, 10, 5, pnl.Height - 20);
            };

            var lblIcon = new Label { Text = icon, Font = new Font("Segoe UI", 18F), ForeColor = fg, Location = new Point(15, 12), AutoSize = true };
            var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9F), ForeColor = TextMedium, Location = new Point(55, 10), AutoSize = true };
            var lblValue = new Label { Text = value, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(55, 32), AutoSize = true };

            pnl.Controls.Add(lblIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);

            // Hover effect
            pnl.MouseEnter += (s, e) => pnl.BackColor = bg;
            pnl.MouseLeave += (s, e) => pnl.BackColor = CardWhite;
            foreach (Control c in pnl.Controls) { c.MouseEnter += (s, e) => pnl.BackColor = bg; c.MouseLeave += (s, e) => pnl.BackColor = CardWhite; }

            return pnl;
        }

        private void CreateToolbar()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 0, 0, 15) };

            // Search
            txtSearch = new TextEdit
            {
                Location = new Point(0, 5),
                Size = new Size(280, 40),
                Properties = { NullText = "🔍 Hasta ara (isim veya ID)...", AutoHeight = false, Appearance = { Font = new Font("Segoe UI", 10F), BackColor = CardWhite } }
            };
            txtSearch.TextChanged += (s, e) => FilterPatients();
            pnl.Controls.Add(txtSearch);

            // Buttons
            var btnRefresh = CreateToolbarButton("↻ Yenile", 300, PrimaryGreen, Color.White);
            btnRefresh.Click += (s, e) => LoadPatients();
            pnl.Controls.Add(btnRefresh);

            var btnDiet = CreateToolbarButton("📋 Diyet Ata", 400, CardWhite, TextDark);
            pnl.Controls.Add(btnDiet);

            var btnReport = CreateToolbarButton("📈 Rapor Gör", 510, CardWhite, TextDark);
            pnl.Controls.Add(btnReport);

            var btnDelete = CreateToolbarButton("🗑️ Sil", 620, DangerBg, DangerRed);
            btnDelete.Click += BtnSil_Click;
            pnl.Controls.Add(btnDelete);

            this.Controls.Add(pnl);
        }

        private SimpleButton CreateToolbarButton(string text, int x, Color bg, Color fg)
        {
            return new SimpleButton
            {
                Text = text,
                Location = new Point(x, 5),
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = bg, ForeColor = fg },
                Cursor = Cursors.Hand
            };
        }

        private void CreatePatientListPanel(Panel parent)
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(20) };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card.Width, card.Height, 20);

            // Header
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50 };
            var lblTitle = new Label { Text = "📋 HASTA LİSTESİ", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(0, 10), AutoSize = true };
            pnlHeader.Controls.Add(lblTitle);
            card.Controls.Add(pnlHeader);

            // Column Headers
            var pnlCols = new Panel { Dock = DockStyle.Top, Height = 35, BackColor = BackgroundLight };
            pnlCols.Controls.Add(new Label { Text = "HASTA BİLGİLERİ", Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = TextLight, Location = new Point(10, 10), AutoSize = true });
            pnlCols.Controls.Add(new Label { Text = "BMI", Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = TextLight, Location = new Point(pnlCols.Width - 60, 10), Anchor = AnchorStyles.Right, AutoSize = true });
            card.Controls.Add(pnlCols);

            // List
            pnlPatientList = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = CardWhite };
            card.Controls.Add(pnlPatientList);

            parent.Controls.Add(card);
        }

        private void CreateRightFormPanel(Panel parent)
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(25) };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card.Width, card.Height, 20);

            // Header
            var lblHeader = new Label { Text = "📝 Yeni Hasta Kaydı", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = PrimaryGreen, Dock = DockStyle.Top, Height = 45 };
            card.Controls.Add(lblHeader);

            // Tabs
            tabControl = new XtraTabControl { Dock = DockStyle.Fill };
            tabControl.AppearancePage.Header.Font = new Font("Segoe UI", 10F);

            var tab1 = new XtraTabPage { Text = "Kimlik & Fiziksel", BackColor = CardWhite };
            CreateTab1Content(tab1);
            tabControl.TabPages.Add(tab1);

            var tab2 = new XtraTabPage { Text = "Yaşam & Tıbbi", BackColor = CardWhite };
            CreateTab2Content(tab2);
            tabControl.TabPages.Add(tab2);

            card.Controls.Add(tabControl);

            // Save Button
            var btnSave = new SimpleButton
            {
                Text = "💾 KAYDET",
                Dock = DockStyle.Bottom,
                Height = 55,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Appearance = { BackColor = PurpleColor, ForeColor = Color.White },
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnEkle_Click;
            card.Controls.Add(btnSave);

            parent.Controls.Add(card);
        }

        private void CreateTab1Content(XtraTabPage page)
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10) };

            int y = 10;
            scroll.Controls.Add(CreateInputRow("Ad Soyad", txtAdSoyad = new TextEdit { Properties = { NullText = "Hastanın adı ve soyadı" } }, ref y));
            scroll.Controls.Add(CreateInputRow("Kullanıcı Adı", txtKullaniciAdi = new TextEdit { Properties = { NullText = "Giriş için kullanıcı adı" } }, ref y));
            scroll.Controls.Add(CreateInputRow("Parola", txtParola = new TextEdit { Properties = { UseSystemPasswordChar = true, NullText = "••••••" } }, ref y));

            // Gender
            var pnlGender = new Panel { Location = new Point(10, y), Size = new Size(350, 70) };
            pnlGender.Controls.Add(new Label { Text = "Cinsiyet", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextMedium, Location = new Point(0, 0), AutoSize = true });
            rbErkek = new RadioButton { Text = "Erkek", Location = new Point(5, 30), Checked = true, Font = new Font("Segoe UI", 10F), AutoSize = true };
            rbKadin = new RadioButton { Text = "Kadın", Location = new Point(100, 30), Font = new Font("Segoe UI", 10F), AutoSize = true };
            pnlGender.Controls.Add(rbErkek);
            pnlGender.Controls.Add(rbKadin);
            scroll.Controls.Add(pnlGender);
            y += 75;

            // Numbers
            scroll.Controls.Add(CreateNumberRow("Yaş", spnYas = new SpinEdit { Properties = { MinValue = 1, MaxValue = 120 }, Value = 30 }, ref y));
            scroll.Controls.Add(CreateNumberRow("Boy (cm)", spnBoy = new SpinEdit { Properties = { MinValue = 50, MaxValue = 250 }, Value = 170 }, ref y));
            spnBoy.EditValueChanged += (s, e) => UpdateTDEE();
            scroll.Controls.Add(CreateNumberRow("Kilo (kg)", spnKilo = new SpinEdit { Properties = { MinValue = 20, MaxValue = 300 }, Value = 70 }, ref y));
            spnKilo.EditValueChanged += (s, e) => UpdateTDEE();
            scroll.Controls.Add(CreateNumberRow("Hedef Kilo", spnHedefKilo = new SpinEdit { Properties = { MinValue = 20, MaxValue = 300 }, Value = 65 }, ref y));

            // TDEE Box
            var pnlTdee = new Panel { Location = new Point(10, y), Size = new Size(350, 70), BackColor = LightGreen };
            pnlTdee.Paint += (s, e) => DrawRoundedBorder(e.Graphics, pnlTdee.Width, pnlTdee.Height, 12, PrimaryGreen);
            lblTdeeValue = new Label { Text = "2000 kcal", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = PrimaryGreen, Location = new Point(15, 12), AutoSize = true };
            lblTdeeHedef = new Label { Text = "Günlük Kalori İhtiyacı (TDEE)", Font = new Font("Segoe UI", 9F), ForeColor = TextDark, Location = new Point(15, 42), AutoSize = true };
            pnlTdee.Controls.Add(lblTdeeValue);
            pnlTdee.Controls.Add(lblTdeeHedef);
            scroll.Controls.Add(pnlTdee);

            page.Controls.Add(scroll);
        }

        private void CreateTab2Content(XtraTabPage page)
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10) };
            int y = 10;

            cmbLifestyle = new ComboBoxEdit();
            cmbLifestyle.Properties.Items.AddRange(new[] { "Öğrenci", "Ofis Çalışanı", "Serbest Meslek", "Sporcu", "Emekli" });
            cmbLifestyle.SelectedIndex = 1;
            scroll.Controls.Add(CreateComboRow("Yaşam Tarzı", cmbLifestyle, ref y));

            cmbActivity = new ComboBoxEdit();
            cmbActivity.Properties.Items.AddRange(new[] { "Hareketsiz", "Hafif Aktif", "Orta Aktif", "Çok Aktif", "Ekstra Aktif" });
            cmbActivity.SelectedIndex = 1;
            cmbActivity.SelectedIndexChanged += (s, e) => UpdateTDEE();
            scroll.Controls.Add(CreateComboRow("Aktivite Seviyesi", cmbActivity, ref y));

            page.Controls.Add(scroll);
        }

        private Panel CreateInputRow(string label, TextEdit input, ref int y)
        {
            var pnl = new Panel { Location = new Point(10, y), Size = new Size(350, 70) };
            pnl.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextMedium, Location = new Point(0, 0), AutoSize = true });
            input.Location = new Point(0, 28);
            input.Size = new Size(340, 38);
            input.Properties.AutoHeight = false;
            input.Font = new Font("Segoe UI", 10F);
            input.Properties.Appearance.BackColor = BackgroundLight;
            pnl.Controls.Add(input);
            y += 75;
            return pnl;
        }

        private Panel CreateNumberRow(string label, SpinEdit input, ref int y)
        {
            var pnl = new Panel { Location = new Point(10, y), Size = new Size(350, 70) };
            pnl.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextMedium, Location = new Point(0, 0), AutoSize = true });
            input.Location = new Point(0, 28);
            input.Size = new Size(150, 38);
            input.Properties.AutoHeight = false;
            input.Font = new Font("Segoe UI", 10F);
            input.Properties.Appearance.BackColor = BackgroundLight;
            pnl.Controls.Add(input);
            y += 75;
            return pnl;
        }

        private Panel CreateComboRow(string label, ComboBoxEdit input, ref int y)
        {
            var pnl = new Panel { Location = new Point(10, y), Size = new Size(350, 70) };
            pnl.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextMedium, Location = new Point(0, 0), AutoSize = true });
            input.Location = new Point(0, 28);
            input.Size = new Size(340, 38);
            input.Properties.AutoHeight = false;
            input.Font = new Font("Segoe UI", 10F);
            input.Properties.Appearance.BackColor = BackgroundLight;
            pnl.Controls.Add(input);
            y += 75;
            return pnl;
        }

        #region Logic
        private void LoadPatients()
        {
            try
            {
                _patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                PopulatePatientList();
                UpdateSummaryCards();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hastalar yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterPatients()
        {
            if (_patients == null) return;
            string query = txtSearch.Text?.ToLower() ?? "";
            var filtered = _patients.Where(p => p.AdSoyad.ToLower().Contains(query) || p.Id.ToString().Contains(query)).ToList();
            PopulatePatientList(filtered);
        }

        private void PopulatePatientList(List<PatientEntity> list = null)
        {
            pnlPatientList.SuspendLayout();
            pnlPatientList.Controls.Clear();
            var patients = list ?? _patients;
            if (patients == null) return;

            foreach (var p in patients)
            {
                var row = CreatePatientRow(p);
                pnlPatientList.Controls.Add(row);
            }
            pnlPatientList.ResumeLayout(true);
        }

        private Panel CreatePatientRow(PatientEntity patient)
        {
            var wrapper = new Panel { Dock = DockStyle.Top, Height = 75, Padding = new Padding(0, 0, 0, 10) };
            var row = new Panel { Dock = DockStyle.Fill, BackColor = BackgroundLight, Cursor = Cursors.Hand };
            row.Paint += (s, e) => DrawRoundedBorder(e.Graphics, row.Width, row.Height, 12);

            // Avatar
            Color avatarColor = GetBmiColor(patient.BMI);
            string initials = GetInitials(patient.AdSoyad);
            var pnlAvatar = new Panel { Location = new Point(12, 10), Size = new Size(45, 45) };
            pnlAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(avatarColor)) e.Graphics.FillEllipse(brush, 0, 0, 44, 44);
                using (var font = new Font("Segoe UI", 11F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(initials, font, brush, new Rectangle(0, 0, 45, 45), sf);
            };
            row.Controls.Add(pnlAvatar);

            // Name
            row.Controls.Add(new Label { Text = patient.AdSoyad, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(70, 10), AutoSize = true });

            // Details
            row.Controls.Add(new Label { Text = $"{patient.Yas} yaş | {patient.Boy} cm | {patient.GuncelKilo} kg", Font = new Font("Segoe UI", 9F), ForeColor = TextMedium, Location = new Point(70, 32), AutoSize = true });

            // BMI
            row.Controls.Add(new Label { Text = $"{patient.BMI:F1}", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = avatarColor, Location = new Point(row.Width - 70, 18), Anchor = AnchorStyles.Right, AutoSize = true });

            // Selection
            bool isSelected = _selectedPatient != null && _selectedPatient.Id == patient.Id;
            if (isSelected) row.BackColor = LightGreen;

            row.Click += (s, e) => SelectPatient(patient);
            foreach (Control c in row.Controls) c.Click += (s, e) => SelectPatient(patient);

            wrapper.Controls.Add(row);
            return wrapper;
        }

        private void SelectPatient(PatientEntity patient)
        {
            _selectedPatient = patient;
            PopulatePatientList();

            txtAdSoyad.Text = patient.AdSoyad;
            txtKullaniciAdi.Text = patient.KullaniciAdi;
            txtParola.Text = "";
            rbErkek.Checked = patient.Cinsiyet == "Erkek";
            rbKadin.Checked = patient.Cinsiyet == "Kadın";
            spnYas.Value = patient.Yas;
            spnBoy.Value = (decimal)patient.Boy;
            spnKilo.Value = (decimal)patient.GuncelKilo;

            SetComboValue(cmbLifestyle, patient.LifestyleDescription);
            SetComboValue(cmbActivity, patient.ActivityDescription);
            UpdateTDEE();
        }

        private void SetComboValue(ComboBoxEdit cmb, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (var item in cmb.Properties.Items)
                if (item.ToString() == value) { cmb.SelectedItem = item; return; }
        }

        private void UpdateSummaryCards()
        {
            if (_patients == null) return;
            lblTotalCount.Text = _patients.Count.ToString();
            lblOverweightCount.Text = _patients.Count(p => p.BMI >= 25 && p.BMI < 30).ToString();
            lblNormalCount.Text = _patients.Count(p => p.BMI >= 18.5 && p.BMI < 25).ToString();
            lblRiskCount.Text = _patients.Count(p => p.BMI >= 30).ToString();
        }

        private void UpdateTDEE()
        {
            double weight = (double)spnKilo.Value;
            double height = (double)spnBoy.Value;
            double age = (double)spnYas.Value;
            double bmr = 10 * weight + 6.25 * height - 5 * age + (rbErkek.Checked ? 5 : -161);
            double[] multipliers = { 1.2, 1.375, 1.55, 1.725, 1.9 };
            int idx = cmbActivity?.SelectedIndex ?? 1;
            double tdee = bmr * multipliers[Math.Min(idx, multipliers.Length - 1)];
            if (lblTdeeValue != null) lblTdeeValue.Text = $"{(int)tdee} kcal";
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text) || string.IsNullOrWhiteSpace(txtKullaniciAdi.Text))
            {
                XtraMessageBox.Show("Lütfen Ad Soyad ve Kullanıcı Adı alanlarını doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var patient = new PatientEntity
                {
                    AdSoyad = txtAdSoyad.Text.Trim(),
                    KullaniciAdi = txtKullaniciAdi.Text.Trim(),
                    ParolaHash = string.IsNullOrEmpty(txtParola.Text) ? "123456" : txtParola.Text,
                    Cinsiyet = rbErkek.Checked ? "Erkek" : "Kadın",
                    Yas = (int)spnYas.Value,
                    Boy = (double)spnBoy.Value,
                    GuncelKilo = (double)spnKilo.Value,
                    BaslangicKilosu = (double)spnKilo.Value,
                    DoctorId = AuthContext.UserId
                };

                _patientService.AddPatient(patient);
                XtraMessageBox.Show("Hasta başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPatients();
                ClearForm();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (_selectedPatient == null) { XtraMessageBox.Show("Lütfen silmek için bir hasta seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (XtraMessageBox.Show($"{_selectedPatient.AdSoyad} hastasını silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _patientService.DeletePatient(_selectedPatient.Id);
                LoadPatients();
                ClearForm();
            }
        }

        private void ClearForm()
        {
            _selectedPatient = null;
            txtAdSoyad.Text = txtKullaniciAdi.Text = txtParola.Text = "";
            spnYas.Value = 30; spnBoy.Value = 170; spnKilo.Value = 70; spnHedefKilo.Value = 65;
            rbErkek.Checked = true;
            cmbLifestyle.SelectedIndex = 1; cmbActivity.SelectedIndex = 1;
            PopulatePatientList();
        }

        private Color GetBmiColor(double bmi) => bmi < 18.5 ? WarningOrange : bmi < 25 ? SuccessGreen : bmi < 30 ? WarningOrange : DangerRed;

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "??";
            var parts = name.Split(' ');
            return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : name.Substring(0, Math.Min(2, name.Length)).ToUpper();
        }

        private void DrawRoundedBorder(Graphics g, int w, int h, int r, Color? borderColor = null)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = new GraphicsPath())
            {
                int d = r * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(w - d - 1, 0, d, d, 270, 90);
                path.AddArc(w - d - 1, h - d - 1, d, d, 0, 90);
                path.AddArc(0, h - d - 1, d, d, 90, 90);
                path.CloseFigure();
                using (var pen = new Pen(borderColor ?? BorderGray)) g.DrawPath(pen, path);
            }
        }
        #endregion
    }
}
