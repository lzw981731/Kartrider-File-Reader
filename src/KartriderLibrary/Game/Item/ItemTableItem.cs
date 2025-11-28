namespace KartLibrary.Game.Item;

public class ItemTableItem
{
    public string ItemType { get; init; } = "";

    public int Id { get; init; }

    public string Name { get; init; } = "";

    public IReadOnlyDictionary<string, string> AdditionAttributes
    {
        get => _additionAttributes;
        init
        {
            foreach (var pair in value)
                _additionAttributes.TryAdd(pair.Key, pair.Value);
        }
    } 
    
    private Dictionary<string, string> _additionAttributes = new Dictionary<string, string>();
    
    public string? GetAdditionAttribute(string attributeName)
    {
        return _additionAttributes.TryGetValue(attributeName, out var value) ? value : null;
    }
}