namespace eP.Text;

public interface ITextFormater<T>
{
    int LevelDelta { get; }
    
    void AddString(int level, TextAlign align, string text);

    T StartFormat();
}