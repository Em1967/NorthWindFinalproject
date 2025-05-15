// See https://aka.ms/new-console-template for more information
using NorthwindConsole.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
﻿using NLog;
string path = Directory.GetCurrentDirectory() + "//nlog.config";


var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
IConfiguration config = builder.Build();

string connectionString = config.GetConnectionString("DefaultConnection");

var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
optionsBuilder.UseSqlServer(connectionString);
var options = optionsBuilder.Options;
using var context = new DataContext(options);

logger.Info("Program started");

bool exit = false;

while (!exit)
{
    Console.Clear();
    Console.WriteLine("=== Northwind Console App ===");
    Console.WriteLine("1. Product Menu");
    Console.WriteLine("2. Category Menu (coming soon)");
    Console.WriteLine("0. Exit");
    Console.Write("Select an option: ");
    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            logger.Info("User opened Product Menu.");
            ShowProductMenu(options);
            break;
        case "2":
            logger.Info("User selected Category Menu (not yet implemented).");
            Console.WriteLine("Category Menu not implemented yet.");
            Console.ReadKey();
            break;
        case "0":
            logger.Info("User exited the application.");
            logger.Info("Program ended");
            exit = true;
            break;
        default:
            logger.Warn("Invalid menu choice: {0}", choice);
            Console.WriteLine("Invalid choice. Try again.");
            Console.ReadKey();
            break;
    }
}
static void ShowProductMenu(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    bool back = false;

    while (!back)
    {
        Console.Clear();
        Console.WriteLine("=== Product Menu ===");
        Console.WriteLine("3. Edit Product");
        Console.WriteLine("2. Add New Product");
        Console.WriteLine("1. Display All Products");
        Console.WriteLine("0. Back to Main Menu");
        Console.Write("Choose an option: ");
        string? input = Console.ReadLine();

        switch (input)
        {
            case "3":
                logger.Info("User selected to edit a product.");
                EditProduct(options);
                break;
            case "2":
                logger.Info("User selected to add a new product.");
                AddNewProduct(options);
                break;
            case "1":
                logger.Info("User selected to display all products.");
                DisplayProducts(options);
                break;
            case "0":
                logger.Info("Returning to main menu.");
                back = true;
                break;
            default:
                logger.Warn("Invalid product menu choice: {0}", input);
                Console.WriteLine("Invalid choice. Try again.");
                Console.ReadKey();
                break;
        }
    }
}
static void DisplayProducts(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();

    using var db = new DataContext(options);

    Console.Clear();
    Console.WriteLine("Display products:");
    Console.WriteLine("1. All");
    Console.WriteLine("2. Active (Not Discontinued)");
    Console.WriteLine("3. Discontinued");
    Console.Write("Choose a filter: ");
    string? filter = Console.ReadLine();

    List<Product> products = filter switch
    {
        "2" => db.Products.Where(p => p.Discontinued == false).ToList(),
        "3" => db.Products.Where(p => p.Discontinued == true).ToList(),
        _ => db.Products.ToList(),
    };

    Console.Clear();
    Console.WriteLine("Products:");

    foreach (var p in products)
    {
        var status = p.Discontinued ? "[Discontinued]" : "[Active]";
        Console.WriteLine($"{status} {p.ProductName}");
    }

    logger.Info("Displayed {0} products (Filter: {1})", products.Count, filter);
    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}

static void AddNewProduct(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    using var db = new DataContext(options);

    try
    {
        Console.Clear();
        Console.WriteLine("=== Add New Product ===");

        Console.Write("Enter Product Name: ");
        string? name = Console.ReadLine();

        Console.Write("Enter Supplier ID (number): ");
        int supplierId = int.Parse(Console.ReadLine() ?? "0");

        Console.Write("Enter Category ID (number): ");
        int categoryId = int.Parse(Console.ReadLine() ?? "0");

        Console.Write("Enter Quantity Per Unit: ");
        string? quantityPerUnit = Console.ReadLine();

        Console.Write("Enter Unit Price: ");
        decimal price = decimal.Parse(Console.ReadLine() ?? "0");

        Console.Write("Enter Units In Stock: ");
        short stock = short.Parse(Console.ReadLine() ?? "0");

        Console.Write("Is Discontinued? (y/n): ");
        bool discontinued = Console.ReadLine()?.ToLower() == "y";

        Product newProduct = new()
        {
            ProductName = name,
            SupplierId = supplierId,
            CategoryId = categoryId,
            QuantityPerUnit = quantityPerUnit,
            UnitPrice = price,
            UnitsInStock = stock,
            Discontinued = discontinued
        };

        db.Products.Add(newProduct);
        db.SaveChanges();

        logger.Info("Product added: {0}", name);
        Console.WriteLine("Product added successfully!");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error adding product");
        Console.WriteLine("Failed to add product. Check logs for details.");
    }

    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();

}
static void EditProduct(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    using var db = new DataContext(options);

    try
    {
        Console.Clear();
        Console.WriteLine("=== Edit Product ===");

        Console.Write("Enter Product ID to edit: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Invalid ID. Returning...");
            return;
        }

        var product = db.Products.FirstOrDefault(p => p.ProductId == productId);

        if (product == null)
        {
            Console.WriteLine("Product not found.");
            return;
        }

        Console.WriteLine($"Current Name: {product.ProductName}");
        Console.Write("New Name (leave blank to keep): ");
        string? name = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(name)) product.ProductName = name;

        Console.WriteLine($"Current Unit Price: {(product.UnitPrice.HasValue ? product.UnitPrice.Value.ToString("C") : "None")}");
Console.Write("New Price (leave blank to keep current): ");
string? priceInput = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(priceInput))
{
    if (decimal.TryParse(priceInput, out decimal newPrice))
    {
        product.UnitPrice = newPrice;
    }
    else
    {
        Console.WriteLine("Invalid input. Price not changed.");
    }
}

        Console.WriteLine($"Current Stock: {product.UnitsInStock}");
        Console.Write("New Stock (leave blank to keep): ");
        string? stockInput = Console.ReadLine();
        if (short.TryParse(stockInput, out short newStock)) product.UnitsInStock = newStock;

        Console.WriteLine($"Currently Discontinued: {product.Discontinued}");
        Console.Write("Is Discontinued? (y/n, blank to keep): ");
        string? discInput = Console.ReadLine();
        if (discInput?.ToLower() == "y") product.Discontinued = true;
        else if (discInput?.ToLower() == "n") product.Discontinued = false;

        db.SaveChanges();
        logger.Info("Product {0} updated.", productId);
        Console.WriteLine("Product updated successfully.");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error editing product.");
        Console.WriteLine("Failed to edit product.");
    }

    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}