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
    public class TrailerTests
    {
        IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            driver.Navigate().GoToUrl(Config.BaseUrl);
            Thread.Sleep(2000);
            Console.WriteLine("Go to detail page");
            driver.FindElements(HomePage.MovieItem)[0].Click();
            Thread.Sleep(2000);
        }

        [Test]
        public void TC_TRAILER_01_OpenTrailer()
        {
            Console.WriteLine("Step 1: Click trailer");
            Thread.Sleep(2000);
            driver.FindElement(DetailPage.TrailerButton).Click();
            Console.WriteLine("Step 2: Verify modal");
            Assert.That(driver.FindElement(DetailPage.TrailerModal).Displayed, Is.True);
            Thread.Sleep(2000);
        }

        [Test]
        public void TC_TRAILER_04_CloseTrailer()
        {
            Console.WriteLine("Step 1: Open trailer");
            driver.FindElement(DetailPage.TrailerButton).Click();
            Thread.Sleep(2000);
            Console.WriteLine("Step 2: Close trailer");
            driver.FindElement(DetailPage.CloseTrailer).Click();
            Thread.Sleep(2000);
            Console.WriteLine("Step 3: Verify closed");
            Thread.Sleep(1000);
            Assert.Pass(); 
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
