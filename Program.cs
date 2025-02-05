using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using AForge.Math;
public partial class MainForm : Form
{
     private TcpListener videoListener;
    private TcpListener audioListener;
    private TcpClient videoClient;
    private TcpClient audioClient;
    private NetworkStream videoStream;
    private NetworkStream audioStream;
    private CancellationTokenSource cts;
    private int selectedDisplayIndex;
    private ImageFormat compressionFormat;
    private long compressionQuality;
    private StreamDisplayForm streamDisplayForm;
    private int screenWidth;
    private int screenHeight;
    private int fps;
    private IWaveIn waveIn;
    private WaveOutEvent waveOut;
    private BufferedWaveProvider waveProvider;
    private CancellationTokenSource callCts;
    private System.Windows.Forms.CheckBox chkNoiseCancellation;

    public MainForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
      ;
        this.rbServer = new System.Windows.Forms.RadioButton();
        this.rbClient = new System.Windows.Forms.RadioButton();
        this.txtIPAddress = new System.Windows.Forms.TextBox();
        this.txtPort = new System.Windows.Forms.TextBox();
        this.cmbResolution = new System.Windows.Forms.ComboBox();
        this.cmbFPS = new System.Windows.Forms.ComboBox();
        this.cmbCompression = new System.Windows.Forms.ComboBox();
        this.btnStart = new System.Windows.Forms.Button();
        this.cmbDisplay = new System.Windows.Forms.ComboBox();
        this.lblDisplay = new System.Windows.Forms.Label();
        this.lblCompressionType = new System.Windows.Forms.Label();
        this.trkCompressionLevel = new System.Windows.Forms.TrackBar();
        this.lblCompressionLevel = new System.Windows.Forms.Label();
        this.cmbAudioInput = new System.Windows.Forms.ComboBox();
        this.cmbAudioOutput = new System.Windows.Forms.ComboBox();
        this.cmbBitrate = new System.Windows.Forms.ComboBox();
        this.btnStartCall = new System.Windows.Forms.Button();
        this.btnStopCall = new System.Windows.Forms.Button();
        this.trkInputVolume = new System.Windows.Forms.TrackBar();
        this.trkOutputVolume = new System.Windows.Forms.TrackBar();
        this.lblInputVolume = new System.Windows.Forms.Label();
        this.lblOutputVolume = new System.Windows.Forms.Label();
        this.txtAudioPort = new System.Windows.Forms.TextBox();
        this.trkMicrophoneBoost = new System.Windows.Forms.TrackBar();
        this.lblMicrophoneBoost = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.trkCompressionLevel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.trkInputVolume)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.trkOutputVolume)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.trkMicrophoneBoost)).BeginInit();
        this.SuspendLayout();

        // Set the form size to 1024x768
        this.ClientSize = new System.Drawing.Size(1024, 768);

        // rbServer
        this.rbServer.AutoSize = true;
        this.rbServer.Location = new System.Drawing.Point(12, 12);
        this.rbServer.Name = "rbServer";
        this.rbServer.Size = new System.Drawing.Size(56, 17);
        this.rbServer.TabIndex = 0;
        this.rbServer.TabStop = true;
        this.rbServer.Text = "Server";
        this.rbServer.UseVisualStyleBackColor = true;

        // rbClient
        this.rbClient.AutoSize = true;
        this.rbClient.Location = new System.Drawing.Point(74, 12);
        this.rbClient.Name = "rbClient";
        this.rbClient.Size = new System.Drawing.Size(51, 17);
        this.rbClient.TabIndex = 1;
        this.rbClient.TabStop = true;
        this.rbClient.Text = "Client";
        this.rbClient.UseVisualStyleBackColor = true;

        // txtIPAddress
        this.txtIPAddress.Location = new System.Drawing.Point(12, 35);
        this.txtIPAddress.Name = "txtIPAddress";
        this.txtIPAddress.Size = new System.Drawing.Size(113, 20);
        this.txtIPAddress.TabIndex = 2;
        this.txtIPAddress.Text = "127.0.0.1";

        // txtPort
        this.txtPort.Location = new System.Drawing.Point(131, 35);
        this.txtPort.Name = "txtPort";
        this.txtPort.Size = new System.Drawing.Size(59, 20);
        this.txtPort.TabIndex = 3;
        this.txtPort.Text = "5000";

        // txtAudioPort
        this.txtAudioPort.Location = new System.Drawing.Point(196, 35);
        this.txtAudioPort.Name = "txtAudioPort";
        this.txtAudioPort.Size = new System.Drawing.Size(59, 20);
        this.txtAudioPort.TabIndex = 23;
        this.txtAudioPort.Text = "5001";

        // cmbResolution
        this.cmbResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbResolution.FormattingEnabled = true;
        this.cmbResolution.Items.AddRange(new object[] {
            "source",
            "1920x1080",
            "1280x720",
            "1024x768",
            "800x600"});
        this.cmbResolution.Location = new System.Drawing.Point(12, 61);
        this.cmbResolution.Name = "cmbResolution";
        this.cmbResolution.Size = new System.Drawing.Size(100, 21);
        this.cmbResolution.TabIndex = 4;
        this.cmbResolution.SelectedIndexChanged += new System.EventHandler(this.cmbResolution_SelectedIndexChanged);

        // cmbFPS
        this.cmbFPS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbFPS.FormattingEnabled = true;
        this.cmbFPS.Items.AddRange(new object[] {
            "15",
            "30",
            "60"});
        this.cmbFPS.Location = new System.Drawing.Point(118, 61);
        this.cmbFPS.Name = "cmbFPS";
        this.cmbFPS.Size = new System.Drawing.Size(72, 21);
        this.cmbFPS.TabIndex = 5;
        this.cmbFPS.SelectedIndexChanged += new System.EventHandler(this.cmbFPS_SelectedIndexChanged);

        // cmbCompression
        this.cmbCompression.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbCompression.FormattingEnabled = true;
        this.cmbCompression.Items.AddRange(new object[] {
            "JPEG",
            "PNG",
            "GIF"});
        this.cmbCompression.Location = new System.Drawing.Point(12, 87);
        this.cmbCompression.Name = "cmbCompression";
        this.cmbCompression.Size = new System.Drawing.Size(100, 21);
        this.cmbCompression.TabIndex = 6;
        this.cmbCompression.SelectedIndexChanged += new System.EventHandler(this.cmbCompression_SelectedIndexChanged);

        // btnStart
        this.btnStart.Location = new System.Drawing.Point(12, 142);
        this.btnStart.Name = "btnStart";
        this.btnStart.Size = new System.Drawing.Size(178, 23);
        this.btnStart.TabIndex = 7;
        this.btnStart.Text = "Start";
        this.btnStart.UseVisualStyleBackColor = true;
        this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

        // cmbDisplay
        this.cmbDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbDisplay.FormattingEnabled = true;
        this.cmbDisplay.Location = new System.Drawing.Point(68, 114);
        this.cmbDisplay.Name = "cmbDisplay";
        this.cmbDisplay.Size = new System.Drawing.Size(121, 21);
        this.cmbDisplay.TabIndex = 9;

        // lblDisplay
        this.lblDisplay.AutoSize = true;
        this.lblDisplay.Location = new System.Drawing.Point(12, 117);
        this.lblDisplay.Name = "lblDisplay";
        this.lblDisplay.Size = new System.Drawing.Size(42, 13);
        this.lblDisplay.TabIndex = 10;
        this.lblDisplay.Text = "Display:";

        // lblCompressionType
        this.lblCompressionType.AutoSize = true;
        this.lblCompressionType.Location = new System.Drawing.Point(118, 90);
        this.lblCompressionType.Name = "lblCompressionType";
        this.lblCompressionType.Size = new System.Drawing.Size(97, 13);
        this.lblCompressionType.TabIndex = 11;
        this.lblCompressionType.Text = "Compression Type:";

        // trkCompressionLevel
        this.trkCompressionLevel.Location = new System.Drawing.Point(12, 171);
        this.trkCompressionLevel.Maximum = 100;
        this.trkCompressionLevel.Minimum = 1;
        this.trkCompressionLevel.Name = "trkCompressionLevel";
        this.trkCompressionLevel.Size = new System.Drawing.Size(178, 45);
        this.trkCompressionLevel.TabIndex = 12;
        this.trkCompressionLevel.Value = 50;
        this.trkCompressionLevel.Scroll += new System.EventHandler(this.trkCompressionLevel_Scroll);

        // lblCompressionLevel
        this.lblCompressionLevel.AutoSize = true;
        this.lblCompressionLevel.Location = new System.Drawing.Point(196, 171);
        this.lblCompressionLevel.Name = "lblCompressionLevel";
        this.lblCompressionLevel.Size = new System.Drawing.Size(94, 13);
        this.lblCompressionLevel.TabIndex = 13;
        this.lblCompressionLevel.Text = "Compression Level:";

        // Additional controls for audio settings
        this.cmbAudioInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbAudioInput.FormattingEnabled = true;
        this.cmbAudioInput.Location = new System.Drawing.Point(12, 200);
        this.cmbAudioInput.Name = "cmbAudioInput";
        this.cmbAudioInput.Size = new System.Drawing.Size(178, 21);
        this.cmbAudioInput.TabIndex = 14;

        this.cmbAudioOutput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbAudioOutput.FormattingEnabled = true;
        this.cmbAudioOutput.Location = new System.Drawing.Point(12, 230);
        this.cmbAudioOutput.Name = "cmbAudioOutput";
        this.cmbAudioOutput.Size = new System.Drawing.Size(178, 21);
        this.cmbAudioOutput.TabIndex = 15;

        this.cmbBitrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbBitrate.FormattingEnabled = true;
        this.cmbBitrate.Items.AddRange(new object[] {
            "16",
            "32",
            "64",
            "128",
            "256"});
        this.cmbBitrate.Location = new System.Drawing.Point(12, 260);
        this.cmbBitrate.Name = "cmbBitrate";
        this.cmbBitrate.Size = new System.Drawing.Size(178, 21);
        this.cmbBitrate.TabIndex = 16;

        this.btnStartCall.Location = new System.Drawing.Point(12, 290);
        this.btnStartCall.Name = "btnStartCall";
        this.btnStartCall.Size = new System.Drawing.Size(75, 23);
        this.btnStartCall.TabIndex = 17;
        this.btnStartCall.Text = "Start Call";
        this.btnStartCall.UseVisualStyleBackColor = true;
        this.btnStartCall.Click += new System.EventHandler(this.btnStartCall_Click);

        this.btnStopCall.Location = new System.Drawing.Point(115, 290);
        this.btnStopCall.Name = "btnStopCall";
        this.btnStopCall.Size = new System.Drawing.Size(75, 23);
        this.btnStopCall.TabIndex = 18;
        this.btnStopCall.Text = "Stop Call";
        this.btnStopCall.UseVisualStyleBackColor = true;
        this.btnStopCall.Click += new System.EventHandler(this.btnStopCall_Click);

        // TrackBar for Input Volume
        this.trkInputVolume.Location = new System.Drawing.Point(12, 320);
        this.trkInputVolume.Maximum = 100;
        this.trkInputVolume.Name = "trkInputVolume";
        this.trkInputVolume.Size = new System.Drawing.Size(178, 45);
        this.trkInputVolume.TabIndex = 19;
        this.trkInputVolume.Value = 50; // Default volume 50%
        this.trkInputVolume.Scroll += new System.EventHandler(this.trkInputVolume_Scroll);

        // TrackBar for Output Volume
        this.trkOutputVolume.Location = new System.Drawing.Point(12, 370);
        this.trkOutputVolume.Maximum = 100;
        this.trkOutputVolume.Name = "trkOutputVolume";
        this.trkOutputVolume.Size = new System.Drawing.Size(178, 45);
        this.trkOutputVolume.TabIndex = 20;
        this.trkOutputVolume.Value = 50; // Default volume 50%
        this.trkOutputVolume.Scroll += new System.EventHandler(this.trkOutputVolume_Scroll);

        // Label for Input Volume
        this.lblInputVolume.AutoSize = true;
        this.lblInputVolume.Location = new System.Drawing.Point(196, 320);
        this.lblInputVolume.Name = "lblInputVolume";
        this.lblInputVolume.Size = new System.Drawing.Size(70, 13);
        this.lblInputVolume.TabIndex = 21;
        this.lblInputVolume.Text = "Input Volume:";

        // Label for Output Volume
        this.lblOutputVolume.AutoSize = true;
        this.lblOutputVolume.Location = new System.Drawing.Point(196, 370);
        this.lblOutputVolume.Name = "lblOutputVolume";
        this.lblOutputVolume.Size = new System.Drawing.Size(78, 13);
        this.lblOutputVolume.TabIndex = 22;
        this.lblOutputVolume.Text = "Output Volume:";

        // TrackBar for Microphone Boost
        this.trkMicrophoneBoost.Location = new System.Drawing.Point(12, 420);
        this.trkMicrophoneBoost.Maximum = 30; // Assuming boost range from 0 to 30 dB
        this.trkMicrophoneBoost.Name = "trkMicrophoneBoost";
        this.trkMicrophoneBoost.Size = new System.Drawing.Size(178, 45);
        this.trkMicrophoneBoost.TabIndex = 24;
        this.trkMicrophoneBoost.Value = 0; // Default boost 0 dB
        this.trkMicrophoneBoost.Scroll += new System.EventHandler(this.trkMicrophoneBoost_Scroll);

        // Label for Microphone Boost
        this.lblMicrophoneBoost.AutoSize = true;
        this.lblMicrophoneBoost.Location = new System.Drawing.Point(196, 420);
        this.lblMicrophoneBoost.Name = "lblMicrophoneBoost";
        this.lblMicrophoneBoost.Size = new System.Drawing.Size(92, 13);
        this.lblMicrophoneBoost.TabIndex = 25;
        this.lblMicrophoneBoost.Text = "Microphone Boost:";
        // Label for noise cancell
        this.chkNoiseCancellation = new System.Windows.Forms.CheckBox();
        this.chkNoiseCancellation.AutoSize = true;
        this.chkNoiseCancellation.Location = new System.Drawing.Point(12, 470);
        this.chkNoiseCancellation.Name = "chkNoiseCancellation";
        this.chkNoiseCancellation.Size = new System.Drawing.Size(116, 17);
        this.chkNoiseCancellation.TabIndex = 26;
        this.chkNoiseCancellation.Text = "Noise Cancellation";
        this.chkNoiseCancellation.UseVisualStyleBackColor = true;
        this.chkNoiseCancellation.CheckedChanged += new System.EventHandler(this.chkNoiseCancellation_CheckedChanged);

        // Add the new controls to the form
        this.Controls.Add(this.chkNoiseCancellation);
        this.Controls.Add(this.cmbAudioInput);
        this.Controls.Add(this.cmbAudioOutput);
        this.Controls.Add(this.cmbBitrate);
        this.Controls.Add(this.btnStartCall);
        this.Controls.Add(this.btnStopCall);
        this.Controls.Add(this.trkInputVolume);
        this.Controls.Add(this.trkOutputVolume);
        this.Controls.Add(this.lblInputVolume);
        this.Controls.Add(this.lblOutputVolume);
        this.Controls.Add(this.txtAudioPort);
        this.Controls.Add(this.trkMicrophoneBoost);
        this.Controls.Add(this.lblMicrophoneBoost);

        // Existing controls...
        this.Controls.Add(this.lblCompressionLevel);
        this.Controls.Add(this.trkCompressionLevel);
        this.Controls.Add(this.lblCompressionType);
        this.Controls.Add(this.lblDisplay);
        this.Controls.Add(this.cmbDisplay);
        this.Controls.Add(this.btnStart);
        this.Controls.Add(this.cmbCompression);
        this.Controls.Add(this.cmbFPS);
        this.Controls.Add(this.cmbResolution);
        this.Controls.Add(this.txtPort);
        this.Controls.Add(this.txtIPAddress);
        this.Controls.Add(this.rbClient);
        this.Controls.Add(this.rbServer);
        this.Name = "MainForm";
        this.Text = "Screen Share App";
        this.Load += new System.EventHandler(this.MainForm_Load);
        ((System.ComponentModel.ISupportInitialize)(this.trkCompressionLevel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.trkInputVolume)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.trkOutputVolume)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.trkMicrophoneBoost)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }


    private void MainForm_Load(object sender, EventArgs e)
    {
        // Load available displays into the ComboBox
        for (int i = 0; i < Screen.AllScreens.Length; i++)
        {
            cmbDisplay.Items.Add($"Display {i + 1}");
        }
        cmbDisplay.SelectedIndex = 0;

        // Set default compression format
        cmbCompression.SelectedIndex = 0;
        cmbResolution.SelectedIndex = 0;
        cmbFPS.SelectedIndex = 1; // Default to 30 FPS

        // Set the compression quality based on the trackbar value
        compressionQuality = trkCompressionLevel.Value;

        // Set initial FPS value
        fps = int.Parse(cmbFPS.SelectedItem.ToString());

        // Load available audio input and output devices
        for (int i = 0; i < WaveIn.DeviceCount; i++)
        {
            var deviceInfo = WaveIn.GetCapabilities(i);
            cmbAudioInput.Items.Add(deviceInfo.ProductName);
        }

        for (int i = 0; i < WaveOut.DeviceCount; i++)
        {
            var deviceInfo = WaveOut.GetCapabilities(i);
            cmbAudioOutput.Items.Add(deviceInfo.ProductName);
        }

        cmbAudioInput.SelectedIndex = 0;
        cmbAudioOutput.SelectedIndex = 0;
        cmbBitrate.SelectedIndex = 2; // Default to 64 Kbps
    }

    private void trkCompressionLevel_Scroll(object sender, EventArgs e)
    {
        compressionQuality = trkCompressionLevel.Value;
        lblCompressionLevel.Text = $"Compression Level: {compressionQuality}";
        Console.WriteLine($"Compression Quality Changed: {compressionQuality}");
    }

    private void cmbCompression_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCompressionFormat();
        Console.WriteLine($"Compression Format Changed: {cmbCompression.SelectedItem}");
    }
    
    private void UpdateCompressionFormat()
    {
        string selectedFormat = cmbCompression.SelectedItem.ToString();
        switch (selectedFormat)
        {
            case "JPEG":
                compressionFormat = ImageFormat.Jpeg;
                break;
            case "PNG":
                compressionFormat = ImageFormat.Png;
                break;
            case "GIF":
                compressionFormat = ImageFormat.Gif;
                break;
            default:
                compressionFormat = ImageFormat.Jpeg;
                break;
        }
    }


private void cmbResolution_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateResolution();
    }

    private void UpdateResolution()
    {
        if (cmbResolution.SelectedItem != null)
        {
            string selectedResolution = cmbResolution.SelectedItem.ToString();
            if (selectedResolution == "source")
            {
                // Use the native resolution of the selected display
                Rectangle bounds = Screen.AllScreens[selectedDisplayIndex].Bounds;
                screenWidth = bounds.Width;
                screenHeight = bounds.Height;
                Console.WriteLine($"Resolution changed to source: {screenWidth}x{screenHeight}");
            }
            else
            {
                // Parse the selected resolution
                string[] dimensions = selectedResolution.Split('x');
                if (dimensions.Length == 2 &&
                    int.TryParse(dimensions[0], out screenWidth) &&
                    int.TryParse(dimensions[1], out screenHeight))
                {
                    Console.WriteLine($"Resolution changed to: {screenWidth}x{screenHeight}");
                }
                else
                {
                    Console.WriteLine("Invalid resolution format.");
                }
            }

            // Notify the stream display form of the resolution change
            if (streamDisplayForm != null)
            {
                streamDisplayForm.UpdateStreamResolution(screenWidth, screenHeight);
            }
        }
        else
        {
            Console.WriteLine("No resolution selected.");
        }
    }


    private void cmbFPS_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbFPS.SelectedItem != null)
        {
            fps = int.Parse(cmbFPS.SelectedItem.ToString());
            Console.WriteLine($"FPS changed to: {fps}");
        }
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        selectedDisplayIndex = cmbDisplay.SelectedIndex;

        UpdateCompressionFormat();

        cts = new CancellationTokenSource();

        if (rbServer.Checked)
        {
            // Start video server
            await StartVideoServerAsync(cts.Token, int.Parse(txtPort.Text));
            
            // Start audio server
            StartVoiceCallServer(cts.Token);
        }
        else
        {
            // Start video client
            await StartVideoClientAsync(cts.Token, int.Parse(txtPort.Text));
            
            // Start audio client
            StartVoiceCallClient(cts.Token);
        }
    }

   
    private async Task StartVideoServerAsync(CancellationToken cancellationToken, int port)
    {
        videoListener = new TcpListener(IPAddress.Parse(txtIPAddress.Text), port);
        videoListener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                videoClient = await videoListener.AcceptTcpClientAsync();
                _ = Task.Run(() => VideoServerLoopAsync(videoClient, cancellationToken), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Video server error: {ex.Message}");
        }
        finally
        {
            videoListener.Stop();
        }
    }

    private async Task VideoServerLoopAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        {
            videoStream = client.GetStream();
            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                Bitmap screenshot = CaptureScreen(selectedDisplayIndex);
                byte[] imageBytes = ImageToByte(screenshot, compressionFormat, compressionQuality);
                try
                {
                    await videoStream.WriteAsync(imageBytes, 0, imageBytes.Length, cancellationToken);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Video server stream error: {ex.Message}");
                    break;
                }
                await Task.Delay(1000 / fps, cancellationToken);
            }
        }
    }


      private Bitmap CaptureScreen(int displayIndex)
    {
        Rectangle bounds = Screen.AllScreens[displayIndex].Bounds;
        Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);
        using (Graphics g = Graphics.FromImage(screenshot))
        {
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
        }

        // Resize the screenshot to the desired resolution if it's not the source resolution
        if (screenWidth != bounds.Width || screenHeight != bounds.Height)
        {
            Bitmap resizedScreenshot = new Bitmap(screenshot, new Size(screenWidth, screenHeight));
            screenshot.Dispose(); // Dispose of the original screenshot to free up memory
            return resizedScreenshot;
        }

        return screenshot;
    }

    private byte[] ImageToByte(Image img, ImageFormat format, long quality)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            if (format == ImageFormat.Jpeg)
            {
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                img.Save(ms, GetEncoder(format), encoderParameters);
            }
            else
            {
                img.Save(ms, format);
            }
            return ms.ToArray();
        }
    }

    private ImageCodecInfo GetEncoder(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }

      private async Task StartVideoClientAsync(CancellationToken cancellationToken, int port)
    {
        videoClient = new TcpClient();
        try
        {
            await videoClient.ConnectAsync(txtIPAddress.Text, port);
            videoStream = videoClient.GetStream();
            streamDisplayForm = new StreamDisplayForm();
            streamDisplayForm.Show();

            await VideoClientLoopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Video client error: {ex.Message}");
        }
    }

    private async Task VideoClientLoopAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024 * 1024];
        while (!cancellationToken.IsCancellationRequested && videoClient.Connected)
        {
            try
            {
                int bytesRead = await videoStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead > 0)
                {
                    using (MemoryStream ms = new MemoryStream(buffer, 0, bytesRead))
                    {
                        Image img = Image.FromStream(ms);
                        streamDisplayForm.pbStream.Image = img;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Video client stream error: {ex.Message}");
                break;
            }
        }
    }
    private void trkInputVolume_Scroll(object sender, EventArgs e)
    {
        // Adjust input volume dynamically if needed
    }

    private void trkOutputVolume_Scroll(object sender, EventArgs e)
    {
        if (waveOut != null)
        {
            waveOut.Volume = trkOutputVolume.Value / 100f;
        }
    }

    private async void btnStartCall_Click(object sender, EventArgs e)
    {
        callCts = new CancellationTokenSource();
        if (rbServer.Checked)
        {
            StartVoiceCallServer(callCts.Token);
        }
        else
        {
            StartVoiceCallClient(callCts.Token);
        }
    }

    private void btnStopCall_Click(object sender, EventArgs e)
    {
        callCts?.Cancel();
    }

       private void StartVoiceCallServer(CancellationToken cancellationToken)
    {
        int inputDeviceNumber = cmbAudioInput.SelectedIndex;
        waveIn = new WaveInEvent
        {
            DeviceNumber = inputDeviceNumber,
            WaveFormat = new WaveFormat(8000, 16, 1) // 8 KHz, 16-bit, Mono
        };

        waveIn.DataAvailable += (s, a) =>
        {
            try
            {
                if (audioClient != null && audioClient.Connected)
                {
                    byte[] buffer = a.Buffer;

                    // Apply noise cancellation if enabled
                    if (chkNoiseCancellation.Checked)
                    {
                        buffer = ApplyNoiseCancellation(buffer, a.BytesRecorded);
                    }

                    buffer = AdjustVolume(buffer, a.BytesRecorded, trkInputVolume.Value, trkMicrophoneBoost.Value);
                    audioStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending audio data: {ex.Message}");
            }
        };

        waveIn.StartRecording();

        Task.Run(async () =>
        {
            audioListener = new TcpListener(IPAddress.Parse(txtIPAddress.Text), int.Parse(txtAudioPort.Text));
            audioListener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                audioClient = await audioListener.AcceptTcpClientAsync();
                audioStream = audioClient.GetStream();
            }
        }, cancellationToken);
    }
    private void StartVoiceCallClient(CancellationToken cancellationToken)
    {
        int outputDeviceNumber = cmbAudioOutput.SelectedIndex;
        waveOut = new WaveOutEvent
        {
            DeviceNumber = outputDeviceNumber,
            Volume = trkOutputVolume.Value / 100f // Set initial volume
        };

        waveProvider = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
        waveOut.Init(waveProvider);
        waveOut.Play();

        Task.Run(async () =>
        {
            audioClient = new TcpClient();
            await audioClient.ConnectAsync(txtIPAddress.Text, int.Parse(txtAudioPort.Text));
            audioStream = audioClient.GetStream();

            await ReceiveAudioData(cancellationToken);
        }, cancellationToken);
    }

    private async Task ReceiveAudioData(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                int bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead > 0)
                {
                    waveProvider.AddSamples(buffer, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Audio client stream error: {ex.Message}");
                break;
            }
        }
    }

    private byte[] AdjustVolume(byte[] buffer, int bytesRecorded, int volumePercent, int boostDb)
    {
        float volume = volumePercent / 100f;
        float boostFactor = (float)Math.Pow(10, boostDb / 20.0); // Convert dB to linear scale

        for (int i = 0; i < bytesRecorded; i += 2)
        {
            short sample = BitConverter.ToInt16(buffer, i);
            sample = (short)(sample * volume * boostFactor);
            byte[] bytes = BitConverter.GetBytes(sample);
            buffer[i] = bytes[0];
            buffer[i + 1] = bytes[1];
        }
        return buffer;
    }

    private byte[] ApplyNoiseCancellation(byte[] buffer, int bytesRecorded)
    {
        // Convert the byte array to a short array for processing
        short[] samples = new short[bytesRecorded / 2];
        Buffer.BlockCopy(buffer, 0, samples, 0, bytesRecorded);

        // Use AForge.NET to apply noise cancellation
        // For simplicity, we'll use a basic noise reduction algorithm
        // In a real-world application, you may use a more advanced algorithm
        for (int i = 0; i < samples.Length; i++)
        {
            // Simple noise gate
            if (Math.Abs(samples[i]) < 500) // Threshold for noise
            {
                samples[i] = 0;
            }
        }

        // Convert the short array back to a byte array
        Buffer.BlockCopy(samples, 0, buffer, 0, bytesRecorded);
        return buffer;
    }
    
private void chkNoiseCancellation_CheckedChanged(object sender, EventArgs e)
    {
        // Handle noise cancellation enable/disable
        if (chkNoiseCancellation.Checked)
        {
            MessageBox.Show("Noise Cancellation Enabled");
        }
        else
        {
            MessageBox.Show("Noise Cancellation Disabled");
        }
    }

    private void cmbBitrate_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Handle bitrate change
        if (waveIn != null)
        {
            waveIn.StopRecording();
            waveIn.WaveFormat = new WaveFormat(8000, int.Parse(cmbBitrate.SelectedItem.ToString()), 1);
            waveIn.StartRecording();
        }
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        cts?.Cancel();
        waveIn?.StopRecording();
        waveOut?.Stop();

        base.OnFormClosing(e);
    }
    private void trkMicrophoneBoost_Scroll(object sender, EventArgs e)
    {
        lblMicrophoneBoost.Text = $"Microphone Boost: {trkMicrophoneBoost.Value} dB";
    }
    private System.Windows.Forms.RadioButton rbServer;
    private System.Windows.Forms.RadioButton rbClient;
    private System.Windows.Forms.TextBox txtIPAddress;
    private System.Windows.Forms.TextBox txtPort;
    private System.Windows.Forms.ComboBox cmbResolution;
    private System.Windows.Forms.ComboBox cmbFPS;
    private System.Windows.Forms.ComboBox cmbCompression;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.ComboBox cmbDisplay;
    private System.Windows.Forms.Label lblDisplay;
    private System.Windows.Forms.Label lblCompressionType;
    private System.Windows.Forms.TrackBar trkCompressionLevel;
    private System.Windows.Forms.Label lblCompressionLevel;
    private System.Windows.Forms.ComboBox cmbAudioInput;
    private System.Windows.Forms.ComboBox cmbAudioOutput;
    private System.Windows.Forms.ComboBox cmbBitrate;
    private System.Windows.Forms.Button btnStartCall;
    private System.Windows.Forms.Button btnStopCall;
    private System.Windows.Forms.TrackBar trkInputVolume;
    private System.Windows.Forms.TrackBar trkOutputVolume;
    private System.Windows.Forms.Label lblInputVolume;
    private System.Windows.Forms.Label lblOutputVolume;
    private System.Windows.Forms.TextBox txtAudioPort;
    private System.Windows.Forms.TrackBar trkMicrophoneBoost;
    private System.Windows.Forms.Label lblMicrophoneBoost;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public partial class StreamDisplayForm : Form
{
    public StreamDisplayForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.pbStream = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.pbStream)).BeginInit();
        this.SuspendLayout();

        // pbStream
        this.pbStream.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pbStream.Location = new System.Drawing.Point(0, 0);
        this.pbStream.Name = "pbStream";
        this.pbStream.Size = new System.Drawing.Size(800, 450); // initial size
        this.pbStream.SizeMode = PictureBoxSizeMode.StretchImage; // Ensure the image is stretched to fit the PictureBox
        this.pbStream.TabIndex = 0;
        this.pbStream.TabStop = false;

        // StreamDisplayForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450); // initial size
        this.Controls.Add(this.pbStream);
        this.Name = "StreamDisplayForm";
        this.Text = "Stream Display";
        ((System.ComponentModel.ISupportInitialize)(this.pbStream)).EndInit();
        this.ResumeLayout(false);
    }

    public PictureBox pbStream;

    // Method to update the stream resolution
    public void UpdateStreamResolution(int width, int height)
    {
        this.pbStream.Size = new Size(width, height);
        this.ClientSize = new Size(width, height);
    }
}