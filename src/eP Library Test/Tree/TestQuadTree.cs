using System.Drawing;
using System.Numerics;
using eP.Command;
using eP.Testing;
using eP.Tree;

namespace ePLibraryTest.Tree;

public class TestQuadTree: TestStage
{
    private Vector2 _minHalfSize = new Vector2(1, 1); 
    private Vector2 _maxHalfSize = new Vector2(5120, 5120);
    private Vector2 _testVec;
    private Vector2 _testVecPos;
    private RectangleF _testRange = new RectangleF(300, -513, 590, 262);
    private QuadTree<Vector2> _testTree;
    private Vector2[] _testVecArray;
    private Vector2[] _testVecArray2;

    private int _searchLoopCount = 120000;
    
    private int _testCaseCount = 500000;
    
    public TestQuadTree()
    {
        Random random = new Random(0);
        _testTree = new QuadTree<Vector2>(_minHalfSize, _maxHalfSize);
        _testVecArray = 
        [
            ..Enumerable.Range(1, _testCaseCount).Select(x => new Vector2
            (
                (random.NextSingle() - 0.5f) * _maxHalfSize.X * 2,
                (random.NextSingle() - 0.5f) * _maxHalfSize.Y * 2
            ))
        ];
        _testVecArray2 = 
        [
            ..Enumerable.Range(1, _testCaseCount).Select(x => new Vector2
            (
                (random.NextSingle() - 0.5f) * _maxHalfSize.X * 2,
                (random.NextSingle() - 0.5f) * _maxHalfSize.Y * 2
            ))
        ];
        _testVecPos = _testVec = random.GetItems(_testVecArray, 1).First();

        AddStep("Test insert speed", StepQuadTreeInsert);
        AddStep("Test modify speed", StepModifySpeed);
        // AddStep("Test norm search", StepNormalSearch);
        // AddStep("Test quad search", StepQuadTreeSearch);
        AddStep("Remove", StepRemove);
        AddStep("Clear", StepQuadTreeClear);
    }

    private void StepNormalSearch()
    {
        for (int i = 0; i < _searchLoopCount; i++)
        {
            Vector2[] answer = _testVecArray.Where(x => _testRange.Contains(x.X, x.Y)).ToArray();
        }
    }
    
    private void StepQuadTreeSearch()
    {
        for (int i = 0; i < _searchLoopCount; i++)
        {
            var searchResult = _testTree.SelectRange(_testRange);
        }
    }

    private void StepQuadTreeInsert()
    {
        foreach(var vec in _testVecArray)
            _testTree.AddItem(vec, vec);
    }
    
    private void StepQuadTreeClear()
    {
        List<QuadTreeNode<Vector2>> test = [_testTree.RootNode];
        for (int i = 0; i < test.Count; i++)
        {
            QuadTreeNode<Vector2> topEle = test[i];
            foreach(var child in topEle.Children)
                if(child is not null)
                    test.Add(child);
        }
        _testTree.Clear();
    }

    private void StepModifySpeed()
    {
        foreach (var newVec in _testVecArray2)
        {
            bool result = _testTree.TryModifyItemPosition(_testVecPos, newVec, _testVec);
            if(result)
                _testVecPos = newVec;
        }
    }
    
    private void StepRemove()
    {
        bool result = _testTree.TryRemoveItem(_testVecPos, _testVec);
        if (!result)
            throw new Exception("CAN'T REMOVE!");
    }
    
    [Command("dumpTree")]
    private CommandExecuteResult CommandDumpTree(IConsole console, CommandArgumentQueue argumentQueue)
    {
        console.Write(_testTree.RootNode.ToString());
        return new CommandExecuteResult(ResultType.Success, "");
    }
}