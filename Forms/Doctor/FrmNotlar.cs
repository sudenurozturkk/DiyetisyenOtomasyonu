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
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Hasta Notları Sayfası - Mockup'a Uygun Tasarım
    /// Sol: Yeni Not Ekle (Hasta Seçimi + Not İçeriği)
    /// Sağ: Not Geçmişi (Arama + Kart Listesi)
    /// </summary>
    public class FrmNotlar : XtraForm
    {
        private readonly NoteService _noteService;
        private readonly PatientService _patientService;

        // Colors
        private readonly Color PrimaryGreen = ColorTranslator.FromHtml("#0D9488");
        private readonly Color LightGreen = ColorTranslator.FromHtml("#CCFBF1");
        private readonly Color PurpleColor = ColorTranslator.FromHtml("#9333EA");
        private readonly Color BackgroundLight = ColorTranslator.FromHtml("#F8FAFC");
        private readonly Color CardWhite = Color.White;
        private readonly Color BorderGray = ColorTranslator.FromHtml("#E2E8F0");
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextMedium = ColorTranslator.FromHtml("#64748B");

        // Controls
        private LookUpEdit cmbPatient;
        private MemoEdit txtNote;
        private Panel pnlNotesList;
        private TextEdit txtSearch;

        private int _selectedPatientId;
        private List<Note> _allNotes = new List<Note>();

        public FrmNotlar()
        {
            InitializeComponent();
            _noteService = new NoteService();
            _patientService = new PatientService();
            SetupUI();
            LoadPatients();
            LoadAllNotes();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1200, 700);
            this.Name = "FrmNotlar";
            this.Text = "Hasta Notları";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BackgroundLight;
            this.Padding = new Padding(20);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // Header
            CreateHeader();
            
            // Main Content (Split)
            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel1,
                SplitterDistance = 400,
                SplitterWidth = 30,
                BackColor = BackgroundLight,
                Panel1 = { BackColor = BackgroundLight },
                Panel2 = { BackColor = BackgroundLight }
            };
            
            CreateLeftPanel(mainSplit.Panel1);
            CreateRightPanel(mainSplit.Panel2);
            
            this.Controls.Add(mainSplit);
        }

        private void CreateHeader()
        {
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = CardWhite,
                Padding = new Padding(25, 20, 25, 20)
            };
            pnlHeader.Paint += (s, e) => DrawRoundedBorder(e.Graphics, pnlHeader.Width, pnlHeader.Height, 15);

            // Icon
            var lblIcon = new Label
            {
                Text = "📝",
                Font = new Font("Segoe UI", 24F),
                Location = new Point(25, 20),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblIcon);

            // Title
            var lblTitle = new Label
            {
                Text = "Hasta Notları Yönetimi",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(75, 18),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            // Subtitle
            var lblSubtitle = new Label
            {
                Text = "Hastalarınız hakkında önemli notları ekleyin, düzenleyin ve geçmiş notları görüntüleyerek takibini yapın.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextMedium,
                Location = new Point(75, 48),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblSubtitle);

            this.Controls.Add(pnlHeader);
        }

        private void CreateLeftPanel(Panel parent)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(25)
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card.Width, card.Height, 20);

            // Header
            var pnlCardHeader = new Panel { Dock = DockStyle.Top, Height = 40 };
            var lblPlus = new Label { Text = "➕", Font = new Font("Segoe UI", 14F), ForeColor = PrimaryGreen, Location = new Point(0, 5), AutoSize = true };
            var lblCardTitle = new Label { Text = "Yeni Not Ekle", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(35, 5), AutoSize = true };
            pnlCardHeader.Controls.Add(lblPlus);
            pnlCardHeader.Controls.Add(lblCardTitle);
            card.Controls.Add(pnlCardHeader);

            // Content Container
            var contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };

            // Patient Selection
            var lblPatient = new Label { Text = "Hasta Seçiniz", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextDark, Dock = DockStyle.Top, Height = 30 };
            contentPanel.Controls.Add(lblPatient);

            var pnlCombo = new Panel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 0, 0, 10) };
            cmbPatient = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties = {
                    NullText = "-- Listeden Seçin --",
                    Appearance = { Font = new Font("Segoe UI", 10F) },
                    AutoHeight = false
                }
            };
            cmbPatient.Height = 38;
            cmbPatient.Properties.DisplayMember = "AdSoyad";
            cmbPatient.Properties.ValueMember = "Id";
            cmbPatient.Properties.Columns.Clear();
            cmbPatient.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AdSoyad", "Hasta", 220));
            cmbPatient.EditValueChanged += CmbPatient_EditValueChanged;
            pnlCombo.Controls.Add(cmbPatient);
            contentPanel.Controls.Add(pnlCombo);

            // Note Content
            var lblNote = new Label { Text = "Not İçeriği", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextDark, Dock = DockStyle.Top, Height = 35 };
            contentPanel.Controls.Add(lblNote);

            txtNote = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = {
                    NullText = "Hasta hakkında klinik gözlemlerinizi, diyet uyumunu ve diğer önemli detayları buraya yazınız...",
                    ScrollBars = ScrollBars.Vertical,
                    Appearance = { Font = new Font("Segoe UI", 10F), BackColor = BackgroundLight }
                }
            };
            contentPanel.Controls.Add(txtNote);

            card.Controls.Add(contentPanel);

            // Save Button
            var btnSave = new SimpleButton
            {
                Text = "💾 NOT EKLE",
                Dock = DockStyle.Bottom,
                Height = 50,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Appearance = { BackColor = PurpleColor, ForeColor = Color.White },
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnAddNote_Click;
            card.Controls.Add(btnSave);

            parent.Controls.Add(card);
        }

        private void CreateRightPanel(Panel parent)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(25)
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card.Width, card.Height, 20);

            // Header Row
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50 };

            var lblHistory = new Label { Text = "🕐", Font = new Font("Segoe UI", 14F), ForeColor = TextMedium, Location = new Point(0, 8), AutoSize = true };
            var lblTitle = new Label { Text = "Not Geçmişi", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(35, 8), AutoSize = true };
            pnlHeader.Controls.Add(lblHistory);
            pnlHeader.Controls.Add(lblTitle);

            // Search Box
            txtSearch = new TextEdit
            {
                Location = new Point(350, 5),
                Size = new Size(200, 35),
                Properties = { NullText = "🔍 Notlarda ara...", Appearance = { Font = new Font("Segoe UI", 10F) }, AutoHeight = false }
            };
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.TextChanged += (s, e) => FilterNotes();
            pnlHeader.Controls.Add(txtSearch);

            card.Controls.Add(pnlHeader);

            // Notes List
            pnlNotesList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            card.Controls.Add(pnlNotesList);

            // Footer
            var lblFooter = new Label
            {
                Text = "... Daha eski notlar arşivlendi",
                Dock = DockStyle.Bottom,
                Height = 30,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = TextMedium,
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblFooter);

            parent.Controls.Add(card);
        }

        private void LoadPatients()
        {
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                cmbPatient.Properties.DataSource = patients;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hastalar yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAllNotes()
        {
            try
            {
                // Load all notes for all patients of this doctor
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                _allNotes.Clear();
                foreach (var p in patients)
                {
                    var notes = _noteService.GetPatientNotes(p.Id);
                    foreach (var n in notes)
                    {
                        n.PatientName = p.AdSoyad; // Store patient name for display
                    }
                    _allNotes.AddRange(notes);
                }
                PopulateNotesList(_allNotes);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Notlar yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbPatient_EditValueChanged(object sender, EventArgs e)
        {
            if (cmbPatient.EditValue != null)
            {
                _selectedPatientId = (int)cmbPatient.EditValue;
            }
        }

        private void FilterNotes()
        {
            string query = txtSearch.Text?.ToLower() ?? "";
            var filtered = _allNotes.Where(n =>
                n.Content.ToLower().Contains(query) ||
                (n.PatientName?.ToLower().Contains(query) ?? false) ||
                n.Date.ToString().Contains(query)
            ).ToList();
            PopulateNotesList(filtered);
        }

        private void PopulateNotesList(List<Note> notes)
        {
            pnlNotesList.SuspendLayout();
            pnlNotesList.Controls.Clear();

            if (notes.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Henüz not bulunmuyor.",
                    Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                    ForeColor = TextMedium,
                    Dock = DockStyle.Top,
                    Height = 50,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlNotesList.Controls.Add(lblEmpty);
            }
            else
            {
                var orderedNotes = notes.OrderByDescending(n => n.Date).Take(20).ToList();
                foreach (var note in orderedNotes)
                {
                    var card = CreateNoteCard(note);
                    pnlNotesList.Controls.Add(card);
                }
            }

            pnlNotesList.ResumeLayout(true);
        }

        private Panel CreateNoteCard(Note note)
        {
            var wrapper = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 0, 0, 15) };
            
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundLight
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card.Width, card.Height, 15, BorderGray);

            // Avatar
            Color avatarColor = GetAvatarColor(note.PatientName ?? "");
            string initials = GetInitials(note.PatientName ?? "Hasta");
            
            var pnlAvatar = new Panel { Location = new Point(15, 15), Size = new Size(45, 45) };
            pnlAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(avatarColor))
                    e.Graphics.FillEllipse(brush, 0, 0, 44, 44);
                using (var font = new Font("Segoe UI", 11F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(initials, font, brush, new Rectangle(0, 0, 45, 45), sf);
            };
            card.Controls.Add(pnlAvatar);

            // Patient Name
            var lblName = new Label
            {
                Text = note.PatientName ?? "Hasta",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(70, 12),
                AutoSize = true
            };
            card.Controls.Add(lblName);

            // Date
            var lblDate = new Label
            {
                Text = "📅 " + FormatDate(note.Date),
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(70, 35),
                AutoSize = true
            };
            card.Controls.Add(lblDate);

            // Content
            var lblContent = new Label
            {
                Text = note.Content.Length > 150 ? note.Content.Substring(0, 147) + "..." : note.Content,
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextDark,
                Location = new Point(15, 65),
                Size = new Size(card.Width > 0 ? card.Width - 30 : 400, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(lblContent);

            wrapper.Controls.Add(card);
            return wrapper;
        }

        private string FormatDate(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return "Bugün, " + date.ToString("HH:mm");
            if (date.Date == DateTime.Today.AddDays(-1))
                return "Dün, " + date.ToString("HH:mm");
            return date.ToString("d MMMM yyyy, HH:mm");
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "??";
            var parts = name.Split(' ');
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
            return name.Substring(0, Math.Min(2, name.Length)).ToUpper();
        }

        private Color GetAvatarColor(string name)
        {
            if (string.IsNullOrEmpty(name)) return PrimaryGreen;
            int hash = Math.Abs(name.GetHashCode());
            Color[] colors = { PrimaryGreen, PurpleColor, ColorTranslator.FromHtml("#F97316"), ColorTranslator.FromHtml("#3B82F6") };
            return colors[hash % colors.Length];
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
                using (var pen = new Pen(borderColor ?? BorderGray))
                    g.DrawPath(pen, path);
            }
        }

        private void BtnAddNote_Click(object sender, EventArgs e)
        {
            if (_selectedPatientId == 0)
            {
                XtraMessageBox.Show("Lütfen hasta seçin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNote.Text))
            {
                XtraMessageBox.Show("Not içeriği boş olamaz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _noteService.AddNote(_selectedPatientId, AuthContext.UserId, AuthContext.UserName, txtNote.Text.Trim());
                txtNote.Text = string.Empty;
                LoadAllNotes();
                XtraMessageBox.Show("Not başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Not eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
