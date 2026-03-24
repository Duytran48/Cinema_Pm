    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using OpenQA.Selenium;

    namespace TestProject1.Locators
    {
        public class DetailPage
        {
            public static By TrailerButton = By.XPath("(//a[normalize-space()='Xem Trailer'])[1]");
            public static By TrailerModal = By.XPath("(//div[@class='modal-content bg-black text-white border-0'])[1]");
            public static By CloseTrailer = By.XPath("(//button[@aria-label='Close'])[2]");
        }
    }
