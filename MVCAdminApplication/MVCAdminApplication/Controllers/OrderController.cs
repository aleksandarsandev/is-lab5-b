using ClosedXML.Excel;
using GemBox.Document;
using Microsoft.AspNetCore.Mvc;
using MVCAdminApplication.Models;
using Newtonsoft.Json;
using System.Text;

namespace MVCAdminApplication.Controllers
{
    public class OrderController : Controller
    {
        public OrderController()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        }
        public IActionResult Index()
        {
            HttpClient client = new HttpClient();
            string URL = "http://localhost:5054/api/Admin/GetAllOrders";
            HttpResponseMessage response = client.GetAsync(URL).Result;

            var data = response.Content.ReadAsAsync<List<Order>>().Result;
            return View(data);
        }

        public IActionResult Details(Guid Id)
        {
            HttpClient client = new HttpClient();
            string URL = "http://localhost:5054/api/Admin/GetDetailsForOrder";
            var model = new
            {
                Id = Id
            };
            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var data = response.Content.ReadAsAsync<Order>().Result;
            return View(data);
        }
        public FileContentResult CreateInvoice(Guid Id)
        {
            HttpClient client = new HttpClient();
            string URL = "http://localhost:5054/api/Admin/GetDetailsForOrder";
            var model = new
            {
                Id = Id
            };
            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var data = response.Content.ReadAsAsync<Order>().Result;
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Invoice.docx");
            var document = DocumentModel.Load(templatePath);
            document.Content.Replace("{{OrderNumber}}", data.Id.ToString());
            document.Content.Replace("{{UserName}}", data.Owner.FirstName.ToString());
            StringBuilder sb = new StringBuilder();
            var totalPrice = 0.0;
            foreach (var item in data.ProductInOrders)
            {
                totalPrice += item.Quantity * item.OrderedProduct.Price;
                sb.AppendLine(item.OrderedProduct.MovieId + " with quantiti of: " + item.Quantity
                    + " and price of: " + item.OrderedProduct.Price + "$");
            }
            document.Content.Replace("{{ProductList}}", sb.ToString());
            document.Content.Replace("{{Total}}", totalPrice.ToString());
            var stream = new MemoryStream();
            document.Save(stream, new PdfSaveOptions());
            return File(stream.ToArray(), new PdfSaveOptions().ContentType, "ExportInvoice.pdf");
        }
        public IActionResult ExportAllOrders()
        {
            string fileName = "AllOrders.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            using (var workBook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workBook.Worksheets.Add("Orders");

                worksheet.Cell(1, 1).Value = "Order ID";
                worksheet.Cell(1, 2).Value = "Customer Username";

                HttpClient client = new HttpClient();
                string URL = "http://localhost:5054/api/Admin/GetAllOrders";
                HttpResponseMessage response = client.GetAsync(URL).Result;

                var data = response.Content.ReadAsAsync<List<Order>>().Result;

                for (int i = 0; i < data.Count(); i++)
                {
                    var order = data[i];
                    worksheet.Cell(i + 2, 1).Value = order.Id.ToString();
                    worksheet.Cell(i + 2, 2).Value = order.Owner.FirstName;

                    for (int j = 0; j < order.ProductInOrders.Count(); j++)
                    {
                        worksheet.Cell(1, j + 3).Value = "Product - " + (j + 1);
                        worksheet.Cell(i + 2, j + 3).Value = order.ProductInOrders.ElementAt(j).OrderedProduct.MovieId.ToString();

                    }
                }

                using (var stream = new MemoryStream())
                {
                    workBook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, contentType, fileName);
                }
            }
        }
    }
}
