using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Covid19.Models;
using RestSharp;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace Covid19.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        Item obj;
        Item1 objrep;
        Vision VAPI;
        private static string Exito = "";
        private readonly DatabaseContext _context;
        private readonly IHostingEnvironment _environment;
        String RutaIA;
        public HomeController(ILogger<HomeController> logger,IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _environment = hostingEnvironment;
         //   _context = context;
        }
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        private void CargarData()
        {
            var data = ApiCovid19();
            obj = new Item();
            objrep = new Item1();

            obj.total_cases = data.Substring(16, 9);
            obj.total_deaths = data.Substring(43, 6);
            obj.total_recovered = data.Substring(70, 7);
            obj.new_cases = data.Substring(92, 6);
            obj.new_deaths = data.Substring(114, 5);




        }
        private void CargarData1()
        {
            var data = ApiCovid19REP();

            objrep = new Item1();


            objrep.total_cases1 = data.Substring(124, 5);
            objrep.total_deaths1 = data.Substring(187, 2);
            objrep.total_recovered1 = data.Substring(227, 2);

        }
        private void CargarData2(string ruta)
        {      
           Program.LoadDataPredictionIA(ruta);
            var data = Program.GetDataPredictionIA();
            VAPI = new Vision();


            VAPI.Agua = data.Substring(214, 2);
            VAPI.Dedos = data.Substring(214, 2);
            VAPI.Jabon = data.Substring(214, 2);
            VAPI.Palma = data.Substring(214, 2);
            VAPI.Secado = data.Substring(214, 2);
            VAPI.Vuelta = data.Substring(214, 2);

        }
        public string ApiCovid19()
        {

            var client = new RestClient("https://coronavirus-monitor.p.rapidapi.com/coronavirus/worldstat.php");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "coronavirus-monitor.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "69b3b33a9emshe1d67371ba80fcdp1ba705jsnc0a0224d0454");

            IRestResponse response = client.Execute(request);


            return response.Content;

        }
        public string ApiCovid19REP()
        {

            var client1 = new RestClient("https://coronavirus-monitor.p.rapidapi.com/coronavirus/latest_stat_by_country.php?country=Dominican%20Republic");
            var request1 = new RestRequest(Method.GET);
            request1.AddHeader("x-rapidapi-host", "coronavirus-monitor.p.rapidapi.com");
            request1.AddHeader("x-rapidapi-key", "69b3b33a9emshe1d67371ba80fcdp1ba705jsnc0a0224d0454");

            IRestResponse response = client1.Execute(request1);


            return response.Content;

        }
        public IActionResult Index()
        {          
            IACovid();
            ViewBag.Prediccion = RutaIA;
            return View();
        }

        private void IACovid()
        {
            CargarData();
            ViewBag.TotalCasos = obj.total_cases;
            ViewBag.TotalRecuperados = obj.total_recovered;
            ViewBag.TotalMuertos = obj.total_deaths;
            ViewBag.NuevosMuertos = obj.new_deaths;
            ViewBag.NuevosCasos = obj.new_cases;
            CargarData1();

            ViewBag.TotalCasos1 = objrep.total_cases1;
            ViewBag.TotalRecuperados1 = objrep.total_recovered1;
            ViewBag.TotalMuertos1 = objrep.total_deaths1;
        }

        public IActionResult Camara()
        {
            

            return View();
        }

        [HttpPost]
        public IActionResult Capture(string name)
        {
            var files = HttpContext.Request.Form.Files;
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Getting Filename  
                        var fileName = file.FileName;
                        // Unique filename "Guid"  
                        var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                        // Getting Extension  
                        var fileExtension = Path.GetExtension(fileName);
                        // Concating filename + fileExtension (unique filename)  
                        var newFileName = string.Concat(myUniqueFileName, fileExtension);
                        //  Generating Path to store photo   
                        var filepath = Path.Combine(_environment.WebRootPath, "CameraPhotos") + $@"\{newFileName}";

                        if (!string.IsNullOrEmpty(filepath))
                        {
                            // Storing Image in Folder  
                            StoreInFolder(file, filepath);
                            RutaIA = IAVision(filepath)
                            +"..............Has Realizado con Exito las sigueintes practicas: "
                            +" * "+Exito

                            ;
                          
                        }

                        //var imageBytes = System.IO.File.ReadAllBytes(filepath);
                        //if (imageBytes != null)
                        //{
                        //    // Storing Image in Folder  
                        //    StoreInDatabase(imageBytes);
                        //}

                    }
                }
                return Json(RutaIA);
            }
            else
            {
                return Json(false);
            }
        }

        
        private void StoreInFolder(IFormFile file, string fileName)
        {
            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
        }
        private void StoreInDatabase(byte[] imageBytes)
        {
            try
            {
                if (imageBytes != null)
                {
                    string base64String = Convert.ToBase64String(imageBytes, 0, imageBytes.Length);
                    string imageUrl = string.Concat("data:image/jpg;base64,", base64String);
                    ImageStore imageStore = new ImageStore()
                    {
                        CreateDate = DateTime.Now,
                        ImageBase64String = imageUrl,
                        ImageId = 0
                    };
                    _context.ImageStore.Add(imageStore);
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string IAVision(String ruta)
        {
            CargarData2(ruta);
            ViewBag.data = Program.GetDataPredictionIA();
            var datos = Program.GetDataPredictionIA().Substring(280, 10);

            if (datos.Contains("Secado"))
            {
                Exito = Exito + " * Secado";
                return  "Secado de Manos: " + VAPI.Secado+"% de Prediccion";
            }
            else if (datos.Contains("Agua"))
            {
                Exito = Exito + " * Agua";
                return "Aplicar Agua: " + VAPI.Agua + "% de Prediccion";
            }
            else if (datos.Contains("Dedos"))
            {
                Exito = Exito + " * Dedos";
                return "Lavado entre los Dedos: " + VAPI.Dedos + "% de Prediccion";
            }
            else if (datos.Contains("Jabon"))
            {
                Exito = Exito + " * Jabon";
                return "Aplicar Jabon: " + VAPI.Jabon + "% de Prediccion";
            }
            else if (datos.Contains("Palma"))
            {
                Exito = Exito + " * Palma";
                return "Palma con Palma: " + VAPI.Palma + "% de Prediccion";
            }
            else if (datos.Contains("Vuelta"))
            {
                Exito = Exito + " * Vuelta";
                return "Vuelta de Manos: " + VAPI.Vuelta + "% de Prediccion";
            }
            else
            {
                return "prediccion no estimada";
            }
         
            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    public class Item1
    {
        public string id { get; set; }
        public string country_name1 { get; set; }
        public string total_cases1 { get; set; }
        public string new_cases1 { get; set; }
        public string active_cases1 { get; set; }
        public string total_deaths1 { get; set; }
        public string new_deaths1 { get; set; }
        public string total_recovered1 { get; set; }
        public string serious_critical1 { get; set; }
        public string region1 { get; set; }
        public string total_cases_per1m1 { get; set; }
        public string record_date1 { get; set; }
    }
    public class Vision
    {
        public string Agua { get; set; }
        public string Dedos { get; set; }
        public string Jabon { get; set; }
        public string Palma { get; set; }
        public string Secado { get; set; }
        public string Vuelta { get; set; }
    }
    public class Item
    {
        public string id { get; set; }
        public string country_name { get; set; }
        public string total_cases { get; set; }
        public string new_cases { get; set; }
        public string active_cases { get; set; }
        public string total_deaths { get; set; }
        public string new_deaths { get; set; }
        public string total_recovered { get; set; }
        public string serious_critical { get; set; }
        public string region { get; set; }
        public string total_cases_per1m { get; set; }
        public string record_date { get; set; }
    }

   
  
  
}
