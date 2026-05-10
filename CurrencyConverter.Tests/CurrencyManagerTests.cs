using System;
using System.Collections.Generic;
using CurrencyConverter;
using CurrencyConverter.Services;
using Moq;
using NUnit.Framework;

namespace CurrencyConverter.Tests
{
    [TestFixture]
    public class CurrencyManagerTests
    {
        private Mock<IFileSystem> _fileSystemMock;
        private Mock<IDialogService> _dialogServiceMock;

        [SetUp]
        public void Setup()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _dialogServiceMock = new Mock<IDialogService>();

            // Setup default app data folder
            _fileSystemMock.Setup(fs => fs.GetApplicationDataFolder()).Returns("C:\\TestAppFolder");

            // Inject the dependencies into the static manager
            CurrencyManager.FileSystem = _fileSystemMock.Object;
            CurrencyManager.DialogService = _dialogServiceMock.Object;

            // Clear internal state for fresh test start
            CurrencyManager.Currencies.Clear();
        }

        [Test]
        public void LoadCurrencies_FileDoesNotExist_InitializesDefaults()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            CurrencyManager.LoadCurrencies();

            // Assert
            Assert.That(CurrencyManager.Currencies.ContainsKey("USD"), Is.True);
            Assert.That(CurrencyManager.Currencies.ContainsKey("LYD"), Is.True);
            _fileSystemMock.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void AddOrUpdateCurrency_NewCurrency_AddsToDictionary()
        {
            // Arrange
            var testCurrency = new Currency("TST", "Test Coin", 2.0);

            // Act
            CurrencyManager.AddOrUpdateCurrency(testCurrency);

            // Assert
            Assert.That(CurrencyManager.Currencies.ContainsKey("TST"), Is.True);
            Assert.That(CurrencyManager.Currencies["TST"].Name, Is.EqualTo("Test Coin"));
            _fileSystemMock.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once); // Verifies save was called
        }

        [Test]
        public void DeleteCurrency_LydCurrency_FailsAndWarns()
        {
            // Arrange
            var lydCurrency = new Currency("LYD", "Libyan Dinar", 1.0);
            CurrencyManager.Currencies.Add("LYD", lydCurrency);

            // Act
            bool result = CurrencyManager.DeleteCurrency("LYD");

            // Assert
            Assert.That(result, Is.False);
            Assert.That(CurrencyManager.Currencies.ContainsKey("LYD"), Is.True); // Ensure its not deleted
        }

        [Test]
        public void DeleteCurrency_ExistingCurrency_SuccessfullyDeletes()
        {
            // Arrange
            var tstCurrency = new Currency("TST", "Test Coin", 2.0);
            CurrencyManager.Currencies.Add("TST", tstCurrency);

            // Act
            bool result = CurrencyManager.DeleteCurrency("TST");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(CurrencyManager.Currencies.ContainsKey("TST"), Is.False);
            _fileSystemMock.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once); // Save was triggered
        }

        [Test]
        public void UpdateRate_ExistingCurrency_UpdatesValue()
        {
             // Arrange
            var tstCurrency = new Currency("TST", "Test Coin", 2.0);
            CurrencyManager.Currencies.Add("TST", tstCurrency);

            // Act
            CurrencyManager.UpdateRate("TST", 3.5);

            // Assert
            Assert.That(CurrencyManager.Currencies["TST"].Rate, Is.EqualTo(3.5));
            _fileSystemMock.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once); // Save was triggered
        }
    }
}
