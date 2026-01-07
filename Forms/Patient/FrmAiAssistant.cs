using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmAiAssistant : XtraForm
    {
        private readonly AiAssistantService _aiService;
        private ListBoxControl listChat;
        private MemoEdit txtQuestion;
        private SimpleButton btnAsk;
        private GroupControl grpQuickButtons;

        public FrmAiAssistant()
        {
            InitializeComponent();
            _aiService = new AiAssistantService();
            InitializeUI();
            AddWelcomeMessage();
        }

        private void InitializeUI()
        {
            this.Text = "AI Asistan";
            this.Size = new Size(900, 650);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = UiStyles.BackgroundColor;

            // Sağ panel - Chat alanı (add FIRST since it's Dock.Fill)
            var pnlChat = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = UiStyles.CardBackgroundColor
            };

            // Input panel at bottom (add to pnlChat FIRST)
            var pnlInput = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = UiStyles.CardBackgroundColor
            };

            txtQuestion = new MemoEdit
            {
                Location = new Point(10, 10),
                Size = new Size(580, 70),
                Properties = { NullText = "Sorunuzu buraya yazın..." }
            };
            pnlInput.Controls.Add(txtQuestion);

            btnAsk = new SimpleButton
            {
                Text = "Sor",
                Location = new Point(10, 85),
                Size = new Size(100, 28),
                Font = UiStyles.BodyBoldFont,
                Appearance = { BackColor = UiStyles.PrimaryColor, ForeColor = Color.White }
            };
            btnAsk.Click += BtnAsk_Click;
            pnlInput.Controls.Add(btnAsk);

            var btnClear = new SimpleButton
            {
                Text = "Temizle",
                Location = new Point(120, 85),
                Size = new Size(100, 28)
            };
            btnClear.Click += (s, e) => {
                listChat.Items.Clear();
                AddWelcomeMessage();
            };
            pnlInput.Controls.Add(btnClear);

            pnlChat.Controls.Add(pnlInput);

            // Title at top (add SECOND)
            var lblTitle = new LabelControl
            {
                Text = "🤖 AI Beslenme Asistanı",
                Dock = DockStyle.Top,
                Height = 50,
                Font = UiStyles.TitleFont,
                ForeColor = UiStyles.PrimaryColor,
                Padding = new Padding(15, 15, 0, 0),
                BackColor = UiStyles.CardBackgroundColor
            };
            pnlChat.Controls.Add(lblTitle);

            // Chat list (add LAST since it's Dock.Fill)
            listChat = new ListBoxControl
            {
                Dock = DockStyle.Fill
            };
            pnlChat.Controls.Add(listChat);

            this.Controls.Add(pnlChat);

            // Sol panel - Hızlı sorular (add AFTER pnlChat)
            grpQuickButtons = new GroupControl
            {
                Text = "Hızlı Sorular",
                Dock = DockStyle.Left,
                Width = 200,
                Font = UiStyles.BodyBoldFont,
                BackColor = UiStyles.CardBackgroundColor
            };

            int yPos = 40;
            string[] questions = {
                "Öğün Öner",
                "Alışveriş Listesi",
                "Dışarıda Yemek",
                "Su Tüketimi",
                "Kilo Verme",
                "Egzersiz"
            };

            foreach (var q in questions)
            {
                var btn = new SimpleButton
                {
                    Text = q,
                    Location = new Point(10, yPos),
                    Size = new Size(170, 35)
                };
                btn.Click += (s, e) => AskQuestion(q);
                grpQuickButtons.Controls.Add(btn);
                yPos += 45;
            }

            this.Controls.Add(grpQuickButtons);
        }

        private void AddWelcomeMessage()
        {
            listChat.Items.Add("🤖 AI Asistan: Merhaba! Size nasıl yardımcı olabilirim?\n" +
                              "Sol taraftaki hızlı sorulardan birini seçebilir veya kendi sorunuzu yazabilirsiniz.\n");
        }

        private void BtnAsk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQuestion.Text))
            {
                XtraMessageBox.Show("Lütfen bir soru yazın", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AskQuestion(txtQuestion.Text);
            txtQuestion.Text = string.Empty;
        }

        private void AskQuestion(string question)
        {
            // Kullanıcı sorusu
            listChat.Items.Add($"👤 Siz: {question}\n");

            // AI cevabı
            var answer = _aiService.GetResponse(question);
            listChat.Items.Add($"🤖 AI Asistan:\n{answer}\n");
            listChat.Items.Add("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

            // Son mesaja scroll
            if (listChat.Items.Count > 0)
                listChat.SelectedIndex = listChat.Items.Count - 1;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(900, 650);
            this.Name = "FrmAiAssistant";
            this.ResumeLayout(false);
        }
    }
}
