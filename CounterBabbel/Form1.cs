using System;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScreenCapture.NET;
using Tesseract;

namespace CounterBabbel
{
	public partial class Form1 : Form
	{
		private HttpListener listener;

		// Selezione area
		private Rectangle selectionRect = new Rectangle(100, 100, 400, 200);

		// Componenti cattura e OCR
		private DX11ScreenCaptureService screenCaptureService;
		private IScreenCapture screenCapture;
		private ICaptureZone captureZone;
		private TesseractEngine ocrEngine;

		public Form1()
		{
			InitializeComponent();

			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.StartPosition = FormStartPosition.Manual;
			this.BackColor = Color.Blue;
			this.TransparencyKey = Color.Blue;

			int offset = 8;
			this.Bounds = new Rectangle(selectionRect.X, selectionRect.Y, selectionRect.Width + offset, selectionRect.Height + offset);

			this.TopMost = true;

			this.Resize += (s, e) => this.Invalidate();

			Button btn = new Button
			{
				Text = "Start Scan",
				Size = new Size(100, 30),
				Location = new Point(this.ClientSize.Width - 110, this.ClientSize.Height - 40),
				BackColor = Color.White,
				Cursor = Cursors.Hand,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};
			btn.Click += Button1_Click;
			this.Controls.Add(btn);

			this.Load += Form1_Load;
		}

		private void Button1_Click(object? sender, EventArgs e)
		{
			selectionRect = this.Bounds;
			InitializeCapture();
			StartListener();
			this.Hide();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// Inizializzazione eventuale all'avvio
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Rectangle rect = new Rectangle(2, 2,
				this.ClientSize.Width - 4, this.ClientSize.Height - 4);
			using (Pen p = new Pen(Color.Red, 4))
			{
				e.Graphics.DrawRectangle(p, rect);
			}
		}

		private void InitializeCapture()
		{
			screenCaptureService = new DX11ScreenCaptureService();

			var graphicsCards = screenCaptureService.GetGraphicsCards();
			if (graphicsCards == null || !graphicsCards.Any())
			{
				MessageBox.Show("Nessuna scheda grafica rilevata!");
				return;
			}

			var displays = screenCaptureService.GetDisplays(graphicsCards.First());
			if (displays == null || !displays.Any())
			{
				MessageBox.Show("Nessun display rilevato!");
				return;
			}

			screenCapture = screenCaptureService.GetScreenCapture(displays.First());
			if (screenCapture == null)
			{
				MessageBox.Show("Impossibile creare lo screenCapture!");
				return;
			}

			selectionRect = this.Bounds;
			captureZone = screenCapture.RegisterCaptureZone(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height);

			string tessdataPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
			if (!System.IO.Directory.Exists(tessdataPath))
			{
				MessageBox.Show($"Cartella tessdata non trovata: {tessdataPath}");
				return;
			}
			ocrEngine = new TesseractEngine(tessdataPath, "ita+eng", EngineMode.Default);
		}

		private void StartListener()
		{
			if (listener != null && listener.IsListening)
				return; // già in ascolto

			listener = new HttpListener();
			listener.Prefixes.Add("http://localhost:5000/");
			listener.Start();

			Task.Run(() =>
			{
				while (listener.IsListening)
				{
					var context = listener.GetContext();
					var response = context.Response;

					try
					{
						UpdateCaptureZone();

						var (imageData, width, height) = CaptureScreen();

						string text = ReadOCR(imageData, width, height);

						byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
						response.ContentLength64 = buffer.Length;
						response.OutputStream.Write(buffer, 0, buffer.Length);
					}
					catch (Exception ex)
					{
						byte[] buffer = System.Text.Encoding.UTF8.GetBytes($"Errore: {ex.Message}");
						response.ContentLength64 = buffer.Length;
						response.OutputStream.Write(buffer, 0, buffer.Length);
					}
					finally
					{
						response.OutputStream.Close();
					}
				}
			});
		}

		private void UpdateCaptureZone()
		{
			if (captureZone != null)
			{
				// Liberare captureZone se l'API lo consente.
			}
			selectionRect = this.Bounds;
			captureZone = screenCapture.RegisterCaptureZone(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height);
		}

		private (byte[] data, int width, int height) CaptureScreen()
		{
			screenCapture.CaptureScreen();

			byte[] imageData;
			int width, height;

			using (captureZone.Lock())
			{
				var image = captureZone.Image;
				width = image.Width;
				height = image.Height;

				imageData = new byte[width * height * 4];
				int idx = 0;
				for (int y = 0; y < height; y++)
					for (int x = 0; x < width; x++)
					{
						var color = image[x, y];
						imageData[idx++] = color.B;
						imageData[idx++] = color.G;
						imageData[idx++] = color.R;
						imageData[idx++] = color.A;
					}
			}
			return (imageData, width, height);
		}

		private string ReadOCR(byte[] imageData, int width, int height)
		{
			byte[] bmpData = CreateBMP(imageData, width, height);

			using var pix = Pix.LoadFromMemory(bmpData);
			using var page = ocrEngine.Process(pix);
			string text = page.GetText().Trim();

			return string.IsNullOrEmpty(text) ? "[Nessun testo rilevato]" : text;
		}

		private byte[] CreateBMP(byte[] pixelData, int width, int height)
		{
			int stride = width * 4;
			int imageSize = stride * height;
			int fileSize = 54 + imageSize;

			byte[] bmp = new byte[fileSize];

			bmp[0] = 0x42; bmp[1] = 0x4D;
			BitConverter.GetBytes(fileSize).CopyTo(bmp, 2);
			BitConverter.GetBytes(54).CopyTo(bmp, 10);

			BitConverter.GetBytes(40).CopyTo(bmp, 14);
			BitConverter.GetBytes(width).CopyTo(bmp, 18);
			BitConverter.GetBytes(height).CopyTo(bmp, 22);
			BitConverter.GetBytes((short)1).CopyTo(bmp, 26);
			BitConverter.GetBytes((short)32).CopyTo(bmp, 28);
			BitConverter.GetBytes(imageSize).CopyTo(bmp, 34);

			for (int y = 0; y < height; y++)
			{
				int srcRow = y * stride;
				int dstRow = (height - 1 - y) * stride;
				Array.Copy(pixelData, srcRow, bmp, 54 + dstRow, stride);
			}

			return bmp;
		}
	}
}
