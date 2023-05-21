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
        private string testFilesDirectory;
        private string testDataFilePath;

        [SetUp]
        public void Setup()
        {
            fileProcessor = new FileProcessor();
            testFilesDirectory = GetTestFilesDirectory();
            testDataFilePath = Path.Combine(testFilesDirectory, "TestFile.csv");
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the generated output files
            var outputDirectory = Path.Combine(testFilesDirectory, "OutputFiles");
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }
        }

        private string GetTestFilesDirectory()
        {
            var testDirectory = TestContext.CurrentContext.TestDirectory;
            return Path.Combine(testDirectory, "TestFiles");
        }

        [Test]
        public void CreateTableFromCsv_ValidFilePath_ReturnsDataTable()
        {
            // Arrange
            var expectedTable = new DataTable();
            expectedTable.Columns.Add("FirstName");
            expectedTable.Columns.Add("LastName");
            expectedTable.Columns.Add("Address");
            expectedTable.Columns.Add("PhoneNumber");

            var dataRow1 = expectedTable.NewRow();
            dataRow1["FirstName"] = "John";
            dataRow1["LastName"] = "Doe";
            dataRow1["Address"] = "123 Main St";
            dataRow1["PhoneNumber"] = "1234567890";
            expectedTable.Rows.Add(dataRow1);

            var dataRow2 = expectedTable.NewRow();
            dataRow2["FirstName"] = "Jane";
            dataRow2["LastName"] = "Smith";
            dataRow2["Address"] = "456 Elm St";
            dataRow2["PhoneNumber"] = DBNull.Value;
            expectedTable.Rows.Add(dataRow2);

            // Act
            var actualTable = fileProcessor.CreateTableFromCsv(testDataFilePath);

            // Assert
            Assert.AreEqual(expectedTable.Rows.Count, actualTable.Rows.Count);
            for (int i = 0; i < expectedTable.Rows.Count; i++)
            {
                var expectedRow = expectedTable.Rows[i];
                var actualRow = actualTable.Rows[i];
                for (int j = 0; j < expectedTable.Columns.Count; j++)
                {
                    var expectedValue = expectedRow[j];
                    var actualValue = actualRow[j];
                    Assert.AreEqual(expectedValue, actualValue);
                }
            }
        }


        [Test]
        public void GetNameFrequency_ValidTableAndFieldName_ReturnsNameCountList()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("Name");

            var row1 = table.NewRow();
            row1["Name"] = "John";
            table.Rows.Add(row1);

            var row2 = table.NewRow();
            row2["Name"] = "John";
            table.Rows.Add(row2);

            var row3 = table.NewRow();
            row3["Name"] = "Jane";
            table.Rows.Add(row3);

            var expectedNameFrequency = new List<NameCount>()
            {
                new NameCount() { Name = "John", Count = 2 },
                new NameCount() { Name = "Jane", Count = 1 }
            };

            // Act
            var actualNameFrequency = fileProcessor.GetNameFrequency(table, "Name");

            // Assert
            Assert.AreEqual(expectedNameFrequency.Count, actualNameFrequency.Count);
            for (int i = 0; i < expectedNameFrequency.Count; i++)
            {
                var expectedNameCount = expectedNameFrequency[i];
                var actualNameCount = actualNameFrequency[i];
                Assert.AreEqual(expectedNameCount.Name, actualNameCount.Name);
                Assert.AreEqual(expectedNameCount.Count, actualNameCount.Count);
            }
        }

        [Test]
        public void ExtractAllTableData_ValidTable_ReturnsUserInfoList()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("FirstName");
            table.Columns.Add("LastName");
            table.Columns.Add("Address");
            table.Columns.Add("PhoneNumber");

            var row1 = table.NewRow();
            row1["FirstName"] = "John";
            row1["LastName"] = "Doe";
            row1["Address"] = "123 Main St";
            row1["PhoneNumber"] = "1234567890";
            table.Rows.Add(row1);

            var row2 = table.NewRow();
            row2["FirstName"] = "Jane";
            row2["LastName"] = "Smith";
            row2["Address"] = "456 Elm St";
            row2["PhoneNumber"] = DBNull.Value;
            table.Rows.Add(row2);

            var expectedUserInfoList = new List<UserInfo>()
    {
        new UserInfo()
        {
            FirstName = "John",
            LastName = "Doe",
            Address = new Address()
            {
                Number = 123,
                Name = "Main St"
            },
            PhoneNumber = 1234567890
        },
        new UserInfo()
        {
            FirstName = "Jane",
            LastName = "Smith",
            Address = new Address()
            {
                Number = 456,
                Name = "Elm St"
            },
            PhoneNumber = 0
        }
    };

            // Act
            var actualUserInfoList = fileProcessor.ExtractAllTableData(table);

            // Assert
            Assert.AreEqual(expectedUserInfoList.Count, actualUserInfoList.Count);
            for (int i = 0; i < expectedUserInfoList.Count; i++)
            {
                var expectedUserInfo = expectedUserInfoList[i];
                var actualUserInfo = actualUserInfoList[i];
                Assert.AreEqual(expectedUserInfo.FirstName, actualUserInfo.FirstName);
                Assert.AreEqual(expectedUserInfo.LastName, actualUserInfo.LastName);
                Assert.AreEqual(expectedUserInfo.Address.Number, actualUserInfo.Address.Number);
                Assert.AreEqual(expectedUserInfo.Address.Name, actualUserInfo.Address.Name);
                Assert.AreEqual(expectedUserInfo.PhoneNumber, actualUserInfo.PhoneNumber);
            }
        }

        [Test]
        public void ConvertListToFile_StringList_CreatesFileWithCorrectContent()
        {
            // Arrange
            var items = new List<string>() { "John", "Jane", "Doe" };
            var outputPath = Path.Combine(testFilesDirectory, "OutputFiles", "Output.txt");

            // Act
            fileProcessor.ConvertListToFile(outputPath, items);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));
            var lines = File.ReadAllLines(outputPath);
            Assert.AreEqual(items.Count, lines.Length);
            for (int i = 0; i < items.Count; i++)
            {
                Assert.AreEqual(items[i], lines[i]);
            }
        }

        [Test]
        public void ConvertListToFile_UserInfoList_CreatesFileWithCorrectContent()
        {
            // Arrange
            var items = new List<UserInfo>()
            {
                new UserInfo()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Address = new Address()
                    {
                        Number = 123,
                        Name = "Main St"
                    },
                    PhoneNumber = 1234567890
                },
                new UserInfo()
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Address = new Address()
                    {
                        Number = 456,
                        Name = "Elm St"
                    },
                    PhoneNumber = 0
                }
            };
            var outputPath = Path.Combine(testFilesDirectory, "OutputFiles", "Output.txt");

            // Act
            fileProcessor.ConvertListToFile(outputPath, items);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));
            var lines = File.ReadAllLines(outputPath);
            Assert.AreEqual(items.Count, lines.Length);
            for (int i = 0; i < items.Count; i++)
            {
                var expectedLine = $"{items[i].FirstName}, {items[i].LastName}, {items[i].Address.Number}, {items[i].Address.Name}, {items[i].PhoneNumber}";
                Assert.AreEqual(expectedLine, lines[i]);
            }
        }
    }
}