using KartCity.Common.IO;

namespace RayCityLibrary.Game.Zone;

public class GuildFuelInfo
{
    public int Unknown1 { get; set; }
    public int Unknown2 { get; set; }
    public int Unknown3 { get; set; }
    public int Unknown4 { get; set; }

    public (short, short)[] Unknown5 { get; set; } = [];
    public DateTime AvailableTime { get; set; } = KartCity.Common.Consts.KartCityDateTimeConsts.Forever;

    public void EncodeObject(BinaryWriter writer)
    {
        writer.Write(Unknown1);
        writer.Write(Unknown2);
        writer.Write(Unknown3);
        writer.Write(Unknown4);
        
        writer.Write(Unknown5.Length);
        foreach (var item in Unknown5)
        {
            writer.Write(item.Item1);
            writer.Write(item.Item2);
        }
        
        writer.WriteKRDateTime(AvailableTime);
    }
}