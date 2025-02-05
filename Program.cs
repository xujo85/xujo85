using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NAudio.Wave;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VideoStreamingApp
{
    public partial class MainForm : Form
    {
        private UdpClient udpClient;
        private UdpClient udpReceiver;
        private Thread streamingThread;
        private Thread receivingThread;
        private bool isStreaming = false;
        private bool isReceiving = false;
        private int selectedResolutionWidth = 640;
        private int selectedResolutionHeight = 480;
        private long selectedCompressionQuality = 50L;
        private Aes aes;
        private WaveInEvent waveIn;
        private UdpClient audioClient;
        private UdpClient audioReceiver;
        private Thread audioStreamingThread;
        private Thread audioReceivingThread;
        private bool isAudioStreaming = false;
        private List<UdpClient> clientList = new List<UdpClient>();
        private ComboBox comboBoxAudioDevices;
        private ComboBox comboBoxResolution;
        private TrackBar trackBarCompression;
        private ComboBox comboBoxFPS;
        private TextBox textBoxPort;
        private Button buttonStart;
        private PictureBox pictureBoxPreview;
        private ComboBox comboBoxScreens;
        private System.Windows.Forms.Timer screenCaptureTimer;
        private PreviewWindow previewWindow;
        private bool isServer = true;
        private RadioButton radioServer;
        private RadioButton radioClient;
        private TextBox textBoxIPAddress;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application Error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Screen and Audio Streaming App";
            this.Size = new System.Drawing.Size(350, 400); // Reduced size since preview is in separate window

            // Client/Server Selection
            GroupBox groupBoxMode = new GroupBox { Text = "Mode", Left = 10, Top = 10, Width = 310, Height = 50 };
            radioServer = new RadioButton { Text = "Server", Left = 10, Top = 20, Width = 100, Checked = true };
            radioClient = new RadioButton { Text = "Client", Left = 120, Top = 20, Width = 100 };
            groupBoxMode.Controls.AddRange(new Control[] { radioServer, radioClient });

            // IP Address input (for client mode)
            Label labelIP = new Label { Text = "Server IP:", Left = 10, Top = 70, Width = 100 };
            textBoxIPAddress = new TextBox { Left = 120, Top = 70, Width = 200, Text = "127.0.0.1", Enabled = false };

            // Existing controls (adjusted positions)
            Label labelScreens = new Label { Text = "Select Screen:", Left = 10, Top = 110, Width = 100 };
            comboBoxScreens = new ComboBox { Left = 120, Top = 110, Width = 200 };

            Label labelAudioDevices = new Label { Text = "Select Audio:", Left = 10, Top = 150, Width = 100 };
            comboBoxAudioDevices = new ComboBox { Left = 120, Top = 150, Width = 200 };

            Label labelResolution = new Label { Text = "Resolution:", Left = 10, Top = 190, Width = 100 };
            comboBoxResolution = new ComboBox { Left = 120, Top = 190, Width = 200 };
            comboBoxResolution.Items.AddRange(new string[] { "640x480", "1280x720", "1920x1080" });
            comboBoxResolution.SelectedIndex = 0;

            Label labelCompression = new Label { Text = "Compression:", Left = 10, Top = 230, Width = 100 };
            trackBarCompression = new TrackBar { Left = 120, Top = 225, Width = 200, Minimum = 10, Maximum = 100, Value = 50 };

            Label labelFPS = new Label { Text = "FPS:", Left = 10, Top = 270, Width = 100 };
            comboBoxFPS = new ComboBox { Left = 120, Top = 270, Width = 200 };
            comboBoxFPS.Items.AddRange(new string[] { "15", "30", "60" });
            comboBoxFPS.SelectedIndex = 1;

            Label labelPort = new Label { Text = "Port:", Left = 10, Top = 310, Width = 100 };
            textBoxPort = new TextBox { Left = 120, Top = 310, Width = 200, Text = "5000" };

            buttonStart = new Button { Text = "Start Streaming", Left = 10, Top = 350, Width = 310 };
            buttonStart.Click += ButtonStart_Click;

            this.Controls.AddRange(new Control[] {
                groupBoxMode,
                labelIP, textBoxIPAddress,
                labelScreens, comboBoxScreens,
                labelAudioDevices, comboBoxAudioDevices,
                labelResolution, comboBoxResolution,
                labelCompression, trackBarCompression,
                labelFPS, comboBoxFPS,
                labelPort, textBoxPort,
                buttonStart
            });

            // Event handlers for radio buttons
            radioServer.CheckedChanged += (s, e) => {
                textBoxIPAddress.Enabled = !radioServer.Checked;
                isServer = radioServer.Checked;
                UpdateControlsState();
            };

            radioClient.CheckedChanged += (s, e) => {
                textBoxIPAddress.Enabled = radioClient.Checked;
                isServer = !radioClient.Checked;
                UpdateControlsState();
            };

            // Create preview window
            previewWindow = new PreviewWindow();
            previewWindow.Show();
        }

        private void UpdateControlsState()
        {
            comboBoxScreens.Enabled = isServer;
            comboBoxAudioDevices.Enabled = isServer;
            comboBoxResolution.Enabled = isServer;
            trackBarCompression.Enabled = isServer;
            comboBoxFPS.Enabled = isServer;
        }
             private void ButtonStart_Click(object sender, EventArgs e)
    {
        if (!isStreaming)
        {
            try
            {
                StartStreaming();
                buttonStart.Text = "Stop Streaming";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting stream: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                StopStreaming();
            }
        }
        else
        {
            try
            {
                StopStreaming();
                buttonStart.Text = "Start Streaming";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping stream: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }


        public MainForm()
        {
            try
            {
                MessageBox.Show("Starting initialization");
                InitializeComponent();
                MessageBox.Show("Component initialization complete");

                try
                {
                    InitializeScreens();
                    MessageBox.Show("Screens initialized");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Screen initialization failed: {ex.Message}");
                }

                try
                {
                    InitializeAudioDevices();
                    MessageBox.Show("Audio devices initialized");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Audio initialization failed: {ex.Message}");
                }

                try
                {
                    InitializeSettings();
                    MessageBox.Show("Settings initialized");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Settings initialization failed: {ex.Message}");
                }

                try
                {
                    InitializeEncryption();
                    MessageBox.Show("Encryption initialized");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Encryption initialization failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Main initialization failed: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            }
        }

        private void InitializeScreens()
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                comboBoxScreens.Items.Add($"Screen {Screen.AllScreens.ToList().IndexOf(screen) + 1} ({screen.Bounds.Width}x{screen.Bounds.Height})");
            }
            if (comboBoxScreens.Items.Count > 0)
            {
                comboBoxScreens.SelectedIndex = 0;
            }
        }

        private void InitializeAudioDevices()
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                comboBoxAudioDevices.Items.Add(capabilities.ProductName);
            }
            if (comboBoxAudioDevices.Items.Count > 0)
            {
                comboBoxAudioDevices.SelectedIndex = 0;
            }
        }

        private void StartStreaming()
        {
            try
            {
                if (isServer)
                {
                    StartServerMode();
                }
                else
                {
                    StartClientMode();
                }
                buttonStart.Text = "Stop Streaming";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting stream: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                StopStreaming();
            }
        }

            private void StartServerMode()
        {
            if (comboBoxFPS.SelectedItem == null)
            {
                MessageBox.Show("Please select a FPS value.");
                return;
            }
            
            isStreaming = true;
            udpClient = new UdpClient();
            screenCaptureTimer.Interval = 1000 / int.Parse(comboBoxFPS.SelectedItem.ToString());
            screenCaptureTimer.Start();

            streamingThread = new Thread(SendVideoStream);
            streamingThread.Start();
            StartAudioStreaming();
        }

        private void StartClientMode()
        {
            isReceiving = true;
            int port = int.Parse(textBoxPort.Text);
            IPAddress serverIP = IPAddress.Parse(textBoxIPAddress.Text);

            udpReceiver = new UdpClient(port);
            receivingThread = new Thread(() => ReceiveVideoStream(serverIP));
            receivingThread.Start();

            audioReceiver = new UdpClient(port + 1);
            audioReceivingThread = new Thread(() => ReceiveAudioStream(serverIP));
            audioReceivingThread.Start();
        }

        private void ReceiveVideoStream(IPAddress serverIP)
        {
            while (isReceiving)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpReceiver.Receive(ref remoteEP);
                    byte[] decryptedData = DecryptData(data);

                    using (MemoryStream ms = new MemoryStream(decryptedData))
                    {
                        Image receivedImage = Image.FromStream(ms);
                        previewWindow.UpdatePreview(receivedImage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving video: {ex.Message}");
                }
                Thread.Sleep(1);
            }
        }

        private void ReceiveAudioStream(IPAddress serverIP)
        {
            // Implement audio receiving logic here
        }

        // Modified screen capture to use preview window
        private void ScreenCaptureTimer_Tick(object sender, EventArgs e)
        {
            if (isStreaming && comboBoxScreens.SelectedIndex >= 0)
            {
                Screen selectedScreen = Screen.AllScreens[comboBoxScreens.SelectedIndex];
                using (Bitmap screenshot = new Bitmap(selectedScreen.Bounds.Width, selectedScreen.Bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(selectedScreen.Bounds.X, selectedScreen.Bounds.Y, 0, 0, selectedScreen.Bounds.Size);
                    }

                    var resizedScreenshot = new Bitmap(screenshot, new Size(selectedResolutionWidth, selectedResolutionHeight));
                    previewWindow.UpdatePreview(resizedScreenshot);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isStreaming)
            {
                StopStreaming();
            }
            previewWindow?.Close();
            aes?.Dispose();
            base.OnFormClosing(e);
        }

      private void StopStreaming()
        {
            isStreaming = false;
            screenCaptureTimer?.Stop(); // Use null-conditional operator
            streamingThread?.Abort();
            udpClient?.Close();
            StopAudioStreaming();
            buttonStart.Text = "Start Streaming";
        }
        private void SendVideoStream()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, int.Parse(textBoxPort.Text));
            while (isStreaming)
            {
                if (pictureBoxPreview.Image != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        EncoderParameters encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, selectedCompressionQuality);
                        ImageCodecInfo jpegCodec = ImageCodecInfo.GetImageEncoders()[1];

                        lock (pictureBoxPreview.Image)
                        {
                            pictureBoxPreview.Image.Save(ms, jpegCodec, encoderParams);
                        }

                        byte[] buffer = EncryptData(ms.ToArray());
                        udpClient.Send(buffer, buffer.Length, remoteEndPoint);
                    }
                }
                Thread.Sleep(1);
            }
        }

        private void StartAudioStreaming()
        {
            if (comboBoxAudioDevices.SelectedIndex >= 0)
            {
                isAudioStreaming = true;
                waveIn = new WaveInEvent();
                waveIn.DeviceNumber = comboBoxAudioDevices.SelectedIndex;
                waveIn.WaveFormat = new WaveFormat(44100, 16, 2);
                waveIn.DataAvailable += AudioDataAvailable;
                waveIn.StartRecording();
                audioClient = new UdpClient();
                audioStreamingThread = new Thread(SendAudioStream);
                audioStreamingThread.Start();
            }
        }

        private void StopAudioStreaming()
        {
            isAudioStreaming = false;
            waveIn?.StopRecording();
            waveIn?.Dispose();
            audioStreamingThread?.Abort();
            audioClient?.Close();
        }

        private void AudioDataAvailable(object sender, WaveInEventArgs e)
        {
            if (isAudioStreaming)
            {
                byte[] encryptedAudio = EncryptData(e.Buffer);
                audioClient.Send(encryptedAudio, encryptedAudio.Length, new IPEndPoint(IPAddress.Broadcast, int.Parse(textBoxPort.Text) + 1));
            }
        }

        private void InitializeSettings()
        {
            comboBoxResolution.SelectedIndexChanged += (s, e) =>
            {
                string[] dimensions = comboBoxResolution.SelectedItem.ToString().Split('x');
                selectedResolutionWidth = int.Parse(dimensions[0]);
                selectedResolutionHeight = int.Parse(dimensions[1]);
            };

            trackBarCompression.ValueChanged += (s, e) =>
            {
                selectedCompressionQuality = trackBarCompression.Value;
            };

            comboBoxFPS.SelectedIndexChanged += (s, e) =>
            {
                if (screenCaptureTimer != null && comboBoxFPS.SelectedItem != null)
                {
                    screenCaptureTimer.Interval = 1000 / int.Parse(comboBoxFPS.SelectedItem.ToString());
                }
            };
        }

        private void InitializeEncryption()
        {
            aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes("1234567890123456"); // 16-byte key (example)
            aes.IV = Encoding.UTF8.GetBytes("1234567890123456");
        }

        private byte[] EncryptData(byte[] data)
        {
            using (var encryptor = aes.CreateEncryptor())
            {
                return encryptor.TransformFinalBlock(data, 0, data.Length);
            }
        }

        private byte[] DecryptData(byte[] data)
        {
            using (var decryptor = aes.CreateDecryptor())
            {
                return decryptor.TransformFinalBlock(data, 0, data.Length);
            }
        }

        private void SendAudioStream()
        {
            while (isAudioStreaming)
            {
                Thread.Sleep(1);
            }
        }
    }

    public class PreviewWindow : Form
    {
        private PictureBox pictureBox;

        public PreviewWindow()
        {
            this.Text = "Preview";
            this.Size = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            this.Controls.Add(pictureBox);
        }

        public void UpdatePreview(Image image)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() => UpdatePreview(image)));
                return;
            }

            pictureBox.Image?.Dispose();
            pictureBox.Image = image;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            pictureBox.Image?.Dispose();
            base.OnFormClosing(e);
        }
    }
}