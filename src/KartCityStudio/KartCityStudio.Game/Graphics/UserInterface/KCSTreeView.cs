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
    public partial class KCSTreeView : TreeView
    {
        public KCSTreeView()
        {
            NodeFlowsContainer.Padding = new MarginPadding { Horizontal = 10, Vertical = 6 };
        }

        protected override DrawableTreeViewNode CreateDrawableTreeViewItem(int depth, TreeViewNode item) => new KCSTreeViewNode(depth, item);

        protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new KCSScrollContainer<Drawable>()
        {
            AutoHideScrollerBar = true
        };
    }
}
