using ExcerciseTwo.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

string dataFileName = "Data.csv";
string outputDirectoryName = "OutputFiles";

string projectDirectory = GetProjectDirectory();
string inputFilePath = Path.Combine(projectDirectory, "Files", dataFileName);
string outputDirectory = Path.Combine(projectDirectory, outputDirectoryName);

string defaultOrderedNames = "NamesOrderedByFrequency";
string userOrderedNamesOutputDirectory = getTargetOutputDirectory(outputDirectory, defaultOrderedNames);

string defaultAddressesSortedAlphabetically = "AddressesSortedAlphabetically";
string userAddressesOutputDirectory = getTargetOutputDirectory(outputDirectory, defaultAddressesSortedAlphabetically);


FileProcessor fileProcessor = new FileProcessor();
DataTable table = fileProcessor.CreateTableFromCsv(inputFilePath);

List<NameCount> firstNameFrequency = fileProcessor.GetNameFrequency(table, "FirstName");
List<NameCount> lastNameFrequency = fileProcessor.GetNameFrequency(table, "LastName");

List<NameCount> joinedLists = firstNameFrequency
    .Concat(lastNameFrequency)
    .OrderByDescending(x => x.Count)
    .ThenBy(x => x.Name)
    .ToList();

fileProcessor.ConvertListToFile(userOrderedNamesOutputDirectory, joinedLists);


List<UserInfo> users = fileProcessor.ExtractAllTableData(table);
List<Address> sortedAddresses = users
    .OrderBy(x => x.Address.Name)
    .Select(x => x.Address)
    .ToList();

fileProcessor.ConvertListToFile(userAddressesOutputDirectory, sortedAddresses);

Console.WriteLine("Successfully imported and processed files.");

//Following DRY principles.
string GetProjectDirectory()
{
    string currentDirectory = Directory.GetCurrentDirectory();
    string projectDirectory = Directory.GetParent(currentDirectory).Parent.Parent.FullName;
    return projectDirectory;
}

//Allow the user to choose the name for the output file
string getTargetOutputDirectory(string outputDirectory, string targetFile)
{
    Console.WriteLine($"Specify a name for {targetFile} or leave blank for default:");
    string userInput = Console.ReadLine();
    string outputFilePath = Path.Combine(outputDirectory, $"{userInput}.txt");

    if (string.IsNullOrEmpty(userInput))
    {
        outputFilePath = Path.Combine(outputDirectory, $"{targetFile}.txt");
        Console.WriteLine($"No input provided. Using default value: {outputFilePath}");
    }
    Console.WriteLine($"Using provided value: {outputFilePath}");
    return outputFilePath;
}
public class FileProcessor
{
    public DataTable CreateTableFromCsv(string filePath)
    {
        // Create a new DataTable
        DataTable table = new DataTable();

        // Read the CSV file
        using (StreamReader reader = new StreamReader(filePath))
        {
            // Read the header row
            string headerLine = reader.ReadLine();
            string[] headers = headerLine.Split(',');

            // Add columns to the DataTable based on the headers
            foreach (string header in headers)
            {
                table.Columns.Add(header.Trim());
            }

            // Read the remaining rows
            while (!reader.EndOfStream)
            {
                string dataLine = reader.ReadLine();
                string[] values = dataLine.Split(',');

                // Create a new DataRow
                DataRow row = table.NewRow();

                // Set the values in the DataRow
                for (int i = 0; i < values.Length; i++)
                {
                    row[i] = values[i].Trim();
                }

                // Add the DataRow to the DataTable
                table.Rows.Add(row);
            }
        }

        return table;
    }

    public List<NameCount> GetNameFrequency(DataTable table, string fieldName)
    {
        List<NameCount> nameFrequency = table.AsEnumerable()
            .GroupBy(row => row.Field<string>(fieldName))
            .Where(group => !string.IsNullOrEmpty(group.Key))
            .Select(group => new NameCount()
            {
                Name = group.Key,
                Count = group.Count()
            })
            .ToList();

        return nameFrequency;
    }

    public List<UserInfo> ExtractAllTableData(DataTable table)
    {
        List<UserInfo> userInfoList = new List<UserInfo>();
        foreach (DataRow row in table.Rows)
        {
            int addressNumber = 0;
            string addressName = "";

            string[] parts = row.Field<string>("Address")?.Split(' ', 2);
            if (parts?.Length == 2)
            {
                int.TryParse(parts[0], out addressNumber);
                addressName = parts[1];
            }

            string firstName = row["FirstName"]?.ToString();
            string lastName = row["LastName"]?.ToString();
            string address = row["Address"]?.ToString();
            long phoneNumber = row["PhoneNumber"] != DBNull.Value ? Convert.ToInt64(row["PhoneNumber"]) : 0;

            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                UserInfo userInfo = new UserInfo()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Address = new Address()
                    {
                        Number = addressNumber,
                        Name = addressName
                    },
                    PhoneNumber = phoneNumber
                };

                userInfoList.Add(userInfo);
            }
        }
        return userInfoList;
    }

    //Here we make use of generics to ensure that we don't have to create a redefined function per object we need to create a file for
    public void ConvertListToFile<T>(string outputFilePath, List<T> items)
    {
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            if (typeof(T) == typeof(string))
            {
                foreach (var item in items)
                {
                    writer.WriteLine(item);
                }
            }
            else
            {
                foreach (var obj in items)
                {
                    StringBuilder lineBuilder = new StringBuilder();

                    foreach (var property in obj.GetType().GetProperties())
                    {
                        string fieldName = property.Name;
                        dynamic fieldValue = property.GetValue(obj);
                        lineBuilder.Append($"{fieldValue}, ");
                    }

                    string line = lineBuilder.ToString().TrimEnd(',', ' ');
                    writer.WriteLine(line);
                }
            }
        }
    }
}