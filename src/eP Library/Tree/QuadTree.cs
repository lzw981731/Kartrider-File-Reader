using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace eP.Tree;

/// <summary>
/// <see cref="QuadTree{T}"/> is a special version of quadtree. <br/>
/// Instead of original quadtree, <see cref="QuadTree{T}"/> will set a fixed range of position 
/// </summary>
/// <typeparam name="T"></typeparam>
public class QuadTree<T>
{
    public Vector2 MaxRectHalfSize { get; init; }
    public Vector2 InitialBasePoint { get; init; }
    public Vector2 MinRectHalfSize { get; init; }
    
    public QuadTreeNode<T> RootNode { get; init; }

    public QuadTree(Vector2 minRectHalfSize, Vector2 maxRectHalfSize, Vector2 initialBasePoint)
    {
        MinRectHalfSize = minRectHalfSize;
        MaxRectHalfSize = maxRectHalfSize;
        InitialBasePoint = initialBasePoint;

        RootNode = new QuadTreeNode<T>(this, null, QuadTreeNode<T>.QuadTreeNodeRole.BottomLeft, InitialBasePoint, MaxRectHalfSize);
    }
    
    public QuadTree(Vector2 minRectHalfSize, Vector2 maxRectHalfSize): 
        this(minRectHalfSize, maxRectHalfSize, Vector2.Zero)
    {
        
    }

    public void AddItem(Vector2 position, T item)
    {
        QuadTreeNode<T> node = RootNode;

        while (!node.IsLeaf)
            node = node.GetOrCreateNextNodeByPos(position);
        
        node.AddItem(position, item);
    }

    public bool TryModifyItemPosition(Vector2 oldPosition, Vector2 newPosition, T item)
    {
        QuadTreeNode<T>? searchNode = RootNode;
        while (!(searchNode?.IsLeaf ?? true))
            searchNode = searchNode?.GetNextNodeByPos(oldPosition);

        if (searchNode is null)
            return false;

        QuadTreeNode<T>.QuadTreeNodeItem? targetQuadItem = null;
        foreach (var quadItem in searchNode.Items)
        {
            if (quadItem.ItemPosition == oldPosition && (quadItem.Item?.Equals(item) ?? false))
            {
                targetQuadItem = quadItem;
                break;
            }
        }
        if (targetQuadItem is null)
            return false;

        targetQuadItem.InternalItemPosition = newPosition;
        if (!searchNode.IsPointInRange(newPosition))
        {
            searchNode.RemoveItemNoChk(targetQuadItem);
            
            while (!searchNode.IsPointInRange(newPosition) && searchNode.Parent is not null)
            {
                if (searchNode is { IsLeaf: true, Items.Count: 0 })
                {
                    QuadTreeNode<T> tmpParent = searchNode.Parent;
                    tmpParent.RemoveChildNodeNoChk((int)searchNode.NodeRole);
                    searchNode = tmpParent;
                }
                else
                {
                    searchNode = searchNode.Parent;
                }
            }

            while (!searchNode.IsLeaf)
                searchNode = searchNode.GetOrCreateNextNodeByPos(newPosition);
            
            searchNode.AddItem(targetQuadItem);
        }
        
        return true;
    }

    public bool TryRemoveItem(Vector2 position, T item)
    {
        QuadTreeNode<T>? searchNode = RootNode;
        while (!(searchNode?.IsLeaf ?? true))
            searchNode = searchNode?.GetNextNodeByPos(position);

        if (searchNode is null)
            return false;
        
        QuadTreeNode<T>.QuadTreeNodeItem? targetQuadItem = null;
        foreach (var quadItem in searchNode.Items)
        {
            if (quadItem.ItemPosition == position && (quadItem.Item?.Equals(item) ?? false))
            {
                targetQuadItem = quadItem;
                break;
            }
        }
        if (targetQuadItem is null)
            return false;
        
        searchNode.RemoveItemNoChk(targetQuadItem);
        
        while (searchNode.Parent is not null && searchNode is { IsLeaf: true, Items.Count: 0 })
        {
            QuadTreeNode<T> tmpParent = searchNode.Parent;
            tmpParent.RemoveChildNodeNoChk((int)searchNode.NodeRole);
            searchNode = tmpParent;
        }

        return true;
    }
    
    /// <summary>
    /// Get all items which its location is in given rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public IReadOnlyCollection<T> SelectRange(RectangleF rect)
    {
        Vector2 minPos = new Vector2(rect.X, rect.Y);
        Vector2 maxPos = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

        Queue<QuadTreeNode<T>> searchQueue = [];
        searchQueue.Enqueue(RootNode);

        List<T> output = [];

        while (searchQueue.Count > 0)
        {
            QuadTreeNode<T> topEle = searchQueue.Dequeue();
            if (!topEle.IsLeaf)
            {
                int xFlags =
                    (minPos.X <= topEle.BasePoint.X ? 0b01 : 0b00) |
                    (maxPos.X  > topEle.BasePoint.X ? 0b10 : 0b00) ;
                int yFlags =
                    (minPos.Y <= topEle.BasePoint.Y ? 0b01 : 0b00) |
                    (maxPos.Y  > topEle.BasePoint.Y ? 0b10 : 0b00) ;

                for (int i = 0; i < 4; i++)
                {
                    QuadTreeNode<T>? nextNode = topEle.Children[i];
                    if (nextNode is not null && ((xFlags >> (i & 0b01)) & 1) == 1 && ((yFlags >> ((i >> 1) & 0b1)) & 1) == 1)
                    {
                        searchQueue.Enqueue(nextNode);
                    }
                }
            }
            else
            {
                foreach (var item in topEle.Items)
                {
                    // if(rect.Contains(item.ItemPosition.X, item.ItemPosition.Y))
                    //     output.Add(item.Item);
                    if(RectContains(ref item.InternalItemPosition, ref minPos, ref maxPos))
                        output.Add(item.Item);
                }
            }
        }

        return output;
    }

    public void Clear()
    {
        Queue<QuadTreeNode<T>> queue = [];
        queue.Enqueue(RootNode);

        QuadTreeNode<T> curNode = RootNode;

        while (!RootNode.IsLeaf)
        {
            while(!curNode.IsLeaf)
                foreach(var child in curNode.Children)
                    if (child is not null)
                        curNode = child;

            QuadTreeNode<T>? tmpParent = curNode.Parent;
            tmpParent?.RemoveChildNodeNoChk((int) curNode.NodeRole);

            curNode = tmpParent ?? RootNode;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private unsafe bool RectContains(ref Vector2 pos, ref Vector2 min, ref Vector2 max)
    {
        // // v1: posX posY minX minY
        // // v2: maxX maxY posX posY
        // fixed (float* posPtr = &pos.X)
        // fixed (float* minPtr = &min.X)
        // fixed (float* maxPtr = &max.X)
        // {
        //     Vector128<float> v1 = Sse.LoadLow(new(), minPtr);
        //     v1 = Sse.LoadHigh(v1, posPtr);
        //     Vector128<float> v2 = Sse.MoveHighToLow(new(), v1);
        //     v2 = Sse.LoadHigh(v2, maxPtr);
        //     Vector128<byte> v3 = Sse.CompareLessThanOrEqual(v1, v2).AsByte();
        //     return (v3[0] & v3[4] & v3[8] & v3[12] & 1) == 1;
        // }

        return min.X <= pos.X && pos.X <= max.X && min.Y <= pos.Y && pos.Y <= max.Y;
    }
}