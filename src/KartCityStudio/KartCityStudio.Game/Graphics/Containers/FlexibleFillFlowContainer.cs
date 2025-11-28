using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Framework.Logging;
using osuTK;

namespace KartCityStudio.Game.Graphics.Containers
{
    public partial class FlexibleFillFlowContainer<T> : FlowContainer<T> where T: Drawable
    {
        private readonly Container<T> contentContainer;

        protected override Container<T> Content => contentContainer;
        public Direction Direction { get; set; }

        public float MinAutoFillSize { get; set; } = 100f;

        public FlexibleFillFlowContainer(Direction direction)
        {
            Direction = direction;
        }

        protected override IEnumerable<Vector2> ComputeLayoutPositions()
        {
            return null;
        }

        protected override void UpdateAfterAutoSize()
        {
            base.UpdateAfterAutoSize();

        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {

            return true;
        }
    }
}
