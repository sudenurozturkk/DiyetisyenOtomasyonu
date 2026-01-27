using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Shared;
using PatientEntity = DiyetisyenOtomasyonu.Domain.Patient;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Hasta Notlari Yonetimi - DevExpress Modern Tasarim
    /// </summary>
    public partial class FrmNotesModern : XtraForm
    {
        private readonly PatientRepository _patientRepository;
        private readonly NoteRepository _noteRepository;
        private List<PatientEntity> _patients;
        private List<Note> _allNotes;

        // DevExpress Controls
        private PanelControl mainPanel;
        private PanelControl headerPanel;
        private PanelControl statsPanel;
        private PanelControl filterPanel;
        private PanelControl contentPanel;
        private PanelControl formPanel;
        
        private GridControl gridNotes;
        private GridView gridView;
        
        private ComboBoxEdit cmbPatient, cmbCategory, cmbFilterPatient;
        private MemoEdit txtNoteContent;
        private TextEdit txtSearch;
        private LabelControl lblTotalNotes, lblTodayNotes, lblWeekNotes;

        // Renkler
        private readonly Color Primary = ColorTranslator.FromHtml("#0D9488");
        private readonly Color PrimaryLight = ColorTranslator.FromHtml("#14B8A6");
        private readonly Color Success = ColorTranslator.FromHtml("#22C55E");
        private readonly Color Warning = ColorTranslator.FromHtml("#F59E0B");
        private readonly Color Info = ColorTranslator.FromHtml("#3B82F6");
        private readonly Color Danger = ColorTranslator.FromHtml("#EF4444");
        private readonly Color Purple = ColorTranslator.FromHtml("#8B5CF6");
        private readonly Color Background = ColorTranslator.FromHtml("#F1F5F9");
        private readonly Color CardBg = Color.White;
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextGray = ColorTranslator.FromHtml("#64748B");
        private readonly Color Border = ColorTranslator.FromHtml("#E2E8F0");

        public FrmNotesModern()
        {
            _patientRepository = new PatientRepository();
            _noteRepository = new NoteRepository();
            InitializeComponent();
            SetupUI();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1200, 750);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Background;
            this.Name = "FrmNotesModern";
            this.Text = "Hasta Notlari";
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // Main Panel
            mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Background
            };
            this.Controls.Add(mainPanel);

            // Header
            CreateHeader();
            
            // Stats Section
            CreateStatsSection();
            
            // Content Section (Sol: Form, Sag: Grid)
            CreateContentSection();
        }

        private void CreateHeader()
        {
            headerPanel = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(mainPanel.Width, 70),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Primary
            };

            var lblTitle = new LabelControl
            {
                Text = "Hasta Notlari Yonetimi",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(350, 30)
            };
            headerPanel.Controls.Add(lblTitle);

            // Tarih
            var lblDate = new LabelControl
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                Location = new Point(headerPanel.Width - 250, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(220, 22)
            };
            headerPanel.Controls.Add(lblDate);

            mainPanel.Controls.Add(headerPanel);
        }

        private void CreateStatsSection()
        {
            statsPanel = new PanelControl
            {
                Location = new Point(20, 85),
                Size = new Size(mainPanel.Width - 40, 90),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            int x = 0;
            int cardWidth = 200;
            int cardHeight = 80;
            int gap = 20;

            // Toplam Notlar - Mor
            lblTotalNotes = new LabelControl { Text = "0", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White };
            CreateStatCard(statsPanel, x, 0, cardWidth, cardHeight, "Toplam Not", lblTotalNotes, Purple);
            x += cardWidth + gap;

            // Bugun - Yesil
            lblTodayNotes = new LabelControl { Text = "0", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White };
            CreateStatCard(statsPanel, x, 0, cardWidth, cardHeight, "Bugun Eklenen", lblTodayNotes, Success);
            x += cardWidth + gap;

            // Bu Hafta - Mavi
            lblWeekNotes = new LabelControl { Text = "0", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White };
            CreateStatCard(statsPanel, x, 0, cardWidth, cardHeight, "Bu Hafta", lblWeekNotes, Info);
            x += cardWidth + gap;

            // Yeni Not Ekle Butonu
            var btnAddNote = new SimpleButton
            {
                Text = "+ YENI NOT EKLE",
                Location = new Point(x, 15),
                Size = new Size(180, 50),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddNote.Appearance.BackColor = Warning;
            btnAddNote.Appearance.ForeColor = Color.White;
            btnAddNote.Appearance.BorderColor = Warning;
            btnAddNote.Appearance.Options.UseBackColor = true;
            btnAddNote.Appearance.Options.UseForeColor = true;
            btnAddNote.Appearance.Options.UseBorderColor = true;
            btnAddNote.Click += (s, e) => ShowAddNoteForm();
            statsPanel.Controls.Add(btnAddNote);

            mainPanel.Controls.Add(statsPanel);
        }

        private void CreateStatCard(PanelControl parent, int x, int y, int width, int height, string title, LabelControl valueLabel, Color color)
        {
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BorderStyle = BorderStyles.NoBorder,
                BackColor = color
            };

            var lblTitle = new LabelControl
            {
                Text = title,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(220, 255, 255, 255),
                Location = new Point(15, 12),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 30, 18)
            };
            card.Controls.Add(lblTitle);

            valueLabel.Location = new Point(15, 35);
            valueLabel.AutoSizeMode = LabelAutoSizeMode.None;
            valueLabel.Size = new Size(width - 30, 35);
            card.Controls.Add(valueLabel);

            parent.Controls.Add(card);
        }

        private void CreateContentSection()
        {
            contentPanel = new PanelControl
            {
                Location = new Point(20, 190),
                Size = new Size(mainPanel.Width - 40, mainPanel.Height - 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            // Sol Panel - Not Ekleme Formu
            CreateFormPanel();

            // Sag Panel - Not Listesi
            CreateNotesListPanel();

            mainPanel.Controls.Add(contentPanel);
        }

        private void CreateFormPanel()
        {
            formPanel = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(380, contentPanel.Height),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BorderStyle = BorderStyles.Simple,
                BackColor = CardBg
            };

            // Baslik
            var headerBar = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(formPanel.Width, 50),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Primary
            };
            var lblFormTitle = new LabelControl
            {
                Text = "Yeni Not Ekle",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 13),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 25)
            };
            headerBar.Controls.Add(lblFormTitle);
            formPanel.Controls.Add(headerBar);

            int y = 70;
            int labelX = 20;
            int inputX = 20;
            int inputWidth = 340;

            // Hasta Secimi
            var lblPatient = new LabelControl
            {
                Text = "Hasta Seciniz *",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(labelX, y)
            };
            formPanel.Controls.Add(lblPatient);
            y += 25;

            cmbPatient = new ComboBoxEdit
            {
                Location = new Point(inputX, y),
                Size = new Size(inputWidth, 32)
            };
            cmbPatient.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            formPanel.Controls.Add(cmbPatient);
            y += 50;

            // Kategori
            var lblCategory = new LabelControl
            {
                Text = "Kategori",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(labelX, y)
            };
            formPanel.Controls.Add(lblCategory);
            y += 25;

            cmbCategory = new ComboBoxEdit
            {
                Location = new Point(inputX, y),
                Size = new Size(inputWidth, 32)
            };
            cmbCategory.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbCategory.Properties.Items.AddRange(new[] { "Genel", "Beslenme", "Tibbi", "Egzersiz", "Psikolojik", "Ilerleme" });
            cmbCategory.SelectedIndex = 0;
            formPanel.Controls.Add(cmbCategory);
            y += 50;

            // Not Icerigi
            var lblContent = new LabelControl
            {
                Text = "Not Icerigi *",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(labelX, y)
            };
            formPanel.Controls.Add(lblContent);
            y += 25;

            txtNoteContent = new MemoEdit
            {
                Location = new Point(inputX, y),
                Size = new Size(inputWidth, 180)
            };
            txtNoteContent.Properties.NullValuePrompt = "Hasta hakkinda notunuzu yazin...";
            formPanel.Controls.Add(txtNoteContent);
            y += 200;

            // Butonlar
            var btnCancel = new SimpleButton
            {
                Text = "Temizle",
                Location = new Point(inputX, y),
                Size = new Size(160, 42),
                Font = new Font("Segoe UI", 10F)
            };
            btnCancel.Appearance.BackColor = CardBg;
            btnCancel.Appearance.ForeColor = TextDark;
            btnCancel.Appearance.BorderColor = Border;
            btnCancel.Appearance.Options.UseBackColor = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.Appearance.Options.UseBorderColor = true;
            btnCancel.Click += (s, e) => ClearForm();
            formPanel.Controls.Add(btnCancel);

            var btnSave = new SimpleButton
            {
                Text = "KAYDET",
                Location = new Point(inputX + 175, y),
                Size = new Size(165, 42),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.Appearance.BackColor = Success;
            btnSave.Appearance.ForeColor = Color.White;
            btnSave.Appearance.BorderColor = Success;
            btnSave.Appearance.Options.UseBackColor = true;
            btnSave.Appearance.Options.UseForeColor = true;
            btnSave.Appearance.Options.UseBorderColor = true;
            btnSave.Click += (s, e) => SaveNote();
            formPanel.Controls.Add(btnSave);

            contentPanel.Controls.Add(formPanel);
        }

        private void CreateNotesListPanel()
        {
            var listPanel = new PanelControl
            {
                Location = new Point(400, 0),
                Size = new Size(contentPanel.Width - 410, contentPanel.Height),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyles.Simple,
                BackColor = CardBg
            };

            // Baslik
            var headerBar = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(listPanel.Width, 50),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = PrimaryLight
            };
            var lblListTitle = new LabelControl
            {
                Text = "Not Gecmisi",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Primary, // Okunabilirlik için koyu renk yapıldı
                Location = new Point(15, 13),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 25)
            };
            headerBar.Controls.Add(lblListTitle);
            listPanel.Controls.Add(headerBar);

            // Filtre Satiri
            filterPanel = new PanelControl
            {
                Location = new Point(0, 50),
                Size = new Size(listPanel.Width, 50),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            var lblFilter = new LabelControl
            {
                Text = "Hasta:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextDark, // Okunabilirlik için koyu renk
                Location = new Point(15, 15)
            };
            filterPanel.Controls.Add(lblFilter);

            cmbFilterPatient = new ComboBoxEdit
            {
                Location = new Point(60, 10),
                Size = new Size(180, 30)
            };
            cmbFilterPatient.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbFilterPatient.SelectedIndexChanged += (s, e) => FilterNotes();
            filterPanel.Controls.Add(cmbFilterPatient);

            txtSearch = new TextEdit
            {
                Location = new Point(260, 10),
                Size = new Size(200, 30)
            };
            txtSearch.Properties.NullValuePrompt = "Notlarda ara...";
            txtSearch.EditValueChanged += (s, e) => FilterNotes();
            filterPanel.Controls.Add(txtSearch);

            var btnRefresh = new SimpleButton
            {
                Text = "Yenile",
                Location = new Point(475, 10),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 9F)
            };
            btnRefresh.Appearance.BackColor = Info;
            btnRefresh.Appearance.ForeColor = Color.White;
            btnRefresh.Appearance.BorderColor = Info;
            btnRefresh.Appearance.Options.UseBackColor = true;
            btnRefresh.Appearance.Options.UseForeColor = true;
            btnRefresh.Appearance.Options.UseBorderColor = true;
            btnRefresh.Click += (s, e) => { LoadNotes(); UpdateStats(); };
            filterPanel.Controls.Add(btnRefresh);

            listPanel.Controls.Add(filterPanel);

            // Grid
            gridNotes = new GridControl
            {
                Location = new Point(5, 105),
                Size = new Size(listPanel.Width - 10, listPanel.Height - 115),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            
            gridView = new GridView(gridNotes);
            gridNotes.MainView = gridView;

            // Kolonlar
            gridView.Columns.Add(new GridColumn { Caption = "Hasta", FieldName = "PatientName", VisibleIndex = 0, Width = 150 });
            gridView.Columns.Add(new GridColumn { Caption = "Kategori", FieldName = "CategoryText", VisibleIndex = 1, Width = 100 });
            gridView.Columns.Add(new GridColumn { Caption = "Icerik", FieldName = "Content", VisibleIndex = 2, Width = 350 });
            gridView.Columns.Add(new GridColumn { Caption = "Tarih", FieldName = "DateText", VisibleIndex = 3, Width = 140 });

            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsView.RowAutoHeight = true;
            gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
            gridView.Appearance.Row.Font = new Font("Segoe UI", 10);
            gridView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            gridView.Appearance.HeaderPanel.BackColor = Color.FromArgb(240, 240, 240);
            gridView.Appearance.HeaderPanel.ForeColor = Color.FromArgb(64, 64, 64); // Koyu gri
            gridView.Appearance.HeaderPanel.Options.UseBackColor = true;
            gridView.Appearance.HeaderPanel.Options.UseForeColor = true;
            gridView.RowHeight = 50;

            // Satir renklendirme
            gridView.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "CategoryText")
                {
                    var categoryText = gridView.GetRowCellValue(e.RowHandle, "CategoryText")?.ToString();
                    switch (categoryText)
                    {
                        case "Beslenme": e.Appearance.ForeColor = Success; break;
                        case "Tibbi": e.Appearance.ForeColor = Info; break;
                        case "Egzersiz": e.Appearance.ForeColor = Warning; break;
                        case "Psikolojik": e.Appearance.ForeColor = Purple; break;
                        case "Ilerleme": e.Appearance.ForeColor = Danger; break;
                        default: e.Appearance.ForeColor = Primary; break;
                    }
                    e.Appearance.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            };

            // Alternatif satir rengi
            gridView.RowStyle += (s, e) =>
            {
                if (e.RowHandle % 2 == 0)
                    e.Appearance.BackColor = Color.FromArgb(250, 252, 255);
                else
                    e.Appearance.BackColor = Color.White;
            };

            listPanel.Controls.Add(gridNotes);
            contentPanel.Controls.Add(listPanel);
        }

        private void ShowAddNoteForm()
        {
            // Sol formu vurgula
            formPanel.BackColor = Color.FromArgb(240, 253, 250);
            cmbPatient.Focus();
        }

        private void ClearForm()
        {
            if (cmbPatient.Properties.Items.Count > 0)
                cmbPatient.SelectedIndex = 0;
            cmbCategory.SelectedIndex = 0;
            txtNoteContent.Text = "";
            formPanel.BackColor = CardBg;
        }

        private void SaveNote()
        {
            try
            {
                var selectedPatient = cmbPatient?.EditValue as PatientItem;
                if (selectedPatient == null)
                {
                    XtraMessageBox.Show("Lutfen hasta secin!", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNoteContent?.Text))
                {
                    XtraMessageBox.Show("Lutfen not icerigi girin!", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var categoryMap = new Dictionary<string, NoteCategory>
                {
                    { "Genel", NoteCategory.General },
                    { "Beslenme", NoteCategory.Nutrition },
                    { "Tibbi", NoteCategory.Medical },
                    { "Egzersiz", NoteCategory.Exercise },
                    { "Psikolojik", NoteCategory.Psychological },
                    { "Ilerleme", NoteCategory.Progress }
                };

                var note = new Note
                {
                    PatientId = selectedPatient.Id,
                    DoctorId = AuthContext.UserId,
                    DoctorName = AuthContext.UserName,
                    Content = txtNoteContent.Text.Trim(),
                    Date = DateTime.Now,
                    Category = categoryMap.ContainsKey(cmbCategory.Text) ? categoryMap[cmbCategory.Text] : NoteCategory.General
                };

                _noteRepository.Add(note);
                
                ClearForm();
                LoadNotes();
                UpdateStats();
                ToastNotification.ShowSuccess("Not basariyla kaydedildi!");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                _patients = _patientRepository.GetByDoctorId(AuthContext.UserId).ToList();
                
                // Hasta dropdown
                cmbPatient.Properties.Items.Clear();
                foreach (var p in _patients)
                {
                    cmbPatient.Properties.Items.Add(new PatientItem { Id = p.Id, Name = p.AdSoyad });
                }
                if (cmbPatient.Properties.Items.Count > 0)
                    cmbPatient.SelectedIndex = 0;
                
                // Filtre dropdown
                cmbFilterPatient.Properties.Items.Clear();
                cmbFilterPatient.Properties.Items.Add(new PatientItem { Id = 0, Name = "Tum Hastalar" });
                foreach (var p in _patients)
                {
                    cmbFilterPatient.Properties.Items.Add(new PatientItem { Id = p.Id, Name = p.AdSoyad });
                }
                cmbFilterPatient.SelectedIndex = 0;

                LoadNotes();
                UpdateStats();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Veri yuklenirken hata: {ex.Message}", "Hata");
            }
        }

        private void LoadNotes()
        {
            try
            {
                _allNotes = new List<Note>();
                
                if (_patients == null || _patients.Count == 0)
                {
                    UpdateStats();
                    gridNotes.DataSource = null;
                    return;
                }
                
                foreach (var patient in _patients)
                {
                    var patientNotes = _noteRepository.GetByPatientId(patient.Id);
                    if (patientNotes != null)
                    {
                        foreach (var note in patientNotes)
                        {
                            note.PatientName = patient.AdSoyad;
                            _allNotes.Add(note);
                        }
                    }
                }

                _allNotes = _allNotes.OrderByDescending(n => n.Date).ToList();
                FilterNotes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadNotes Error: " + ex.Message);
                _allNotes = new List<Note>();
                gridNotes.DataSource = null;
            }
        }

        private void FilterNotes()
        {
            if (_allNotes == null)
            {
                _allNotes = new List<Note>();
            }
            
            var filtered = _allNotes.ToList();

            // Hasta filtresi
            var selectedPatient = cmbFilterPatient?.EditValue as PatientItem;
            if (selectedPatient != null && selectedPatient.Id > 0)
            {
                filtered = filtered.Where(n => n.PatientId == selectedPatient.Id).ToList();
            }

            // Arama
            var search = txtSearch?.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(n =>
                    (n.Content?.ToLower().Contains(search) ?? false) ||
                    (n.PatientName?.ToLower().Contains(search) ?? false)
                ).ToList();
            }

            // Grid'e bind et
            var displayList = filtered.Select(n => new
            {
                n.Id,
                n.PatientId,
                n.PatientName,
                CategoryText = GetCategoryText(n.Category),
                n.Content,
                DateText = FormatDate(n.Date),
                n.Date
            }).ToList();

            gridNotes.DataSource = displayList;
            gridNotes.RefreshDataSource();
        }

        private void UpdateStats()
        {
            if (_allNotes == null) _allNotes = new List<Note>();
            
            if (lblTotalNotes != null)
                lblTotalNotes.Text = _allNotes.Count.ToString();
            if (lblTodayNotes != null)
                lblTodayNotes.Text = _allNotes.Count(n => n.Date.Date == DateTime.Today).ToString();
            if (lblWeekNotes != null)
                lblWeekNotes.Text = _allNotes.Count(n => n.Date >= DateTime.Today.AddDays(-7)).ToString();
        }

        private string FormatDate(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return $"Bugun, {date:HH:mm}";
            if (date.Date == DateTime.Today.AddDays(-1))
                return $"Dun, {date:HH:mm}";
            return date.ToString("dd MMM yyyy, HH:mm");
        }

        private string GetCategoryText(NoteCategory category)
        {
            switch (category)
            {
                case NoteCategory.Medical: return "Tibbi";
                case NoteCategory.Nutrition: return "Beslenme";
                case NoteCategory.Exercise: return "Egzersiz";
                case NoteCategory.Psychological: return "Psikolojik";
                case NoteCategory.Progress: return "Ilerleme";
                default: return "Genel";
            }
        }

        private class PatientItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }
    }
}
