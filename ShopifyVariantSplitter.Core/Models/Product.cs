using System.Collections.Generic;

namespace ShopifyVariantSplitter.Core.Models
{
    public class Product
    {
        public string? Handle { get; set; }
        public string? Title { get; set; }
        public string? BodyHtml { get; set; }
        public string? Vendor { get; set; }
        public string? ProductCategory { get; set; }
        public string? Type { get; set; }
        public string? Tags { get; set; }
        public bool Published { get; set; }
        public string? Option1Name { get; set; }
        public string? Option1Value { get; set; }
        public string? Option2Name { get; set; }
        public string? Option2Value { get; set; }
        public string? Option3Name { get; set; }
        public string? Option3Value { get; set; }
        public List<Variant> Variants { get; set; } = new List<Variant>();
        public List<Image> Images { get; set; } = new List<Image>();
        public string? Status { get; set; }
    }

    public class Variant
    {
        public string? Sku { get; set; }
        public decimal Grams { get; set; }
        public string? InventoryTracker { get; set; }
        public int InventoryQty { get; set; }
        public string? InventoryPolicy { get; set; }
        public string? FulfillmentService { get; set; }
        public decimal Price { get; set; }
        public decimal CompareAtPrice { get; set; }
        public bool RequiresShipping { get; set; }
        public bool Taxable { get; set; }
        public string? Barcode { get; set; }
        public string? ImageSrc { get; set; }
        public int ImagePosition { get; set; }
        public string? ImageAltText { get; set; }
        public string? Weight { get; set; }
        public string? WeightUnit { get; set; }
        public string? TaxCode { get; set; }
        public decimal Cost { get; set; }
    }

    public class Image
    {
        public string? Src { get; set; }
        public int Position { get; set; }
        public string? AltText { get; set; }
    }
}