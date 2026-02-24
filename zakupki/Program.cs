using HtmlAgilityPack;
using Microsoft.Playwright;
using System.Net;
using System.Text;
using System.Text.Json;

namespace zakupki
{
    internal class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var url = "https://apizakupki.nefteavtomatika.ru/api/purchases?page=1&order_by[number]=desc&per_page=30";

            await using var stream = await client.GetStreamAsync(url);

            var doc = await JsonDocument.ParseAsync(stream);

            Console.WriteLine(
                JsonSerializer.Serialize(doc, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                })
            );
        }
    } 
}
