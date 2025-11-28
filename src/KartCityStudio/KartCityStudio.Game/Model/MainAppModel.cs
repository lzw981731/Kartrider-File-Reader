using System;
using System.Threading.Tasks;

namespace KartCityStudio.Game.Model;

public class MainAppModel
{
    public string Title { get; set; } = "";

    public event TitleUpdatedEventDelegate? TitleUpdated;

    public MainAppModel()
    {

    }
}

public delegate Task TitleUpdatedEventDelegate(string newTitle);
