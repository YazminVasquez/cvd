using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Covid19
{
    public class Program
    {
        public static string DataPredictionIA;
        public static void Main(string[] args)
        {

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        public static void LoadDataPredictionIA(string ruta)
        {
            MakePredictionRequest(ruta).Wait();
        }
        public static string GetDataPredictionIA()
        {
            return DataPredictionIA;
        }
        public static async Task MakePredictionRequest(string imageFilePath)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "9d5fdb8bb04942898b4e2a4d4b0cf1c9");

            // Prediction URL - replace this example URL with your valid Prediction URL.
            string url = @"https://hands.cognitiveservices.azure.com/customvision/v3.0/Prediction/d9cd2a4d-5f6d-4406-ab7f-752687e5671a/classify/iterations/Iteration17/image";

            HttpResponseMessage response;

            // Request body. Try this sample with a locally stored image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);

                DataPredictionIA = await response.Content.ReadAsStringAsync();
                Console.WriteLine(DataPredictionIA);
            }
        }
        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            try
            {
                FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return null;
            }

        }

    }
}
