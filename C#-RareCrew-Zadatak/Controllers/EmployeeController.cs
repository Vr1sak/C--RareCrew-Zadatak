using C__RareCrew_Zadatak.Models;
using C__RareCrew_Zadatak.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace C__RareCrew_Zadatak.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly HttpClient _http;

        public EmployeeController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("API");
        }

        public async Task<IActionResult> Index()
        {

            var employees = await _http
            .GetFromJsonAsync<List<Employee>>("https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==");
            var employeesPrikaz = new List<EmployeeResponse>();

            if (employees == null)
            {
                employeesPrikaz = new List<EmployeeResponse>();
                return View(employeesPrikaz);
            }

            employeesPrikaz = employees
           .GroupBy(e => e.EmployeeName)
           .Select(g => new EmployeeResponse
           {
               EmployeeName = g.Key,
               UkupnoSati = (int)g.Sum(x =>
               (DateTime.Parse(x.EndTimeUtc) - DateTime.Parse(x.StarTimeUtc))
               .TotalHours
           )
           })
           .OrderByDescending(x => x.UkupnoSati)
           .ToList();


            return View(employeesPrikaz);
        }

        public async Task<IActionResult> Chart()
        {
            var employees = await _http
            .GetFromJsonAsync<List<Employee>>("https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==");
            if (employees == null)
                return NotFound("Nema zaposleih");

            var employeesPrikaz = employees
                .GroupBy(e => e.EmployeeName)
                .Select(g => new {
                    EmployeeName = g.Key,
                    UkupnoSati = g.Sum(x =>
                        (DateTime.Parse(x.EndTimeUtc)
                         - DateTime.Parse(x.StarTimeUtc))
                        .TotalHours)
                })
                .OrderByDescending(x => x.UkupnoSati)
                .ToList();

            double ukupnoSati = employeesPrikaz.Sum(x => x.UkupnoSati);

            var pie = new Chart
            {
                Width = 600,
                Height = 400,
                Palette = ChartColorPalette.Pastel
            };

            var area = new ChartArea("main");
            pie.ChartAreas.Add(area);

            pie.Legends.Add(new Legend("Legend") { Docking = Docking.Right });

            pie.Titles.Add("Ukupno sati po zaposlenom");

            var series = new Series
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = false,
                ChartArea = area.Name
            };

            pie.Series.Add(series);

            foreach (var e in employeesPrikaz)
            {
                if (string.IsNullOrEmpty(e.EmployeeName))
                    continue;

                double procenat = Math.Round(e.UkupnoSati / ukupnoSati * 100, 2);

                var pt = new DataPoint
                {
                    AxisLabel = e.EmployeeName,
                    YValues = new[] { e.UkupnoSati }
                };

                pt.LegendText = e.EmployeeName;

                pt.Label = $"{procenat:F2}%";
                series.Points.Add(pt);
            }

            using var ms = new MemoryStream();
            pie.SaveImage(ms, ChartImageFormat.Png);
            return File(ms.ToArray(), "image/png");
        }

    }
  

 }
