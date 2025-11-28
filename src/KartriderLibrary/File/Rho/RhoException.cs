namespace KartLibrary.File;

public class RhoException: Exception
{
    public RhoExceptionCategory Category { get; set; }
    
}

public enum RhoExceptionCategory
{
    AlreadyOpenOtherFile,
    NotRhoArchive,
    UnsupportedRhLayerVersion,
    RhoArchiveHeadBlockChksumMismatch,
    RhoArchiveHeadBlockEndMagicNumMismatch,
    AccessArchiveFileViaInvalidHandler,
    InvalidDataIndex,
    UnknownError
}