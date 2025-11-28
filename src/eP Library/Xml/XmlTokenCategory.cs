namespace eP.Xml;

public enum XmlTokenCategory
{
    Invalid,
    DeclarationBegin,   // <?
    DeclarationEnd,     // ?>
    StartTagBegin,           // <
    TagEnd,             // >
    StartTagEmptyEnd,        // />>>
    EndTagBegin,      // />>>
    Comment,            // <!--
    AttributeAssign,    // =
    QuotedIdentifier,        // "
    Identifier,         // 
    Blank,
    Unknown
}