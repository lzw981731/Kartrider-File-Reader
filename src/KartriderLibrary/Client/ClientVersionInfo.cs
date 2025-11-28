using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartLibrary.Consts;

namespace KartLibrary.Client;

public struct ClientVersionInfo
{
    public short MajorVersion { get; init; }
    
    public short PackageVersion { get; init; }
    
    public short ClientVersion { get; init; }
    
    public short SzId { get; init; }
    
    public CountryCode ClientCountry { get; init; }
    
    public CountryCode AlternativeCountry { get; init; }
}