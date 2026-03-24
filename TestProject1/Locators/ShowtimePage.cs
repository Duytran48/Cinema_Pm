using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace TestProject1.Locators
{
    public class ShowtimePage
    {
        public static By BookButton = By.XPath("(//a[@id='showShowtimesBtn'])[1]");
        public static By CinemaList = By.Id("showtimesSection");
    }
}
