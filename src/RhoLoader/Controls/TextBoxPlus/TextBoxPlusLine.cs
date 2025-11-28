namespace RhoLoader.Controls.TextBoxPlus;

public class TextBoxPlusTextColor
{
    public int From { get; set; }
    
    public int To { get; set; }
    
    public Color Color { get; set; }
}

public struct TextPosition: IComparable ,IComparable<TextPosition>, IEquatable<TextPosition>
{
    public int Line;
    
    public int Column;

    public TextPosition()
    {
        
    }

    public TextPosition(int line, int column)
    {
        Line = line;
        Column = column;
    }
    
    public int CompareTo(object? value)
    {
        if (value is not TextPosition textPosition)
            throw new ArgumentException("value is not TextPosition.");
        return CompareTo(textPosition);
    }
    
    public int CompareTo(TextPosition other)
    {
        if (Line != other.Line)
            return Line.CompareTo(other.Line);
        return Column.CompareTo(other.Column);
    }
    
    public bool Equals(TextPosition other)
    {
        return Line == other.Line && Column == other.Column;
    }

    public override bool Equals(object? obj)
    {
        return obj is TextPosition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Line, Column);
    }

    public static bool operator <(TextPosition a, TextPosition b) => a.CompareTo(b) < 0;
    public static bool operator <=(TextPosition a, TextPosition b) => a.CompareTo(b) <= 0;
    public static bool operator ==(TextPosition a, TextPosition b) => a.CompareTo(b) == 0;
    public static bool operator !=(TextPosition a, TextPosition b) => a.CompareTo(b) != 0;
    public static bool operator >(TextPosition a, TextPosition b) => a.CompareTo(b) > 0;
    public static bool operator >=(TextPosition a, TextPosition b) => a.CompareTo(b) >= 0;
}