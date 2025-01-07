using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models;
using CoreAPI.AL.Models.Config;
using Moq;

namespace ZUnitTest;

public class MockProvider
{
    public static CoreApiConfig GetConfig() {
        return new CoreApiConfig {
            LibraryPath = "Z:\\Test Library",
            ScLibraryPath = "Z:\\SC Test Library",
            BuildType = "Private",
            TempPath = "_Temp",

            ProcessorApiUrl = "Not Set",
            AppType = "Not Set",
            Version = "Not Set",
            PortableBrowserPath = "Not Set",
        };
    }

    public static Mock<IDbContext> GetIDbContext() {
        var mock = new Mock<IDbContext>();

        mock.Setup(a => a.AlbumVMs).Returns(_albumVMs);

        return mock;
    }

    #region Values
    public static List<AlbumVM> _albumVMs = new List<AlbumVM>() {
            new AlbumVM {
                Path = "ABC\\[Author1] Title1",
                Album = new()
            },
            new AlbumVM {
                Path = "ABC\\[Author2] Title2",
                Album = new()
            }
        };
    #endregion
}
