using HtmlAgilityPack;
using Microsoft.Playwright;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

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

            int currentIndex = 0;

            for (int i = 1; i <= lastPage; i++)
            {
                Console.WriteLine($"Просмотр страниц {i}");
                currentIndex = await ParsePage(i, zakupki, client, currentIndex);
            }

            for (int i = 0; i< total; i++)
            {
                //Thread.Sleep(300);
                Console.WriteLine($"Просмотр записи {i+1}");
                await ParseZakupka(zakupki[i], client);
                
            }

            SaveToXml(zakupki);
            Console.ReadKey();
        }
        private static async Task<int> ParsePage(int page, Zakupka[] zakupka, HttpClient client, int startIndex)
        {
            string url = $"https://apizakupki.nefteavtomatika.ru/api/purchases?page={page}&order_by[number]=desc&per_page=30";

            await using var stream = await client.GetStreamAsync(url);
            var doc = await JsonDocument.ParseAsync(stream);

            var data = doc.RootElement.GetProperty("data");

            return ParseData(zakupka, data, startIndex);
        }
        static int j = 0;
        private static int ParseData(Zakupka[] zakupka,JsonElement data,int startIndex)
        {
            int j = startIndex;

            foreach (var item in data.EnumerateArray())
            {
                zakupka[j] = new Zakupka
                {
                    id = item.GetProperty("id").GetInt32(),
                    url = $"https://zakupki.nefteavtomatika.ru/purchases/{item.GetProperty("id").GetInt32()}/show"
                };

                j++;
            }

            return j;
        }
        private static async Task ParseZakupka (Zakupka zakupka, HttpClient client)
        {
            string url = $"https://apizakupki.nefteavtomatika.ru/api/purchases/{zakupka.id}";

            await using var stream = await client.GetStreamAsync(url);
            var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;
            var data = root.GetProperty("purchase");

            zakupka.descripton = data.GetProperty("description").GetString();
            zakupka.dateStart = DateTime.Parse(data.GetProperty("status_start_at").GetString());
            zakupka.dateEnd = DateTime.Parse(data.GetProperty("status_end_at").GetString());

            var company = data.GetProperty("company");
            var name = company.GetProperty("name");

            zakupka.companyName = name.GetProperty("full").GetString();

            var author = data.GetProperty("author");

            zakupka.FioContact = author.GetProperty("last_name").GetString() + " " + author.GetProperty("first_name").GetString() + " " + author.GetProperty("patronymic").GetString();
            zakupka.contactNumber = author.GetProperty("phone").ToString();

            var materials = data.GetProperty("materials");
            int countPoz = materials.GetArrayLength();
            if (countPoz > 0)
            {
                int i = 0;
                zakupka.purchasePozition = new Pozition[countPoz];
                foreach (var material in materials.EnumerateArray())
                {
                    zakupka.purchasePozition[i] = new Pozition();
                    zakupka.purchasePozition[i].count = material.GetProperty("amount").GetInt32();
                    var mater = material.GetProperty("material");
                    zakupka.purchasePozition[i].name = mater.GetProperty("name_full").GetString();
                    zakupka.purchasePozition[i].unit = mater.GetProperty("unit_rest").GetString();
                    i++;
                }
            }

        }
        private static void SaveToXml(Zakupka[] zakupki)
        {
            var serializer = new XmlSerializer(typeof(Zakupka[]));

            using var fs = new FileStream("zakupki.xml", FileMode.Create);
            serializer.Serialize(fs, zakupki);

            Console.WriteLine("XML файл сохранён.");
        }
    } 
}
