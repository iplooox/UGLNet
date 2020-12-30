using System;
using System.Linq;
using UGLNet.Events.Inventory;
using UGLNet.Interfaces.Inventory;

namespace UGLNet
{
    public class Inventory<T> : BaseInventory<T> where T : IItem
    {
        /// <summary>
        /// Triggers when isntance of the item has been added to inventory.
        /// </summary>
        public EventHandler<ItemAddedEvent<T>> OnItemAdded { get; set; }
        /// <summary>
        /// Triggers when instance of the item has been removed (quantity decreased).
        /// </summary>
        public EventHandler<ItemRemovedEvent> OnItemRemoved { get; set; }
        /// <summary>
        /// Triggers when instance of the item has been destroyed from a given slot.
        /// </summary>
        public EventHandler<ItemDestroyedEvent> OnItemDestroyed { get; set; }
        /// <summary>
        /// Triggers when inventory size has been changed.
        /// </summary>
        public EventHandler<InventorySizeChangedEvent> OnInventorySizeChanged { get; set; }
        /// <summary>
        /// Triggers when item is moved in inventory.
        /// </summary>
        public EventHandler<ItemMovedEvent> OnItemMoved { get; set; }
        /// <summary>
        /// Triggers when item is splitted in inventory.
        /// </summary>
        public EventHandler<ItemSplittedEvent> OnItemSplitted { get; set; }
        /// <summary>
        /// Triggers when item is merged in inventory.
        /// </summary>
        public EventHandler<ItemMergedEvent> OnItemMerged { get; set; }

        /// <summary>
        /// Create instance of inventory
        /// </summary>
        /// <param name="inventorySize">Defines size of items array</param>
        public Inventory(int inventorySize) : base(inventorySize)
        {
        }

        /// <summary>
        /// Adds item to inventory, if maxQuantity is bigger then 1 it will try to stack items in same instance.
        /// </summary>
        /// <remarks>
        /// Trigger OnItemAdded event.
        /// </remarks>
        /// <param name="item">Item you wish to add, has to implement IItem interface</param>
        /// <returns>Return true if the item has been added</returns>
        public new bool AddItem(T item)
        {
            var (result, updatedIndexes) = base.AddItem(item);

            OnItemAdded?.Invoke(this, new ItemAddedEvent<T>(item, updatedIndexes, result));

            return result;
        }

        /// <summary>
        /// Removes item from inventory, if maxQuantity is bigger then 1 it will try to find instance of item and decrease quantity instead.
        /// </summary>
        /// <remarks>
        /// The removal is happening from decreasing order, meaning the last item in inventory will be removed first.
        /// Trigger OnItemRemoved event and OnItemDestroyed if the given item was competly removed from inventory.
        /// </remarks>
        /// <param name="item">Item you wish to remove, has to implement IItem</param>
        /// <returns>Return true if the item has been removed, false if there is not enought quantity in inventory to be removed</returns>
        public new bool RemoveItem(T item) => item == null ? false : RemoveItem(item.Id, item.Quantity);

        /// <summary>
        /// Removes item from inventory, if maxQuantity is bigger then 1 it will try to find instance of item and decrease quantity instead.
        /// </summary>
        /// <remarks>
        /// The removal is happening from decreasing order, meaning the last item in inventory will be removed first.
        /// Trigger OnItemRemoved event and OnItemDestroyed if the given item was competly removed from inventory.
        /// </remarks>
        /// <param name="Id">Id of the item you wish to remove</param>
        /// <param name="quantity">Quantity to be removed</param>
        /// <returns>Return true if the item has been removed, false if there is not enought quantity in inventory to be removed</returns>
        public new bool RemoveItem(string Id, int quantity)
        {
            var (result, updatedIndexes, destroyedIndexes) = base.RemoveItem(Id, quantity);

            // If there is none instances of item left, the inventory will fire ItemDestroyedEvent on each instance that existed before calling remove.
            if (destroyedIndexes.Count != 0)
            {
                foreach (var destroyedIndex in destroyedIndexes)
                {
                    OnItemDestroyed?.Invoke(this, new ItemDestroyedEvent(destroyedIndex, result));
                }
            }

            int[] diffIndexes = updatedIndexes.Union(destroyedIndexes).ToArray();

            OnItemRemoved?.Invoke(this, new ItemRemovedEvent(Id, quantity, diffIndexes, result));
            return result;
        }

        /// <summary>
        /// Destroys instance at gived index, disregards quantity.
        /// </summary>
        /// <remarks>
        /// Can be used for example when player wants to throw item out of the inventory.
        /// Trigger OnItemDestroyed event.
        /// </remarks>
        /// <param name="index">Index the item is positioned at.</param>
        /// <returns>Returns true if the index was in bounds of the items array.</returns>

        public new bool DestroyItem(int index)
        {
            bool result = base.DestroyItem(index);

            OnItemDestroyed?.Invoke(this, new ItemDestroyedEvent(index, result));
            return result;
        }

        /// <summary>
        /// Used to change size of the inventory.
        /// </summary>
        /// <remarks>
        /// It will move items around in order to contain them in smaller size inventory.
        /// Triggers OnInventorySizeChanged event.
        /// </remarks>
        /// <param name="size">New size of the inventory</param>
        /// <returns>Return true if there was enought empty spaces to contain existing items, return false if there is not enought space.</returns>
        public new bool ChangeSizeOfInventory(int size)
        {
            bool result = base.ChangeSizeOfInventory(size);

            OnInventorySizeChanged?.Invoke(this, new InventorySizeChangedEvent(size, result));
            return result;
        }

        /// <summary>
        /// Move item to given index.
        /// </summary>
        /// <remarks>
        /// If there is item at given index it will swap them around.
        /// Triggers OnItemMoved event.
        /// </remarks>
        /// <param name="oldIndex">Index the given item has to be moved from.</param>
        /// <param name="newIndex">Index the given item has to be moved to.</param>
        /// <returns>Return true if provided indexes was in bound of the array, and move was successful.</returns>
        public new bool MoveItemToIndex(int oldIndex, int newIndex)
        {
            bool result = base.MoveItemToIndex(oldIndex, newIndex);

            OnItemMoved?.Invoke(this, new ItemMovedEvent(oldIndex, newIndex, result));
            return result;
        }
        /// <summary>
        /// Split instance of item into 2, moving quantity to second instance.
        /// </summary>
        /// <param name="index">Index of item to split</param>
        /// <param name="quantityToSplit">Quantity to split</param>
        /// <returns>Return true if action was successfull, returns false if action failed.</returns>
        public new bool SplitItem(int index, int quantityToSplit)
        {
            var (result, newInstanceIndex) = base.SplitItem(index, quantityToSplit);

            OnItemSplitted?.Invoke(this, new ItemSplittedEvent(result ? new int[2] { index, newInstanceIndex.Value } : null, result));

            return result;
        }
        /// <summary>
        /// Merge 2 instances of item into 1 slot, will just move quantity to max of an item, if there is too much.
        /// </summary>
        /// <param name="fromIndex">Index of item to merge and destroy.</param>
        /// <param name="toIndex">Index of item to merge item into.</param>
        /// <returns>Returns true if action was successfull.</returns>
        public new bool Mergeitem(int fromIndex, int toIndex)
        {
            bool result = base.Mergeitem(fromIndex, toIndex);

            OnItemMerged?.Invoke(this, new ItemMergedEvent(result ? new int[2] { fromIndex, toIndex } : null, result));

            return result;
        }
    }
}
