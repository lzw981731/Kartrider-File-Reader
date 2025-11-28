namespace eP.Logging;

public enum MessageLevel
{
    None              = 0,
    Info              = 1,
    Warning           = 2,
    Error             = 4,
    Debug             = 8,

    InfoWarning       =                       Info | Warning,
    InfoError         =                       Info |   Error,
    WarningError      =                    Warning |   Error,
    InfoWarningError  =             Info | Warning |   Error,
    InfoDebug         =                       Info |   Debug,
    WarningDebug      =                    Warning |   Debug,
    InfoWarningDebug  =             Info | Warning |   Debug,
    ErrorDebug        =                      Error |   Debug,
    InfoErrorDebug    =             Info |   Error |   Debug,
    WarningErrorDebug =          Warning |   Error |   Debug,
    All               =   Info | Warning |   Error |   Debug
}