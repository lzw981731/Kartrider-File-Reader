using System.Linq;
using KartCityStudio.Game.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace KartCityStudio.Game.Graphics.UserInterface;

public partial class KCSFormattableText: FillFlowContainer<SpriteText>
{
    private string text = "";

    private readonly FormattableTextParser parser;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            Scheduler.AddOnce(updateText);
        }
    }

    public StylizedText[] StylizedTexts
    {
        set => updateStylizedText(value);
    }

    public KCSFormattableText(FormattableTextParser? parser = null)
    {
        this.Direction = FillDirection.Horizontal;
        this.parser = parser ?? new FormattableTextParser()
        {

        };
    }

    private void updateText()
    {
        this.Clear();
        StylizedText[][] sylizedTextLines = parser.Parse(text);
        updateStylizedText(sylizedTextLines.FirstOrDefault([]));
    }

    private void updateStylizedText(StylizedText[] stylizedTexts)
    {
        foreach (StylizedText stylizedText in stylizedTexts)
        {
            SpriteText spriteText = new SpriteText()
            {
                Text = stylizedText.Text,
                Font = KCSFont.DefaultMono.With(size: 18),
            };
            if (stylizedText.Size is not null)
                spriteText.Font = spriteText.Font.With(size: stylizedText.Size);
            if (stylizedText.Colour is not null)
                spriteText.Colour = stylizedText.Colour.Value;
            this.Add(spriteText);
        }
    }
}
