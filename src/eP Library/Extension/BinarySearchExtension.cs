namespace eP.Extension;

public static class BinarySearchExtension
{
    public static int AdvanceBinSearch<T>(this IList<T> list, T item,Comparison<T> comparer, BinarySearchMode searchMode)
    {
        if (list.Count == 0)
            return -1;

        int beginIndex = 0;
        int endIndex = list.Count - 1;

        while (beginIndex < (endIndex - 1))
        {
            int middleIndex = (beginIndex + endIndex) >> 1;
            int compareResult = comparer(list[middleIndex], item);
            if (compareResult < 0) // Middle < item
            {
                beginIndex = middleIndex;
            }
            else if (compareResult > 0) // Middle > item
            {
                endIndex = middleIndex;
            }
            else
            {
                if (searchMode == BinarySearchMode.Equal)
                {
                    return middleIndex;
                }
                else if (searchMode.HasFlag(BinarySearchMode.LessThan))
                {
                    endIndex = middleIndex;
                }
                else if (searchMode.HasFlag(BinarySearchMode.GreatThan))
                {
                    beginIndex = middleIndex;
                }
            }
        }

        int compareBeginResult = comparer(list[beginIndex], item);
        int compareEndResult = comparer(list[endIndex], item);

        if (searchMode == BinarySearchMode.Equal)
        {
            if (compareBeginResult == 0)
                return beginIndex;
            else if (compareEndResult == 0)
                return endIndex;
            else
                return -1;
        }
        else if (searchMode.HasFlag(BinarySearchMode.LessThan))
        {
            if (compareEndResult < 0 || (compareEndResult == 0 && searchMode.HasFlag(BinarySearchMode.Equal))) // A C B
                return endIndex;
            else if (compareBeginResult < 0|| (compareBeginResult == 0 && searchMode.HasFlag(BinarySearchMode.Equal)))  // A B C
                return beginIndex;
            else // B A C
                return -1;
        }
        else if (searchMode.HasFlag(BinarySearchMode.GreatThan))
        {
            if (compareBeginResult > 0 || (compareBeginResult == 0 && searchMode.HasFlag(BinarySearchMode.Equal))) // B A C
                return beginIndex;
            else if (compareEndResult > 0 || (compareEndResult == 0 && searchMode.HasFlag(BinarySearchMode.Equal)))  // A B C
                return endIndex;
            else // A C B
                return -1;
        }

        return -1;
    }
}

public enum BinarySearchMode
{
    Equal            = 1,
    LessThan         = 2,
    GreatThan = 4,
    LessThanOrEqual  = LessThan | Equal,
    GreatThanOrEqual  = GreatThan | Equal,
}