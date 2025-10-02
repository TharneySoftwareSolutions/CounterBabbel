namespace CounterBabbelClient
{
	public partial class Main : Form
	{
		private readonly HttpClient httpClient = new HttpClient();

		private TextBox txtOriginal;
		private TextBox txtTranslated;
		private Button btnTranslate;
		public Main()
		{
			InitializeComponent();
			this.Text = "OCR + Traduzione";

			txtOriginal = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Top, Height = 150 };
			txtTranslated = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Top, Height = 150 };
			btnTranslate = new Button { Text = "Ottieni testo OCR e traduci", Dock = DockStyle.Top };

			btnTranslate.Click += BtnTranslate_Click;

			this.Controls.Add(txtTranslated);
			this.Controls.Add(txtOriginal);
			this.Controls.Add(btnTranslate);
		}
		private async void BtnTranslate_Click(object sender, EventArgs e)
		{
			try
			{
				// Chiama l'endpoint OCR
				string ocrText = await httpClient.GetStringAsync("http://localhost:5000/");

				txtOriginal.Text = ocrText;

				// Traduci il testo (puoi sostituire con chiamata a API traduzione)
				string translatedText = TranslateText(ocrText);
				txtTranslated.Text = translatedText;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Errore nel recupero o traduzione testo: " + ex.Message);
			}
		}

		private string TranslateText(string input)
		{
			// Qui puoi usare un'API di traduzione. Per esempio finta inversione testo:
			if (string.IsNullOrWhiteSpace(input)) return "";
			// Layout di esempio: sostituire con reale API
			return new string(input.Reverse().ToArray());
		}
	}
}
