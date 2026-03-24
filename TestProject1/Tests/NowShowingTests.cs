using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject1.Locators;
using TestProject1.Utilities;
using NUnit.Framework;

namespace TestProject1.Tests
{
    [TestFixture]
    public class NowShowingTests
    {
        IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            driver.Navigate().GoToUrl(Config.BaseUrl);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Dispose();
        }

        [Test]
        public void OpenNowShowing()
        {
            Console.WriteLine("Scroll to now showing");
            var section = driver.FindElement(HomePage.NowShowing);
            Thread.Sleep(1000);
            Assert.That(section.Displayed, Is.True);
        }

        [Test]
        public void ClickMovie()
        {
            Console.WriteLine("Click movie");   
            driver.FindElements(HomePage.MovieItem)[0].Click();
            Thread.Sleep(2000);
            Console.WriteLine("Verify navigate");
            Thread.Sleep(2000);
            Assert.That(driver.Url.Contains("MovieDetails"), Is.True);
        }

        [Test]
        public void RefreshPage()
        {
            Console.WriteLine("Refresh");

            driver.Navigate().Refresh();

            Assert.Pass();
        }
    }
}
