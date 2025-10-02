using System;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;

namespace CounterBabbelClient
{
	public partial class Main : Form
	{
		private readonly HttpClient httpClient = new HttpClient();

		private TextBox txtOriginal;
		private TextBox txtConsole;
		private TrackBar trackBarDelay;
		private Label labelDelay;
		private Button btnTranslate;

		private int delayMs = 1000;

		public Main()
		{
			InitializeComponent();
			this.Text = "OCR + Traduzione";

			// Original text
			txtOriginal = new TextBox
			{
				Multiline = true,
				ScrollBars = ScrollBars.Vertical,
				Dock = DockStyle.Top,
				Height = 150,
				ReadOnly = true
			};

			// Label to show delay value
			labelDelay = new Label
			{
				Text = $"Delay: {delayMs} ms",
				Dock = DockStyle.Top,
				Height = 20,
				TextAlign = System.Drawing.ContentAlignment.MiddleCenter
			};

			// TrackBar to select delay
			trackBarDelay = new TrackBar
			{
				Minimum = 500,
				Maximum = 5000,
				TickFrequency = 500,
				SmallChange = 100,
				LargeChange = 500,
				Value = delayMs,
				Dock = DockStyle.Top
			};
			trackBarDelay.Scroll += TrackBarDelay_Scroll;

			// Console-style TextBox for translated text
			txtConsole = new TextBox
			{
				Multiline = true,
				ScrollBars = ScrollBars.Vertical,
				Dock = DockStyle.Fill,
				ReadOnly = true,
				BackColor = System.Drawing.Color.Black,
				ForeColor = System.Drawing.Color.LimeGreen,
				Font = new System.Drawing.Font("Consolas", 10)
			};

			btnTranslate = new Button
			{
				Text = "Ottieni testo OCR e traduci",
				Dock = DockStyle.Top,
				Height = 30,
				BackColor = System.Drawing.Color.White,
				Cursor = Cursors.Hand
			};
			btnTranslate.Click += BtnTranslate_Click;

			// Add controls in order (top to bottom)
			this.Controls.Add(txtConsole);
			this.Controls.Add(trackBarDelay);
			this.Controls.Add(labelDelay);
			this.Controls.Add(btnTranslate);
			this.Controls.Add(txtOriginal);
		}

		private void TrackBarDelay_Scroll(object sender, EventArgs e)
		{
			delayMs = trackBarDelay.Value;
			labelDelay.Text = $"Delay: {delayMs} ms";
		}

		private async void BtnTranslate_Click(object sender, EventArgs e)
		{
			try
			{
				// Chiama l'endpoint OCR
				string ocrText = await httpClient.GetStringAsync("http://localhost:5000/");

				txtOriginal.Text = ocrText;

				// Traduci il testo (qui inversione di esempio)
				string translatedText = TranslateText(ocrText);

				// Append al "console" con nuova linea
				if (txtConsole.Text.Length > 0)
					txtConsole.AppendText(Environment.NewLine);
				txtConsole.AppendText(translatedText);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Errore nel recupero o traduzione testo: " + ex.Message);
			}
		}

		private string TranslateText(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return "";

			// Logica finta di traduzione, qui invertiamo il testo
			return new string(input.Reverse().ToArray());
		}
	}
}
