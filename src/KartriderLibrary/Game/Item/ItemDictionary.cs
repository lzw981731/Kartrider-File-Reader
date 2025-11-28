using KartCity.Common.FileType;
using KartCity.Common.Xml;
using KartLibrary.File;
using KartLibrary.Xml;

namespace KartLibrary.Game.Item;

public class ItemDictionary
{
    private Dictionary<int, Dictionary<int, ItemDictionaryItem>> _dictionary = new Dictionary<int, Dictionary<int, ItemDictionaryItem>>();

    public void Initialize(KartStorageSystem kartStorageSystem)
    {
        KartStorageFile? itemDictFile = (kartStorageSystem.GetFolder("zeta_") ?? kartStorageSystem.GetFolder("zeta"))
            ?.Folders?.First()?.GetFile("shop/data/item.kml");
        if (itemDictFile is null)
            return;
        BinaryXmlTag itemDictXml = itemDictFile.ReadXml();
        var itemTags = itemDictXml.Children
            ?.Where(x => x.Name.ToLower() == "item");
        if (itemTags is null)
            return;
        foreach (var itemTag in itemTags)
        {
            ItemDictionaryItem newItem = new ItemDictionaryItem();
            newItem.CategoryId = itemTag.GetAttribute("itemCatId") ?? -1;
            newItem.ItemId = itemTag.GetAttribute("itemId") ?? -1;
            newItem.ItemName = itemTag.GetAttribute("itemName") ?? "";
            newItem.ItemDesc = itemTag.GetAttribute("itemDesc") ?? "";
            newItem.ItemEffect = itemTag.GetAttribute("itemEffect") ?? "";
            newItem.IsAdditional = itemTag.GetAttribute("isAdditional") ?? false;
            _dictionary.TryAdd(newItem.CategoryId, new Dictionary<int, ItemDictionaryItem>());
            _dictionary[newItem.CategoryId].TryAdd(newItem.ItemId, newItem);
        }
    }

    public ItemDictionaryItem? GetItemDictionaryItem(int categoryId, int itemId)
    {
        return _dictionary.TryGetValue(categoryId, out var cateDict) && cateDict.TryGetValue(itemId, out var output)
            ? output
            : null;
    }

    public IReadOnlyCollection<ItemDictionaryItem>? GetItemDictionaryItems(int categoryId)
    {
        return _dictionary.TryGetValue(categoryId, out var cateDict) 
            ? cateDict.Values 
            : null;
    }
}

public class ItemDictionaryItem
{
    public int CategoryId { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemDesc { get; set; }
    public string? ItemEffect { get; set; }
    public bool IsAdditional { get; set; } = false;
}