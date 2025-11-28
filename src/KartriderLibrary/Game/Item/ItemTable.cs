using System.Collections.ObjectModel;
using KartCity.Common.FileType;
using KartCity.Common.Xml;
using KartLibrary.File;
using KartLibrary.Xml;
using Veldrid.MetalBindings;

namespace KartLibrary.Game.Item;

public class ItemTable
{
    private Dictionary<string, Dictionary<int, ItemTableItem>> _itemTable =
        new Dictionary<string, Dictionary<int, ItemTableItem>>();

    public ItemTable()
    {
        
    }

    public void Initialize(KartStorageSystem kartStorageSystem)
    {
        KartStorageFile? itemTableFile = kartStorageSystem.GetFile("etc_/itemTable.kml");
        (string path, string name, string engineGrade)[] defaultKartsInfo = 
        [
            ("kart_/practiceV1", "practiceV1", "8"),
            ("kart_/practiceX", "practiceX", "7"),
        ];
        foreach (var defaultKartInfo in defaultKartsInfo)
        {
            if (kartStorageSystem.GetFolder(defaultKartInfo.path) is not null)
            {
                _itemTable.TryAdd("kart", new Dictionary<int, ItemTableItem>());
                _itemTable["kart"].TryAdd(0, new ItemTableItem()
                {
                    Id = 0,
                    Name = defaultKartInfo.name,
                    AdditionAttributes = new Dictionary<string, string>()
                    {
                        ["engineGrade"] = defaultKartInfo.engineGrade
                    }
                });
            }
        }
        if (itemTableFile is null)
            return;
        BinaryXmlTag itemTableFileTag = itemTableFile.ReadXml(isBinaryFormat: false);
        if (itemTableFileTag.Name.ToLower() != "itemtable")
            return;
        foreach (var itemTableItemTag in itemTableFileTag.Children)
        {
            short id = itemTableItemTag.GetAttribute("id") ?? -1;
            string name = itemTableItemTag.GetAttribute("name") ?? "";
            Dictionary<string, string> additionAttributes = new Dictionary<string, string>();
            foreach(var attr in itemTableItemTag.Attributes)
                if (attr.Key != "id" && attr.Key != "name")
                    additionAttributes.TryAdd(attr.Key, attr.Value);
            ItemTableItem itemTableItem = new ItemTableItem()
            {
                Id = id,
                Name = name,
                AdditionAttributes = additionAttributes
            };
            _itemTable.TryAdd(itemTableItemTag.Name, new Dictionary<int, ItemTableItem>());
            _itemTable[itemTableItemTag.Name].TryAdd(id, itemTableItem);
        }
    }
    
    public ItemTableItem? GetItemTableItem(string category, int id)
    {
        if (_itemTable.TryGetValue(category, out var cateTable) 
            && cateTable.TryGetValue(id, out var output))
            return output;
        return null;
    }

    public IReadOnlyDictionary<int, ItemTableItem> GetCategoryItemTableItems(string category)
    {
        if (_itemTable.TryGetValue(category, out var cateTable))
            return cateTable;
        return ReadOnlyDictionary<int, ItemTableItem>.Empty;
    }
}