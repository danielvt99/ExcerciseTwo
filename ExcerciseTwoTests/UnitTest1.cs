using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcerciseTwo.Models;

namespace YourNamespace.Tests
{
    [TestFixture]
    public class FileProcessorTests
    {
        private FileProcessor fileProcessor;
        private string basePath;

        [SetUp]
        public void SetUp()
        {
            fileProcessor = new FileProcessor();
            basePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");
            Directory.CreateDirectory(basePath);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(basePath, true);
        }

        [Test]
        public void CreateTableFromCsv_ValidFilePath_ReturnsDataTable()
        {
            // Arrange
            string filePath = Path.Combine(basePath, "Data.csv");
            CreateCsvFile(filePath, "FirstName,LastName,Address,PhoneNumber\nJohn,Doe,123 Main St,1234567890\nJane,Smith,456 Elm St,0987654321");

            // Act
            DataTable table = fileProcessor.CreateTableFromCsv(filePath);

            // Assert
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual("John", table.Rows[0]["FirstName"]);
            Assert.AreEqual("Doe", table.Rows[0]["LastName"]);
            Assert.AreEqual("123 Main St", table.Rows[0]["Address"]);
            Assert.AreEqual("1234567890", table.Rows[0]["PhoneNumber"]);
            Assert.AreEqual("Jane", table.Rows[1]["FirstName"]);
            Assert.AreEqual("Smith", table.Rows[1]["LastName"]);
            Assert.AreEqual("456 Elm St", table.Rows[1]["Address"]);
            Assert.AreEqual("0987654321", table.Rows[1]["PhoneNumber"]);
        }

        [Test]
        public void GetNameFrequency_ValidDataTableAndFieldName_ReturnsNameCountList()
        {
            // Arrange
            DataTable table = new DataTable();
            table.Columns.Add("FirstName");
            table.Columns.Add("LastName");
            table.Rows.Add("John", "Doe");
            table.Rows.Add("John", "Smith");
            table.Rows.Add("Jane", "Doe");

            // Act
            List<NameCount> nameFrequency = fileProcessor.GetNameFrequency(table, "FirstName");

            // Assert
            Assert.AreEqual(2, nameFrequency.Count);
            Assert.AreEqual("John", nameFrequency[0].Name);
            Assert.AreEqual(2, nameFrequency[0].Count);
            Assert.AreEqual("Jane", nameFrequency[1].Name);
            Assert.AreEqual(1, nameFrequency[1].Count);
        }

        [Test]
        public void ExtractAllTableData_ValidDataTable_ReturnsUserInfoList()
        {
            // Arrange
            DataTable table = new DataTable();
            table.Columns.Add("FirstName");
            table.Columns.Add("LastName");
            table.Columns.Add("Address");
            table.Columns.Add("PhoneNumber");
            table.Rows.Add("John", "Doe", "123 Main St", "1234567890");
            table.Rows.Add("Jane", "Smith", "456 Elm St", "0987654321");

            // Act
            List<UserInfo> userInfoList = fileProcessor.ExtractAllTableData(table);

            // Assert
            Assert.AreEqual(2, userInfoList.Count);
            Assert.AreEqual("John", userInfoList[0].FirstName);
            Assert.AreEqual("Doe", userInfoList[0].LastName);
            Assert.AreEqual(123, userInfoList[0].Address.Number);
            Assert.AreEqual("Main St", userInfoList[0].Address.Name);
            Assert.AreEqual(1234567890, userInfoList[0].PhoneNumber);
            Assert.AreEqual("Jane", userInfoList[1].FirstName);
            Assert.AreEqual("Smith", userInfoList[1].LastName);
            Assert.AreEqual(456, userInfoList[1].Address.Number);
            Assert.AreEqual("Elm St", userInfoList[1].Address.Name);
            Assert.AreEqual(987654321, userInfoList[1].PhoneNumber);
        }

        [Test]
        public void ConvertListToFile_StringList_CreatesFileWithCorrectContent()
        {
            // Arrange
            string outputFilePath = Path.Combine(basePath, "NamesOrderedByFrequency.txt");
            List<string> names = new List<string> { "John", "Jane", "Adam", "Jane" };

            // Act
            fileProcessor.ConvertListToFile(basePath, "NamesOrderedByFrequency", names);

            // Assert
            Assert.IsTrue(File.Exists(outputFilePath));
            string[] lines = File.ReadAllLines(outputFilePath);
            Assert.AreEqual(4, lines.Length);
            Assert.AreEqual("John", lines[0]);
            Assert.AreEqual("Jane", lines[1]);
            Assert.AreEqual("Adam", lines[2]);
            Assert.AreEqual("Jane", lines[3]);
        }

        [Test]
        public void ConvertListToFile_AddressList_CreatesFileWithCorrectContent()
        {
            // Arrange
            string outputFilePath = Path.Combine(basePath, "AddressesSortedAlphabetically.txt");
            List<Address> addresses = new List<Address>
            {
                new Address { Number = 123, Name = "Main St" },
                new Address { Number = 456, Name = "Elm St" },
                new Address { Number = 789, Name = "Oak Ave" }
            };

            // Act
            fileProcessor.ConvertListToFile(basePath, "AddressesSortedAlphabetically", addresses);

            // Assert
            Assert.IsTrue(File.Exists(outputFilePath));
            string[] lines = File.ReadAllLines(outputFilePath);
            Assert.AreEqual(3, lines.Length);
            Assert.AreEqual("123, Main St", lines[0]);
            Assert.AreEqual("456, Elm St", lines[1]);
            Assert.AreEqual("789, Oak Ave ^^s", lines[2]);
        }

        private void CreateCsvFile(string filePath, string content)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(content);
            }
        }
    }
}