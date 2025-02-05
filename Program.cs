using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace SimpleScreenShare
{
    public partial class MainForm : Form
    {
          #region Variables
        // Form controls
        private GroupBox roleGroup;
        private GroupBox networkGroup;
        private GroupBox streamGroup;
        private RadioButton serverRadio;
        private RadioButton clientRadio;
        private TextBox ipTextBox;
        private TextBox portTextBox;
        private ComboBox resolutionComboBox;
        private NumericUpDown fpsNumeric;
        private ComboBox compressionMethodComboBox;
        private TrackBar compressionStrengthTrackBar;
        private ComboBox windowsComboBox;
        private Button startButton;
        private Label statusLabel;

        // State variables
        private bool isServer = false;
        private bool isStreaming = false;
        private TcpListener server = null;
        private TcpClient client = null;
        private Thread streamThread = null;
        private Form streamWindow = null;
        private PictureBox streamPictureBox = null;
        private DateTime lastSettingsUpdate = DateTime.MinValue;
        private readonly object settingsLock = new object();

        // Settings
        private Size currentResolution;
        private int currentFps = 30;
        private string currentCompressionMethod = "JPEG";
        private int currentCompressionStrength = 50;
        private string selectedWindow = "Entire Screen";

        // Constants
        private const int DEFAULT_PORT = 8080;
        private const string DEFAULT_IP = "127.0.0.1";

        // Window handles dictionary
        private Dictionary<string, IntPtr> windowHandles = new Dictionary<string, IntPtr>();
 
        // Win32 API declarations
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        #endregion

        public MainForm()
        {
            try
            {
                InitializeComponents();
                InitializeDefaultValues();
                InitializeWindowListUpdater();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Initialization Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                throw;
            }
        }
               private void InitializeDefaultValues()
        {
            // Initialize resolution combo box
            PopulateResolutions();
            
            // Initialize compression methods
            PopulateCompressionMethods();
            
            // Initialize windows list
            PopulateWindows();
            
            // Set default values
            resolutionComboBox.SelectedIndex = 0;
            compressionMethodComboBox.SelectedIndex = 0;
            fpsNumeric.Value = 30;
            compressionStrengthTrackBar.Value = 50;
            windowsComboBox.SelectedIndex = 0;

            // Set initial status
            statusLabel.Text = "Ready";
            statusLabel.ForeColor = SystemColors.ControlText;
        }
    private void UpdateStreamSettings()
    {
        // Prevent too frequent updates (throttle to max once per second)
        if ((DateTime.UtcNow - lastSettingsUpdate).TotalSeconds < 1)
            return;

        lock (settingsLock)
        {
            try
            {
                if (isStreaming)
                {
                    // Update FPS
                    currentFps = (int)fpsNumeric.Value;

                    // Update resolution
                    if (resolutionComboBox.SelectedItem.ToString() != "Source")
                    {
                        var parts = resolutionComboBox.SelectedItem.ToString().Split('x');
                        currentResolution = new Size(
                            int.Parse(parts[0]),
                            int.Parse(parts[1])
                        );
                    }
                    else
                    {
                        currentResolution = Screen.PrimaryScreen.Bounds.Size;
                    }

                    // Update compression method
                    currentCompressionMethod = compressionMethodComboBox.SelectedItem.ToString();
                    currentCompressionStrength = compressionStrengthTrackBar.Value;

                    // Update selected window
                    selectedWindow = windowsComboBox.SelectedItem.ToString();

                    // Update status
                    statusLabel.Text = $"Settings updated - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                    lastSettingsUpdate = DateTime.UtcNow;

                    // Log the change
                    //Debug.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Settings updated by {Environment.UserName}");
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Failed to update settings: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }
        }
    }
    private void InitializeComponents()
    {
        // Add real-time update handlers for all settings controls
        resolutionComboBox.SelectedIndexChanged += (s, e) => UpdateStreamSettings();
        
        fpsNumeric.ValueChanged += (s, e) => UpdateStreamSettings();
        
        compressionMethodComboBox.SelectedIndexChanged += (s, e) => UpdateStreamSettings();
        
        compressionStrengthTrackBar.ValueChanged += (s, e) => {
            // Update tooltip or label showing current value
            statusLabel.Text = $"Compression Strength: {compressionStrengthTrackBar.Value}%";
            UpdateStreamSettings();
        };

    windowsComboBox.SelectedIndexChanged += (s, e) => UpdateStreamSettings();
        // Main form setup - Increased width for horizontal spacing
        this.Size = new Size(600, 850);
        this.Text = "Simple Screen Share";
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        // Role selection group
        roleGroup = new GroupBox()
        {
            Text = "Role",
            Location = new Point(35, 35),
            Size = new Size(530, 100)
        };

        serverRadio = new RadioButton()
        {
            Text = "Server",
            Location = new Point(40, 45),
            Width = 220,  // Fixed width for proper spacing
            Font = new Font(this.Font.FontFamily, 10)
        };

        clientRadio = new RadioButton()
        {
            Text = "Client",
            Location = new Point(serverRadio.Right + 20, 45),  // 20px gap
            Width = 220,  // Fixed width for proper spacing
            Font = new Font(this.Font.FontFamily, 10)
        };

        roleGroup.Controls.AddRange(new Control[] { serverRadio, clientRadio });

        // Network settings group
        networkGroup = new GroupBox()
        {
            Text = "Network Settings",
            Location = new Point(35, 170),
            Size = new Size(530, 140)
        };

        var ipLabel = new Label()
        {
            Text = "IP Address:",
            Location = new Point(40, 60),
            Width = 90,
            Font = new Font(this.Font.FontFamily, 10)
        };

        ipTextBox = new TextBox()
        {
            Text = DEFAULT_IP,
            Location = new Point(ipLabel.Right + 20, 57),  // 20px gap
            Width = 150,
            Font = new Font(this.Font.FontFamily, 10)
        };

        var portLabel = new Label()
        {
            Text = "Port:",
            Location = new Point(ipTextBox.Right + 20, 60),  // 20px gap
            Width = 50,
            Font = new Font(this.Font.FontFamily, 10)
        };

        portTextBox = new TextBox()
        {
            Text = DEFAULT_PORT.ToString(),
            Location = new Point(portLabel.Right + 20, 57),  // 20px gap
            Width = 100,
            Font = new Font(this.Font.FontFamily, 10)
        };

        networkGroup.Controls.AddRange(new Control[] { ipLabel, ipTextBox, portLabel, portTextBox });

        // Stream settings group
        streamGroup = new GroupBox()
        {
            Text = "Stream Settings",
            Location = new Point(35, 345),
            Size = new Size(530, 350)
        };

        var resolutionLabel = new Label()
        {
            Text = "Resolution:",
            Location = new Point(40, 55),
            Width = 90,
            Font = new Font(this.Font.FontFamily, 10)
        };

        resolutionComboBox = new ComboBox()
        {
            Location = new Point(resolutionLabel.Right + 20, 52),  // 20px gap
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font(this.Font.FontFamily, 10)
        };

        var fpsLabel = new Label()
        {
            Text = "FPS:",
            Location = new Point(40, 125),
            Width = 90,
            Font = new Font(this.Font.FontFamily, 10)
        };

        fpsNumeric = new NumericUpDown()
        {
            Location = new Point(fpsLabel.Right + 20, 122),  // 20px gap
            Width = 100,
            Minimum = 1,
            Maximum = 60,
            Value = 30,
            Font = new Font(this.Font.FontFamily, 10)
        };

        var compressionLabel = new Label()
        {
            Text = "Compression:",
            Location = new Point(40, 195),
            Width = 90,
            Font = new Font(this.Font.FontFamily, 10)
        };

        compressionMethodComboBox = new ComboBox()
        {
            Location = new Point(compressionLabel.Right + 20, 192),  // 20px gap
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font(this.Font.FontFamily, 10)
        };

        var compressionStrengthLabel = new Label()
        {
            Text = "Strength:",
            Location = new Point(40, 265),
            Width = 90,
            Font = new Font(this.Font.FontFamily, 10)
        };

        compressionStrengthTrackBar = new TrackBar()
        {
            Location = new Point(compressionStrengthLabel.Right + 20, 265),  // 20px gap
            Width = 350,
            Minimum = 1,
            Maximum = 100,
            Value = 50,
            TickFrequency = 10
        };

        var sourceLabel = new Label()
        {
            Text = "Source:",
            Location = new Point(40, 305),
            Width = 90,
            Font = new Font(this.Font.FontFamily, 10)
        };

        windowsComboBox = new ComboBox()
        {
            Location = new Point(sourceLabel.Right + 20, 302),  // 20px gap
            Width = 350,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font(this.Font.FontFamily, 10)
        };

        streamGroup.Controls.AddRange(new Control[] {
            resolutionLabel, resolutionComboBox,
            fpsLabel, fpsNumeric,
            compressionLabel, compressionMethodComboBox,
            compressionStrengthLabel, compressionStrengthTrackBar,
            sourceLabel, windowsComboBox
        });

        // Status label
        statusLabel = new Label()
        {
            Location = new Point(35, 730),
            Size = new Size(530, 35),
            TextAlign = ContentAlignment.MiddleCenter,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            Font = new Font(this.Font.FontFamily, 10)
        };

        // Control button
        startButton = new Button()
        {
            Text = "Start Stream",
            Location = new Point(35, 780),
            Size = new Size(530, 45),
            Font = new Font(this.Font.FontFamily, 11, FontStyle.Bold)
        };

        // Add all controls to form
        this.Controls.AddRange(new Control[] {
            roleGroup,
            networkGroup,
            streamGroup,
            statusLabel,
            startButton
        });

        // Event handlers remain the same
        serverRadio.CheckedChanged += (s, e) => {
            isServer = serverRadio.Checked;
            ipTextBox.Enabled = !isServer;
            statusLabel.Text = isServer ? "Ready to start server" : "Ready to connect to server";
        };

        startButton.Click += OnStartButtonClick;
    }
    private void OnStartButtonClick(object sender, EventArgs e)
    {
        if (!isStreaming)
        {
            if (int.TryParse(portTextBox.Text, out int port))
            {
                try
                {
                    StartStream(
                        ipTextBox.Text,
                        port,
                        resolutionComboBox.SelectedItem.ToString(),
                        (int)fpsNumeric.Value,
                        compressionMethodComboBox.SelectedItem.ToString(),
                        compressionStrengthTrackBar.Value,
                        windowsComboBox.SelectedItem.ToString()
                    );
                    startButton.Text = "Stop Stream";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start stream: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    StopStream();
                    return;
                }
            }
            else
            {
                MessageBox.Show("Invalid port number!", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        else
        {
            StopStream();
            startButton.Text = "Start Stream";
        }
        isStreaming = !isStreaming;
    }
    private void CreateStreamWindow()
    {
        streamWindow = new Form()
        {
            Text = "Stream Viewer",
            Size = new Size(1024, 768),
            StartPosition = FormStartPosition.CenterScreen,
            MinimumSize = new Size(640, 480)
        };

        streamPictureBox = new PictureBox()
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black
        };

        streamWindow.Controls.Add(streamPictureBox);
        
        // Add window title with connection info
        streamWindow.Text = isServer ? 
            $"Stream Viewer - Server (Port: {portTextBox.Text})" : 
            $"Stream Viewer - Connected to {ipTextBox.Text}:{portTextBox.Text}";

        streamWindow.FormClosing += (s, e) => {
            if (isStreaming)
            {
                StopStream();
                startButton.Text = "Start Stream";
                isStreaming = false;
            }
        };
    }

        private void PopulateResolutions()
        {
            resolutionComboBox.Items.Clear();
            string[] resolutions = new[]
            {
                "Source",
                "1920x1080",
                "1280x720",
                "854x480",
                "640x360"
            };
            resolutionComboBox.Items.AddRange(resolutions);
            resolutionComboBox.SelectedIndex = 0;
        }


        private void PopulateCompressionMethods()
        {
            compressionMethodComboBox.Items.Clear();
            string[] methods = new[]
            {
                "JPEG",
                "PNG",
                "GIF"
            };
            compressionMethodComboBox.Items.AddRange(methods);
            compressionMethodComboBox.SelectedIndex = 0;
        }

           private void PopulateWindows()
        {
            string currentSelection = windowsComboBox.SelectedItem?.ToString();
            windowsComboBox.Items.Clear();
            windowHandles.Clear();

            windowsComboBox.Items.Add("Entire Screen");

            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if (IsWindowVisible(hWnd))
                    {
                        StringBuilder sb = new StringBuilder(256);
                        if (GetWindowText(hWnd, sb, 256) > 0)
                        {
                            string title = sb.ToString().Trim();
                            if (!string.IsNullOrEmpty(title))
                            {
                                windowHandles[title] = hWnd;
                                windowsComboBox.Items.Add(title);
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error populating windows: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(currentSelection) && windowsComboBox.Items.Contains(currentSelection))
            {
                windowsComboBox.SelectedItem = currentSelection;
            }
            else
            {
                windowsComboBox.SelectedIndex = 0;
            }
        }
        

    private void StartStream(string ip, int port, string resolution, int fps,
        string compressionMethod, int compressionStrength, string windowSource)
    {
        try
        {
            // Initialize settings
            UpdateStreamSettings();

            if (isServer)
            {
                StartServer(port);
            }
            else
            {
                StartClient(ip, port);
            }

            // Enable controls during streaming
            EnableSettingsControlsDuringStream(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start stream: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            StopStream();
        }
    }
    private void EnableSettingsControlsDuringStream(bool enable)
    {
        // Allow these controls to be modified during streaming
        resolutionComboBox.Enabled = enable;
        fpsNumeric.Enabled = enable;
        compressionMethodComboBox.Enabled = enable;
        compressionStrengthTrackBar.Enabled = enable;
        windowsComboBox.Enabled = enable;

        // These should be disabled during streaming
        serverRadio.Enabled = !enable;
        clientRadio.Enabled = !enable;
        ipTextBox.Enabled = !enable && !isServer;
        portTextBox.Enabled = !enable;
    }

    private void StartServer(int port)
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            streamThread = new Thread(() =>
            {
                while (isStreaming)
                {
                    try
                    {
                        this.Invoke((MethodInvoker)delegate {
                            statusLabel.Text = "Waiting for client connection...";
                            statusLabel.ForeColor = Color.Blue;
                        });

                        using (var client = server.AcceptTcpClient())
                        {
                            string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                            
                            this.Invoke((MethodInvoker)delegate {
                                CreateStreamWindow();
                                streamWindow.Show();
                                statusLabel.Text = $"Client connected from {clientIp}";
                                statusLabel.ForeColor = Color.Green;
                            });

                            using (var stream = client.GetStream())
                            {
                                while (isStreaming && client.Connected)
                                {
                                    var screenshot = CaptureScreen();
                                    var compressedImage = CompressImage(screenshot);
                                    screenshot.Dispose();

                                    if (compressedImage.Length > 0)
                                    {
                                        // Send size first
                                        var size = BitConverter.GetBytes(compressedImage.Length);
                                        stream.Write(size, 0, size.Length);
                                        
                                        // Send image data
                                        stream.Write(compressedImage, 0, compressedImage.Length);
                                        
                                        // Update stream window with current frame
                                        if (streamPictureBox != null && !streamPictureBox.IsDisposed)
                                        {
                                            using (var ms = new MemoryStream(compressedImage))
                                            {
                                                var image = Image.FromStream(ms);
                                                streamPictureBox.Invoke((MethodInvoker)delegate {
                                                    var oldImage = streamPictureBox.Image;
                                                    streamPictureBox.Image = image;
                                                    oldImage?.Dispose();
                                                });
                                            }
                                        }

                                        // Update status periodically
                                        if (DateTime.Now.Second % 5 == 0) // Every 5 seconds
                                        {
                                            this.Invoke((MethodInvoker)delegate {
                                                statusLabel.Text = $"Streaming to client {clientIp}";
                                            });
                                        }
                                    }

                                    Thread.Sleep(1000 / currentFps); // Maintain FPS
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (isStreaming)
                        {
                            this.Invoke((MethodInvoker)delegate {
                                statusLabel.Text = $"Client disconnected: {ex.Message}";
                                statusLabel.ForeColor = Color.Orange;
                                // Don't stop streaming - wait for new client
                            });
                        }
                    }
                }
            });
            streamThread.Start();
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Failed to start server: {ex.Message}";
            statusLabel.ForeColor = Color.Red;
            throw;
        }
    }
    private void StartClient(string ip, int port)
    {
        try
        {
            client = new TcpClient();
            var connectTask = client.ConnectAsync(ip, port);
            
            // Add timeout for connection attempt
            if (!connectTask.Wait(5000)) // 5 second timeout
            {
                throw new Exception("Connection timeout");
            }

            // Only create and show window after successful connection
            this.Invoke((MethodInvoker)delegate {
                CreateStreamWindow();
                streamWindow.Show();
                statusLabel.Text = $"Connected to {ip}:{port}";
                statusLabel.ForeColor = Color.Green;
            });

            streamThread = new Thread(() =>
            {
                try
                {
                    using (var stream = client.GetStream())
                    {
                        while (isStreaming && client.Connected)
                        {
                            // Receive size first
                            byte[] sizeBytes = new byte[4];
                            int bytesRead = stream.Read(sizeBytes, 0, 4);
                            if (bytesRead < 4) break; // Connection lost

                            int size = BitConverter.ToInt32(sizeBytes, 0);

                            // Receive image data
                            byte[] imageData = new byte[size];
                            bytesRead = 0;
                            while (bytesRead < size)
                            {
                                int read = stream.Read(imageData, bytesRead, size - bytesRead);
                                if (read == 0) break; // Connection lost
                                bytesRead += read;
                            }

                            if (bytesRead < size) break; // Incomplete data received

                            using (var ms = new MemoryStream(imageData))
                            {
                                var image = Image.FromStream(ms);
                                if (streamPictureBox != null && !streamPictureBox.IsDisposed)
                                {
                                    streamPictureBox.Invoke((MethodInvoker)delegate {
                                        var oldImage = streamPictureBox.Image;
                                        streamPictureBox.Image = image;
                                        oldImage?.Dispose();
                                    });
                                }
                            }

                            // Update connection status periodically
                            if (DateTime.Now.Second % 5 == 0) // Every 5 seconds
                            {
                                this.Invoke((MethodInvoker)delegate {
                                    statusLabel.Text = $"Connected to {ip}:{port} - Receiving data";
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (isStreaming)
                    {
                        this.Invoke((MethodInvoker)delegate {
                            statusLabel.Text = $"Connection error: {ex.Message}";
                            statusLabel.ForeColor = Color.Red;
                            StopStream();
                            startButton.Text = "Start Stream";
                            isStreaming = false;
                        });
                    }
                }
            });
            streamThread.Start();
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Failed to connect: {ex.Message}";
            statusLabel.ForeColor = Color.Red;
            throw;
        }
    }
    private void InitializeWindowListUpdater()
    {
        var windowListTimer = new System.Windows.Forms.Timer();
        windowListTimer.Interval = 5000; // Update every 5 seconds
        windowListTimer.Tick += (s, e) => {
            if (isStreaming)
            {
                string currentSelection = windowsComboBox.SelectedItem?.ToString();
                PopulateWindows();
                if (!string.IsNullOrEmpty(currentSelection))
                {
                    windowsComboBox.SelectedItem = currentSelection;
                }
            }
        };
        windowListTimer.Start();
    }
    private void StopStream()
    {
        isStreaming = false;
        
        if (streamThread != null && streamThread.IsAlive)
        {
            streamThread.Join(1000);
            if (streamThread.IsAlive)
            {
                streamThread.Abort();
            }
        }

        if (server != null)
        {
            server.Stop();
            server = null;
        }

        if (client != null)
        {
            client.Close();
            client = null;
        }

        if (streamWindow != null && !streamWindow.IsDisposed)
        {
            streamWindow.Invoke((MethodInvoker)delegate {
                streamWindow.Close();
            });
            streamWindow = null;
            streamPictureBox = null;
        }

        // Reset controls state
        EnableSettingsControlsDuringStream(false);
        statusLabel.Text = "Stream stopped";
        statusLabel.ForeColor = SystemColors.ControlText;
    }

        private Bitmap CaptureScreen()
        {
            Bitmap screenshot;
            
            if (selectedWindow == "Entire Screen")
            {
                var bounds = Screen.PrimaryScreen.Bounds;
                screenshot = new Bitmap(bounds.Width, bounds.Height);
                using (var g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
            }
            else
            {
                if (windowHandles.TryGetValue(selectedWindow, out IntPtr hWnd))
                {
                    // Get window bounds
                    RECT rect;
                    GetWindowRect(hWnd, out rect);
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    screenshot = new Bitmap(width, height);
                    using (var g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
                    }
                }
                else
                {
                    // Fallback to entire screen if window not found
                    var bounds = Screen.PrimaryScreen.Bounds;
                    screenshot = new Bitmap(bounds.Width, bounds.Height);
                    using (var g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }
                }
            }

            // Resize if needed
            if (currentResolution != screenshot.Size)
            {
                var resized = new Bitmap(currentResolution.Width, currentResolution.Height);
                using (var g = Graphics.FromImage(resized))
                {
                    g.DrawImage(screenshot, 0, 0, currentResolution.Width, currentResolution.Height);
                }
                screenshot.Dispose();
                return resized;
            }

            return screenshot;
        }

      private byte[] CompressImage(Image image)
{
    using (var ms = new MemoryStream())
    {
        if (currentCompressionMethod == "JPEG")
        {
            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, currentCompressionStrength);
            image.Save(ms, jpegEncoder, encoderParameters);
        }
        else if (currentCompressionMethod == "PNG")
        {
            image.Save(ms, ImageFormat.Png);
        }
        else // GIF or fallback
        {
            image.Save(ms, ImageFormat.Gif);
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
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopStream();
            base.OnFormClosing(e);
        }

        #region Win32 Structs and Methods
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        #endregion
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}   