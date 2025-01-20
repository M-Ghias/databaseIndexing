using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


public class Program
{
    public static string dataFilePath = "data.txt";
    public static string indexFilePath = "non_clustered_index.txt";
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Initializing data...");
        await InitializeDataAsync();

        Console.WriteLine("Choose an option:");
        Console.WriteLine("1. Add Record");
        Console.WriteLine("2. Search by Name");
        Console.WriteLine("3. Remove Record");
        Console.WriteLine("4. Exit");

        while (true)
        {
            Console.Write("Enter your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter ID: ");
                    int id = int.Parse(Console.ReadLine() ?? "0");
                    Console.Write("Enter Name: ");
                    string name = Console.ReadLine()!;
                    Console.Write("Enter Age: ");
                    int age = int.Parse(Console.ReadLine() ?? "0");
                    await AddRecordAsync(id, name, age);
                    break;

                case "2":
                    Console.Write("Enter Name to Search: ");
                    string searchName = Console.ReadLine()!;
                    var result = await SearchByNameAsync(searchName);
                    Console.WriteLine(result);
                    break;

                case "3":
                    Console.Write("Enter Name to Remove: ");
                    string removeName = Console.ReadLine()!;
                    await RemoveRecordAsync(removeName);
                    break;

                case "4":
                    Console.WriteLine("Exiting...");
                    return;

                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
    }

    public static async Task InitializeDataAsync()
    {
        if (!File.Exists(dataFilePath))
        {
            await File.WriteAllLinesAsync(dataFilePath, new[]
            {
                "1,Ghias,30",
                "2,Riaz,25",
                "3,Ahmed,40"
            });
        }

        await UpdateIndexesAsync();
    }

    public static async Task AddRecordAsync(int id, string name, int age)
    {
        if (string.IsNullOrWhiteSpace(name) || age <= 0 || id <= 0)
        {
            Console.WriteLine("Invalid input. Record not added.");
            return;
        }

        var record = $"{id},{name},{age}";

        await File.AppendAllTextAsync(dataFilePath, record + Environment.NewLine);
        await UpdateIndexesAsync();
        Console.WriteLine("Record added successfully.");
    }

    public static async Task<string> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Invalid input. Name cannot be empty.";
        }

        if (!File.Exists(indexFilePath) || !File.Exists(dataFilePath))
        {
            return "Index or data file not found.";
        }

        var indexEntries = await File.ReadAllLinesAsync(indexFilePath);
        var matchingClusteredIds = indexEntries
            .Where(entry => entry.StartsWith(name + ","))
            .Select(entry => entry.Split(',')[1])
            .ToList();

        if (!matchingClusteredIds.Any())
        {
            return "Record not found.";
        }

        var dataEntries = await File.ReadAllLinesAsync(dataFilePath);
        var results = matchingClusteredIds
            .Select(id => dataEntries.FirstOrDefault(line => line.StartsWith(id + ",")))
            .Where(record => record != null)
            .ToList();

        return results.Any() ? string.Join(Environment.NewLine, results) : "Record not found in data file.";
    }

    public static async Task RemoveRecordAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Invalid input. Name cannot be empty.");
            return;
        }

        if (!File.Exists(dataFilePath) || !File.Exists(indexFilePath))
        {
            Console.WriteLine("Data or index file not found.");
            return;
        }

        var dataEntries = (await File.ReadAllLinesAsync(dataFilePath)).ToList();
        var originalCount = dataEntries.Count;

        dataEntries = dataEntries.Where(line => !line.Contains($",{name},")).ToList();

        if (dataEntries.Count == originalCount)
        {
            Console.WriteLine("Record not found. No changes made.");
            return;
        }

        await File.WriteAllLinesAsync(dataFilePath, dataEntries);
        await UpdateIndexesAsync();
        Console.WriteLine("Record removed successfully.");
    }

    public static async Task UpdateIndexesAsync()
    {
        if (!File.Exists(dataFilePath))
        {
            Console.WriteLine("Data file not found. Cannot update indexes.");
            return;
        }

        var dataEntries = await File.ReadAllLinesAsync(dataFilePath);
        var indexEntries = dataEntries
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 2)
            .Select(parts => $"{parts[1]},{parts[0]}") // Name, Clustered ID
            .ToList();

        await File.WriteAllLinesAsync(indexFilePath, indexEntries);
    }
}
