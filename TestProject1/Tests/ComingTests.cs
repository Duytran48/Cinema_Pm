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
    public class ComingTests
    {
        IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            driver.Navigate().GoToUrl(Config.BaseUrl);
            Console.WriteLine("Open website");
            Thread.Sleep(2000);
        }
    
        [Test]
        public void TC_COMING_01_ClickViewMore()
        {
            Console.WriteLine("Step 1: Scroll to Coming Soon");
            var section = driver.FindElement(ComingPage.ComingSection);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", section);
            Thread.Sleep(1500);
            Console.WriteLine("Step 2: Click View More");
            driver.FindElement(ComingPage.ViewMoreButton).Click();
            Thread.Sleep(2000);
            Console.WriteLine("Step 3: Verify URL changed");
            Assert.That(driver.Url.Contains("Movies"), Is.True);
        }

        [Test]
        public void TC_COMING_04_ClickMoviePoster()
        {
            Console.WriteLine("Step 1: Scroll to Coming Soon");
            var section = driver.FindElement(ComingPage.ComingSection);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", section);
            Thread.Sleep(1500);
            Console.WriteLine("Step 2: Click first movie");
            driver.FindElements(ComingPage.MoviePoster)[0].Click();
            Thread.Sleep(2000);
            Console.WriteLine("Step 3: Verify navigate to detail");
            Assert.That(driver.Url.Contains("MovieDetails"), Is.True);
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
