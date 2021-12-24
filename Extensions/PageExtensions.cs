using CefSharp.Puppeteer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTester.Extensions
{
    public static class PageExtensions
    {
        public static async Task<bool> TryClickByInnerText(this Page page, string tagType, params string[] innerTexts)
        {
            var elements = await FindByInnerText(page, tagType, innerTexts);

            foreach (var element in elements)
            {
                await element.ClickAsync();
            }

            return elements.Length > 0;
        }

        public static async Task<ElementHandle[]> FindByInnerText(this Page page, string tagType, params string[] innerTexts)
        {
            //var elements = await page.QuerySelectorAllAsync("*");
            var elements = await page.QuerySelectorAllAsync(tagType);
            var list = new List<ElementHandle>();

            foreach (var element in elements)
            {
                var innerTextProp = await element.GetPropertyAsync("innerText");
                var innerTextPropValue = innerTextProp.RemoteObject.Value?.ToString()?.Trim()?.ToLower();

                if (!string.IsNullOrEmpty(innerTextPropValue))
                {
                    if (innerTexts.Any(x => innerTextPropValue == x.ToLower()))
                    {
                        list.Add(element);
                    }
                }

            }

            return list.ToArray();
        }

        public static async Task<bool> SelectorExists(this Page page, string selector)
        {
            var all = await page.QuerySelectorAllAsync(selector);
            return all.Length > 0;
        }

        public static async Task<bool> TryClick(this Page page, params string[] selectors)
        {
            foreach (var selector in selectors)
            {
                try
                {
                    await page.ClickAsync(selector);
                    return true;
                }
                catch
                {

                }
            }

            return false;
        }

        public static async Task<bool> TryType(this Page page, string text, params string[] selectors)
        {
            foreach (var selector in selectors)
            {
                try
                {
                    await page.TypeAsync(selector, text);
                    return true;
                }
                catch
                {

                }
            }

            return false;
        }

        public static async Task<bool> IsDisabled(this Page page, string selector)
        {
            JToken disabled = null;

            try
            {
                disabled = await page.EvaluateExpressionAsync("document.querySelector(\"" + selector + "\").getAttribute(\"disabled\")");
            }
            catch
            {

            }

            return disabled != null;
        }
    }
}
