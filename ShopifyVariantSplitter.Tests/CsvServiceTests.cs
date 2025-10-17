using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShopifyVariantSplitter.Core.Models;
using ShopifyVariantSplitter.Core.Services;

[TestClass]
public class CsvServiceTests
{
    private CsvService _csvService;
    private string _testCsvFilePath;

    [TestInitialize]
    public void TestInitialize()
    {
        _csvService = new CsvService();

        // Copy the CSV file to the test output directory for deployment
        var sourceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "products_export_1.csv");
        var testOutputDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _testCsvFilePath = Path.Combine(testOutputDirectory, "products_export_1.csv");

        // Copy the source CSV file to the test output directory if it exists
        if (File.Exists(sourceFilePath) && !File.Exists(_testCsvFilePath))
        {
            File.Copy(sourceFilePath, _testCsvFilePath);
        }
    }

    [TestMethod]
    [DeploymentItem("products_export_1.csv")]
    public void ImportProducts_ValidCsvFile_ReturnsProducts()
    {
        // Act
        var products = _csvService.ImportProducts("products_export_1.csv");

        // Assert
        Assert.IsNotNull(products);
        Assert.IsTrue(products.Any());

        // Check that we have the expected products from the CSV
        var mustacheWaxProduct = products.FirstOrDefault(p => p.Handle == "mustache-wax");
        Assert.IsNotNull(mustacheWaxProduct);
        Assert.AreEqual("Mustache Wax", mustacheWaxProduct.Title);
        Assert.AreEqual("State Trooper Stache Wax", mustacheWaxProduct.Vendor);
        Assert.IsTrue(mustacheWaxProduct.Published);

        // Check variants
        Assert.IsTrue(mustacheWaxProduct.Variants.Any());
        var firstVariant = mustacheWaxProduct.Variants.First();
        Assert.AreEqual("og1oz-light-black-usa", firstVariant.Sku);
        Assert.AreEqual(15.99m, firstVariant.Price);
        Assert.AreEqual("lb", firstVariant.WeightUnit);
    }

    [TestMethod]
    [DeploymentItem("products_export_1.csv")]
    public void ImportProducts_MultipleProductsWithVariants_ReturnsAllProducts()
    {
        // Act
        var products = _csvService.ImportProducts("products_export_1.csv");

        // Assert
        Assert.IsNotNull(products);

        // Count unique product handles
        var uniqueHandles = products.Select(p => p.Handle).Distinct().ToList();
        CollectionAssert.Contains(uniqueHandles, "mustache-wax");
        CollectionAssert.Contains(uniqueHandles, "kent-x-small-mens-beard-and-mustache-pocket-comb-for-grooming-and-styling-fine-toothed-hand-made-of-quality-cellulose-acetate-saw-cut-hand-polished-made-in-england");

        // Check that variants are properly grouped
        var kentCombProduct = products.FirstOrDefault(p => p.Handle == "kent-x-small-mens-beard-and-mustache-pocket-comb-for-grooming-and-styling-fine-toothed-hand-made-of-quality-cellulose-acetate-saw-cut-hand-polished-made-in-england");
        Assert.IsNotNull(kentCombProduct);
        Assert.AreEqual(1, kentCombProduct.Variants.Count); // Should have 1 variant
        Assert.AreEqual("kent-81t-tortoiseshell-usa", kentCombProduct.Variants.First().Sku);
    }

    [TestMethod]
    public void ImportProducts_FileNotFound_ThrowsException()
    {
        // Arrange
        var nonExistentFilePath = "nonexistent.csv";

        // Act & Assert
        Assert.ThrowsException<FileNotFoundException>(() => _csvService.ImportProducts(nonExistentFilePath));
    }

    [TestMethod]
    [DeploymentItem("products_export_1.csv")]
    public void ExportProducts_ValidProducts_CreatesCsvFile()
    {
        // Arrange
        var products = _csvService.ImportProducts("products_export_1.csv");
        var originalProduct = products.First(p => p.Handle == "mustache-wax");
        var exportFilePath = "test_export.csv";

        // Act
        _csvService.ExportProducts(new List<Product> { originalProduct }, exportFilePath);

        // Assert
        Assert.IsTrue(File.Exists(exportFilePath));

        var fileContent = File.ReadAllText(exportFilePath);
        Assert.IsTrue(fileContent.Contains("Handle"));
        Assert.IsTrue(fileContent.Contains("mustache-wax"));
        Assert.IsTrue(fileContent.Contains("Mustache Wax"));

        // Cleanup
        if (File.Exists(exportFilePath))
        {
            File.Delete(exportFilePath);
        }
    }

    [TestMethod]
    public void ExportProducts_EmptyProductList_CreatesValidCsvWithHeaders()
    {
        // Arrange
        var emptyProducts = new List<Product>();
        var exportFilePath = "test_export_empty.csv";

        // Act
        _csvService.ExportProducts(emptyProducts, exportFilePath);

        // Assert
        Assert.IsTrue(File.Exists(exportFilePath));

        var fileContent = File.ReadAllText(exportFilePath);
        Assert.IsTrue(fileContent.Contains("Handle"));
        // Should only contain headers, no data rows
        var lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(1, lines.Length); // Only header line

        // Cleanup
        if (File.Exists(exportFilePath))
        {
            File.Delete(exportFilePath);
        }
    }


    [TestMethod]
    [DeploymentItem("products_export_1.csv")]
    public void ImportProducts_ProductWithMultipleVariants_AllVariantsImported()
    {
        // Act
        var products = _csvService.ImportProducts("products_export_1.csv");
        var mustacheWax = products.FirstOrDefault(p => p.Handle == "mustache-wax");

        // Assert
        Assert.IsNotNull(mustacheWax);
        Assert.IsTrue(mustacheWax.Variants.Count >= 7, $"Expected at least 7 variants, but got {mustacheWax.Variants.Count}");

        // Check that we have variants with different SKUs and prices
        var skus = mustacheWax.Variants.Select(v => v.Sku).Distinct().ToList();
        CollectionAssert.Contains(skus, "og1oz-light-black-usa");
        CollectionAssert.Contains(skus, "stmw-med-neutral-backroom-1oz-usa");

        var prices = mustacheWax.Variants.Select(v => v.Price).Distinct().ToList();
        CollectionAssert.Contains(prices, 15.99m);
        CollectionAssert.Contains(prices, 17.99m);
    }

    [TestMethod]
    [DeploymentItem("products_export_1.csv")]
    public void ImportProducts_VariantWeightUnit_CorrectlyParsed()
    {
        // Act
        var products = _csvService.ImportProducts("products_export_1.csv");
        var mustacheWax = products.FirstOrDefault(p => p.Handle == "mustache-wax");

        // Assert
        Assert.IsNotNull(mustacheWax);
        var variant = mustacheWax.Variants.FirstOrDefault(v => v.Sku == "og1oz-light-black-usa");
        Assert.IsNotNull(variant);
        Assert.AreEqual("lb", variant.WeightUnit);
        Assert.AreEqual("", variant.Weight); // Should be empty as per CSV structure
    }
}