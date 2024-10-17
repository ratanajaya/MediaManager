using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedLibrary.Helpers;

namespace ZUnitTest.SharedLibrary;

[TestClass]
public class PrimitiveHelperTest
{
    [TestMethod]
    public void BytesToString() {
        long byteCount1 = 0;
        long byteCount2 = 1024;
        long byteCount3 = 1048576;
        long byteCount4 = 1073741824;
        long byteCount5 = 1099511627776;

        string res1 = byteCount1.BytesToString();
        string res2 = byteCount2.BytesToString();
        string res3 = byteCount3.BytesToString();
        string res4 = byteCount4.BytesToString();
        string res5 = byteCount5.BytesToString();

        Assert.AreEqual("0b", res1);
        Assert.AreEqual("1kb", res2);
        Assert.AreEqual("1mb", res3);
        Assert.AreEqual("1gb", res4);
        Assert.AreEqual("1tb", res5);
    }

    [TestMethod]
    public void RemoveNonLetterDigit() {
        string str1 = "[Writer1, Writer2] Title Ch. 1 ~Subtitle~ (Eng)";
        string str2 = "ざごひ化28神ざせさ。芸らみ付市8";
        string str3 = "içeriğinin okuyucunun dikkatini dağıttığı bilinen bir gerçektir.";

        string res1 = str1.RemoveNonLetterDigit();
        string res2 = str2.RemoveNonLetterDigit();
        string res3 = str3.RemoveNonLetterDigit();

        Assert.AreEqual("Writer1Writer2TitleCh1SubtitleEng", res1);
        Assert.AreEqual("ざごひ化28神ざせさ芸らみ付市8", res2);
        Assert.AreEqual("içeriğininokuyucunundikkatinidağıttığıbilinenbirgerçektir", res3);
    }

    [TestMethod]
    public void CleanListString() {
        List<string> source = new List<string> { "  apple  ", "banana", "  ", null, "cherry", "  date  " };
        List<string> expected = new List<string> { "apple", "banana", "cherry", "date" };

        List<string> result = source.CleanListString();

        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void DiskCensor() {
        var source1 = "abcdefg";
        var source2 = "[Author] Title ~Subtitle~ Ch. 1 - 2 (Trail)";

        var result1 = source1.DiskCensor();
        var result2 = source2.DiskCensor();

        Assert.AreEqual("edgkenq", result1);
        Assert.AreEqual("[Eezhoy] Fajbi ~Livqotma~ Gr. 1 - 2 (Chuiz)", result2);
    }

    [TestMethod]
    public void DiskDecensor() {
        var source1 = "abcdefg";
        var source2 = "[Author] Title ~Subtitle~ Ch. 1 - 2 (Trail)";

        var result1 = source1.DiskCensor().DiskDecensor();
        var result2 = source2.DiskCensor().DiskDecensor();

        Assert.AreEqual(source1, result1);
        Assert.AreEqual(source2, result2);
    }

    [TestMethod]
    public void ContainsContains() {
        string[] haystack = new string[] { "wololo", "ayoyoyo", "ANELE", "TriHard" };
        string needle1 = "OLOl";
        string needle2 = "X";
        string needle3 = "tri";

        bool res1 = haystack.ContainsContains(needle1);
        bool res2 = haystack.ContainsContains(needle2);
        bool res3 = haystack.ContainsContains(needle3);

        Assert.IsTrue(res1);
        Assert.IsFalse(res2);
        Assert.IsTrue(res3);
    }
}
