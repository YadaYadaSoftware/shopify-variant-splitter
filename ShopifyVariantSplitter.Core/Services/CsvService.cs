using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using ShopifyVariantSplitter.Core.Models;

namespace ShopifyVariantSplitter.Core.Services
{
    public class CsvService
    {
        public List<Product> ImportProducts(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Configure CSV reader to handle missing fields gracefully
            csv.Context.Configuration.MissingFieldFound = null;

            // Read header to configure mapping
            csv.Read();
            csv.ReadHeader();

            var products = new List<Product>();
            var productGroups = new Dictionary<string, Product>();

            while (csv.Read())
            {
                var handle = csv.GetField("Handle");

                if (string.IsNullOrEmpty(handle) || !productGroups.ContainsKey(handle))
                {
                    productGroups[handle] = new Product
                    {
                        Handle = handle,
                        Title = csv.GetField("Title") ?? "",
                        BodyHtml = csv.GetField("Body (HTML)") ?? "",
                        Vendor = csv.GetField("Vendor") ?? "",
                        ProductCategory = csv.GetField("Product Category") ?? "",
                        Type = csv.GetField("Type") ?? "",
                        Tags = csv.GetField("Tags") ?? "",
                        Published = csv.GetField("Published")?.ToLower() == "true",
                        Option1Name = csv.GetField("Option1 Name") ?? "",
                        Option1Value = csv.GetField("Option1 Value") ?? "",
                        Option2Name = csv.GetField("Option2 Name") ?? "",
                        Option2Value = csv.GetField("Option2 Value") ?? "",
                        Option3Name = csv.GetField("Option3 Name") ?? "",
                        Option3Value = csv.GetField("Option3 Value") ?? "",
                        Status = csv.GetField("Status") ?? "",
                        GiftCard = csv.GetField("Gift Card")?.ToLower() == "true",
                        SeoTitle = csv.GetField("SEO Title") ?? "",
                        SeoDescription = csv.GetField("SEO Description") ?? "",
                        GoogleShoppingCategory = csv.GetField("Google Shopping / Google Product Category") ?? "",
                        GoogleShoppingGender = csv.GetField("Google Shopping / Gender") ?? "",
                        GoogleShoppingAgeGroup = csv.GetField("Google Shopping / Age Group") ?? "",
                        GoogleShoppingMpn = csv.GetField("Google Shopping / MPN") ?? "",
                        GoogleShoppingCondition = csv.GetField("Google Shopping / Condition") ?? "",
                        GoogleShoppingCustomProduct = csv.GetField("Google Shopping / Custom Product") ?? "",
                        GoogleShoppingCustomLabel0 = csv.GetField("Google Shopping / Custom Label 0") ?? "",
                        GoogleShoppingCustomLabel1 = csv.GetField("Google Shopping / Custom Label 1") ?? "",
                        GoogleShoppingCustomLabel2 = csv.GetField("Google Shopping / Custom Label 2") ?? "",
                        GoogleShoppingCustomLabel3 = csv.GetField("Google Shopping / Custom Label 3") ?? "",
                        GoogleShoppingCustomLabel4 = csv.GetField("Google Shopping / Custom Label 4") ?? "",
                        GoogleCustomProduct = csv.GetField("Google: Custom Product (product.metafields.mm-google-shopping.custom_product)") ?? "",
                        ConstitutiveIngredients = csv.GetField("Constitutive ingredients (product.metafields.shopify.constitutive-ingredients)") ?? "",
                        HoldLevel = csv.GetField("Hold level (product.metafields.shopify.hold-level)") ?? "",
                        TargetGender = csv.GetField("Target gender (product.metafields.shopify.target-gender)") ?? ""
                    };
                }

                if (string.IsNullOrEmpty(handle))
                    continue;

                var product = productGroups[handle];

                // Add variant data
                if (!string.IsNullOrEmpty(csv.GetField("Variant SKU")))
                {
                    var variant = new Variant
                    {
                        Sku = csv.GetField("Variant SKU") ?? "",
                        Grams = decimal.TryParse(csv.GetField("Variant Grams") ?? "0", out var grams) ? grams : 0,
                        InventoryTracker = csv.GetField("Variant Inventory Tracker") ?? "",
                        InventoryQty = int.TryParse(csv.GetField("Variant Inventory Qty") ?? "0", out var qty) ? qty : 0,
                        InventoryPolicy = csv.GetField("Variant Inventory Policy") ?? "",
                        FulfillmentService = csv.GetField("Variant Fulfillment Service") ?? "",
                        Price = decimal.TryParse(csv.GetField("Variant Price") ?? "0", out var price) ? price : 0,
                        CompareAtPrice = decimal.TryParse(csv.GetField("Variant Compare At Price") ?? "0", out var comparePrice) ? comparePrice : 0,
                        RequiresShipping = csv.GetField("Variant Requires Shipping")?.ToLower() == "true",
                        Taxable = csv.GetField("Variant Taxable")?.ToLower() == "true",
                        Barcode = csv.GetField("Variant Barcode") ?? "",
                        ImageSrc = csv.GetField("Image Src") ?? "",
                        ImagePosition = int.TryParse(csv.GetField("Image Position") ?? "0", out var pos) ? pos : 0,
                        ImageAltText = csv.GetField("Image Alt Text") ?? "",
                        Weight = "",
                        WeightUnit = csv.GetField("Variant Weight Unit") ?? "",
                        TaxCode = csv.GetField("Variant Tax Code") ?? "",
                        Cost = decimal.TryParse(csv.GetField("Cost per item") ?? "0", out var cost) ? cost : 0,
                        IncludedUnitedStates = false, // Default to false since field may not exist in current CSV
                        PriceUnitedStates = 0, // Default to 0 since field may not exist in current CSV
                        CompareAtPriceUnitedStates = 0, // Default to 0 since field may not exist in current CSV
                        IncludedInternational = false, // Default to false since field may not exist in current CSV
                        PriceInternational = 0, // Default to 0 since field may not exist in current CSV
                        CompareAtPriceInternational = 0 // Default to 0 since field may not exist in current CSV
                    };

                    product.Variants.Add(variant);
                }

                // Add image data
                if (!string.IsNullOrEmpty(csv.GetField("Image Src")))
                {
                    var image = new Image
                    {
                        Src = csv.GetField("Image Src") ?? "",
                        Position = int.TryParse(csv.GetField("Image Position") ?? "0", out var pos) ? pos : 0,
                        AltText = csv.GetField("Image Alt Text") ?? ""
                    };

                    product.Images.Add(image);
                }
            }

            return productGroups.Values.ToList();
        }

        public void ExportProducts(List<Product> products, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Write header
            csv.WriteHeader<Product>();
            csv.NextRecord();

            // Write each product as a separate row
            foreach (var product in products)
            {
                foreach (var variant in product.Variants)
                {
                    csv.WriteRecord(new
                    {
                        product.Handle,
                        product.Title,
                        product.BodyHtml,
                        product.Vendor,
                        product.ProductCategory,
                        product.Type,
                        product.Tags,
                        product.Published,
                        product.Option1Name,
                        product.Option1Value,
                        Option1LinkedTo = "",
                        product.Option2Name,
                        product.Option2Value,
                        Option2LinkedTo = "",
                        product.Option3Name,
                        product.Option3Value,
                        Option3LinkedTo = "",
                        variant.Sku,
                        variant.Grams,
                        variant.InventoryTracker,
                        variant.InventoryQty,
                        variant.InventoryPolicy,
                        variant.FulfillmentService,
                        variant.Price,
                        variant.CompareAtPrice,
                        variant.RequiresShipping,
                        variant.Taxable,
                        variant.Barcode,
                        variant.ImageSrc,
                        variant.ImagePosition,
                        variant.ImageAltText,
                        GiftCard = product.GiftCard,
                        SEO_Title = product.SeoTitle ?? "",
                        SEO_Description = product.SeoDescription ?? "",
                        GoogleShoppingCategory = product.GoogleShoppingCategory ?? "",
                        GoogleShoppingGender = product.GoogleShoppingGender ?? "",
                        GoogleShoppingAgeGroup = product.GoogleShoppingAgeGroup ?? "",
                        GoogleShoppingMPN = product.GoogleShoppingMpn ?? "",
                        GoogleShoppingCondition = product.GoogleShoppingCondition ?? "",
                        GoogleShoppingCustomProduct = product.GoogleShoppingCustomProduct ?? "",
                        GoogleShoppingCustomLabel0 = product.GoogleShoppingCustomLabel0 ?? "",
                        GoogleShoppingCustomLabel1 = product.GoogleShoppingCustomLabel1 ?? "",
                        GoogleShoppingCustomLabel2 = product.GoogleShoppingCustomLabel2 ?? "",
                        GoogleShoppingCustomLabel3 = product.GoogleShoppingCustomLabel3 ?? "",
                        GoogleShoppingCustomLabel4 = product.GoogleShoppingCustomLabel4 ?? "",
                        GoogleCustomProduct = product.GoogleCustomProduct ?? "",
                        ConstitutiveIngredients = product.ConstitutiveIngredients ?? "",
                        HoldLevel = product.HoldLevel ?? "",
                        TargetGender = product.TargetGender ?? "",
                        VariantImage = variant.ImageSrc,
                        variant.WeightUnit,
                        variant.TaxCode,
                        variant.Cost,
                        IncludedUnitedStates = variant.IncludedUnitedStates,
                        PriceUnitedStates = variant.PriceUnitedStates,
                        CompareAtPriceUnitedStates = variant.CompareAtPriceUnitedStates,
                        IncludedInternational = variant.IncludedInternational,
                        PriceInternational = variant.PriceInternational,
                        CompareAtPriceInternational = variant.CompareAtPriceInternational,
                        product.Status
                    });
                    csv.NextRecord();
                }
            }
        }
    }
}