using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    public partial class FrmGoalsNotes : XtraForm
    {
        private readonly GoalService _goalService;
        private readonly PatientService _patientService;
        private readonly NoteService _noteService;

        private LookUpEdit cmbPatient;
        private GridControl gridGoals;
        private GridView viewGoals;
        private MemoEdit txtNote;
        private MemoEdit memoNotes;
        private LabelControl lblPatientInfo;

        private BindingList<Goal> _goals;
        private int _selectedPatientId;

        // Teal Theme Colors
        private readonly Color PrimaryColor = Color.FromArgb(0, 121, 107);      // Teal 700
        private readonly Color DarkColor = Color.FromArgb(0, 77, 64);           // Teal 900
        private readonly Color LightColor = Color.FromArgb(178, 223, 219);      // Teal 100
        private readonly Color AccentColor = Color.FromArgb(38, 166, 154);      // Teal 400
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);      // Green
        private readonly Color DangerColor = Color.FromArgb(239, 68, 68);       // Red
        private readonly Color WarningColor = Color.FromArgb(245, 158, 11);     // Amber
        private readonly Color InfoColor = Color.FromArgb(14, 165, 233);        // Blue
        private readonly Color CardColor = Color.White;
        private readonly Color BackgroundColor = Color.FromArgb(240, 253, 250); // Teal 50
        private readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private readonly Color TextSecondary = Color.FromArgb(100, 116, 139);

        public FrmGoalsNotes()
        {
            InitializeComponent();
            _goalService = new GoalService();
            _patientService = new PatientService();
            _noteService = new NoteService();
            InitializeUI();
            LoadPatients();
        }

        private void InitializeUI()
        {
            this.Text = "Hedef ve Notlar";
            this.BackColor = BackgroundColor;

            // ANA PANEL
            var mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor,
                Padding = new Padding(10)
            };

            // ============ UST PANEL - Hasta secimi ============
            var pnlTop = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 75,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor
            };

            var lblTitle = new LabelControl
            {
                Text = "HEDEFLER VE NOTLAR",
                Location = new Point(15, 12),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = PrimaryColor
            };
            pnlTop.Controls.Add(lblTitle);

            var lblPatient = new LabelControl
            {
                Text = "Hasta Secin:",
                Location = new Point(15, 45),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            pnlTop.Controls.Add(lblPatient);

            cmbPatient = new LookUpEdit
            {
                Location = new Point(105, 41),
                Size = new Size(250, 28)
            };
            cmbPatient.Properties.DisplayMember = "AdSoyad";
            cmbPatient.Properties.ValueMember = "Id";
            cmbPatient.Properties.NullText = "-- Hasta seçiniz --";
            cmbPatient.Properties.Appearance.Font = new Font("Segoe UI", 9F);
            cmbPatient.Properties.PopupFormSize = new Size(320, 180);
            cmbPatient.Properties.Columns.Clear();
            cmbPatient.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AdSoyad", "Hasta Adi", 180));
            cmbPatient.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("BMIKategori", "Durum", 100));
            cmbPatient.EditValueChanged += CmbPatient_EditValueChanged;
            pnlTop.Controls.Add(cmbPatient);

            lblPatientInfo = new LabelControl
            {
                Text = "",
                Location = new Point(370, 45),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = InfoColor,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(450, 22)
            };
            pnlTop.Controls.Add(lblPatientInfo);

            // ============ ANA ICERIK - SplitContainer ============
            var splitMain = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = true,
                SplitterPosition = 550,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            // SOL PANEL - Hedefler
            CreateGoalsPanel(splitMain.Panel1);

            // SAG PANEL - Notlar
            CreateNotesPanel(splitMain.Panel2);

            // ONEMLI: Controls ekleme sirasi
            mainPanel.Controls.Add(splitMain);
            mainPanel.Controls.Add(pnlTop);

            this.Controls.Add(mainPanel);
        }

        private void CreateGoalsPanel(SplitGroupPanel panel)
        {
            panel.Padding = new Padding(5, 10, 5, 5);

            var grpGoals = new GroupControl
            {
                Text = "AKTIF HEDEFLER",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = SuccessColor,
                Appearance = { BackColor = CardColor }
            };

            // Buton paneli
            var pnlButtons = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 50,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.FromArgb(240, 253, 244)
            };

            var btnAddGoal = new SimpleButton
            {
                Text = "+ Hedef Ekle",
                Location = new Point(10, 10),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = SuccessColor, ForeColor = Color.White },
                AllowFocus = false
            };
            btnAddGoal.Click += BtnAddGoal_Click;
            pnlButtons.Controls.Add(btnAddGoal);

            var btnDeleteGoal = new SimpleButton
            {
                Text = "- Sil",
                Location = new Point(135, 10),
                Size = new Size(70, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = DangerColor, ForeColor = Color.White },
                AllowFocus = false
            };
            btnDeleteGoal.Click += BtnDeleteGoal_Click;
            pnlButtons.Controls.Add(btnDeleteGoal);

            var btnComplete = new SimpleButton
            {
                Text = "Tamamla",
                Location = new Point(215, 10),
                Size = new Size(90, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = InfoColor, ForeColor = Color.White },
                AllowFocus = false
            };
            btnComplete.Click += BtnComplete_Click;
            pnlButtons.Controls.Add(btnComplete);

            // Grid
            gridGoals = new GridControl 
            { 
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };
            viewGoals = new GridView(gridGoals)
            {
                OptionsView = {
                    ShowGroupPanel = false,
                    ShowIndicator = false,
                    ColumnAutoWidth = true,
                    RowAutoHeight = false,
                    EnableAppearanceEvenRow = true
                },
                OptionsBehavior = { Editable = false },
                RowHeight = 32
            };
            viewGoals.Appearance.Row.Font = new Font("Segoe UI", 9F);
            viewGoals.Appearance.EvenRow.BackColor = Color.FromArgb(240, 253, 244);
            viewGoals.Appearance.HeaderPanel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            viewGoals.Appearance.HeaderPanel.ForeColor = TextSecondary;

            viewGoals.Columns.Add(new GridColumn { FieldName = "GoalTypeName", Caption = "TIP", Width = 80, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "TargetValue", Caption = "HEDEF", Width = 60, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "CurrentValue", Caption = "MEVCUT", Width = 60, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "Unit", Caption = "BIRIM", Width = 50, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "ProgressPercentage", Caption = "%", Width = 50, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "Status", Caption = "DURUM", Width = 80, Visible = true });

            viewGoals.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "Status")
                {
                    string status = e.CellValue?.ToString() ?? "";
                    if (status == "Tamamlandi")
                        e.Appearance.ForeColor = SuccessColor;
                    else if (status == "Iptal")
                        e.Appearance.ForeColor = DangerColor;
                    else
                        e.Appearance.ForeColor = WarningColor;
                    e.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                }
            };

            gridGoals.MainView = viewGoals;

            // Kontrolleri ekleme sirasi
            grpGoals.Controls.Add(gridGoals);
            grpGoals.Controls.Add(pnlButtons);

            panel.Controls.Add(grpGoals);
        }

        private void CreateNotesPanel(SplitGroupPanel panel)
        {
            panel.Padding = new Padding(5, 10, 5, 5);

            var grpNotes = new GroupControl
            {
                Text = "DOKTOR NOTLARI",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AccentColor,
                Appearance = { BackColor = CardColor }
            };

            // Not yazma alani
            var pnlNoteInput = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 140,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.FromArgb(245, 243, 255)
            };

            var lblNote = new LabelControl
            {
                Text = "Yeni Not Ekle:",
                Location = new Point(10, 8),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            pnlNoteInput.Controls.Add(lblNote);

            txtNote = new MemoEdit
            {
                Location = new Point(10, 30),
                Size = new Size(450, 60),
                Properties = {
                    NullText = "Hasta hakkinda not yazin...",
                    Appearance = { Font = new Font("Segoe UI", 9F) },
                    ScrollBars = ScrollBars.Vertical
                }
            };
            txtNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlNoteInput.Controls.Add(txtNote);

            var btnAddNote = new SimpleButton
            {
                Text = "Not Ekle",
                Location = new Point(10, 98),
                Size = new Size(110, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = AccentColor, ForeColor = Color.White },
                AllowFocus = false
            };
            btnAddNote.Click += BtnAddNote_Click;
            pnlNoteInput.Controls.Add(btnAddNote);

            // Not listesi
            memoNotes = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = {
                    ReadOnly = true,
                    Appearance = { 
                        Font = new Font("Segoe UI", 9F),
                        BackColor = Color.FromArgb(250, 250, 255)
                    },
                    ScrollBars = ScrollBars.Vertical,
                    WordWrap = true
                },
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            // Kontrolleri ekleme sirasi
            grpNotes.Controls.Add(memoNotes);
            grpNotes.Controls.Add(pnlNoteInput);

            panel.Controls.Add(grpNotes);
        }

        private void LoadPatients()
        {
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                cmbPatient.Properties.DataSource = patients;
                
                if (patients.Count == 0)
                {
                    lblPatientInfo.Text = "Henuz hasta eklenmemis - Hastalarim sayfasindan hasta ekleyin";
                    lblPatientInfo.ForeColor = WarningColor;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hastalar yuklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbPatient_EditValueChanged(object sender, EventArgs e)
        {
            if (cmbPatient.EditValue != null)
            {
                _selectedPatientId = (int)cmbPatient.EditValue;
                var patient = _patientService.GetPatientById(_selectedPatientId);
                if (patient != null)
                {
                    lblPatientInfo.Text = $"Kilo: {patient.GuncelKilo} kg | BMI: {patient.BMI:F1} ({patient.BMIKategori})";
                    lblPatientInfo.ForeColor = InfoColor;
                }
                LoadGoals();
                LoadNotes();
            }
        }

        private void LoadGoals()
        {
            try
            {
                var goals = _goalService.GetActiveGoals(_selectedPatientId);
                _goals = new BindingList<Goal>(goals);
                gridGoals.DataSource = _goals;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hedefler yuklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadNotes()
        {
            try
            {
                var notes = _noteService.GetPatientNotes(_selectedPatientId);

                string allNotes = "";
                if (notes.Count == 0)
                {
                    allNotes = "Henuz not eklenmemis.\nYukaridaki alandan not ekleyebilirsiniz.";
                }
                else
                {
                    foreach (var note in notes)
                    {
                        allNotes += $"[{note.Date:dd.MM.yyyy HH:mm}] {note.DoctorName}\n";
                        allNotes += "───────────────────────────────\n";
                        allNotes += note.Content + "\n\n";
                    }
                }
                memoNotes.Text = allNotes;
            }
            catch (Exception ex)
            {
                memoNotes.Text = "Notlar yuklenirken hata: " + ex.Message;
            }
        }

        private void BtnAddGoal_Click(object sender, EventArgs e)
        {
            if (_selectedPatientId == 0)
            {
                XtraMessageBox.Show("Lütfen hasta seçin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var frm = new FrmAddGoal())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _goalService.CreateGoal(_selectedPatientId, frm.SelectedGoalType,
                            frm.TargetValue, frm.Unit, frm.EndDate);
                        LoadGoals();
                        XtraMessageBox.Show("Hedef eklendi", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Hedef eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDeleteGoal_Click(object sender, EventArgs e)
        {
            var selected = viewGoals.GetFocusedRow() as Goal;
            if (selected == null)
            {
                XtraMessageBox.Show("Lütfen silinecek hedefi seçin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = XtraMessageBox.Show("Bu hedefi silmek istediginizden emin misiniz?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _goalService.DeleteGoal(selected.Id);
                    LoadGoals();
                    XtraMessageBox.Show("Hedef silindi", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Hedef silinirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            var selected = viewGoals.GetFocusedRow() as Goal;
            if (selected == null)
            {
                XtraMessageBox.Show("Lütfen tamamlanacak hedefi seçin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                selected.CurrentValue = selected.TargetValue;
                _goalService.UpdateGoal(selected);
                LoadGoals();
                XtraMessageBox.Show("Hedef tamamlandi!", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hedef guncellenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                XtraMessageBox.Show("Not icerigi bos olamaz", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _noteService.AddNote(_selectedPatientId, AuthContext.UserId, AuthContext.UserName, txtNote.Text.Trim());
                txtNote.Text = string.Empty;
                LoadNotes();
                XtraMessageBox.Show("Not eklendi", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Not eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Name = "FrmGoalsNotes";
            this.ResumeLayout(false);
        }
    }

    public class FrmAddGoal : XtraForm
    {
        public GoalType SelectedGoalType { get; private set; }
        public double TargetValue { get; private set; }
        public string Unit { get; private set; }
        public DateTime? EndDate { get; private set; }

        private ComboBoxEdit cmbGoalType;
        private SpinEdit spnTarget;
        private TextEdit txtUnit;
        private DateEdit dateEnd;
        private CheckEdit chkHasEndDate;

        private readonly Color PrimaryColor = Color.FromArgb(79, 70, 229);
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);

        public FrmAddGoal()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Hedef Ekle";
            this.Size = new Size(420, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);

            int yPos = 20;
            var font = new Font("Segoe UI", 9F);
            var lblColor = Color.FromArgb(100, 116, 139);

            var lblType = new LabelControl { Text = "Hedef Tipi:", Location = new Point(20, yPos), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = lblColor };
            cmbGoalType = new ComboBoxEdit { Location = new Point(130, yPos - 2), Size = new Size(250, 28) };
            cmbGoalType.Properties.Items.AddRange(new[] { "Su Icme", "Kilo Verme", "Adim", "Uyku", "Protein Alimi" });
            cmbGoalType.Properties.Appearance.Font = font;
            cmbGoalType.SelectedIndex = 0;
            cmbGoalType.SelectedIndexChanged += (s, e) => {
                switch (cmbGoalType.SelectedIndex)
                {
                    case 0: txtUnit.Text = "litre"; break;
                    case 1: txtUnit.Text = "kg"; break;
                    case 2: txtUnit.Text = "adim"; break;
                    case 3: txtUnit.Text = "saat"; break;
                    case 4: txtUnit.Text = "gram"; break;
                }
            };
            this.Controls.Add(lblType);
            this.Controls.Add(cmbGoalType);

            yPos += 45;

            var lblTarget = new LabelControl { Text = "Hedef Deger:", Location = new Point(20, yPos), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = lblColor };
            spnTarget = new SpinEdit { Location = new Point(130, yPos - 2), Size = new Size(250, 28) };
            spnTarget.Properties.MinValue = 0;
            spnTarget.Properties.MaxValue = 100000;
            spnTarget.Properties.Appearance.Font = font;
            this.Controls.Add(lblTarget);
            this.Controls.Add(spnTarget);

            yPos += 45;

            var lblUnit = new LabelControl { Text = "Birim:", Location = new Point(20, yPos), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = lblColor };
            txtUnit = new TextEdit { Location = new Point(130, yPos - 2), Size = new Size(250, 28), Text = "litre" };
            txtUnit.Properties.Appearance.Font = font;
            this.Controls.Add(lblUnit);
            this.Controls.Add(txtUnit);

            yPos += 45;

            chkHasEndDate = new CheckEdit { Text = "Bitis Tarihi Belirle", Location = new Point(130, yPos), Font = font };
            chkHasEndDate.Properties.Appearance.ForeColor = lblColor;
            chkHasEndDate.CheckedChanged += (s, e) => dateEnd.Enabled = chkHasEndDate.Checked;
            this.Controls.Add(chkHasEndDate);

            yPos += 35;

            var lblEnd = new LabelControl { Text = "Bitis Tarihi:", Location = new Point(20, yPos), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = lblColor };
            dateEnd = new DateEdit { Location = new Point(130, yPos - 2), Size = new Size(250, 28), Enabled = false };
            dateEnd.DateTime = DateTime.Now.AddMonths(1);
            dateEnd.Properties.Appearance.Font = font;
            this.Controls.Add(lblEnd);
            this.Controls.Add(dateEnd);

            yPos += 50;

            var btnOk = new SimpleButton
            {
                Text = "Ekle",
                Location = new Point(200, yPos),
                Size = new Size(90, 36),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = SuccessColor, ForeColor = Color.White }
            };
            btnOk.Click += (s, e) => {
                if (spnTarget.Value <= 0)
                {
                    XtraMessageBox.Show("Hedef deger 0dan buyuk olmalidir", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SelectedGoalType = (GoalType)cmbGoalType.SelectedIndex;
                TargetValue = (double)spnTarget.Value;
                Unit = txtUnit.Text;
                EndDate = chkHasEndDate.Checked ? (DateTime?)dateEnd.DateTime : null;
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnOk);

            var btnCancel = new SimpleButton
            {
                Text = "Iptal",
                Location = new Point(300, yPos),
                Size = new Size(80, 36),
                Font = new Font("Segoe UI", 9F),
                Appearance = { BackColor = Color.FromArgb(107, 114, 128), ForeColor = Color.White }
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(420, 350);
            this.Name = "FrmAddGoal";
            this.ResumeLayout(false);
        }
    }
}
