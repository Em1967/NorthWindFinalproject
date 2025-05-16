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
    Console.WriteLine("2. Category Menu");
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
    logger.Info("User opened Category Menu.");
    ShowCategoryMenu(options);
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
        Console.WriteLine("4. Delete Product");
        Console.WriteLine("3. Edit Product");
        Console.WriteLine("2. Add New Product");
        Console.WriteLine("1. Display All Products");
        Console.WriteLine("0. Back to Main Menu");
        Console.Write("Choose an option: ");
        string? input = Console.ReadLine();

        switch (input)
        {
            case "4":
                logger.Info("User selected to delete a product.");
                DeleteProduct(options);
                break;
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
static void ShowCategoryMenu(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    bool back = false;

    while (!back)
    {
        Console.Clear();
        Console.WriteLine("=== Category Menu ===");
        Console.WriteLine("1. Display All Categories");
        Console.WriteLine("2. Add New Category");
        Console.WriteLine("3. Edit Category");
        Console.WriteLine("4. Delete Category");
        Console.WriteLine("5. Display all categories with active products");
        Console.WriteLine("6. Display a specific category with active products");
        Console.WriteLine("0. Back to Main Menu");
        Console.Write("Choose an option: ");
        string? input = Console.ReadLine();

        switch (input)
        {
            case "1":
                logger.Info("User selected to display all categories.");
                DisplayCategories(options);
                break;
            case "2":
                logger.Info("User selected to add a new category.");
                AddCategory(options);
                break;
            case "3":
                logger.Info("User selected to edit a category.");
                EditCategory(options);
                break;
            case "4":
                logger.Info("User selected to delete a category.");
                DeleteCategory(options);
                break;
                case "5":
                logger.Info("User selected to display all categories with active products.");
                DisplayAllCategoriesWithActiveProducts(options);
                break;
            case "0":
                logger.Info("Returning to main menu from Category Menu.");
                back = true;
                break;
            default:
                logger.Warn("Invalid category menu choice: {0}", input);
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
        Console.WriteLine($"{status} ID: {p.ProductId} - {p.ProductName}");
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

static void DeleteProduct(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();

    using var db = new DataContext(options);

    Console.Write("Enter Product ID to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Invalid ID.");
        logger.Warn("Invalid input for product ID during delete.");
        return;
    }

    var product = db.Products.Find(id);
    if (product == null)
    {
        Console.WriteLine("Product not found.");
        logger.Warn("Product ID {0} not found for deletion.", id);
        return;
    }

    // Check if there are related OrderDetails
    bool hasOrders = db.OrderDetails.Any(od => od.ProductId == id);
    if (hasOrders)
    {
        Console.WriteLine("Cannot delete product; it has related order records.");
        logger.Warn("Attempted to delete product ID {0} with related OrderDetails.", id);
        return;
    }

    db.Products.Remove(product);
    db.SaveChanges();

    Console.WriteLine("Product deleted successfully.");
    logger.Info("Deleted product ID {0} - {1}", id, product.ProductName);
}
static void DisplayCategories(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    using var db = new DataContext(options);

    Console.Clear();
    Console.WriteLine("=== Categories ===");

    var categories = db.Categories.ToList();

    foreach (var cat in categories)
    {
        Console.WriteLine($"ID: {cat.CategoryId} - {cat.CategoryName}");
        if (!string.IsNullOrWhiteSpace(cat.Description))
            Console.WriteLine($"   Description: {cat.Description}");
    }

    logger.Info("Displayed {0} categories.", categories.Count);
    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}
static void AddCategory(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    using var db = new DataContext(options);

    Console.Clear();
    Console.WriteLine("=== Add New Category ===");

    Console.Write("Enter Category Name: ");
    string? name = Console.ReadLine();

    Console.Write("Enter Description (optional): ");
    string? description = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Category name is required.");
        logger.Warn("Attempted to add category with no name.");
        return;
    }

    var category = new Category
    {
        CategoryName = name,
        Description = description
    };

    try
    {
        db.Categories.Add(category);
        db.SaveChanges();

        Console.WriteLine("Category added successfully.");
        logger.Info("Added category: {0}", name);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error adding category.");
        logger.Error(ex, "Failed to add category: {0}", name);
    }

    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}
static void EditCategory(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    using var db = new DataContext(options);

    Console.Clear();
    Console.WriteLine("=== Edit Category ===");

    Console.Write("Enter Category ID to edit: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Invalid ID.");
        logger.Warn("Invalid category ID input for edit.");
        return;
    }

    var category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
    if (category == null)
    {
        Console.WriteLine("Category not found.");
        logger.Warn("Category ID {0} not found for edit.", id);
        return;
    }

    Console.WriteLine($"Current Name: {category.CategoryName}");
    Console.Write("New Name (leave blank to keep): ");
    string? name = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(name)) category.CategoryName = name;

    Console.WriteLine($"Current Description: {category.Description}");
    Console.Write("New Description (leave blank to keep): ");
    string? description = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(description)) category.Description = description;

    try
    {
        db.SaveChanges();
        Console.WriteLine("Category updated successfully.");
        logger.Info("Category ID {0} updated.", id);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error updating category.");
        logger.Error(ex, "Failed to update category ID {0}", id);
    }

    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}
static void DeleteCategory(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    using var db = new DataContext(options);

    Console.Clear();
    Console.WriteLine("=== Delete Category ===");
    Console.Write("Enter Category ID to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Invalid ID.");
        logger.Warn("Invalid input for category ID during delete.");
        return;
    }

    var category = db.Categories.Find(id);
    if (category == null)
    {
        Console.WriteLine("Category not found.");
        logger.Warn("Category ID {0} not found for deletion.", id);
        return;
    }

    // Check if any products exist in this category
    bool hasProducts = db.Products.Any(p => p.CategoryId == id);
    if (hasProducts)
    {
        Console.WriteLine("Cannot delete category; it has related products.");
        logger.Warn("Attempted to delete category ID {0} with related products.", id);
        return;
    }

    db.Categories.Remove(category);
    db.SaveChanges();

    Console.WriteLine("Category deleted successfully.");
    logger.Info("Deleted category ID {0} - {1}", id, category.CategoryName);

    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}
static void DisplayAllCategoriesWithActiveProducts(DbContextOptions<DataContext> options)
{
    var logger = LogManager.GetCurrentClassLogger();
    using var db = new DataContext(options);

    Console.Clear();
    Console.WriteLine("=== Categories and Active Products ===\n");

    var categories = db.Categories
        .Include(c => c.Products)
        .ToList();

    foreach (var category in categories)
    {
        var activeProducts = category.Products
            .Where(p => p.Discontinued == false)
            .ToList();

        if (activeProducts.Any())
        {
            Console.WriteLine($"Category: {category.CategoryName}");
            foreach (var product in activeProducts)
            {
                Console.WriteLine($"  - {product.ProductName}");
            }
            Console.WriteLine();
        }
    }

    logger.Info("Displayed all categories with their active products.");
    Console.WriteLine("Press any key to return...");
    Console.ReadKey();
}