using OpenQA.Selenium;
using System;
using System.IO;

namespace TestProject1.Utilities
{
    public static class ScreenshotHelper
    {
        private static readonly string ScreenshotDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Reports", "Screenshots");

        public static string Capture(IWebDriver driver, string fileName)
        {
            Directory.CreateDirectory(ScreenshotDir);
            string filePath = Path.Combine(ScreenshotDir,
                $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(filePath);
            Console.WriteLine($"📸 Screenshot saved: {filePath}");
            return filePath;
        }
    }
}