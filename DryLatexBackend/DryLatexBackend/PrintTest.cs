using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.VisualBasic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace DryLatexBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        public PrintController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
      
        decimal netWeight;
        
        decimal total; 
        private readonly IConfiguration _configuration;


        [HttpPost("end-day")]
        public IActionResult EndDay()
        {

           
                return Ok("Day closed.");
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintRequest request)
        {
            calculatemoney(request.Weight, request.Bucket, request.Deduct, request.Price);
            string connString =
               "Server=localhost;" +
               "Database=latexapp;" +
               "User ID=root;" +
               "Password=131001;";
            string sql = @"INSERT INTO latexapp.drylatexsum (CustomerName, TotalWeight, TotalAmount)
            VALUES (@Name,@weight,@total);";

            using (var conn = new MySqlConnection(connString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Name", request.Name);
                cmd.Parameters.AddWithValue("@weight",netWeight);
                cmd.Parameters.AddWithValue("@total", total);

                conn.Open();
                cmd.ExecuteNonQuery();
            }


                Bitmap bmp = new Bitmap(384, 650);
            Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.White);

            System.Drawing.Font titleFont = new System.Drawing.Font("Tahoma", 32, FontStyle.Bold);
            System.Drawing.Font font = new System.Drawing.Font("Tahoma", 30);
            int leftX = 2;
            int rightX = 390;
            int y = 0;

            g.DrawString("ร้านเกสรคลองแงะ", titleFont, Brushes.Black, leftX, y);
            y += 60;

            g.DrawString($"ชื่อ: {request.Name}", font, Brushes.Black, leftX, y);
            y += 40;

            g.DrawString($"วันที่: {DateTime.Now:dd/MM/yyyy}", font, Brushes.Black, leftX, y);
            y += 40;

            g.DrawString("--------------------------------", font, Brushes.Black, leftX, y);
            y += 40;

            DrawLeftRight(g, font, "น้ำหนัก", $"{request.Weight} กก", leftX, rightX, y);
            y += 40;

            DrawLeftRight(g, font, "เข่ง", $"{request.Bucket} กก", leftX, rightX, y);
            y += 40;

            DrawLeftRight(g, font, "หัก", $"{request.Deduct} กก", leftX, rightX, y);
            y += 40;

            DrawLeftRight(g, font, "คงเหลือ", $"{netWeight} กก", leftX, rightX, y);
            y += 40;

            DrawLeftRight(g, font, "ราคา", $"{request.Price} บ.", leftX, rightX, y);
            y += 40;

            g.DrawString("--------------------------------", font, Brushes.Black, leftX, y);
            y += 50;

            DrawLeftRight(g, titleFont, "เป็นเงิน", $"{total} บาท", leftX, rightX, y);
            y += 60;
            Console.WriteLine("Final Y = " + y);
            SerialPort port = new SerialPort("COM3", 9600);
            port.Open();

            var stream = port.BaseStream;

            byte[] imageBytes = ConvertBitmap(bmp);

            stream.Write(imageBytes);
            byte[] feedAndCut = new byte[]
            {
    0x1B, 0x64, 0x03,   // Feed 3 lines
    0x1D, 0x56, 0x00    // Full cut
            };

            stream.Write(feedAndCut, 0, feedAndCut.Length);

            port.Close();

            return Ok("ปริ้นสำเร็จ");

        }
        private void DrawLeftRight(
    Graphics g,
    System.Drawing.Font font,
    string left,
    string right,
    int leftX,
    int rightEdge,
    int y)
        {
            g.DrawString(left, font, Brushes.Black, leftX, y);

            SizeF size = g.MeasureString(right, font);

            float rightX = rightEdge - size.Width;

            g.DrawString(right, font, Brushes.Black, rightX, y);
        }

        private byte[] ConvertBitmap(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            List<byte> bytes = new List<byte>();

            bytes.AddRange(new byte[] { 0x1B, 0x40 });
            bytes.AddRange(new byte[] { 0x1B, 0x33, 24 });
            for (int y = 0; y < height; y += 24)
            {
                bytes.AddRange(new byte[]
                {
                    0x1B,0x2A,33,
                    (byte)(width % 256),
                    (byte)(width / 256)
                });

                for (int x = 0; x < width; x++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        byte slice = 0;

                        for (int b = 0; b < 8; b++)
                        {
                            int yy = y + k * 8 + b;

                            if (yy >= height)
                                continue;

                            Color pixel = bmp.GetPixel(x, yy);

                            int gray =
    (pixel.R * 30 +
     pixel.G * 59 +
     pixel.B * 11) / 100;

                            if (gray < 128)
                                slice |= (byte)(1 << (7 - b));
                        }

                        bytes.Add(slice);
                    }
                }


                // bytes.AddRange(new byte[] { 0x1B, 0x4A, 0x00 });//imply new line 
            }

            return bytes.ToArray();
        }

        private void calculatemoney(string W, string b, string d, string p)
        {

            decimal weight = 0;
            decimal bucket = 0;
            decimal deduct = 0;
            decimal price = 0;
        
            decimal.TryParse(W, out weight);
            decimal.TryParse(b, out bucket);
            decimal.TryParse(d, out deduct);
            decimal.TryParse(p, out price);
          

            netWeight = weight - (bucket + deduct);
            total = netWeight * price;
           
        }
    }
}
