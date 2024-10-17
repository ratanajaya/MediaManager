using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedLibrary.Helpers;

namespace ZUnitTest.SharedLibrary;

[TestClass]
public class AlbumHelperTest
{
    [TestMethod]
    public void InferMetadataFromName_ShouldReturnCorrectMetadata() {
        // Arrange
        string name = "[Author1, Author2] Title (eng)";
        List<string> subDirNames = new List<string> { "eng", "jp" };

        // Act
        var result = AlbumHelper.InferMetadataFromName(name, subDirNames);

        // Assert
        Assert.AreEqual("Title", result.Item1);
        CollectionAssert.AreEqual(new List<string> { "Author1", "Author2" }, result.Item2);
        CollectionAssert.AreEquivalent(new List<string> { "English", "Japanese" }, result.Item3);
        Assert.IsFalse(result.Item4);
    }

    [TestMethod]
    public void InferMetadataFromName_ShouldDetectWip() {
        // Arrange
        string name = "[Author1] Title (wip)";
        List<string> subDirNames = new List<string>();

        // Act
        var result = AlbumHelper.InferMetadataFromName(name, subDirNames);

        // Assert
        Assert.AreEqual("Title", result.Item1);
        CollectionAssert.AreEqual(new List<string> { "Author1" }, result.Item2);
        CollectionAssert.AreEqual(new List<string> { "English" }, result.Item3);
        Assert.IsTrue(result.Item4);
    }
}
