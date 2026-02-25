using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zakupki
{
    internal struct Price
    {
        public decimal prices { get; set; }
        public enum Valut { RUB, EUR, USD }
    }
    internal class Pozition
    {
        public string? name { get; set; }
        public int? count { get; set; }
        public string? unit { get; set; }
        public Price? pricePozition { get; set; }
    }
    internal class Zakupka
    {
        public string? url {  get; set; }
        public int? id { get; set; }
        public string? discripton { get; set; }
        public DateTimeOffset? dateStart { get; set; }
        public DateTimeOffset? dateEnd { get; set; }
        public Price? purchasePrice { get; set; }
        public string? FioContact { get; set; }
        public string? contactNumber { get; set; }
        public string? companyName { get; set; }

        public Pozition[]? purchasePozition { get; set; }
    }
}
