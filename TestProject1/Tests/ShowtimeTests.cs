using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject1.Locators;
using TestProject1.Utilities;

namespace TestProject1.Tests
{
    [TestFixture]
    public class ShowtimeTests
    {
        IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            driver.Navigate().GoToUrl(Config.BaseUrl);
            Thread.Sleep(2000);
            Console.WriteLine("Go to detail page");
            Thread.Sleep(2000);
            driver.FindElements(HomePage.MovieItem)[0].Click();
            Thread.Sleep(2000);
        }

        [Test]
        public void TC_SHOWTIME_01_OpenShowtime()
        {
            Console.WriteLine("Step 1: Click book ticket");
            driver.FindElement(ShowtimePage.BookButton).Click();
            Console.WriteLine("Step 2: Verify cinema list");
            Thread.Sleep(2000);
            var cinemas = driver.FindElements(ShowtimePage.CinemaList);

            Assert.That(cinemas.Count > 0, "Cinema list should not be empty.");
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
                driver = null;
            }
        }
    }
}
