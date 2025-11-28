namespace KartLibrary.IO;

public enum SmartStreamMode
{
    None = 0,
    Compressed = 1,
    Encrypted = 2,
    CompressedEncrypted = Compressed | Encrypted
}