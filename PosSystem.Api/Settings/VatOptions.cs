using System.ComponentModel.DataAnnotations;

namespace PosSystem.Api.Settings
{
    public class VatOptions
    {
        public const string SectionName = "Vat";

        [Range(typeof(decimal), "0.01", "0.99")]
        public decimal Rate { get; set; }
    }
}
