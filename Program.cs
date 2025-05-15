// See https://aka.ms/new-console-template for more information
using NorthwindConsole.Model;
using Microsoft.EntityFrameworkCore;
﻿using NLog;
string path = Directory.GetCurrentDirectory() + "//nlog.config";


var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

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
            ShowProductMenu();
            break;
        case "2":
            logger.Info("User selected Category Menu (not yet implemented).");
            Console.WriteLine("Category Menu not implemented yet.");
            Console.ReadKey();
            break;
        case "0":
            logger.Info("User exited the application.");
            exit = true;
            break;
        default:
            logger.Warn("Invalid menu choice: {0}", choice);
            Console.WriteLine("Invalid choice. Try again.");
            Console.ReadKey();
            break;
    }
}
static void ShowProductMenu()
{
    var logger = LogManager.GetCurrentClassLogger();
    bool back = false;

    while (!back)
    {
        Console.Clear();
        Console.WriteLine("=== Product Menu ===");
        Console.WriteLine("1. Display All Products");
        Console.WriteLine("0. Back to Main Menu");
        Console.Write("Choose an option: ");
        string? input = Console.ReadLine();

        switch (input)
        {
            case "1":
                logger.Info("User selected to display all products.");
                DisplayProducts();
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
static void DisplayProducts()
{
    var logger = LogManager.GetCurrentClassLogger();

    using var db = new DataContext();

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
logger.Info("Program ended");
