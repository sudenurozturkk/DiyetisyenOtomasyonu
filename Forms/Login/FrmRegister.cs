using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;

namespace DiyetisyenOtomasyonu.Forms.Login
{
    /// <summary>
    /// Diyetisyen Kayıt Formu
    /// </summary>
    public partial class FrmRegister : XtraForm
    {
        private readonly UserRepository _userRepository;
        private readonly DoctorRepository _doctorRepository;

        private TextEdit txtAdSoyad;
        private TextEdit txtKullaniciAdi;
        private TextEdit txtParola;
        private TextEdit txtParolaTekrar;
        private TextEdit txtUzmanlik;
        private TextEdit txtTelefon;
        private TextEdit txtEmail;
        private SimpleButton btnKayit;
        private SimpleButton btnIptal;
        private LabelControl lblError;

        // Renkler
        private readonly Color PrimaryColor = Color.FromArgb(79, 70, 229);
        private readonly Color BackgroundColor = Color.FromArgb(248, 250, 252);
        private readonly Color CardColor = Color.White;
        private readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private readonly Color ErrorColor = Color.FromArgb(220, 38, 38);
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);

        public FrmRegister()
        {
            InitializeComponent();
            _userRepository = new UserRepository();
            _doctorRepository = new DoctorRepository();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "DiyetPro - Diyetisyen Kayıt";
            this.Size = new Size(520, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = BackgroundColor;

            // Header
            var headerPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 80,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = PrimaryColor
            };

            var lblTitle = new LabelControl
            {
                Text = "👨‍⚕️ Diyetisyen Kayıt",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(350, 40),
                Location = new Point(85, 20)
            };
            headerPanel.Controls.Add(lblTitle);
            this.Controls.Add(headerPanel);

            // Form Panel
            var formPanel = new PanelControl
            {
                Location = new Point(20, 90),
                Size = new Size(475, 580),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                BackColor = CardColor
            };

            int y = 20;
            int labelHeight = 22;
            int inputHeight = 32;
            int spacing = 8;

            // Error Label
            lblError = new LabelControl
            {
                Text = "",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ErrorColor,
                Location = new Point(20, y),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(430, 20),
                Visible = false
            };
            formPanel.Controls.Add(lblError);
            y += 25;

            // Ad Soyad
            formPanel.Controls.Add(CreateLabel("Ad Soyad *", 20, y));
            y += labelHeight;
            txtAdSoyad = CreateTextEdit(20, y, "Örn: Dr. Ayşe Yılmaz");
            formPanel.Controls.Add(txtAdSoyad);
            y += inputHeight + spacing;

            // Kullanıcı Adı
            formPanel.Controls.Add(CreateLabel("Kullanıcı Adı *", 20, y));
            y += labelHeight;
            txtKullaniciAdi = CreateTextEdit(20, y, "Örn: drayse");
            formPanel.Controls.Add(txtKullaniciAdi);
            y += inputHeight + spacing;

            // Parola
            formPanel.Controls.Add(CreateLabel("Parola * (en az 8 karakter)", 20, y));
            y += labelHeight;
            txtParola = CreateTextEdit(20, y, "");
            txtParola.Properties.UseSystemPasswordChar = true;
            formPanel.Controls.Add(txtParola);
            y += inputHeight + spacing;

            // Parola Tekrar
            formPanel.Controls.Add(CreateLabel("Parola Tekrar *", 20, y));
            y += labelHeight;
            txtParolaTekrar = CreateTextEdit(20, y, "");
            txtParolaTekrar.Properties.UseSystemPasswordChar = true;
            formPanel.Controls.Add(txtParolaTekrar);
            y += inputHeight + spacing;

            // Uzmanlık
            formPanel.Controls.Add(CreateLabel("Uzmanlık Alanı", 20, y));
            y += labelHeight;
            txtUzmanlik = CreateTextEdit(20, y, "Örn: Beslenme ve Diyetetik");
            formPanel.Controls.Add(txtUzmanlik);
            y += inputHeight + spacing;

            // Telefon
            formPanel.Controls.Add(CreateLabel("Telefon", 20, y));
            y += labelHeight;
            txtTelefon = CreateTextEdit(20, y, "Örn: 0532 111 22 33");
            formPanel.Controls.Add(txtTelefon);
            y += inputHeight + spacing;

            // Email
            formPanel.Controls.Add(CreateLabel("E-posta", 20, y));
            y += labelHeight;
            txtEmail = CreateTextEdit(20, y, "Örn: doktor@email.com");
            formPanel.Controls.Add(txtEmail);
            y += inputHeight + 20;

            // Kayıt Butonu
            btnKayit = new SimpleButton
            {
                Text = "✓ KAYIT OL",
                Location = new Point(20, y),
                Size = new Size(210, 50),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Appearance = { BackColor = SuccessColor, ForeColor = Color.White }
            };
            btnKayit.Click += BtnKayit_Click;
            formPanel.Controls.Add(btnKayit);

            // İptal Butonu
            btnIptal = new SimpleButton
            {
                Text = "✗ İPTAL",
                Location = new Point(240, y),
                Size = new Size(210, 50),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Appearance = { BackColor = Color.FromArgb(107, 114, 128), ForeColor = Color.White }
            };
            btnIptal.Click += (s, e) => this.Close();
            formPanel.Controls.Add(btnIptal);

            this.Controls.Add(formPanel);
        }

        private LabelControl CreateLabel(string text, int x, int y)
        {
            return new LabelControl
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(x, y),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(430, 20)
            };
        }

        private TextEdit CreateTextEdit(int x, int y, string nullText)
        {
            var txt = new TextEdit
            {
                Location = new Point(x, y),
                Size = new Size(430, 32),
                Properties = {
                    NullText = nullText,
                    Appearance = { Font = new Font("Segoe UI", 10F) }
                }
            };
            txt.EditValueChanged += (s, e) => ClearError();
            return txt;
        }

        private void BtnKayit_Click(object sender, EventArgs e)
        {
            try
            {
                ClearError();

                // Validasyonlar
                if (string.IsNullOrWhiteSpace(txtAdSoyad.Text))
                {
                    ShowError("Ad Soyad boş olamaz");
                    txtAdSoyad.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtKullaniciAdi.Text))
                {
                    ShowError("Kullanıcı adı boş olamaz");
                    txtKullaniciAdi.Focus();
                    return;
                }

                if (txtKullaniciAdi.Text.Length < 3)
                {
                    ShowError("Kullanıcı adı en az 3 karakter olmalı");
                    txtKullaniciAdi.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtParola.Text))
                {
                    ShowError("Parola boş olamaz");
                    txtParola.Focus();
                    return;
                }

                if (txtParola.Text.Length < 8)
                {
                    ShowError("Parola en az 8 karakter olmalı");
                    txtParola.Focus();
                    return;
                }

                if (txtParola.Text != txtParolaTekrar.Text)
                {
                    ShowError("Parolalar eşleşmiyor");
                    txtParolaTekrar.Focus();
                    return;
                }

                // Kullanıcı adı kontrolü
                if (_userRepository.UsernameExists(txtKullaniciAdi.Text.Trim()))
                {
                    ShowError("Bu kullanıcı adı zaten kullanılıyor");
                    txtKullaniciAdi.Focus();
                    return;
                }

                // Doktor oluştur
                var doctor = new DiyetisyenOtomasyonu.Domain.Doctor
                {
                    AdSoyad = txtAdSoyad.Text.Trim(),
                    KullaniciAdi = txtKullaniciAdi.Text.Trim().ToLower(),
                    ParolaHash = PasswordHasher.HashPassword(txtParola.Text),
                    Role = UserRole.Doctor,
                    AktifMi = true,
                    KayitTarihi = DateTime.Now,
                    Uzmanlik = txtUzmanlik.Text?.Trim() ?? "Beslenme ve Diyetetik",
                    Telefon = txtTelefon.Text?.Trim() ?? "",
                    Email = txtEmail.Text?.Trim() ?? ""
                };

                // Veritabanına kaydet
                int doctorId = _doctorRepository.CreateDoctor(doctor);

                if (doctorId > 0)
                {
                    XtraMessageBox.Show(
                        $"Kayıt başarılı!\n\nKullanıcı adınız: {doctor.KullaniciAdi}\n\nŞimdi giriş yapabilirsiniz.",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError("Kayıt sırasında bir hata oluştu");
                }
            }
            catch (Exception ex)
            {
                ShowError("Hata: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = "⚠ " + message;
            lblError.Visible = true;
        }

        private void ClearError()
        {
            lblError.Text = "";
            lblError.Visible = false;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(520, 720);
            this.Name = "FrmRegister";
            this.ResumeLayout(false);
        }
    }
}

