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
                        Status = csv.GetField("Status") ?? ""
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
                        Weight = csv.GetField("Variant Weight Unit") ?? "",
                        WeightUnit = csv.GetField("Variant Weight Unit") ?? "",
                        TaxCode = csv.GetField("Variant Tax Code") ?? "",
                        Cost = decimal.TryParse(csv.GetField("Cost per item") ?? "0", out var cost) ? cost : 0
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
                        GiftCard = false,
                        SEO_Title = "",
                        SEO_Description = "",
                        GoogleShoppingCategory = "",
                        GoogleShoppingGender = "",
                        GoogleShoppingAgeGroup = "",
                        GoogleShoppingMPN = "",
                        GoogleShoppingCondition = "",
                        GoogleShoppingCustomProduct = "",
                        GoogleShoppingCustomLabel0 = "",
                        GoogleShoppingCustomLabel1 = "",
                        GoogleShoppingCustomLabel2 = "",
                        GoogleShoppingCustomLabel3 = "",
                        GoogleShoppingCustomLabel4 = "",
                        GoogleCustomProduct = "",
                        ConstitutiveIngredients = "",
                        HoldLevel = "",
                        TargetGender = "",
                        VariantImage = variant.ImageSrc,
                        variant.WeightUnit,
                        variant.TaxCode,
                        variant.Cost,
                        product.Status
                    });
                    csv.NextRecord();
                }
            }
        }
    }
}