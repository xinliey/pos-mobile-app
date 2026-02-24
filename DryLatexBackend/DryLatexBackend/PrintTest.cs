using Microsoft.AspNetCore.Mvc;
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

            byte[] bytes = Encoding.ASCII.GetBytes("Hello World\n\n\n");
            port.Write(bytes, 0, bytes.Length);

            port.Close();

            return Ok("Printed successfully");
        }
    }
}