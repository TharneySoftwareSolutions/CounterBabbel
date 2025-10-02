# CounterBabbel
CounterBabbel è traduttore simultaneo di porzioni di schermo.
Ideale per tradurre rapidamente le chat di gioco.

Attraverso DirectX11 un app server cattura una porzione dello schermo in seguito ad una chiamata HTTP.
Tale chiamata esegue una scansione dello schermo e la passa ad una libreria di scansione OCR.

Il client riceve quindi il testo dallo schermo, che si occuperà poi della traduzione.

# Caratteristiche principali
Cattura schermo in un'area selezionabile dall'utente

Riconoscimento testo OCR italiano+inglese con Tesseract

Server HTTP locale che serve il testo OCR estratto

Client Windows Forms con trackbar per configurare intervallo scansione

Mostra testo tradotto in stile console, aggiornato automaticamente

Supporto traduzione con Google Cloud Translation API V3

# Struttura
CounterBabbelServer: codice del server per cattura e OCR

CounterBabbelClient: client WinForms per visualizzazione e traduzione

# Come usare
Clona il repository

Configura le credenziali Google Cloud seguendo la sezione Configurazione

Avvia il server da CounterBabbelServer

Avvia il client da CounterBabbelClient

Modifica il delay di scansione tramite la trackbar nel client

Visualizza testi tradotti in console sul client

# Configurazione Google Cloud Translate API
Crea un progetto su https://console.cloud.google.com/

Abilita Cloud Translation API

Crea un Service Account e scarica il file JSON delle credenziali

Inserisci il percorso del file JSON nelle variabili d'ambiente o nel file appsettings.json, ignorato da Git

# Importante
Non committare MAI file con credenziali (inserisci .json e .env in .gitignore)

Usa il file appsettings.example.json come template di configurazione

Se commetti credenziali per errore, rigenera le chiavi Google

# Dipendenze
ScreenCapture.NET (per la cattura DirectX)

Tesseract OCR

Google.Cloud.Translate.V3

# Licenza
AGPL-3.0 license
