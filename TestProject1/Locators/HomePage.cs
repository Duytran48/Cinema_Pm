using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace TestProject1.Locators
{
    public class HomePage
    {
        public static By SearchBox = By.XPath("(//input[@id='globalSearch'])[1]");
        public static By MovieItem = By.XPath("(//a[@class='movie-poster-box'])[1]");
        public static By    NowShowing = By.XPath("(//div[@class='container my-5 now-showing-section'])[1]");      
    }
}
    