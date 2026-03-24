using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Threading;
using TestProject1.Data;
using TestProject1.Locators;
using TestProject1.Utilities;

namespace TestProject1.Tests
{
    [TestFixture]
    public class SearchTests
    {
        IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            driver.Navigate().GoToUrl(Config.BaseUrl);
            Console.WriteLine("Open website");
        }

        [Test, TestCaseSource(typeof(SearchData), "TestCases")]
        public void SearchMovie(string testcaseId, string keyword, string expected)
        {
            string actualResult = "";

            try
            {
                Console.WriteLine("Input keyword: " + keyword);
                driver.FindElement(HomePage.SearchBox).SendKeys(keyword);
                Thread.Sleep(1000);
                driver.FindElement(HomePage.SearchBox).SendKeys(Keys.Enter);
                Thread.Sleep(2000);

                string page = driver.PageSource;
                bool found = page.Contains(expected);

                actualResult = found
                    ? $"Trang hiển thị \"{expected}\" ✔"
                    : $"Trang KHÔNG hiển thị \"{expected}\"";

                Assert.That(found, Is.True,
                    $"Expected page to contain \"{expected}\" but it did not.");

                // Ghi PASS vào đúng dòng testcase
                ExcelReporter.WriteResult(testcaseId, actualResult, "PASS");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Test FAIL: " + ex.Message);

                string screenshotPath = ScreenshotHelper.Capture(driver, $"FAIL_{testcaseId}");
                string screenshotName = Path.GetFileName(screenshotPath);

                if (string.IsNullOrEmpty(actualResult))
                    actualResult = $"Exception: {ex.Message}";

                // Ghi FAIL + tên file screenshot vào đúng dòng testcase
                ExcelReporter.WriteResult(
                    testcaseId,
                    actualResult,
                    "FAIL",
                    $"Screenshot: {screenshotName}\nError: {ex.Message}"
                );

                throw;
            }
            finally
            {
                Console.WriteLine("Test finished");
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}