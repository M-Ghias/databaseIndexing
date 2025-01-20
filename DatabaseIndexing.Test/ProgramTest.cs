using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class ProgramTests
{
    private const string TestDataFilePath = "test_data.txt";
    private const string TestIndexFilePath = "test_index.txt";

    public ProgramTests()
    {
        // Ensure test files are clean before each test
        if (File.Exists(TestDataFilePath))
            File.Delete(TestDataFilePath);

        if (File.Exists(TestIndexFilePath))
            File.Delete(TestIndexFilePath);

        // Override file paths in Program class
        Program.dataFilePath = TestDataFilePath;
        Program.indexFilePath = TestIndexFilePath;
    }

    [Fact]
    public async Task InitializeDataAsync_CreatesFilesWithDefaultData()
    {
        await Program.InitializeDataAsync();

        Assert.True(File.Exists(TestDataFilePath));
        Assert.True(File.Exists(TestIndexFilePath));

        var dataLines = await File.ReadAllLinesAsync(TestDataFilePath);
        Assert.Contains("1,Ghias,30", dataLines);
        Assert.Contains("2,Riaz,25", dataLines);
        Assert.Contains("3,Ahmed,40", dataLines);

        var indexLines = await File.ReadAllLinesAsync(TestIndexFilePath);
        Assert.Contains("Ghias,1", indexLines);
        Assert.Contains("Riaz,2", indexLines);
        Assert.Contains("Ahmed,3", indexLines);
    }

    [Fact]
    public async Task AddRecordAsync_AddsRecordAndUpdatesIndex()
    {
        await Program.InitializeDataAsync();
        await Program.AddRecordAsync(4, "Ali", 35);

        var dataLines = await File.ReadAllLinesAsync(TestDataFilePath);
        Assert.Contains("4,Ali,35", dataLines);

        var indexLines = await File.ReadAllLinesAsync(TestIndexFilePath);
        Assert.Contains("Ali,4", indexLines);
    }

    [Fact]
    public async Task SearchByNameAsync_ReturnsMatchingRecords()
    {
        await Program.InitializeDataAsync();
        var result = await Program.SearchByNameAsync("Ghias");

        Assert.Contains("1,Ghias", result);
    }

    [Fact]
    public async Task SearchByNameAsync_ReturnsNotFoundForUnknownName()
    {
        await Program.InitializeDataAsync();
        var result = await Program.SearchByNameAsync("Unknown");

        Assert.Equal("Record not found.", result);
    }

    [Fact]
    public async Task RemoveRecordAsync_RemovesRecordAndUpdatesIndex()
    {
        await Program.InitializeDataAsync();
        await Program.RemoveRecordAsync("Riaz");

        var dataLines = await File.ReadAllLinesAsync(TestDataFilePath);
        Assert.DoesNotContain("2,Riaz,25", dataLines);

        var indexLines = await File.ReadAllLinesAsync(TestIndexFilePath);
        Assert.DoesNotContain("Riaz,2", indexLines);
    }

    [Fact]
    public async Task RemoveRecordAsync_DoesNotRemoveIfNameNotFound()
    {
        await Program.InitializeDataAsync();
        var originalDataLines = await File.ReadAllLinesAsync(TestDataFilePath);

        await Program.RemoveRecordAsync("Unknown");

        var dataLines = await File.ReadAllLinesAsync(TestDataFilePath);
        Assert.Equal(originalDataLines, dataLines);
    }

    [Fact]
    public async Task UpdateIndexesAsync_CreatesCorrectIndexes()
    {
        await Program.InitializeDataAsync();
        await Program.AddRecordAsync(5, "Charlie", 28);

        await Program.UpdateIndexesAsync();

        var indexLines = await File.ReadAllLinesAsync(TestIndexFilePath);
        Assert.Contains("Charlie,5", indexLines);
    }
}
