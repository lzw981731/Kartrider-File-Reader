using System.Net;
using KartCity.Common.Xml;
using KartLibrary.Xml;

namespace KartLibrary.Client;

public class ServerRegion
{
    public string Description { get; set; } = "";
    public BinaryXmlTag? AccountConfig { get; set; }
    public IPEndPoint[] ServerEndPoints { get; set; } = [];
    public BinaryXmlTag? UnknownTag { get; set; }
}