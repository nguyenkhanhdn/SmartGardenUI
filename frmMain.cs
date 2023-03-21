using AForge.Video;
using AForge.Video.DirectShow;
using lobe;
using lobe.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartGardenUI
{
    public partial class frmMain : Form
    {
        private VideoCaptureDevice captureDevice;
        private FilterInfoCollection filterInfo;

        private bool cameraOn = false;

        private SoundPlayer simpleSound = new SoundPlayer(Environment.CurrentDirectory + @"\sound\alarm.wav");

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        void StartCamera()
        {
            try
            {
                filterInfo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                captureDevice = new VideoCaptureDevice(filterInfo[0].MonikerString);

                captureDevice.NewFrame += new NewFrameEventHandler(CameraOn);
                if (cameraOn)
                {
                    captureDevice.Start();
                }
                else
                {
                    captureDevice.Stop();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure want to exit application?", "Smart Garden Predictor", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //string imageDir = Path.Combine(Environment.CurrentDirectory, "..", "..");
            var bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bitmap, pictureBox1.ClientRectangle);
           
            byte[] bytes = ImageToByte2(bitmap);
            string className = Predict(bytes);
            ProcessResult(className);
        }
        private void ProcessResult(string level)
        {
            this.textBox1.Text = level;

            //if (level == "nofire")
            //{
            //    timer1.Enabled = false;
            //    textBox2.Text = "Bình thường";
            //    textBox3.Text = "TẮT";
            //    SendToCloud(0, "0", "0");
            //    simpleSound.Stop();
            //    MessageBox.Show("0");
            //    timer1.Enabled = true;

            //}
            
        }

        private string Predict(string fileName)
        {
            var signatureFilePath = Environment.CurrentDirectory + @"\model\signature.json";
            var imageToClassify = fileName;

            lobe.ImageClassifier.Register("onnx", () => new OnnxImageClassifier());
            var classifier = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

            //var classifier2 = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

            var results = classifier.Classify(SixLabors.ImageSharp.Image.Load(imageToClassify).CloneAs<Rgb24>());
            return results.Prediction.Label;
        }

        private string Predict(byte[] bitmap)
        {
            var signatureFilePath = Environment.CurrentDirectory + @"\model\signature.json";

            lobe.ImageClassifier.Register("onnx", () => new OnnxImageClassifier());
            var classifier = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

            var results = classifier.Classify(SixLabors.ImageSharp.Image.Load(bitmap).CloneAs<Rgb24>());
            return results.Prediction.Label;
        }

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        private void SendToCloud(int level, string desc, string onOff)
        {
            try
            {
                const string WRITEKEY = "TLCC0QSDXNRF07ZD";
                string strUpdateBase = "http://api.thingspeak.com/update";
                string strUpdateURI = strUpdateBase + "?key=" + WRITEKEY;
                string strField1 = level.ToString();
                string strField2 = desc;
                string strField3 = onOff;
                HttpWebRequest ThingsSpeakReq;
                HttpWebResponse ThingsSpeakResp;

                strUpdateURI += "&field1=" + strField1;
                strUpdateURI += "&field2=" + strField2;
                strUpdateURI += "&field3=" + strField3;

                ThingsSpeakReq = (HttpWebRequest)WebRequest.Create(strUpdateURI);

                ThingsSpeakResp = (HttpWebResponse)ThingsSpeakReq.GetResponse();

                if (!(string.Equals(ThingsSpeakResp.StatusDescription, "OK")))
                {
                    Exception exData = new Exception(ThingsSpeakResp.StatusDescription);
                    //throw exData;
                }
            }
            catch (Exception ex)
            {
                this.Text = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        private void ribbon1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void CameraOn(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            captureDevice.Stop();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            cameraOn = true;
            StartCamera();
        }
    }
}
