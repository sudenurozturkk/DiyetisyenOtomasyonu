using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraCharts;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using PatientEntity = DiyetisyenOtomasyonu.Domain.Patient;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Hasta Profil Sayfasi
    /// Doktor hasta profilini goruntuler: ilerleme, hedefler, notlar, mesajlasma
    /// 
    /// OOP Principle: Single Responsibility - Hasta profil goruntuleme
    /// Academic: Comprehensive patient profile view for healthcare management
    /// </summary>
    public partial class FrmPatientProfile : XtraForm
    {
        private readonly int _patientId;
        private readonly PatientService _patientService;
        private readonly GoalService _goalService;
        private readonly NoteService _noteService;
        private readonly MessageService _messageService;
        private readonly MealService _mealService;
        private readonly AiAssistantService _aiService;

        private PatientEntity _patient;

        // UI Components
        private PanelControl headerPanel;
        private DevExpress.XtraTab.XtraTabControl tabControl;

        // Renkler
        private readonly Color PrimaryColor = Color.FromArgb(79, 70, 229);
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);
        private readonly Color DangerColor = Color.FromArgb(239, 68, 68);
        private readonly Color WarningColor = Color.FromArgb(245, 158, 11);
        private readonly Color InfoColor = Color.FromArgb(14, 165, 233);
        private readonly Color CardColor = Color.White;
        private readonly Color BackgroundColor = Color.FromArgb(248, 250, 252);
        private readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private readonly Color TextSecondary = Color.FromArgb(100, 116, 139);

        public FrmPatientProfile(int patientId)
        {
            _patientId = patientId;
            _patientService = new PatientService();
            _goalService = new GoalService();
            _noteService = new NoteService();
            _messageService = new MessageService();
            _mealService = new MealService();
            _aiService = new AiAssistantService();

            InitializeComponent();
            LoadPatient();
            InitializeUI();
        }

        private void LoadPatient()
        {
            _patient = _patientService.GetPatientById(_patientId);
        }

        private void InitializeUI()
        {
            this.Text = "Hasta Profili";
            this.BackColor = BackgroundColor;
            this.Padding = new Padding(15);

            if (_patient == null)
            {
                var lblError = new LabelControl
                {
                    Text = "Hasta bulunamadi!",
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = DangerColor,
                    Location = new Point(20, 20)
                };
                this.Controls.Add(lblError);
                return;
            }

            // Header Panel - Hasta Bilgileri
            CreateHeaderPanel();

            // Tab Control - Ilerleme, Hedefler, Notlar, Mesajlar
            CreateTabControl();
        }

        private void CreateHeaderPanel()
        {
            headerPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 180,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor,
                Padding = new Padding(20)
            };

            // Hasta Adi ve BMI Badge
            var lblPatientName = new LabelControl
            {
                Text = _patient.AdSoyad,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(20, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(400, 35)
            };
            headerPanel.Controls.Add(lblPatientName);

            // BMI Badge
            Color bmiColor = _patient.BMI < 18.5 ? WarningColor : 
                             _patient.BMI < 25 ? SuccessColor : 
                             _patient.BMI < 30 ? WarningColor : DangerColor;

            var lblBMI = new LabelControl
            {
                Text = $"BMI: {_patient.BMI:F1} - {_patient.BMIKategori}",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bmiColor,
                Location = new Point(420, 20),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(180, 28),
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            };
            headerPanel.Controls.Add(lblBMI);

            // Istatistik Kartlari
            int statY = 60;
            int statWidth = 150;
            int statSpacing = 20;

            CreateStatCard(headerPanel, 20, statY, statWidth, "Yas", _patient.Yas.ToString(), InfoColor);
            CreateStatCard(headerPanel, 20 + statWidth + statSpacing, statY, statWidth, "Boy", $"{_patient.Boy} cm", PrimaryColor);
            CreateStatCard(headerPanel, 20 + 2 * (statWidth + statSpacing), statY, statWidth, "Kilo", $"{_patient.GuncelKilo} kg", SuccessColor);
            CreateStatCard(headerPanel, 20 + 3 * (statWidth + statSpacing), statY, statWidth, "Baslangic", $"{_patient.BaslangicKilosu} kg", TextSecondary);

            // Kilo Degisimi
            double kiloDiff = _patient.GuncelKilo - _patient.BaslangicKilosu;
            string kiloText = kiloDiff >= 0 ? $"+{kiloDiff:F1} kg" : $"{kiloDiff:F1} kg";
            Color kiloColor = kiloDiff <= 0 ? SuccessColor : DangerColor;

            CreateStatCard(headerPanel, 20 + 4 * (statWidth + statSpacing), statY, statWidth, "Degisim", kiloText, kiloColor);

            // Kayit Tarihi
            var lblRegDate = new LabelControl
            {
                Text = $"Kayit: {_patient.KayitTarihi:dd.MM.yyyy}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(20, 145),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 20)
            };
            headerPanel.Controls.Add(lblRegDate);

            this.Controls.Add(headerPanel);
        }

        private void CreateStatCard(PanelControl parent, int x, int y, int width, string label, string value, Color color)
        {
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, 70),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            var lblLabel = new LabelControl
            {
                Text = label,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(10, 8),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 20, 18)
            };
            card.Controls.Add(lblLabel);

            var lblValue = new LabelControl
            {
                Text = value,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(10, 30),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 20, 30)
            };
            card.Controls.Add(lblValue);

            parent.Controls.Add(card);
        }

        private void CreateTabControl()
        {
            tabControl = new DevExpress.XtraTab.XtraTabControl
            {
                Dock = DockStyle.Fill
            };
            tabControl.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // Tab 1: Ilerleme (Kilo Grafigi)
            var tabProgress = new DevExpress.XtraTab.XtraTabPage { Text = "Ilerleme" };
            CreateProgressTab(tabProgress);
            tabControl.TabPages.Add(tabProgress);

            // Tab 2: Hedefler
            var tabGoals = new DevExpress.XtraTab.XtraTabPage { Text = "Hedefler" };
            CreateGoalsTab(tabGoals);
            tabControl.TabPages.Add(tabGoals);

            // Tab 3: Notlar
            var tabNotes = new DevExpress.XtraTab.XtraTabPage { Text = "Notlar" };
            CreateNotesTab(tabNotes);
            tabControl.TabPages.Add(tabNotes);

            // Tab 4: Mesajlar
            var tabMessages = new DevExpress.XtraTab.XtraTabPage { Text = "Mesajlar" };
            CreateMessagesTab(tabMessages);
            tabControl.TabPages.Add(tabMessages);

            // Tab 5: Diyet Plani
            var tabDiet = new DevExpress.XtraTab.XtraTabPage { Text = "Diyet Plani" };
            CreateDietTab(tabDiet);
            tabControl.TabPages.Add(tabDiet);

            // Tab 6: AI Analizi
            var tabAI = new DevExpress.XtraTab.XtraTabPage { Text = "AI Analizi" };
            CreateAIAnalysisTab(tabAI);
            tabControl.TabPages.Add(tabAI);

            this.Controls.Add(tabControl);
        }

        private void CreateProgressTab(DevExpress.XtraTab.XtraTabPage tab)
        {
            tab.BackColor = BackgroundColor;

            // Kilo Grafigi
            var chartWeight = new ChartControl
            {
                Dock = DockStyle.Fill
            };
            chartWeight.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;

            try
            {
                var weightHistory = _patientService.GetWeightHistory(_patientId, 90); // Son 90 gun
                if (weightHistory.Count > 0)
                {
                    var series = new Series("Kilo (kg)", ViewType.Spline);
                    series.ArgumentScaleType = ScaleType.DateTime;

                    foreach (var entry in weightHistory.OrderBy(w => w.Date))
                    {
                        series.Points.Add(new SeriesPoint(entry.Date, entry.Weight));
                    }

                    var view = (SplineSeriesView)series.View;
                    view.LineStyle.Thickness = 3;
                    view.Color = InfoColor;
                    view.MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;

                    chartWeight.Series.Add(series);
                }
                else
                {
                    var title = new ChartTitle { Text = "Henuz kilo verisi yok" };
                    chartWeight.Titles.Add(title);
                }
            }
            catch
            {
                var title = new ChartTitle { Text = "Grafik yuklenemedi" };
                chartWeight.Titles.Add(title);
            }

            tab.Controls.Add(chartWeight);
        }

        private void CreateGoalsTab(DevExpress.XtraTab.XtraTabPage tab)
        {
            tab.BackColor = BackgroundColor;
            tab.Padding = new Padding(15);

            var grpGoals = new GroupControl
            {
                Text = "HASTA HEDEFLERI",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = SuccessColor
            };

            // Butonlar
            var pnlButtons = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 50,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            var btnAddGoal = new SimpleButton
            {
                Text = "+ Hedef Ekle",
                Location = new Point(10, 10),
                Size = new Size(120, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = SuccessColor, ForeColor = Color.White }
            };
            btnAddGoal.Click += BtnAddGoal_Click;
            pnlButtons.Controls.Add(btnAddGoal);

            grpGoals.Controls.Add(pnlButtons);

            // Grid
            var gridGoals = new GridControl { Dock = DockStyle.Fill, Tag = "gridGoals" };
            var viewGoals = new GridView(gridGoals)
            {
                OptionsView = { ShowGroupPanel = false, ShowIndicator = false, ColumnAutoWidth = true },
                OptionsBehavior = { Editable = false },
                RowHeight = 35
            };
            viewGoals.Columns.Add(new GridColumn { FieldName = "GoalTypeName", Caption = "Hedef Tipi", Width = 100, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "TargetValue", Caption = "Hedef", Width = 80, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "CurrentValue", Caption = "Mevcut", Width = 80, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "Unit", Caption = "Birim", Width = 60, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "ProgressPercentage", Caption = "Ilerleme %", Width = 90, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "Status", Caption = "Durum", Width = 100, Visible = true });

            gridGoals.MainView = viewGoals;

            try
            {
                var goals = _goalService.GetActiveGoals(_patientId);
                gridGoals.DataSource = new BindingList<Goal>(goals);
            }
            catch { }

            grpGoals.Controls.Add(gridGoals);
            tab.Controls.Add(grpGoals);
        }

        private void BtnAddGoal_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmAddGoal())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _goalService.CreateGoal(_patientId, frm.SelectedGoalType, frm.TargetValue, frm.Unit, frm.EndDate);
                        
                        // Refresh grid
                        var grid = FindControlByTag<GridControl>(tabControl, "gridGoals");
                        if (grid != null)
                        {
                            var goals = _goalService.GetActiveGoals(_patientId);
                            grid.DataSource = new BindingList<Goal>(goals);
                        }
                        
                        XtraMessageBox.Show("Hedef eklendi!", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CreateNotesTab(DevExpress.XtraTab.XtraTabPage tab)
        {
            tab.BackColor = BackgroundColor;
            tab.Padding = new Padding(15);

            var grpNotes = new GroupControl
            {
                Text = "DOKTOR NOTLARI",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = PrimaryColor
            };

            // Not ekleme paneli
            var pnlInput = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 120,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Padding = new Padding(10)
            };

            var txtNote = new MemoEdit
            {
                Location = new Point(10, 10),
                Size = new Size(500, 60),
                Properties = { NullText = "Not yazin...", Appearance = { Font = new Font("Segoe UI", 10F) } },
                Tag = "txtNote"
            };
            pnlInput.Controls.Add(txtNote);

            var btnAddNote = new SimpleButton
            {
                Text = "Not Ekle",
                Location = new Point(10, 78),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White }
            };
            btnAddNote.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtNote.Text))
                {
                    XtraMessageBox.Show("Not bos olamaz", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    _noteService.AddNote(_patientId, AuthContext.UserId, AuthContext.UserName, txtNote.Text.Trim());
                    txtNote.Text = "";
                    LoadNotes();
                    XtraMessageBox.Show("Not eklendi!", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            pnlInput.Controls.Add(btnAddNote);

            grpNotes.Controls.Add(pnlInput);

            // Notlar listesi
            var memoNotes = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { ReadOnly = true, Appearance = { Font = new Font("Segoe UI", 10F) }, WordWrap = true },
                Tag = "memoNotes"
            };
            grpNotes.Controls.Add(memoNotes);

            tab.Controls.Add(grpNotes);

            LoadNotes();
        }

        private void LoadNotes()
        {
            var memoNotes = FindControlByTag<MemoEdit>(tabControl, "memoNotes");
            if (memoNotes == null) return;

            try
            {
                var notes = _noteService.GetPatientNotes(_patientId);
                string allNotes = "";
                foreach (var note in notes)
                {
                    allNotes += $"[{note.Date:dd.MM.yyyy HH:mm}] {note.DoctorName}:\n{note.Content}\n\n";
                }
                memoNotes.Text = allNotes.Length > 0 ? allNotes : "Henuz not eklenmemis.";
            }
            catch { memoNotes.Text = "Notlar yuklenemedi."; }
        }

        private void CreateMessagesTab(DevExpress.XtraTab.XtraTabPage tab)
        {
            tab.BackColor = BackgroundColor;
            tab.Padding = new Padding(15);

            var grpMessages = new GroupControl
            {
                Text = "HASTA ILE MESAJLASMA",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = WarningColor
            };

            // Mesaj gecmisi
            var memoMessages = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { ReadOnly = true, Appearance = { Font = new Font("Segoe UI", 10F) }, WordWrap = true },
                Tag = "memoMessages"
            };
            grpMessages.Controls.Add(memoMessages);

            // Mesaj yazma paneli
            var pnlInput = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Padding = new Padding(10)
            };

            var txtMessage = new MemoEdit
            {
                Location = new Point(10, 10),
                Size = new Size(500, 45),
                Properties = { NullText = "Mesaj yazin...", Appearance = { Font = new Font("Segoe UI", 10F) } },
                Tag = "txtMessage"
            };
            pnlInput.Controls.Add(txtMessage);

            var btnSend = new SimpleButton
            {
                Text = "Gonder",
                Location = new Point(10, 60),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = WarningColor, ForeColor = Color.White }
            };
            btnSend.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtMessage.Text))
                {
                    XtraMessageBox.Show("Mesaj bos olamaz", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    _messageService.SendMessage(AuthContext.UserId, _patientId, txtMessage.Text.Trim());
                    txtMessage.Text = "";
                    LoadMessages();
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            pnlInput.Controls.Add(btnSend);

            grpMessages.Controls.Add(pnlInput);
            tab.Controls.Add(grpMessages);

            LoadMessages();
        }

        private void LoadMessages()
        {
            var memoMessages = FindControlByTag<MemoEdit>(tabControl, "memoMessages");
            if (memoMessages == null) return;

            try
            {
                var messages = _messageService.GetConversation(AuthContext.UserId, _patientId);
                string allMessages = "";
                foreach (var msg in messages)
                {
                    string sender = msg.FromUserId == AuthContext.UserId ? "Siz" : _patient.AdSoyad;
                    allMessages += $"[{msg.SentAt:dd.MM.yyyy HH:mm}] {sender}:\n{msg.Content}\n\n";
                }
                memoMessages.Text = allMessages.Length > 0 ? allMessages : "Henuz mesaj yok.";
            }
            catch { memoMessages.Text = "Mesajlar yuklenemedi."; }
        }

        private void CreateDietTab(DevExpress.XtraTab.XtraTabPage tab)
        {
            tab.BackColor = BackgroundColor;
            tab.Padding = new Padding(15);

            var grpDiet = new GroupControl
            {
                Text = "BU HAFTAKI DIYET PLANI",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = SuccessColor
            };

            var gridDiet = new GridControl { Dock = DockStyle.Fill };
            var viewDiet = new GridView(gridDiet)
            {
                OptionsView = { ShowGroupPanel = false, ShowIndicator = false, ColumnAutoWidth = true },
                OptionsBehavior = { Editable = false },
                RowHeight = 35
            };
            viewDiet.Columns.Add(new GridColumn { FieldName = "DayName", Caption = "Gun", Width = 100, Visible = true });
            viewDiet.Columns.Add(new GridColumn { FieldName = "MealTimeName", Caption = "Ogun", Width = 100, Visible = true });
            viewDiet.Columns.Add(new GridColumn { FieldName = "MealName", Caption = "Yemek", Width = 200, Visible = true });
            viewDiet.Columns.Add(new GridColumn { FieldName = "Calories", Caption = "Kalori", Width = 80, Visible = true });
            viewDiet.Columns.Add(new GridColumn { FieldName = "IsConsumed", Caption = "Yendi mi?", Width = 80, Visible = true });

            gridDiet.MainView = viewDiet;

            try
            {
                var weekStart = GetMonday(DateTime.Now);
                var assignments = _mealService.GetWeeklyPlan(_patientId, weekStart);
                gridDiet.DataSource = new BindingList<PatientMealAssignment>(assignments);
            }
            catch { }

            grpDiet.Controls.Add(gridDiet);
            tab.Controls.Add(grpDiet);
        }

        private void CreateAIAnalysisTab(DevExpress.XtraTab.XtraTabPage tab)
        {
            tab.BackColor = BackgroundColor;
            tab.Padding = new Padding(20);

            var pnlAI = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            var lblTitle = new LabelControl
            {
                Text = "YAPAY ZEKA DESTEKLİ HASTA ANALİZİ",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                Location = new Point(0, 0)
            };
            pnlAI.Controls.Add(lblTitle);

            var lblDesc = new LabelControl
            {
                Text = "Bu modül hastanın son 30 günlük verilerini (kilo, öğün uyumu, egzersiz) analiz ederek doktora profesyonel öneriler sunar.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextSecondary,
                Location = new Point(0, 30),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(600, 20)
            };
            pnlAI.Controls.Add(lblDesc);

            var btnRunAI = new SimpleButton
            {
                Text = "Analizi Başlat",
                Location = new Point(0, 65),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White }
            };
            pnlAI.Controls.Add(btnRunAI);

            // Sonuç Paneli
            var grpResult = new GroupControl
            {
                Text = "ANALİZ SONUÇLARI",
                Location = new Point(0, 120),
                Size = new Size(800, 400),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            var lblResultTitle = new LabelControl
            {
                Text = "Sonuç:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(15, 40)
            };
            grpResult.Controls.Add(lblResultTitle);

            var lblResultValue = new LabelControl
            {
                Text = "Analiz bekleniyor...",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(15, 60),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(770, 25),
                Tag = "lblAIResult"
            };
            grpResult.Controls.Add(lblResultValue);

            var lblRecTitle = new LabelControl
            {
                Text = "Profesyonel Öneriler:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(15, 100)
            };
            grpResult.Controls.Add(lblRecTitle);

            var memoRecs = new MemoEdit
            {
                Location = new Point(15, 125),
                Size = new Size(770, 250),
                Properties = { ReadOnly = true, Appearance = { Font = new Font("Segoe UI", 10F) } },
                Tag = "memoAIRecs"
            };
            grpResult.Controls.Add(memoRecs);

            pnlAI.Controls.Add(grpResult);
            tab.Controls.Add(pnlAI);

            btnRunAI.Click += async (s, e) => {
                btnRunAI.Enabled = false;
                btnRunAI.Text = "Analiz Ediliyor...";
                
                try
                {
                    var analysis = await _aiService.GetAdvancedAnalysisAsync(_patientId);
                    lblResultValue.Text = analysis.Result;
                    lblResultValue.ForeColor = PrimaryColor;
                    memoRecs.Text = analysis.Recommendations;
                    
                    XtraMessageBox.Show("Yapay zeka analizi tamamlandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Analiz sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnRunAI.Enabled = true;
                    btnRunAI.Text = "Analizi Başlat";
                }
            };
        }

        private T FindControlByTag<T>(Control parent, string tag) where T : Control
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is T && ctrl.Tag?.ToString() == tag)
                    return (T)ctrl;

                var found = FindControlByTag<T>(ctrl, tag);
                if (found != null)
                    return found;
            }
            return null;
        }

        private DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Name = "FrmPatientProfile";
            this.ResumeLayout(false);
        }
    }
}

