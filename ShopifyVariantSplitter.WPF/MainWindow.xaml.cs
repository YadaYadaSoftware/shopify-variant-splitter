using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using ShopifyVariantSplitter.Models;
using ShopifyVariantSplitter.Services;

namespace ShopifyVariantSplitter
{
    public partial class MainWindow : Window
    {
        private readonly CsvService _csvService;
        private List<Product> _products;
        private List<Product> _splitProducts;

        public MainWindow()
        {
            InitializeComponent();
            _csvService = new CsvService();
            _products = new List<Product>();
            _splitProducts = new List<Product>();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Select Shopify CSV file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    LogTextBox.Clear();
                    StatusText.Text = "Importing CSV file...";
                    Log("Importing from: " + openFileDialog.FileName);

                    _products = _csvService.ImportProducts(openFileDialog.FileName);

                    Log($"Imported {_products.Count} products");
                    Log($"Total variants: {_products.Sum(p => p.Variants.Count)}");

                    StatusText.Text = $"Imported {_products.Count} products with {_products.Sum(p => p.Variants.Count)} variants";
                    ExportButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    Log($"Error importing file: {ex.Message}");
                    StatusText.Text = "Error importing file";
                    MessageBox.Show($"Error importing file: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_products.Any())
            {
                MessageBox.Show("No products to export. Please import a CSV file first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Export Shopify CSV file",
                FileName = "shopify_products_split.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    LogTextBox.Clear();
                    StatusText.Text = "Splitting variants and exporting...";

                    // Split variants into separate products
                    _splitProducts = SplitVariantsIntoProducts();

                    Log($"Split into {_splitProducts.Count} individual products");
                    Log("Exporting to: " + saveFileDialog.FileName);

                    _csvService.ExportProducts(_splitProducts, saveFileDialog.FileName);

                    Log($"Successfully exported {_splitProducts.Count} products");
                    StatusText.Text = $"Exported {_splitProducts.Count} products to {Path.GetFileName(saveFileDialog.FileName)}";
                }
                catch (Exception ex)
                {
                    Log($"Error exporting file: {ex.Message}");
                    StatusText.Text = "Error exporting file";
                    MessageBox.Show($"Error exporting file: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private List<Product> SplitVariantsIntoProducts()
        {
            var splitProducts = new List<Product>();

            foreach (var product in _products)
            {
                // Create a separate product for each variant
                foreach (var variant in product.Variants)
                {
                    var splitProduct = new Product
                    {
                        Handle = $"{product.Handle}-{variant.Sku}",
                        Title = $"{product.Title} - {GetVariantTitle(variant, product)}",
                        BodyHtml = product.BodyHtml,
                        Vendor = product.Vendor,
                        ProductCategory = product.ProductCategory,
                        Type = product.Type,
                        Tags = product.Tags,
                        Published = product.Published,
                        Option1Name = "Title",
                        Option1Value = "Default Title",
                        Status = product.Status,
                        Variants = new List<Variant> { variant },
                        Images = product.Images.Where(img => img.Src == variant.ImageSrc).ToList()
                    };

                    splitProducts.Add(splitProduct);
                }
            }

            return splitProducts;
        }

        private string GetVariantTitle(Variant variant, Product product)
        {
            var titleParts = new List<string>();

            if (!string.IsNullOrEmpty(product.Option1Value) && product.Option1Value != variant.Sku)
                titleParts.Add(product.Option1Value);

            if (!string.IsNullOrEmpty(product.Option2Value) && product.Option2Value != variant.Sku)
                titleParts.Add(product.Option2Value);

            if (!string.IsNullOrEmpty(product.Option3Value) && product.Option3Value != variant.Sku)
                titleParts.Add(product.Option3Value);

            if (!string.IsNullOrEmpty(variant.Sku))
                titleParts.Add(variant.Sku);

            return string.Join(" - ", titleParts);
        }

        private void Log(string message)
        {
            LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            LogTextBox.ScrollToEnd();
        }
    }
}