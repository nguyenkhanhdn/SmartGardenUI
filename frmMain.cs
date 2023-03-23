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
            if (level == "Tomato_Early_blight")
            {
                textBox1.Text = "Bệnh sương mai";
                textBox2.Text = "Trên lá vết bệnh bắt đầu là những đốm nhỏ hình tròn đến hình bầu dục màu nâu sẫm đến đen. Những đốm này to ra, trở thành hình bầu dục đến góc cạnh và thường giới hạn trong các gân chính của lá chét. Chúng có đường kính lên tới 6 mm. Trong điều kiện rất thuận lợi, các đốm riêng lẻ có thể phát triển tới 10-12 mm. Chúng trở nên trông giống như da và sự phát triển của các vòng đồng tâm gần nhau trong mỗi đốm khiến bệnh có tên là đốm mục tiêu. Khi bệnh nặng, các đốm có thể liên kết với nhau và làm cho đầu lá cuộn lên trên và làm chết lá. Các đốm phát triển trên thân dài hơn các đốm trên lá. Điểm mục tiêu đôi khi có thể lây nhiễm sang củ. Nó xuất hiện như bóng tối nhỏ, các tổn thương hơi trũng, hình tròn đến hình dạng không đều (đường kính 10 đến 20 mm), với bờ hơi gồ lên. Một vết thối khô màu nâu, sần sùi sâu đến 6 mm phát triển trong mô bên dưới vết bệnh.";
                textBox3.Text = "Loại bỏ các mảnh vụn thực vật cũ vào cuối mùa sinh trưởng; nếu không, các bào tử sẽ di chuyển từ mảnh vụn sang cà chua mới trồng trong mùa sinh trưởng tiếp theo, do đó bệnh lại bùng phát trở lại. Vứt bỏ các mảnh vụn đúng cách và không đặt nó trên đống phân ủ của bạn trừ khi bạn chắc chắn rằng phân trộn của bạn đủ nóng để tiêu diệt các bào tử.";

            }else if (level == "Tomato_Leaf_Mold")
            {
                textBox1.Text = "Bệnh mốc ở lá";
                textBox2.Text = "Những đốm màu xanh nhạt đến hơi vàng trên mặt trên của lá chuyển sang màu vàng tươi. Các đốm hợp nhất khi bệnh tiến triển và tán lá sau đó chết. Lá bị nhiễm bệnh cuộn lại, khô héo và thường rụng khỏi cây. Hoa, thân và quả có thể bị nhiễm bệnh, mặc dù thường chỉ mô lá bị ảnh hưởng. Khi bệnh biểu hiện trên quả, cà chua bị mốc lá có màu sẫm, sần sùi và thối ở đầu cuống.";
                textBox3.Text = "Độ ẩm tương đối cao (hơn 85%) kết hợp với nhiệt độ cao khuyến khích sự lây lan của bệnh. Với ý nghĩ đó, nếu trồng cà chua trong nhà kính, hãy duy trì nhiệt độ ban đêm cao hơn nhiệt độ bên ngoài. Khám phá thêm Khi trồng, chỉ sử dụng hạt giống sạch bệnh đã được chứng nhận hoặc hạt giống đã được xử lý. Loại bỏ và tiêu hủy tất cả các mảnh vụn cây trồng sau thu hoạch. Vệ sinh nhà kính giữa các vụ mùa. Sử dụng quạt và tránh tưới nước trên cao để giảm thiểu độ ẩm của lá. Ngoài ra, cắm cọc và tỉa cây để tăng khả năng thông gió.";
            }
            else
            {
                textBox1.Text = "Bệnh đốm lá";
                textBox2.Text = "Các triệu chứng thường xuất hiện trên lá, nhưng có thể xuất hiện trên cuống lá, thân và đài hoa. Các triệu chứng đầu tiên xuất hiện dưới dạng các đốm tròn nhỏ, thấm nước, có đường kính từ 1/16 đến 1/8 ở mặt dưới của các lá già.Tâm của các đốm này sau đó chuyển sang màu xám đến rám nắng và có viền màu nâu sẫm. Các đốm này có hình tròn đặc biệt và thường khá nhiều. Khi các đốm già đi, đôi khi chúng to ra và thường liên kết với nhau.Một đặc điểm chẩn đoán bệnh này là sự hiện diện của nhiều cấu trúc giống như mụn, màu nâu sẫm được gọi là pycnidia(quả thể của nấm) có thể dễ dàng nhìn thấy ở trung tâm rám nắng của các đốm. Khi các đốm có nhiều, lá bị ảnh hưởng chuyển sang màu vàng và cuối cùng teo lại, màu nâu và rụng.";
                textBox3.Text = "Điều rất quan trọng là phải loại bỏ nguồn lây nhiễm ban đầu bằng cách loại bỏ hoặc tiêu hủy càng nhiều mảnh vụn cà chua càng tốt sau khi thu hoạch vào mùa thu. Ngoài ra, ở những cánh đồng rộng lớn mà việc loại bỏ thực vật là không thực tế, tàn dư thực vật có thể được che phủ và chôn vùi bằng cách cày sâu . tránh tưới nước từ trên cao hoặc tưới nước sớm trong ngày để lá khô nhanh hơn so với tưới nước vào ban đêm. Ngoài ra, bạn nên tránh làm việc với cây khi chúng bị ướt. Mặc dù tính kháng bệnh đốm lá Septoria đã được xác định tạm thời ở một số dòng cà chua được sử dụng để nhân giống, nhưng cho đến nay không có giống kháng bệnh nào được bán trên thị trường. Thuốc diệt nấm rất hiệu quả để kiểm soát bệnh đốm lá Septoria và các ứng dụng thường cần thiết để bổ sung cho các chiến lược kiểm soát đã vạch ra trước đó. Thuốc diệt nấm chlorothalonil và mancozeb được dán nhãn để chủ nhà sử dụng.";
            }

        }

        private string Predict(string fileName)
        {
            var signatureFilePath = GetParentDir() + @"\model\signature.json";
            var imageToClassify = fileName;

            lobe.ImageClassifier.Register("onnx", () => new OnnxImageClassifier());
            var classifier = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

            //var classifier2 = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

            var results = classifier.Classify(SixLabors.ImageSharp.Image.Load(imageToClassify).CloneAs<Rgb24>());
            return results.Prediction.Label;
        }

        private string Predict(byte[] bitmap)
        {
            var signatureFilePath = GetParentDir() + @"\model\signature.json";

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

        private void button1_Click(object sender, EventArgs e)
        {
            string path = Environment.CurrentDirectory;
            System.IO.DirectoryInfo directoryInfo =System.IO.Directory.GetParent(path);
            MessageBox.Show(directoryInfo.FullName);
        }
        private string GetParentDir()
        {
            string path = Environment.CurrentDirectory;
            System.IO.DirectoryInfo directoryInfo = System.IO.Directory.GetParent(path);
            return directoryInfo.FullName;
        }
    }
}
