using System.Collections.Immutable;
using System.Net.Security;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace eP.Tree;

public class QuadTreeNode<T>
{
    private const int TopRightNodeIdx    = 0b11;
    private const int TopLeftNodeIdx     = 0b10;
    private const int BottomLeftNodeIdx  = 0b00;
    private const int BottomRightNodeIdx = 0b01;
    
    internal readonly List<QuadTreeNodeItem> ItemContainer = [];
    
    private readonly QuadTreeNode<T>?[] _childContainer = new QuadTreeNode<T>[4];

    private Vector2 _minAllowPos;
    private Vector2 _maxAllowPos;
    private Vector2 _basePoint;
    private bool _requireSplit = false;
    
    public QuadTreeNode<T>? Parent { get; internal set; }
    
    public QuadTree<T> RootTree { get; init; }
    
    public QuadTreeNodeRole NodeRole { get; init; }

    public Vector2 BasePoint => _basePoint;
    
    public Vector2 RectHalfSize { get; init; }
    
    public QuadTreeNode<T>? TopLeftChild
    {
        get => _childContainer[TopLeftNodeIdx];
        internal set => _childContainer[TopLeftNodeIdx] = value;
    }
    
    public QuadTreeNode<T>? TopRightChild 
    {
        get => _childContainer[TopRightNodeIdx];
        internal set => _childContainer[TopRightNodeIdx] = value;
    }
    
    public QuadTreeNode<T>? BottomLeftChild 
    {
        get => _childContainer[BottomLeftNodeIdx];
        internal set => _childContainer[BottomLeftNodeIdx] = value;
    }
    
    public QuadTreeNode<T>? BottomRightChild 
    {
        get => _childContainer[BottomRightNodeIdx];
        internal set => _childContainer[BottomRightNodeIdx] = value;
    }

    public bool HasTopLeft => _childContainer[TopLeftNodeIdx] is not null;
    
    public bool HasTopRight => _childContainer[TopRightNodeIdx] is not null;
    
    public bool HasBottomLeft => _childContainer[BottomLeftNodeIdx] is not null;
    
    public bool HasBottomRight => _childContainer[BottomRightNodeIdx] is not null;

    public bool IsLeaf => !(HasTopLeft || HasTopRight || HasBottomLeft || HasBottomRight);

    public IReadOnlyList<QuadTreeNode<T>?> Children => _childContainer;

    public IReadOnlyList<QuadTreeNodeItem> Items => ItemContainer;
    
    public QuadTreeNode(QuadTree<T> rootTree, QuadTreeNode<T>? parent, QuadTreeNodeRole nodeRole, Vector2 basePoint, Vector2 rectHalfSize)
    {
        RootTree = rootTree;
        Parent = parent;
        NodeRole = nodeRole;
        _basePoint = basePoint;
        RectHalfSize = rectHalfSize;

        _minAllowPos = basePoint - rootTree.MinRectHalfSize;
        _maxAllowPos = basePoint + rootTree.MinRectHalfSize;
    }
    
    public bool IsPointInRange(Vector2 point)
    {
        return (Math.Abs(point.X - BasePoint.X) <= RectHalfSize.X) &&
               (Math.Abs(point.Y - BasePoint.Y) <= RectHalfSize.Y);
    }

    internal bool RequireSplit() => _requireSplit; 
    
    internal bool GetRequireSplit(Vector2 point)
    {
        return
            ItemContainer.Count > 4 &&
            (point.X < _minAllowPos.X || point.X > _maxAllowPos.X ||
            point.Y < _minAllowPos.Y || point.Y > _maxAllowPos.Y);
    }

    internal void AddItem(Vector2 position, T item)
    {
        AddItem(new QuadTreeNodeItem(position, item));
    }
    
    internal void AddItem(QuadTreeNodeItem item)
    {
        if (!IsPointInRange(item.ItemPosition))
            throw new Exception("Position not in this node.");
        
        if (!IsLeaf)
            throw new Exception($"{nameof(AddItem)} method should call in the node that is leaf.");
        
        AddItemNoChk(item);

        if (_requireSplit)
        {
            Queue<QuadTreeNode<T>> splitQueue = [];
            splitQueue.Enqueue(this);
            while (splitQueue.Count > 0)
            {
                QuadTreeNode<T> queueItem = splitQueue.Dequeue();
                if (queueItem._requireSplit)
                {
                    queueItem.SplitNodeNoChk();
                    foreach(var child in queueItem.Children)
                        if(child is not null)
                            splitQueue.Enqueue(child);
                }
            }
        }
    }
    
    /// <summary>
    /// Add item without check position.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="item"></param>
    internal void AddItemNoChk(Vector2 position, T item)
    {
        ItemContainer.Add(new (position, item));
        
        _requireSplit = GetRequireSplit(position);
    }
    
    /// <summary>
    /// Add item without check position.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="item"></param>
    internal void AddItemNoChk(QuadTreeNodeItem item)
    {
        ItemContainer.Add(item);
        
        _requireSplit = GetRequireSplit(item.ItemPosition);
    }

    internal void RemoveItemNoChk(QuadTreeNodeItem item)
    {
        ItemContainer.Remove(item);
    }

    internal QuadTreeNode<T> CreateChildNodeNoChk(int nodeIdx)
    {
        Vector2 childRectHalfSize = RectHalfSize / 2;
        Vector2 childBasePoint = BasePoint;
        if ((nodeIdx & 0b01) == 0)
            childBasePoint.X -= childRectHalfSize.X;
        else
            childBasePoint.X += childRectHalfSize.X;
        
        if ((nodeIdx & 0b10) == 0)
            childBasePoint.Y -= childRectHalfSize.Y;
        else
            childBasePoint.Y += childRectHalfSize.Y;
        return new QuadTreeNode<T>(RootTree, this, (QuadTreeNodeRole) nodeIdx, childBasePoint, childRectHalfSize);
    }

    internal void RemoveChildNodeNoChk(int nodeIdx)
    {
        _childContainer[nodeIdx]?.DisposeNoChild();
        _childContainer[nodeIdx] = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal unsafe int GetNodeIdxByPos(Vector2 pos)
    {
        // fixed (float* ptr = &_basePoint.X)
        // {
        //     Vector128<float> v1 = Sse.LoadLow(new(), &pos.X);
        //     Vector128<float> v2 = Sse.LoadLow(new(), ptr);
        //     v1 = Sse.CompareGreaterThan(v1, v2);
        //     return (v1.AsInt32()[1] & 0b10) | (v1.AsInt32()[0] & 0b01);
        // }
        //
        //
        //
        //
        return  (pos.X <= BasePoint.X ? 0b00 : 0b01) | 
                (pos.Y <= BasePoint.Y ? 0b00 : 0b10) ;
    }
        

    internal QuadTreeNode<T>? GetNextNodeByPos(Vector2 pos)
    {
        int nodeIdx = GetNodeIdxByPos(pos);
        return _childContainer[nodeIdx];
    }
    
    internal QuadTreeNode<T> GetOrCreateNextNodeByPos(Vector2 pos)
    {
        int nodeIdx = GetNodeIdxByPos(pos);
        return _childContainer[nodeIdx] ??= CreateChildNodeNoChk(nodeIdx);
    }
    
    internal void SplitNodeNoChk()
    {
        foreach (var item in ItemContainer)
        {
            GetOrCreateNextNodeByPos(item.ItemPosition).AddItemNoChk(item);
        }
        
        ItemContainer.Clear();
        _requireSplit = false;
    }
    
    internal void DisposeNoChild()
    {
        Parent = null;
        ItemContainer.Clear();
    }

    public string FullDump()
    {
        string[] nodeRoleStr = ["BtmLt", "BtmRt", "TopLt", "TopRt"];
        
        Stack<(QuadTreeNode<T> Node, int Level, int Iter)> stack = [];
        stack.Push((this, 1, 0));

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("+-- Root[1]");
        
        while (stack.Count > 0)
        {
            var topEle = stack.Pop();
            if (!topEle.Node.IsLeaf && topEle.Iter < 4)
            {
                QuadTreeNode<T>? nextNode = topEle.Node.Children[topEle.Iter++];
                
                for(int i = 0; i < topEle.Level; i++)
                    stringBuilder.Append("|    ");
                stringBuilder.AppendLine($"+-- {nodeRoleStr[topEle.Iter - 1]}[{(nextNode is not null ? topEle.Level.ToString() : "x")}]");
                
                stack.Push(topEle);
                if (nextNode is not null)
                    stack.Push((nextNode, topEle.Level + 1, 0));
            }
            else if(topEle.Node.IsLeaf)
            {
                foreach (var nodeItem in topEle.Node.Items)
                {
                    for(int i = 0; i < topEle.Level; i++)
                        stringBuilder.Append("|    ");
                    stringBuilder.AppendLine($"--- Item[{nodeItem.ItemPosition.X:.000,10}, {nodeItem.ItemPosition.Y:.000,10}]");

                    string itemStr = nodeItem.Item?.ToString() ?? "(NULL)";
                    string[] itemStrLines = itemStr.Split(Environment.NewLine);

                    foreach (var itemStrLine in itemStrLines)
                    {
                        for(int i = 0; i <= topEle.Level; i++)
                            stringBuilder.Append("|    ");
                        stringBuilder.AppendLine($"  # {itemStrLine}");
                    }
                }
            }
        }
        
        return stringBuilder.ToString();
    }

    public class QuadTreeNodeItem(Vector2 itemPosition, T item)
    {
        internal Vector2 InternalItemPosition = itemPosition;
        
        public Vector2 ItemPosition  => InternalItemPosition;
        
        public T Item { get; internal set; } = item;
    }

    public enum QuadTreeNodeRole
    {
        BottomLeft  = BottomLeftNodeIdx,
        BottomRight = BottomRightNodeIdx,
        TopLeft     = TopLeftNodeIdx,
        TopRight    = TopRightNodeIdx,
    }
}