namespace KartCityStudio.Game.Model;

public interface IArchiveElement
{
    string Name { get; }

    string FullName { get; }

    IArchiveFolder? Parent { get; }
}