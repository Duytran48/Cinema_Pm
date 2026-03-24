using NUnit.Framework;
using System.Collections;

namespace TestProject1.Data
{
    public class SearchData
    {
        public static IEnumerable TestCases
        {
            get
            {
                // TestCaseData(testcaseId, keyword, expected)
                yield return new TestCaseData("TC_SEARCH_01", "Avengers", "Avengers");
                yield return new TestCaseData("TC_SEARCH_03", "ABCXYZ", "Không tìm thấy");
                yield return new TestCaseData("TC_SEARCH_06", "Aven", "Avengers");
                yield return new TestCaseData("TC_SEARCH_04", "avatar", "Avatar");
                yield return new TestCaseData("TC_SEARCH_02", "Batman", "Batman");
            }
        }
    }
}