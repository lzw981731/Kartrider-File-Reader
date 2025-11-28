using System;
using System.Collections.Generic;
using KartCityStudio.Game.Input.Events;

namespace KartCityStudio.Game.Service;

public class WindowEventService
{
    private LinkedList<Func<DragDropEvent, bool>> eventChain = new LinkedList<Func<DragDropEvent, bool>>();

    public void OnDropDrag(DragDropEvent e)
    {
        foreach (var callback in eventChain)
        {
            bool result = callback?.Invoke(e) ?? false;
            if (result)
                return;
        }
    }

    public DragDropEventChainHandle RegisterDropDragEvent(Func<DragDropEvent, bool> callback)
    {
        var node = eventChain.AddLast(callback);
        return new DragDropEventChainHandle(node);
    }

    public void UnregisterDropDragEvent(DragDropEventChainHandle handle)
    {
        eventChain.Remove(handle.baseNode);
    }
}

public class DragDropEventChainHandle
{
    internal LinkedListNode<Func<DragDropEvent, bool>> baseNode;

    internal DragDropEventChainHandle(LinkedListNode<Func<DragDropEvent, bool>> node)
    {
        this.baseNode = node;
    }
}
