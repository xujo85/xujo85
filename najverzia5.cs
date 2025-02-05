using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private TcpListener listener;
    private TcpClient client;
    private NetworkStream stream;
    private CancellationTokenSource cts;
    private int selectedDisplayIndex;
    private ImageFormat compressionFormat;
    private long compressionQuality;
    private StreamDisplayForm streamDisplayForm;
    private int screenWidth;
    private int screenHeight;
    private int fps; // Field to store the current FPS value

    public MainForm()
    {
        InitializeComponent();
    }
private void InitializeComponent()
{
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
    ((System.ComponentModel.ISupportInitialize)(this.trkCompressionLevel)).BeginInit();
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

    // cmbResolution
    this.cmbResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
    this.cmbResolution.FormattingEnabled = true;
    this.cmbResolution.Items.AddRange(new object[] {
        "source",   // Add the "source" option
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

    // MainForm
    this.ClientSize = new System.Drawing.Size(784, 561);
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

        // Add event handler for the compression level slider
        trkCompressionLevel.Scroll += new EventHandler(trkCompressionLevel_Scroll);
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
            await StartServerAsync(cts.Token);
        }
        else
        {
            await StartClientAsync(cts.Token);
        }
    }

    private async Task StartServerAsync(CancellationToken cancellationToken)
    {
        listener = new TcpListener(IPAddress.Parse(txtIPAddress.Text), int.Parse(txtPort.Text));
        listener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => ServerLoopAsync(client, cancellationToken), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Server error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task ServerLoopAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        {
            stream = client.GetStream();
            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                Bitmap screenshot = CaptureScreen(selectedDisplayIndex);
                byte[] imageBytes = ImageToByte(screenshot, compressionFormat, compressionQuality);
                try
                {
                    await stream.WriteAsync(imageBytes, 0, imageBytes.Length, cancellationToken);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Server stream error: {ex.Message}");
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
}    private byte[] ImageToByte(Image img, ImageFormat format, long quality)
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

    private async Task StartClientAsync(CancellationToken cancellationToken)
    {
        client = new TcpClient();
        try
        {
            await client.ConnectAsync(txtIPAddress.Text, int.Parse(txtPort.Text));
            stream = client.GetStream();
            streamDisplayForm = new StreamDisplayForm();
            streamDisplayForm.Show();

            await ClientLoopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Client error: {ex.Message}");
        }
    }

    private async Task ClientLoopAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024 * 1024];
        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
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
                MessageBox.Show($"Client stream error: {ex.Message}");
                break;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        cts?.Cancel();

        base.OnFormClosing(e);
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

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
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