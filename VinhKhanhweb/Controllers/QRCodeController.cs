using Microsoft.AspNetCore.Mvc;
using QRCoder;

public class QRCodeController : Controller
{
    public IActionResult Index(int poiId)
    {
        var url = $"https://192.168.31.99:7170/poi/{poiId}";

        using (QRCodeGenerator qrGen = new QRCodeGenerator())
        using (QRCodeData qrData = qrGen.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrData))
        {
            var qrBytes = qrCode.GetGraphic(20);
            var base64 = Convert.ToBase64String(qrBytes);

            ViewBag.QRCode = $"data:image/png;base64,{base64}";
        }

        return View();
    }
}