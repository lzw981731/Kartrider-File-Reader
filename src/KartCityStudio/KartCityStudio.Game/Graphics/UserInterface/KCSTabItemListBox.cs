using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartCityStudio.Game.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public partial class KCSTabItemListBox : TabItemListBox
    {
        public KCSTabItemListBox()
        {
            ItemsContainer.Padding = new MarginPadding {  };
        }

        protected override DrawableTabItem  CreateDrawableTabItem(TabItem item) => new KCSTabItem(item);

        protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new KCSScrollContainer<Drawable>(direction)
        {
            AutoHideScrollerBar = true,
            ScrollerSize = 1f,
        };
    }
}
