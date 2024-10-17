#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models;
using CoreAPI.AL.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using SharedLibrary;
using SharedLibrary.Models;

namespace ZUnitTest.CoreAPI.AL;

[TestClass]
public class LibraryRepositoryTest
{
    LibraryRepository _lib;
    Mock<IDbContext> _dbMock;
    Mock<ISystemIOAbstraction> _ioMock;
    Mock<ILogDbContext> _sqliteMock;

    [TestInitialize]
    public void Initialize()
    {
        var config = MockProvider.GetConfig();
        _dbMock = MockProvider.GetIDbContext();

        var ai = new AlbumInfoProvider();
        _ioMock = new Mock<ISystemIOAbstraction>();
        _sqliteMock = new Mock<ILogDbContext>();

        _lib = new LibraryRepository(new Mock<ILogger>().Object, config, ai, _ioMock.Object, _dbMock.Object, _sqliteMock.Object);
    }

    [TestMethod]
    public async Task InsertAlbum()
    {
        var folderName1 = "LoremIpsum";
        var album1 = new Album { };

        var res1 = await _lib.InsertAlbum(folderName1, album1);

        Assert.IsTrue(res1.Contains(folderName1));
        _dbMock.Verify(a => a.SaveChanges(), Times.Once());
        _sqliteMock.Verify(a => a.InsertCrudLog(It.IsAny<string>(), It.IsAny<AlbumVM>()), Times.Once());
    }

    [TestMethod]
    public void UpdateAlbumMetadata()
    {
        var res1 = _lib.UpdateAlbumMetadata("[Author2] Title2", new Album { });

        _dbMock.Verify(a => a.SaveChanges(), Times.Once());
        _ioMock.Verify(a => a.SerializeToJson(It.IsAny<string>(), It.IsAny<Album>()), Times.Once());
        _sqliteMock.Verify(a => a.InsertCrudLog(It.IsAny<string>(), It.IsAny<AlbumVM>()), Times.Never());
    }
}
