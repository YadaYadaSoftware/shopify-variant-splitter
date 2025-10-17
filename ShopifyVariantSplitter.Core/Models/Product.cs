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

        // Missing fields from CSV template
        public bool GiftCard { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? GoogleShoppingCategory { get; set; }
        public string? GoogleShoppingGender { get; set; }
        public string? GoogleShoppingAgeGroup { get; set; }
        public string? GoogleShoppingMpn { get; set; }
        public string? GoogleShoppingCondition { get; set; }
        public string? GoogleShoppingCustomProduct { get; set; }
        public string? GoogleShoppingCustomLabel0 { get; set; }
        public string? GoogleShoppingCustomLabel1 { get; set; }
        public string? GoogleShoppingCustomLabel2 { get; set; }
        public string? GoogleShoppingCustomLabel3 { get; set; }
        public string? GoogleShoppingCustomLabel4 { get; set; }
        public string? GoogleCustomProduct { get; set; }
        public string? ConstitutiveIngredients { get; set; }
        public string? HoldLevel { get; set; }
        public string? TargetGender { get; set; }
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

        // Missing fields from CSV template
        public bool IncludedUnitedStates { get; set; }
        public decimal PriceUnitedStates { get; set; }
        public decimal CompareAtPriceUnitedStates { get; set; }
        public bool IncludedInternational { get; set; }
        public decimal PriceInternational { get; set; }
        public decimal CompareAtPriceInternational { get; set; }
    }

    public class Image
    {
        public string? Src { get; set; }
        public int Position { get; set; }
        public string? AltText { get; set; }
    }
}