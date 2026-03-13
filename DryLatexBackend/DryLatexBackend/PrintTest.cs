using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.VisualBasic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Xml.Linq;

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


        [HttpGet("GetBillList")]
        public IActionResult GetBillList()
        {
            string connString =
               "Server=localhost;" +
               "Database=latexapp;" +
               "User ID=root;" +
               "Password=131001;";
            string list = @"SELECT Id,CustomerName , totalweight,bucket,deduct,price ,TotalAmount,Divide FROM latexapp.drylatexsum where DATE(CreatedAt) = CURDATE();";
            List<PrintRequest> bills = new List<PrintRequest>();
            using (var conn = new MySqlConnection(connString))
            using (var cmd = new MySqlCommand(list, conn))
            {

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PrintRequest bill = new PrintRequest
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["CustomerName"].ToString(),
                            TotalWeight = reader["totalweight"].ToString(),
                            Bucket = reader["bucket"].ToString(),
                            Deduct = reader["deduct"].ToString(),

                            Price = reader["price"].ToString(),
                           Total = reader["TotalAmount"].ToString(),
                           Divide = reader["Divide"].ToString()
                        };
                        bills.Add(bill);
                    }
                }
               
            } return Ok(bills);
        }
        [HttpPost("EditData")]
        public IActionResult EditData([FromBody] PrintRequest request)
        {
            calculatemoney(request.TotalWeight, request.Bucket, request.Deduct, request.Price,request.Divide);
            UpdateDB(request.Id,request.Name,request.Price, request.Bucket, request.Deduct,request.TotalWeight,request.Divide);
            string totalCostAfterUpdate = total.ToString();
            return Ok(total);

        }
        [HttpPost("EditDataAndPrint")]
        public IActionResult EditDataAndPrint([FromBody] PrintRequest request)
        {
            calculatemoney(request.TotalWeight, request.Bucket, request.Deduct, request.Price,request.Divide);
            UpdateDB(request.Id, request.Name, request.Price, request.Bucket, request.Deduct, request.TotalWeight,request.Divide);
            string totalCostAfterUpdate = total.ToString();
            Printing(request);
            return Ok(total);

        }

        [HttpPost("end-day")]
        public IActionResult EndDay()
        {
          
            string connString =
               "Server=localhost;" +
               "Database=latexapp;" +
               "User ID=root;" +
               "Password=131001;";
            string sumdry  = @"SELECT"+
    " COALESCE(SUM(CASE WHEN isSheet = 0 THEN NetWeight ELSE 0 END),0) AS weight_normal," +
    " COALESCE(SUM(CASE WHEN isSheet = 0 THEN TotalAmount ELSE 0 END),0) AS cost_normal," +

    " COALESCE(SUM(CASE WHEN isSheet = 1 THEN NetWeight ELSE 0 END),0) AS weight_sheet," +
    " COALESCE(SUM(CASE WHEN isSheet = 1 THEN TotalAmount ELSE 0 END),0) AS cost_sheet" +

" FROM latexapp.drylatexsum"+
" WHERE CreatedAt >= CURDATE()"+
" AND CreatedAt < CURDATE() + INTERVAL 1 DAY;";
              string drylatex = "ขึ้ยาง";
            string sheetlatex = "ยางแผ่น";
            using (var conn = new MySqlConnection(connString))
            using (var cmd = new MySqlCommand(sumdry, conn))
            {
               
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        decimal weightNormal = reader.GetDecimal("weight_normal");
                        decimal costNormal = reader.GetDecimal("cost_normal");

                        decimal weightSheet = reader.GetDecimal("weight_sheet");
                        decimal costSheet = reader.GetDecimal("cost_sheet");
                        return Ok(new //send response of data as json 
                        {
                            drylatex = drylatex,
                            totalWeight = weightNormal,
                            totalCost = costNormal,

                            sheetlatex = sheetlatex,
                            totalSheetWeight = weightSheet,
                            totalSheetCost = costSheet
                            //check this later
                        });
                    }
                
                }
            }

            return Ok(new
            {
                drylatex = "ขี้ยาง",
                totalWeight = 0,
                totalCost = 0,

                sheetlatex = "ยางแผ่น",
                totalSheetWeight = 0,
                totalSheetCost = 0
            });

        }
        

        [HttpPost]
        public IActionResult Print([FromBody] PrintRequest request)
        {
            calculatemoney(request.TotalWeight, request.Bucket, request.Deduct, request.Price,request.Divide);
            Printing(request);
            SaveNewToDB(request.Name, request.Price, request.Bucket, request.Deduct,request.TotalWeight,request.Divide); //the rest already exists in entire class 
           
              

            return Ok("ปริ้นสำเร็จ");

        }
        private void Printing(PrintRequest request)
        {
            Bitmap bmp = new Bitmap(384, 650);
            Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.White);

            System.Drawing.Font titleFont = new System.Drawing.Font("Tahoma", 32, FontStyle.Bold);
            System.Drawing.Font font = new System.Drawing.Font("Tahoma", 30);
            int leftX = 2;
            int rightX = 390;
            int y = 0;

            g.DrawString("ร้านเกษรคลองแงะ", titleFont, Brushes.Black, leftX, y);
            y += 60;

            g.DrawString($"ชื่อ: {request.Name}", font, Brushes.Black, leftX, y);
            y += 40;

            g.DrawString($"วันที่: {DateTime.Now:dd/MM/yyyy}", font, Brushes.Black, leftX, y);
            y += 40;

            g.DrawString("--------------------------------", font, Brushes.Black, leftX, y);
            y += 40;

            DrawLeftRight(g, font, "น้ำหนัก", $"{request.TotalWeight} กก", leftX, rightX, y);
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
            if (request.Divide == "ไม่แบ่ง")
            {
                DrawLeftRight(g, titleFont, "เป็นเงิน", $"{total} บาท", leftX, rightX, y);
            }
            else
            {
                var parts = request.Divide.Split('/');
                string bossPart = parts[0];
                string employeePart = parts[1];
                DrawLeftRight(g, titleFont, $"เถ้าแก่{bossPart}", $"{bossShare} บาท", leftX, rightX, y);
                y += 40;
                DrawLeftRight(g, titleFont, $"ลูกน้อง{employeePart}", $"{employeeShare} บาท", leftX, rightX, y);
            }
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

        decimal bossShare = 0;
        decimal employeeShare = 0;
        private void calculatemoney(string W, string b, string d, string p,string divide)
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
            decimal bossRatio = 1m;
            decimal employeeRatio = 0m;

            switch (divide)
            {
                case "6/4":
                    bossRatio = 0.6m;
                    employeeRatio = 0.4m;
                    break;

                case "5/5":
                    bossRatio = 0.5m;
                    employeeRatio = 0.5m;
                    break;

                case "ไม่แบ่ง":
                default:
                    bossRatio = 1m;
                    employeeRatio = 0m;
                    break;
            }

            bossShare = total * bossRatio;
            employeeShare = total * employeeRatio;

        }
        private void SaveNewToDB(string name,string price,string bucket,string deduct,string totalweight,string divide)
        {//save new data , id will be automized
            string connString =
             "Server=localhost;" +
             "Database=latexapp;" +
             "User ID=root;" +
             "Password=131001;";
            string sql = @"INSERT INTO latexapp.drylatexsum (CustomerName,totalweight,bucket ,deduct, NetWeight,price, TotalAmount,Divide)
            VALUES (@Name,@totalweight,@bucket,@deduct,@weight,@price,@total,@divide);";

            using (var conn = new MySqlConnection(connString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@totalweight", totalweight);
                cmd.Parameters.AddWithValue("@bucket", bucket); 
                cmd.Parameters.AddWithValue("@deduct", deduct);
                cmd.Parameters.AddWithValue("@weight", netWeight);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@total", total);
                cmd.Parameters.AddWithValue("@divide", divide);

                conn.Open();
                cmd.ExecuteNonQuery();

            }
        }
        private void UpdateDB(int id, string name , string price, string bucket, string deduct,string totalweight,string divide)
        {
            string connString =
            "Server=localhost;" +
            "Database=latexapp;" +
            "User ID=root;" +
            "Password=131001;";
            string sql = @"Update latexapp.drylatexsum SET CustomerName = @Name, totalweight = @totalweight,bucket = @bucket, deduct=@deduct,NetWeight=@weight ,price = @price,TotalAmount = @total, Divide=@divide WHERE Id = @Id;";
            using (var conn = new MySqlConnection(connString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@totalweight", totalweight);
                cmd.Parameters.AddWithValue("@bucket", bucket);
                cmd.Parameters.AddWithValue("@deduct", deduct);
                cmd.Parameters.AddWithValue("@weight", netWeight);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@total", total);
                cmd.Parameters.AddWithValue("@divide", divide);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
