using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;

namespace DiyetisyenOtomasyonu.Forms.Login
{
    /// <summary>
    /// Modern Login Formu
    /// 
    /// OOP Principle: Encapsulation - Giriş mantığı kapsüllenir
    /// Academic: User authentication with secure password verification
    /// UI/UX: Clean design with proper validation feedback
    /// </summary>
    public partial class FrmLogin : XtraForm
    {
        private readonly UserRepository _userRepository;
        private TextEdit txtUsername;
        private TextEdit txtPassword;
        private SimpleButton btnLogin;
        private SimpleButton btnRegister;
        private LabelControl lblError;

        // Renk Paleti
        private readonly Color PrimaryColor = Color.FromArgb(79, 70, 229);
        private readonly Color BackgroundColor = Color.FromArgb(248, 250, 252);
        private readonly Color CardColor = Color.White;
        private readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private readonly Color ErrorColor = Color.FromArgb(220, 38, 38);
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);

        public FrmLogin()
        {
            InitializeComponent();
            _userRepository = new UserRepository();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "DiyetPro - Giriş";
            this.Size = new Size(500, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = BackgroundColor;
            this.IconOptions.ShowIcon = false;

            // Header Panel
            var headerPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 130,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = PrimaryColor
            };

            var lblBrand = new LabelControl
            {
                Text = "🥗 DiyetPro",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(300, 50)
            };
            lblBrand.Location = new Point((500 - lblBrand.Width) / 2, 25);
            headerPanel.Controls.Add(lblBrand);

            var lblTagline = new LabelControl
            {
                Text = "Profesyonel Diyetisyen Hasta Takip Sistemi",
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(220, 255, 255, 255),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(350, 25)
            };
            lblTagline.Location = new Point((500 - lblTagline.Width) / 2, 80);
            headerPanel.Controls.Add(lblTagline);

            this.Controls.Add(headerPanel);

            // Login Form Panel
            var formPanel = new PanelControl
            {
                Location = new Point(50, 155),
                Size = new Size(400, 420),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                BackColor = CardColor
            };

            var lblWelcome = new LabelControl
            {
                Text = "Hoş Geldiniz",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(30, 25),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(340, 35)
            };
            formPanel.Controls.Add(lblWelcome);

            var lblSubtitle = new LabelControl
            {
                Text = "Devam etmek için giriş yapın",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextSecondary,
                Location = new Point(30, 60),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(340, 20)
            };
            formPanel.Controls.Add(lblSubtitle);

            // Error Label
            lblError = new LabelControl
            {
                Text = "",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ErrorColor,
                Location = new Point(30, 90),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(340, 20),
                Visible = false
            };
            formPanel.Controls.Add(lblError);

            // Kullanıcı Adı
            var lblUsername = new LabelControl
            {
                Text = "Kullanıcı Adı",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(30, 115),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(340, 20)
            };
            formPanel.Controls.Add(lblUsername);

            txtUsername = new TextEdit
            {
                Location = new Point(30, 138),
                Size = new Size(340, 38),
                Properties = {
                    NullText = "Kullanıcı adınızı girin",
                    Appearance = { Font = new Font("Segoe UI", 11F) }
                }
            };
            txtUsername.EditValueChanged += (s, e) => ClearError();
            formPanel.Controls.Add(txtUsername);

            // Parola
            var lblPassword = new LabelControl
            {
                Text = "Parola",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(30, 185),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(340, 20)
            };
            formPanel.Controls.Add(lblPassword);

            txtPassword = new TextEdit
            {
                Location = new Point(30, 208),
                Size = new Size(340, 38),
                Properties = {
                    NullText = "Parolanızı girin",
                    UseSystemPasswordChar = true,
                    Appearance = { Font = new Font("Segoe UI", 11F) }
                }
            };
            txtPassword.EditValueChanged += (s, e) => ClearError();
            txtPassword.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) DoLogin(); };
            formPanel.Controls.Add(txtPassword);

            // Giriş Butonu
            btnLogin = new SimpleButton
            {
                Text = "GİRİŞ YAP",
                Location = new Point(30, 270),
                Size = new Size(340, 48),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Appearance = {
                    BackColor = PrimaryColor,
                    ForeColor = Color.White,
                    BorderColor = PrimaryColor
                },
                AllowFocus = false
            };
            btnLogin.Click += (s, e) => DoLogin();
            formPanel.Controls.Add(btnLogin);

            // Kayıt Ol Butonu
            btnRegister = new SimpleButton
            {
                Text = "👨‍⚕️ DİYETİSYEN KAYIT OL",
                Location = new Point(30, 330),
                Size = new Size(340, 42),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Appearance = {
                    BackColor = SuccessColor,
                    ForeColor = Color.White,
                    BorderColor = SuccessColor
                },
                AllowFocus = false
            };
            btnRegister.Click += BtnRegister_Click;
            formPanel.Controls.Add(btnRegister);

            // Bilgi
            var lblInfo = new LabelControl
            {
                Text = "Diyetisyen olarak kayıt olup hastalarınızı yönetebilirsiniz",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(30, 380),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(340, 20)
            };
            formPanel.Controls.Add(lblInfo);

            this.Controls.Add(formPanel);
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            using (var registerForm = new FrmRegister())
            {
                registerForm.ShowDialog();
            }
        }

        private void DoLogin()
        {
            try
            {
                ClearError();

                // Validasyon
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    ShowError("Kullanıcı adı boş olamaz");
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    ShowError("Parola boş olamaz");
                    txtPassword.Focus();
                    return;
                }

                // Kullanıcı doğrulama - Repository kullanarak
                var user = _userRepository.GetByUsername(txtUsername.Text.Trim());

                if (user == null)
                {
                    ShowError("Kullanıcı adı veya parola hatalı");
                    return;
                }

                if (!PasswordHasher.VerifyPassword(txtPassword.Text, user.ParolaHash))
                {
                    ShowError("Kullanıcı adı veya parola hatalı");
                    return;
                }

                if (!user.AktifMi)
                {
                    ShowError("Hesabınız pasif durumda");
                    return;
                }

                // Başarılı giriş
                AuthContext.SignIn(user.Id, user.AdSoyad, user.Role);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError("Giriş sırasında hata: " + ex.Message);
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
            this.ClientSize = new System.Drawing.Size(500, 620);
            this.Name = "FrmLogin";
            this.ResumeLayout(false);
        }
    }
}
