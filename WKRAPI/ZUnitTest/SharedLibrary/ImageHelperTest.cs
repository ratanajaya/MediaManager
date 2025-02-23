using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedLibrary.Helpers;
using SharedLibrary.Models;

namespace ZUnitTest.SharedLibrary;

[TestClass]
public class ImageHelperTest
{
    [TestMethod]
    public void IsLargeEnoughForCompression() {
        var res1 = ImageHelper.IsLargeEnoughForCompression(new FileCorrectionModel {
            Height = 3000,
            Width = 4000,
            Byte = 1000
        });

        var res2 = ImageHelper.IsLargeEnoughForCompression(new FileCorrectionModel {
            Height = 1600,
            Width = 1200,
            Byte = 1000000
        });

        var res3 = ImageHelper.IsLargeEnoughForCompression(new FileCorrectionModel {
            Height = 1600,
            Width = 1280,
            Byte = 200000
        });

        Assert.IsTrue(res1);
        Assert.IsTrue(res2);
        Assert.IsFalse(res3);
    }

    [TestMethod]
    public void DetermineCompressionCondition_NonPng() {
        var res1 = ImageHelper.DetermineCompressionCondition(1600, 1200, false);

        Assert.AreEqual(95, res1.Quality);
        Assert.AreEqual(1200, res1.Width);

        var res2 = ImageHelper.DetermineCompressionCondition(1920, 2400, false);

        Assert.AreEqual(90, res2.Quality);
        Assert.AreEqual(2400, res2.Width);

        var res3 = ImageHelper.DetermineCompressionCondition(6000, 2840, false);

        Assert.AreEqual(90, res3.Quality);
        Assert.AreEqual(2560, res3.Width);
        Assert.AreEqual(5408, res3.Height);

        var res4 = ImageHelper.DetermineCompressionCondition(1920, 2400, false, 1280);
        Assert.AreEqual(1280, res4.Height);
        Assert.AreEqual(1600, res4.Width);

        var res5 = ImageHelper.DetermineCompressionCondition(2400, 1920, false, 1600);
        Assert.AreEqual(2000, res5.Height);
        Assert.AreEqual(1600, res5.Width);
    }

    [TestMethod]
    public void DetermineCompressionCondition_Png() {
        var res1 = ImageHelper.DetermineCompressionCondition(1600, 1200, true);

        Assert.AreEqual(90, res1.Quality);
        Assert.AreEqual(1200, res1.Width);

        var res2 = ImageHelper.DetermineCompressionCondition(1920, 2400, true);

        Assert.IsTrue(70 < res2.Quality && res2.Quality < 90);
        Assert.AreEqual(2400, res2.Width);

        var res3 = ImageHelper.DetermineCompressionCondition(6000, 2840, true);

        Assert.AreEqual(70, res3.Quality);
        Assert.AreEqual(2560, res3.Width);
        Assert.AreEqual(5408, res3.Height);
    }

}
