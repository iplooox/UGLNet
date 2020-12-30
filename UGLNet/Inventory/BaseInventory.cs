using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UGLNet.Interfaces;
using UGLNet.Interfaces.Inventory;

namespace UGLNet
{
    public class BaseInventory<T> where T : IItem
    {
        T[] _items = new T[0];

        /// <summary>
        /// Return a clone of internal items, as manipulating items inside inventory would not fire events.
        /// </summary>
        /// <remarks>
        /// Use UnsafeItems if you wish to do so anyways.
        /// </remarks>
        public T[] Items { get => (T[])_items.Clone(); }
        //public T[] Items { get => _items; }
        /// <summary>
        /// Returns internal array of items, use at your own risk.
        /// </summary>
        public T[] UnsafeItems { get => _items; }

        /// <summary>
        /// Returns size of the internal array of items
        /// </summary>
        public int Size { get => _items.Length; }

        /// <summary>
        /// Constructor for the base inventory
        /// </summary>
        /// <param name="inventorySize">Defines size of items array</param>
        public BaseInventory(int inventorySize)
        {
            _items = new T[inventorySize];
        }

        #region AddItem
        /// <summary>
        /// Adds item to inventory, if maxQuantity is bigger then 1 it will try to stack items in same instance.
        /// </summary>
        /// <param name="item">Item you wish to add, has to implement IItem interface</param>
        /// <returns>Return true if the item has been added. UpdatedIndexes defines all indexes that has been modified</returns>
        public (bool success, int[] updatedIndexes) AddItem(T item)
        {
            if (!CanAddItem(item))
            {
                return (false, null);
            }

            int? emptyIndex = _GetEmptyIndex();

            // Find all indexes of the item.
            int[] itemIndexes = _FindItemIndexes(item);

            if (itemIndexes.Length == 0 && emptyIndex != null)
            {
                _AddItem(item, emptyIndex); // None instances found, but there is still avaiable slots in the inventory.
                return (true, new int[1] { emptyIndex.Value });
            }

            Dictionary<int, T> itemsDictionary = _GetItemsWithIndexesDictionary(itemIndexes);

            return (true, _DistributeItems(item, itemsDictionary).ToArray());
        }

        /// <summary>
        /// Validate if given item can be added to inventory.
        /// </summary>
        /// <param name="item">Item you wish to add, has to implement IItem</param>
        /// <returns>Return true if the item can be added</returns>
        public bool CanAddItem(T item) => item == null ? false : CanAddItem(item.Id, item.Quantity, item.MaxQuantity);
        /// <summary>
        /// Validate if given item can be added to inventory.
        /// </summary>
        /// <param name="item">Item you wish to add, has to implement IItem</param>
        /// <returns>Return true if the item can be added</returns>
        public bool CanAddItem(string Id, int Quantity, int MaxQuantity)
        {
            if (Id == null || Quantity == 0)
            {
                return false;
            }

            var emptyIdexes = _GetEmptyIndexes();

            T[] items = _GetItems(x => x?.Id == Id);

            int quantityPossibleToAdd = items.Select(x => x.MaxQuantity - x.Quantity).Sum() + (emptyIdexes.Count() * MaxQuantity);

            if (quantityPossibleToAdd >= Quantity)
                return true;

            return false;
        }

        #endregion

        #region RemoveItem
        /// <summary>
        /// Removes item from inventory, if maxQuantity is bigger then 1 it will try to find instance of item and decrease quantity instead.
        /// </summary>
        /// <remarks>
        /// The removal is happening from decreasing order, meaning the last item in inventory will be removed first.
        /// </remarks>
        /// <param name="item">Item you wish to remove, has to implement IItem</param>
        /// <returns>Return true if the item has been removed, false if there is not enought quantity in inventory to be removed</returns>
        public (bool success, List<int> updatedIndexes, List<int> destroyedIndexes) RemoveItem(T item) => item == null ? (false, null, null) : RemoveItem(item.Id, item.Quantity);
        /// <summary>
        /// Removes item from inventory, if maxQuantity is bigger then 1 it will try to find instance of item and decrease quantity instead.
        /// </summary>
        /// <remarks>
        /// The removal is happening from decreasing order, meaning the last item in inventory will be removed first.
        /// </remarks>
        /// <param name="Id">Id of the item you wish to remove</param>
        /// <param name="quantity">Quantity to be removed</param>
        /// <returns>Return true if the item has been removed, false if there is not enought quantity in inventory to be removed.
        /// updatedIndexes for indexes that quantity has been updated, destroyedIndexes for indexes that has been entirely removed.
        /// </returns>
        public (bool success, List<int> updatedIndexes, List<int> destroyedIndexes) RemoveItem(string Id, int quantity)
        {
            if (!CanRemoveItem(Id, quantity))
            {
                return (false, null, null);
            }

            Dictionary<int, T> itemsWithKeys = _GetItemsWithIndexesDictionary(x => x?.Id == Id);

            int quantityLeftToRemove = quantity;

            List<int> updatedIndexes = new List<int>();
            List<int> destroyedIndexes = new List<int>();

            // Ordering list in order to remove the last items in array to be removed first.
            foreach (var itemWithKey in itemsWithKeys.OrderByDescending(x => x.Key))
            {
                T _item = itemWithKey.Value;

                // The entire item can be removed.
                if (_item.Quantity <= quantityLeftToRemove)
                {
                    quantityLeftToRemove -= _item.Quantity;
                    _RemoveItem(itemWithKey.Key);
                    destroyedIndexes.Add(itemWithKey.Key);
                }
                else
                {
                    // Only quantity has to be removed, as instance has enought items to stay.
                    _item.Quantity -= quantityLeftToRemove;
                    quantityLeftToRemove = 0;
                    updatedIndexes.Add(itemWithKey.Key);
                }

                if (quantityLeftToRemove == 0)
                {
                    break;
                }
            }

            return (true, updatedIndexes, destroyedIndexes);
        }

        /// <summary>
        /// Checks if there is enought quantity of item in inventory to be removed.
        /// </summary>
        /// <param name="item">Item you wish to remove, has to implement IItem</param>
        /// <returns>Return true if there is enought quantity of item in the inventory to be removed</returns>
        public bool CanRemoveItem(T item) => item == null ? false : CanRemoveItem(item.Id, item.Quantity);
        /// <summary>
        /// Checks if there is enought quantity of item in inventory to be removed.
        /// </summary>
        /// <param name="Id">Id of the item you wish to remove</param>
        /// <param name="quantity">Quantity to be removed</param>
        /// <returns>Return true if there is enought quantity of item in the inventory to be removed</returns>
        public bool CanRemoveItem(string Id, int quantity)
        {
            if (Id == null)
            {
                return false;
            }

            Dictionary<int, T> itemsWithKeys = _GetItemsWithIndexesDictionary(x => x?.Id == Id);

            int sumOfItems = itemsWithKeys.Sum(x => x.Value.Quantity);

            if (sumOfItems < quantity)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DestroyItem
        /// <summary>
        /// Destroys instance at gived index, disregards quantity.
        /// </summary>
        /// <remarks>
        /// Can be used for example when player wants to throw item out of the inventory.
        /// </remarks>
        /// <param name="index">Index the item is positioned at.</param>
        /// <returns>Returns true if the index was in bounds of the items array.</returns>
        public bool DestroyItem(int index)
        {
            if (_WrongIndex(index))
            {
                return false;
            }

            _items[index] = default(T);

            return true;
        }

        #endregion

        #region ChangeSizeOfInventory
        /// <summary>
        /// Used to change size of the inventory
        /// </summary>
        /// <remarks>
        /// It will move items around in order to contain them in smaller size inventory.
        /// </remarks>
        /// <param name="size">New size of the inventory</param>
        /// <returns>Return true if there was enought empty spaces to contain existing items, return false if there is not enought space.</returns>
        public bool ChangeSizeOfInventory(int size)
        {
            if (!CanChangeSizeOfInventory(size))
            {
                return false;
            }

            if (size > _items.Length)
            {
                // Just resize the array, as it's bigger then previously.
                Array.Resize(ref _items, size);
                return true;
            }

            T[] newItems = new T[size];

            Dictionary<int, T> itemWithKeyDictionary = _GetItemsWithIndexesDictionary(x => x != null);
            T[] itemsInInventoryWithIndex = itemWithKeyDictionary.OrderBy(x => x.Key).Select(x => x.Value).ToArray();

            // Copy items one by one to array, in same order. To Ensure the items won't get mixed indexes and confusing player.
            for (int i = 0; i < itemWithKeyDictionary.Count; i++)
            {
                newItems[i] = itemsInInventoryWithIndex[i];
            }

            // Replace old inventory with new one.
            _items = newItems;

            return true;
        }

        /// <summary>
        /// Used to check if there is enougt empty spaces to resize inventory to given size.
        /// </summary>
        /// <param name="size">New size of the inventory</param>
        /// <returns>Return true if the inventory can be resized, return false if inventory cannot be changed.</returns>
        public bool CanChangeSizeOfInventory(int size) => size >= (_items.Length - _GetEmptyIndexes().Count());

        #endregion

        #region MoveItemToIndex
        /// <summary>
        /// Move item to given index
        /// </summary>
        /// <remarks>
        /// If there is item at given index it will swap them around.
        /// </remarks>
        /// <param name="oldIndex">Index the given item has to be moved from</param>
        /// <param name="newIndex">Index the given item has to be moved to</param>
        /// <returns>Return true if provided indexes was in bound of the array, and move was successful</returns>
        public bool MoveItemToIndex(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex || _WrongIndex(oldIndex) || _WrongIndex(newIndex))
            {
                return false;
            }

            if (_items[newIndex] == null)
            {
                // If there is none items in the new slot just move it.
                _items[newIndex] = _items[oldIndex];

                // Clear the old slot.
                _items[oldIndex] = default(T);
                return true;
            }

            _SwapItems(oldIndex, newIndex);

            return true;
        }

        #endregion

        #region SplitItem
        public (bool success, int? newIndex) SplitItem(int index, int quantityToSplit)
        {
            // General validation if item can get splitted.
            if (_WrongIndex(index) || _items[index] == null || _items[index].Quantity == 1)
            {
                return (false, null);
            }


            if (quantityToSplit == 0)
            {
                return (false, null);
            }

            // Validation if the quantity fits and doesn't take entire slot.
            if (_items[index].MaxQuantity <= quantityToSplit || _items[index].Quantity <= quantityToSplit)
            {
                return (false, null);
            }

            int? emptyIndex = _GetEmptyIndex();

            // Validation if there is enought spaces left in the inventory to split it.
            if (!emptyIndex.HasValue)
            {
                return (false, null);
            }

            T newItem = (T)_items[index].Clone();
            _items[index].Quantity -= quantityToSplit;

            newItem.Quantity = quantityToSplit;
            _items[emptyIndex.Value] = newItem;

            return (true, emptyIndex);
        }
        #endregion

        #region MergeItem
        public bool Mergeitem(int fromIndex, int toIndex)
        {
            // Item is null or index is out of bounds.
            if (_WrongIndex(fromIndex) || _WrongIndex(toIndex) || _items[fromIndex] == null && _items[toIndex] == null)
            {
                return false;
            }

            // The item is not the same.
            if (!_CompareItem(_items[fromIndex], _items[toIndex]))
            {
                return false;
            }

            // Items are not stackable.
            if (_items[fromIndex].MaxQuantity == 1)
            {
                return false;
            }

            // The received already has maximum quantity.
            if (_items[toIndex].Quantity == _items[toIndex].MaxQuantity)
            {
                return false;
            }

            //int totalQuantity = _items[fromIndex].Quantity + _items[toIndex].Quantity;
            int quantityToMove = _items[fromIndex].Quantity;
            int possibleToAdd = _items[toIndex].MaxQuantity - _items[toIndex].Quantity;

            if (quantityToMove <= possibleToAdd)
            {
                _items[toIndex].Quantity += quantityToMove;
                _RemoveItem(fromIndex);
            }
            else
            {
                _items[toIndex].Quantity += possibleToAdd;
                _items[fromIndex].Quantity -= possibleToAdd;
            }

            return true;
        }
        #endregion

        #region GetIndexesWithItemDictionary
        /// <summary>
        /// Used for retrieving index and item of given id.
        /// </summary>
        /// <param name="Id">Id of the item you want to filter for.</param>
        /// <returns>Returns dictionary of index and item, for given item id.</returns>
        public Dictionary<int, T> GetIndexesWithItemDictionary(string Id) => _GetItemsWithIndexesDictionary(x => x?.Id == Id).ToDictionary(x => x.Key, y => (T)y.Value.Clone());
        /// <summary>
        /// Used for retrieving index and item of given item.
        /// </summary>
        /// <param name="item">Item you want to filter for.</param>
        /// <returns>Returns dictionary of index and item, for given item.</returns>
        public Dictionary<int, T> GetIndexesWithItemDictionary(T item) => _GetItemsWithIndexesDictionary(x => _CompareItem(x, item)).ToDictionary(x => x.Key, y => (T)y.Value.Clone());

        #endregion

        #region Private
        private bool _WrongIndex(int index) => index > _items.Length || index < 0;

        private void _SwapItems(int oldIndex, int newIndex)
        {
            T itemToMove = _items[oldIndex];

            _items[oldIndex] = _items[newIndex];
            _items[newIndex] = itemToMove;
        }

        private bool _CompareItem(T item, T item2) => item?.Id == item2?.Id;

        private int[] _FindItemIndexes(T ItemToFind) => _GetIndexes(item => _CompareItem(item, ItemToFind));

        private int? _GetEmptyIndex() => _GetIndex(item => item == null);

        private int[] _GetEmptyIndexes() => _GetIndexes(item => item == null);

        private int? _GetIndex(Func<T, bool> condition)
        {
            var indexes = _GetIndexes(condition, false);
            return indexes.Length == 0 ? (int?)null : indexes[0];
        }

        private int[] _GetIndexes(Func<T, bool> condition, bool multiple = true)
        {
            List<int> indexes = new List<int>();

            for (int i = 0; i < _items.Length; i++)
            {
                if (!condition(_items[i]))
                {
                    continue;
                }

                if (!multiple)
                {
                    return new int[] { i }; // return first index found.
                }

                indexes.Add(i); //return all instances of the item found.
            }

            return indexes.ToArray();
        }

        private T[] _GetItems(Func<T, bool> condition) => _GetIndexes(condition).Select(x => _items[x]).ToArray();

        private Dictionary<int, T> _GetItemsWithIndexesDictionary(Func<T, bool> condition) => _GetItemsWithIndexesDictionary(_GetIndexes(condition));
        private Dictionary<int, T> _GetItemsWithIndexesDictionary(int[] indexes)
        {
            Dictionary<int, T> items = new Dictionary<int, T>();

            for (int i = 0; i < indexes.Length; i++)
            {
                items.Add(indexes[i], (T)_items[indexes[i]]);
            }

            return items;
        }

        private void _AddItem(T item, int? emptyIndex)
        {
            if (emptyIndex != null)
            {
                _items[emptyIndex.Value] = (T)item.Clone();
            }
        }

        private void _RemoveItem(int? emptyIndex)
        {
            if (emptyIndex != null)
            {
                _items[emptyIndex.Value] = default(T);
            }
        }

        private void _ApplyDistributedItemsToInventory((int, T)[] distributedItems)
        {
            for (int i = 0; i < distributedItems.Length; i++) // Populate successful distributed items.
            {
                _items[distributedItems[i].Item1] = distributedItems[i].Item2;
            }
        }

        private List<int> _DistributeAndAddItemsToInventory(T item, int quantityToDistribute, (int, T)[] distributedItems)
        {
            int[] emptyIndexes = _GetEmptyIndexes();

            // Populate successful distributed items.
            _ApplyDistributedItemsToInventory(distributedItems);

            int emptyIndexUsed = 0;

            List<int> addedIndexes = new List<int>(distributedItems.Select(x => x.Item1));

            while (quantityToDistribute != 0)
            {
                T clonedItem = (T)item.Clone();
                clonedItem.Quantity = quantityToDistribute > clonedItem.MaxQuantity ? clonedItem.MaxQuantity : quantityToDistribute;
                _AddItem(clonedItem, emptyIndexes[emptyIndexUsed]);
                addedIndexes.Add(emptyIndexes[emptyIndexUsed]);

                quantityToDistribute -= clonedItem.Quantity;
                emptyIndexUsed++;
            }

            return addedIndexes;

        }

        private IEnumerable<int> _DistributeItems(T itemToDistribute, Dictionary<int, T> itemInstances)
        {
            int maxQuantity = itemToDistribute.MaxQuantity;
            int quantityToDistribute = itemToDistribute.Quantity;

            // Ignore any instances that are already maxed out.
            (int, T)[] itemInstancesNotMaxedOut = itemInstances.Select(x => (x.Key, x.Value)).Where(x => x.Value.Quantity < maxQuantity).ToArray();

            // Create a new array where we will store distributed items, as it's not guaranteed we can do it. But we also don't want to first validate it and later populate it again.
            List<(int, T)> distributedItems = new List<(int, T)>();

            int itemsTried = 0;

            while (quantityToDistribute != 0 && itemsTried < itemInstancesNotMaxedOut.Count())
            {
                int index = itemInstancesNotMaxedOut[itemsTried].Item1;
                T distItem = itemInstancesNotMaxedOut[itemsTried].Item2;

                int possibleToDistribute = maxQuantity - distItem.Quantity;

                // Create a clone, as otherwise we would overrite item in inventory.
                T distItemClone = (T)distItem.Clone();

                // All quantity can be distributed to given item. 
                if (quantityToDistribute <= possibleToDistribute)
                {

                    distItemClone.Quantity += quantityToDistribute;

                    // Add item to list of distributed items.
                    distributedItems.Add((index, distItemClone));

                    // Distributed succesfully.
                    quantityToDistribute = 0;
                    break;

                }
                else if (quantityToDistribute > possibleToDistribute)
                {
                    distItemClone.Quantity += possibleToDistribute;

                    distributedItems.Add((index, distItemClone));
                    quantityToDistribute -= possibleToDistribute;
                }

                itemsTried++;
            }

            if (quantityToDistribute == 0)
            {
                // Populate successful distributed items.
                _ApplyDistributedItemsToInventory(distributedItems.ToArray());
                return distributedItems.Select(x => x.Item1);
            }
            else
            {
                // Populate successful distributed items.
                // And add the remaining instances.
                return _DistributeAndAddItemsToInventory(itemToDistribute, quantityToDistribute, distributedItems.ToArray());
            }
        }
        #endregion
    }
}
