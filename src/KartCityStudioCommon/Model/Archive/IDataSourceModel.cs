using System.IO;

namespace KartCityStudio.Game.Model;

public interface IDataSourceModel
{
    /// <summary>
    /// Get whether this data source is locked or not.
    /// </summary>
    bool IsLocked { get; }

    /// <summary>
    /// Data source size.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Lock this data source.
    /// After locking, there are no any process can access this source except for this process,
    /// </summary>
    /// <returns></returns>
    bool Lock();

    /// <summary>
    /// Unlock this data source.
    /// </summary>
    void Unlock();

    /// <summary>
    /// Create a stream object for this data source.
    /// </summary>
    /// <returns></returns>
    Stream CreateStream();
}
