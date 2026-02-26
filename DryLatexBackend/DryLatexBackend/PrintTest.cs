using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace DryLatexBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        [HttpPost]
        public IActionResult Print()
        {
            SerialPort port = new SerialPort("COM3", 9600);
            port.Open();

            // Thai support
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Encoding thai = Encoding.GetEncoding("windows-874");
            //byte[] thaiMode = new byte[] { 0x1B, 0x74, 0x16 };
            //port.Write(thaiMode, 0, thaiMode.Length);
            string receipt =
                "ร้านยางเกสรคลองแงะ\r\n" +
                "ชื่อ: ลูกค้า\r\n" +
                "วันที่: 2026/2/26\r\n" +
                "----------------------------\r\n" +
                "น้ำหนัก                  20 กก\r\n" +
                "หัก                     3 กก\r\n" +
                "คงเหลือ                 17 กก\r\n" +
                "ราคา                   30 บาท\r\n" +
                "----------------------------\r\n" +
                "เป็นเงิน                 510 บาท\r\n\r\n\r\n";

            byte[] textBytes = thai.GetBytes(receipt);
            port.Write(textBytes, 0, textBytes.Length);

            // Underline ON
            port.Write(new byte[] { 0x1B, 0x2D, 0x01 }, 0, 3);

            byte[] moneyBytes = thai.GetBytes("510");
            port.Write(moneyBytes, 0, moneyBytes.Length);

            // Underline OFF
            port.Write(new byte[] { 0x1B, 0x2D, 0x00 }, 0, 3);

            byte[] bahtBytes = thai.GetBytes(" บาท\r\n\r\n\r\n");
            port.Write(bahtBytes, 0, bahtBytes.Length);

            // Auto Cut
            byte[] cutCommand = new byte[] { 0x1D, 0x56, 0x00 };
            port.Write(cutCommand, 0, cutCommand.Length);

            port.Close();

            return Ok("Printed successfully");

        }
    }
}