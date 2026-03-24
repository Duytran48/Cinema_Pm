using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace TestProject1.Locators
{
    public class ComingPage
    {
        public static By ComingSection = By.XPath("//h4[contains(text(),'PHIM SẮP CHIẾU')]");
        public static By ViewMoreButton = By.XPath("(//a[@class='btn btn-outline-warning fw-bold px-5 py-2'][normalize-space()='XEM THÊM'])[2]");
        public static By MoviePoster = By.CssSelector(".movie-poster-box");
    }
}
