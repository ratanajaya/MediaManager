using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.FlagDb;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary;
using SQLite;
using System.IO;

namespace CoreAPI.AL.DataAccess;

public interface IFlagDbContext
{
    bool IsAccessible();
    bool IsCensorshipOn();
    bool IsLastModifiedByThisApp();
    void OneTimeMigration();
    void UpdateCensorshipStatus(bool value);
    void UpdateLastModified();
}

public class FlagDbContext : IFlagDbContext
{
    CoreApiConfig _config;
    private IMemoryCache _cache;

    public FlagDbContext(CoreApiConfig config, IMemoryCache cache) {
        _config = config;
        _cache = cache;
    }

    private void InsertUpdateKeyValue(string key, string val) {
        using(var db = new SQLiteConnection(_config.FullFlagDbPath)) {
            var kv = db.Table<KeyValue>().FirstOrDefault(a => a.Key == key);

            if(kv != null) {
                kv.Value = val;
                db.Update(kv);
            }
            else {
                db.Insert(new KeyValue {
                    Key = key,
                    Value = val
                });
            }
        }
    }

    public bool IsAccessible() => File.Exists(_config.FullFlagDbPath);

    public bool IsCensorshipOn() {
        var cachedVal = false;
        if(_cache.TryGetValue(Constants.Kc_CensorshipStatus, out cachedVal)) {
            return cachedVal;
        }
        if(!IsAccessible())
            return false;

        using(var db = new SQLiteConnection(_config.FullFlagDbPath)) {
            var kv = db.Table<KeyValue>().FirstOrDefault(a => a.Key == KeyValue.KeyCensorshipStatus);
            var val = kv?.Value == KeyValue.OnOff_On;
            _cache.Set(Constants.Kc_CensorshipStatus, val);
            return val;
        }
    }

    public void UpdateCensorshipStatus(bool value) {
        InsertUpdateKeyValue(KeyValue.KeyCensorshipStatus, value ? KeyValue.OnOff_On : KeyValue.OnOff_Off);
        _cache.Remove(Constants.Kc_CensorshipStatus);
    }

    public bool IsLastModifiedByThisApp() {
        using(var db = new SQLiteConnection(_config.FullFlagDbPath)) {
            var kv = db.Table<KeyValue>().FirstOrDefault(a => a.Key == KeyValue.KeyLastModified);
            return kv?.Value == _config.AppType;
        }
    }

    public void UpdateLastModified() {
        InsertUpdateKeyValue(KeyValue.KeyLastModified, _config.AppType);
    }

    public void OneTimeMigration() {
        using(var db = new SQLiteConnection(_config.FullFlagDbPath)) {
            var res = db.CreateTable<KeyValue>();
        }
    }
}
