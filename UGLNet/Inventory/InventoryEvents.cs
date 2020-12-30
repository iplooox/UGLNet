using System;
using UGLNet.Interfaces.Inventory;

namespace UGLNet.Events.Inventory
{
    public class BaseEvent : EventArgs
    {
        public bool Successful { get; set; }
    }

    public class ItemAddedEvent<T> : BaseEvent where T : IItem
    {
        public ItemAddedEvent(T item, int[] updatedIndexes, bool successful)
        {
            Item = item;
            Successful = successful;
            UpdatedIndexes = updatedIndexes;
        }

        public T Item { get; set; }
        public int[] UpdatedIndexes { get; set; }
    }

    public class ItemRemovedEvent : BaseEvent
    {
        public ItemRemovedEvent(string id, int quantity, int[] indexes, bool successful)
        {
            Id = id;
            Quantity = quantity;
            Successful = successful;
            UpdatedIndexes = indexes;
        }

        public string Id { get; set; }
        public int Quantity { get; set; }
        public int[] UpdatedIndexes { get; set; }
    }

    public class ItemDestroyedEvent : BaseEvent
    {
        public ItemDestroyedEvent(int index, bool successful)
        {
            Index = index;
            Successful = successful;
        }

        public int Index { get; set; }
    }

    public class InventorySizeChangedEvent : BaseEvent
    {
        public InventorySizeChangedEvent(int size, bool successful)
        {
            Size = size;
            Successful = successful;
        }

        public int Size { get; set; }
    }

    public class ItemMovedEvent : BaseEvent
    {
        public ItemMovedEvent(int oldIndex, int newIndex, bool successful)
        {
            Successful = successful;
            UpdatedIndexes = new int[2] { oldIndex, newIndex };
        }

        public int[] UpdatedIndexes { get; set; }
    }

    public class ItemSplittedEvent : BaseEvent
    {
        public ItemSplittedEvent(int[] updatedIndexes, bool successful)
        {
            Successful = successful;
            UpdatedIndexes = updatedIndexes;
        }

        public int[] UpdatedIndexes { get; set; }
    }

    public class ItemMergedEvent : BaseEvent
    {
        public ItemMergedEvent(int[] updatedIndexes, bool successful)
        {
            Successful = successful;
            UpdatedIndexes = updatedIndexes;
        }

        public int[] UpdatedIndexes { get; set; }
    }
}
