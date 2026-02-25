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
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var url = "https://apizakupki.nefteavtomatika.ru/api/purchases?page=1&order_by[number]=desc&per_page=30";

            await using var stream = await client.GetStreamAsync(url);
            var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            var meta = root.GetProperty("meta");
            int lastPage = meta.GetProperty("last_page").GetInt32();
            int total = meta.GetProperty("total").GetInt32();

            Zakupka[] zakupki = new Zakupka[total];

            for (int i = 1; i <= lastPage; i++)
            {
                Console.WriteLine($"Просмотр страниц {i}");
                await ParsePage(i, zakupki, client);
            }
            Console.ReadKey();
        }
        private static async Task ParsePage (int page, Zakupka[] zakupka, HttpClient client)
        {
            string url = $"https://apizakupki.nefteavtomatika.ru/api/purchases?page={page}&order_by[number]=desc&per_page=30";

            await using var stream = await client.GetStreamAsync(url);
            var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;
            var data = root.GetProperty("data");

            ParseData(zakupka, data);
        }
        static int j = 0;
        private static void ParseData(Zakupka[] zakupka, JsonElement data)
        {
            

            foreach (var item in data.EnumerateArray())
            {
                zakupka[j] = new Zakupka();
                zakupka[j].id = item.GetProperty("id").GetInt32();
                zakupka[j].url = $"https://zakupki.nefteavtomatika.ru/purchases/{zakupka[j].id}/show";
                j++;
            }
        }

    } 
}
