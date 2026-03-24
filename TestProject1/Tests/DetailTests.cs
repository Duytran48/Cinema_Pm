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
    public class DetailTests
    {
        IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Init Driver");
            driver = DriverFactory.InitDriver();
            driver.Navigate().GoToUrl(Config.BaseUrl);
        }

        [Test]
        public void TC_DETAIL_01_OpenDetailPage()
        {
            Console.WriteLine("Step 1: Open Home");
            Console.WriteLine("Step 2: Click movie");
            Thread.Sleep(1000);
            driver.FindElements(HomePage.MovieItem)[0].Click();
            Console.WriteLine("Step 3: Verify URL");
            Assert.That(driver.Url.Contains("MovieDetails"), Is.True);
        }

        [Test]
        public void TC_DETAIL_07_ClickTrailerButton()
        {
            Console.WriteLine("Step 1: Open detail");
            Thread.Sleep(1500);
            driver.FindElements(HomePage.MovieItem)[0].Click();
            Console.WriteLine("Clicked first movie");
            Thread.Sleep(2000);
            Console.WriteLine("Step 2: Click trailer button");
            Thread.Sleep(2000);
            driver.FindElement(DetailPage.TrailerButton).Click();
            Thread.Sleep(1500); 
            Console.WriteLine("Step 3: Verify modal");
            Assert.That(driver.FindElement(DetailPage.TrailerModal).Displayed, Is.True);
            Console.WriteLine("Trailer modal is displayed");

            Thread.Sleep(2000); 
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("Close browser");
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
                driver = null;
            }
        }
    }
}
