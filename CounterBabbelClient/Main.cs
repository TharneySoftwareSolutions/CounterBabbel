using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using Google.Cloud.Translate.V3;

namespace CounterBabbelClient
{
	public partial class Main : Form
	{
		private readonly HttpClient httpClient = new HttpClient();
		private TranslationServiceClient translationClient;

		private TextBox txtConsole;
		private TrackBar trackBarDelay;
		private Label labelDelay;

		private int delayMs = 1000;
		private System.Threading.Timer scanTimer;
		private bool isScanning = false;

		// Configura il tuo project ID di Google Cloud
		private const string projectId = "your-google-cloud-project-id";
		private const string locationId = "global"; // o "us-central1" per esempio

		public Main()
		{
			InitializeComponent();
			this.Text = "OCR + Traduzione";

			// Configura le credenziali Google (sostituisci con il percorso del tuo file JSON)
			Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "path-to-your-json-key-file.json");
			translationClient = TranslationServiceClient.Create();

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
				TickFrequency = 100,
				SmallChange = 100,
				LargeChange = 100,
				Value = delayMs,
				Dock = DockStyle.Top,
				Height = 45
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

			// Add controls in order (top to bottom)
			this.Controls.Add(txtConsole);
			this.Controls.Add(trackBarDelay);
			this.Controls.Add(labelDelay);

			// Avvia scansione automatica
			StartScanning();
		}

		private void TrackBarDelay_Scroll(object sender, EventArgs e)
		{
			// Arrotonda al multiplo di 100 più vicino
			delayMs = (trackBarDelay.Value / 100) * 100;
			trackBarDelay.Value = delayMs;
			labelDelay.Text = $"Delay: {delayMs} ms";

			// Riavvia il timer con il nuovo delay
			RestartScanning();
		}

		private void StartScanning()
		{
			if (isScanning)
				return;

			isScanning = true;
			scanTimer = new System.Threading.Timer(async (state) =>
			{
				await PerformScan();
			}, null, 0, delayMs);
		}

		private void RestartScanning()
		{
			StopScanning();
			StartScanning();
		}

		private void StopScanning()
		{
			if (scanTimer != null)
			{
				scanTimer.Dispose();
				scanTimer = null;
			}
			isScanning = false;
		}

		private async System.Threading.Tasks.Task PerformScan()
		{
			try
			{
				// Chiama l'endpoint OCR
				string ocrText = await httpClient.GetStringAsync("http://localhost:5000/");

				// Traduci il testo con Google Translate V3
				string translatedText = await TranslateTextAsync(ocrText, "it"); // Traduci in italiano

				// Aggiorna la console sulla UI thread
				this.Invoke((MethodInvoker)delegate
				{
					txtConsole.Text = translatedText; // Sostituisce il testo precedente
				});
			}
			catch (Exception ex)
			{
				// Log errore sulla console
				this.Invoke((MethodInvoker)delegate
				{
					txtConsole.Text = $"Errore: {ex.Message}";
				});
			}
		}

		private async System.Threading.Tasks.Task<string> TranslateTextAsync(string input, string targetLanguageCode)
		{
			if (string.IsNullOrWhiteSpace(input)) return "";

			try
			{
				var request = new TranslateTextRequest
				{
					Contents = { input },
					TargetLanguageCode = targetLanguageCode,
					Parent = $"projects/{projectId}/locations/{locationId}",
					MimeType = "text/plain"
				};

				TranslateTextResponse response = await translationClient.TranslateTextAsync(request);

				if (response.Translations.Count > 0)
				{
					return response.Translations[0].TranslatedText;
				}

				return "Nessuna traduzione disponibile";
			}
			catch (Exception ex)
			{
				return $"Errore traduzione: {ex.Message}";
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			StopScanning();
			base.OnFormClosing(e);
		}
	}
}
