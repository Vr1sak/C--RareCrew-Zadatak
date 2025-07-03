using C__RareCrew_Zadatak.Models;
using C__RareCrew_Zadatak.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
                (DateTime.Parse(x.EndTimeUtc)- DateTime.Parse(x.StarTimeUtc))
                .TotalHours
            )
            })
            .OrderByDescending(x => x.UkupnoSati)
            .ToList();


            return View(employeesPrikaz);
        }
    }
}
