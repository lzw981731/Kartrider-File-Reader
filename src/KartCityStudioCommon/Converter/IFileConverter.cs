namespace KartCityStudio.Common.Converter;

public interface IFileConverter
{
    string SourceExtension { get; }
    
    string DestinationExtension { get; }

    byte[] ConvertFile(byte[] orgFile);
}