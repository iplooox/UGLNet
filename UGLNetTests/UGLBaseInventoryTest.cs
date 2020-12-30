using NUnit.Framework;
using System;
using System.Linq;
using UGLNet;
using UGLNet.Instances;

namespace UGLNetTests
{
    public class UGLBaseInventoryTest
    {
        public BaseItem BaseItem = new BaseItem { Id = "1", MaxQuantity = 1, Quantity = 1 };
        public BaseItem stackableItem = new BaseItem { Id = "2", MaxQuantity = 2, Quantity = 1 };

        #region AddItem
        [Test]
        public void CanAddItemToInventory()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);
            inventory.AddItem(BaseItem);

            Assert.IsTrue(inventory.Items[0] == BaseItem);
        }

        [Test]
        public void CanStackItems()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            var stackableItem2 = (BaseItem)stackableItem.Clone();
            inventory.AddItem(stackableItem);
            inventory.AddItem(stackableItem2);
            Assert.IsTrue(inventory.Items[0].Quantity == 2);
        }

        [Test]
        public void CanStackItemsWithCreatingNewInstancesOfIt()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);

            var stackableItem2 = (BaseItem)stackableItem.Clone();
            stackableItem2.Quantity = 2; // This will force it to create new instances as the max is 2.
            inventory.AddItem(stackableItem);
            inventory.AddItem(stackableItem2);

            Assert.IsTrue(inventory.Items[0].Quantity == 2 && inventory.Items[1].Quantity == 1);
        }

        [Test]
        public void CanStackItemsWithCreatingNewInstancesOfItExtreme()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(3);

            var stackableItem2 = (BaseItem)stackableItem.Clone();
            stackableItem2.Quantity = 4; // This will force it to create new instances as the max is 2.
            inventory.AddItem(stackableItem);
            inventory.AddItem(stackableItem2);

            Assert.IsTrue(inventory.Items[0].Quantity == 2 && inventory.Items[1].Quantity == 2 && inventory.Items[2].Quantity == 1);
        }

        [Test]
        public void CannotAddItemWithQuantityZero()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            var stackableItem2 = (BaseItem)stackableItem.Clone();
            stackableItem2.Quantity = 0;
            var (result, _) = inventory.AddItem(stackableItem2);

            Assert.IsTrue(result == false && inventory.Items[0] == null);
        }

        #endregion

        #region CanAddItem
        [Test]
        public void CanAddItemValidationWorksForEmptyItems()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            Assert.IsTrue(inventory.CanAddItem(BaseItem));
        }

        [Test]
        public void CanAddItemReturnFalseOnTooManyItems()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            BaseItem item = (BaseItem)BaseItem.Clone();
            item.Quantity = 5;

            Assert.IsFalse(inventory.CanAddItem(item));
        }

        [Test]
        public void CanAddItemReturnTrueOnCorrectQuantity()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            inventory.AddItem(stackableItem);
            Assert.IsTrue(inventory.CanAddItem(stackableItem));
        }

        [Test]
        public void CanAddItemReturnTrueOnQuantityAndNewInstances()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);
            inventory.AddItem(stackableItem);

            BaseItem _item = (BaseItem)stackableItem.Clone();
            _item.Quantity = 3;

            Assert.IsTrue(inventory.CanAddItem(_item));
        }

        #endregion

        #region RemoveItem
        [Test]
        public void CanRemoveItemFromInventoryOnlyQuantity()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            BaseItem cloneItem = (BaseItem)stackableItem.Clone();
            cloneItem.Quantity = 2;


            inventory.AddItem(cloneItem);

            if (inventory.Items[0] == null)
                throw new Exception("Item was not added");

            inventory.RemoveItem(stackableItem);
            Assert.IsTrue(inventory.Items[0]?.Quantity == 1);
        }

        [Test]
        public void CannotRemoveWrongQuantity()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(3);

            for (int i = 0; i < 3; i++)
            {
                BaseItem cloneItem = (BaseItem)stackableItem.Clone();
                cloneItem.Quantity = 1;
                inventory.AddItem(cloneItem);
            }

            BaseItem itemToRemove = (BaseItem)stackableItem.Clone();
            itemToRemove.Quantity = 4;

            var (result, a, b) = inventory.RemoveItem(itemToRemove);
            Assert.IsFalse(result && inventory.Items[0].Quantity == 1 && inventory.Items[1].Quantity == 1 && inventory.Items[2].Quantity == 1);
        }

        [Test]
        public void CanRemoveItemFromInventoryMultipleInstances()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(3);

            for (int i = 0; i < 3; i++)
            {
                BaseItem cloneItem = (BaseItem)stackableItem.Clone();
                cloneItem.Quantity = i % 2 == 0 ? 2 : 1;
                inventory.AddItem(cloneItem);
            }

            BaseItem itemToRemove = (BaseItem)stackableItem.Clone();
            itemToRemove.Quantity = 4;

            inventory.RemoveItem(itemToRemove);
            Assert.IsTrue(inventory.Items[0]?.Quantity == 1);
        }
        #endregion

        #region CanRemoveItem
        [Test]
        public void CanRemoveReturnFalseWhenQuantityTooHigh()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(3);

            for (int i = 0; i < 3; i++)
            {
                BaseItem cloneItem = (BaseItem)stackableItem.Clone();
                cloneItem.Quantity = 1;
                inventory.AddItem(cloneItem);
            }

            BaseItem itemToRemove = (BaseItem)stackableItem.Clone();
            itemToRemove.Quantity = 4;

            bool result = inventory.CanRemoveItem(itemToRemove);
            Assert.IsFalse(result);
        }

        [Test]
        public void CanRemoveReturnTrueWhenQuantityIsOkay()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(3);

            inventory.AddItem(stackableItem);
            bool result = inventory.CanRemoveItem(stackableItem);
            Assert.IsTrue(result);
        }
        #endregion

        #region ChangeSizeOfInventory
        [Test]
        public void ChangeSizeOfInventoryWithEmptyInventory()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(0);
            inventory.ChangeSizeOfInventory(1);
            Assert.IsTrue(inventory.Size == 1);
        }

        [Test]
        public void ChangeSizeOfInventoryToBiggerWithItems()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);

            inventory.AddItem(BaseItem);
            inventory.AddItem(BaseItem);

            inventory.ChangeSizeOfInventory(4);
            Assert.IsTrue(inventory.Size == 4);
        }

        [Test]
        public void ChangeSizeOfInventoryToSmaller()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(4);

            inventory.AddItem(BaseItem);
            inventory.AddItem(BaseItem);

            inventory.ChangeSizeOfInventory(2);
            Assert.IsTrue(inventory.Size == 2 && inventory.Items[0] == BaseItem && inventory.Items[1] == BaseItem);
        }

        #endregion

        #region CanChangeSizeOfInventory
        [Test]
        public void CanChangeSizeOfInventoryWhenEmpty()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(0);

            Assert.IsTrue(inventory.CanChangeSizeOfInventory(10));
        }

        [Test]
        public void CanChangeSizeOfInventoryWithItemsButBigger()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);
            inventory.AddItem(BaseItem);
            inventory.AddItem(BaseItem);

            Assert.IsTrue(inventory.CanChangeSizeOfInventory(10));
        }

        [Test]
        public void CannotChangeSizeOfInventoryWhenSmallerThenAmountOfItems()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);
            inventory.AddItem(BaseItem);
            inventory.AddItem(BaseItem);

            Assert.IsFalse(inventory.CanChangeSizeOfInventory(1));
        }

        #endregion

        #region MoveItemToIndex
        [Test]
        public void CanMoveItemToIndex()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);

            inventory.AddItem(BaseItem);
            inventory.AddItem(stackableItem);

            bool test1 = inventory.Items[0].Id == BaseItem.Id && inventory.Items[1].Id == stackableItem.Id;

            bool result = inventory.MoveItemToIndex(0, 1);

            bool test2 = inventory.Items[1].Id == BaseItem.Id && inventory.Items[0].Id == stackableItem.Id;

            Assert.IsTrue(test1 && test2 && result);
        }

        [Test]
        public void CanMoveItemToIndexWithEmptySlot()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);

            inventory.AddItem(BaseItem);

            bool test1 = inventory.Items[0].Id == BaseItem.Id && inventory.Items[1] == null;

            bool result = inventory.MoveItemToIndex(0, 1);

            bool test2 = inventory.Items[0] == null && inventory.Items[1].Id == BaseItem.Id;

            Assert.IsTrue(test1 && test2 && result);
        }

        [Test]
        public void CannotMoveItemToIndexWithInvalidIndex()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(0);

            Assert.IsFalse(inventory.MoveItemToIndex(0, 1));
        }

        [Test]
        public void CannotMoveItemToIndexWithInvalidIndex1()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            Assert.IsFalse(inventory.MoveItemToIndex(-1, 1));
        }

        [Test]
        public void CannotMoveItemToIndexWithInvalidIndex2()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(0);

            Assert.IsFalse(inventory.MoveItemToIndex(1, 0));
        }

        #endregion

        #region DestoryItem
        [Test]
        public void CanDestroyItemAtIndex()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            inventory.AddItem(BaseItem);

            bool test1 = inventory.Items[0].Id == BaseItem.Id;

            bool result = inventory.DestroyItem(0);

            bool test2 = inventory.Items[0] == null;

            Assert.IsTrue(test1 && test2 && result);
        }

        [Test]
        public void CannotDestroyItemAtInvalidIndex()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            inventory.AddItem(BaseItem);

            Assert.IsFalse(inventory.DestroyItem(-1));
        }

        [Test]
        public void CannotDestroyItemAtInvalidIndex2()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(0);

            Assert.IsFalse(inventory.DestroyItem(1));
        }

        #endregion

        #region GetIndexesWithItemDictionary
        [Test]
        public void GetIndexesWithItemDictionaryReturnCorrectIndexes()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(4);

            inventory.AddItem(BaseItem);
            inventory.AddItem(stackableItem);
            inventory.AddItem(stackableItem);
            inventory.AddItem(BaseItem);
            inventory.AddItem(BaseItem);

            var indexItemDictinary = inventory.GetIndexesWithItemDictionary(BaseItem);

            var keys = indexItemDictinary.Keys.ToArray();

            Assert.IsTrue(keys[0] == 0 && keys[1] == 2 && keys[2] == 3);
        }
        #endregion

        #region SplitItem

        [Test]
        public void CanSplitItem()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);

            BaseItem item = (BaseItem)stackableItem.Clone();
            item.Quantity = 2;

            inventory.AddItem(item);
            inventory.SplitItem(0, 1);

            Assert.IsTrue(inventory.Size == 2 && inventory.Items[0].Quantity == 1 && inventory.Items[1].Quantity == 1);
        }

        [Test]
        public void CannotSplitItemWithZeroQuantity()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);

            BaseItem item = (BaseItem)stackableItem.Clone();
            item.Quantity = 2;

            inventory.AddItem(item);
            var (result, _) = inventory.SplitItem(0, 0);

            Assert.IsTrue(inventory.Items[0].Quantity == 2 && inventory.Items[1] == null);
        }

        #endregion

        #region MergeItem
        [Test]
        public void CanMergeItem()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);

            inventory.AddItem(stackableItem);
            inventory.AddItem(stackableItem);

            inventory.Mergeitem(1, 0);

            Assert.IsTrue(inventory.Items[0].Quantity == 2 && inventory.Items[1] == null);
        }

        [Test]
        public void CannotMergeItemWithTooMuchQuantity()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(2);

            BaseItem item = (BaseItem)stackableItem.Clone();
            item.Quantity = 2;
            inventory.AddItem(item);
            inventory.AddItem(item);

            bool result = inventory.Mergeitem(1, 0);

            Assert.IsTrue(result == false && inventory.Items[0].Quantity == 2 && inventory.Items[1].Quantity == 2);
        }

        #endregion

        #region NullChecks

        #region AddItem
        [Test]
        public void AddItemNullCheck()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.AddItem(default(BaseItem)));

            Assert.IsTrue(result);
        }
        #endregion

        #region CanAddItem
        [Test]
        public void CanAddItemNullCheck()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.CanAddItem(default(BaseItem)));

            Assert.IsTrue(result);
        }

        [Test]
        public void CanAddItemNullCheck2()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.CanAddItem(default(string), default(int), default(int)));

            Assert.IsTrue(result);
        }
        #endregion

        #region RemoveItem

        [Test]
        public void RemoveItemNullCheck()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = false;

            try
            {
                inventory.RemoveItem(default(BaseItem));
                result = true;
            }
            catch
            {
                result = false;
            }

            Assert.IsTrue(result);
        }

        [Test]
        public void RemoveItemNullCheck2()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = false;

            try
            {
                inventory.RemoveItem(default(string), default(int));
                result = true;
            }
            catch
            {
                result = false;
            }

            Assert.IsTrue(result);
        }
        #endregion

        #region CanRemoveItem

        [Test]
        public void CanRemoveItemNullCheck()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.CanRemoveItem(default(BaseItem)));

            Assert.IsTrue(result);
        }

        [Test]
        public void CanRemoveItemNullCheck2()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.CanRemoveItem(default(string), default(int)));

            Assert.IsTrue(result);
        }

        #endregion

        #region ChangeSizeOfInventory

        [Test]
        public void ChangeSizeofInventoryInputValidation()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.ChangeSizeOfInventory(default(int)));

            Assert.IsTrue(result);
        }

        [Test]
        public void ChangeSizeofInventoryInputValidation2()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.ChangeSizeOfInventory(-1));

            Assert.IsTrue(result);
        }

        [Test]
        public void ChangeSizeofInventoryInputValidation3()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.ChangeSizeOfInventory(10));

            Assert.IsTrue(result);
        }

        #endregion

        #region MoveItemToIndex

        [Test]
        public void MoveItemToIndexInputValidation()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.MoveItemToIndex(default(int), default(int)));

            Assert.IsTrue(result);
        }

        [Test]
        public void MoveItemToIndexInputValidation2()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.MoveItemToIndex(-1, default(int)));

            Assert.IsTrue(result);
        }

        [Test]
        public void MoveItemToIndexInputValidation3()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.MoveItemToIndex(default(int), 10));

            Assert.IsTrue(result);
        }

        #endregion

        #region DestoryItem

        [Test]
        public void DestroyItemInputValidation()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.DestroyItem(default(int)));

            Assert.IsTrue(result);
        }

        [Test]
        public void DestroyItemInputValidation1()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.DestroyItem(-1));

            Assert.IsTrue(result);
        }

        [Test]
        public void DestroyItemInputValidation2()
        {
            BaseInventory<BaseItem> inventory = new BaseInventory<BaseItem>(1);

            bool result = GivesError(() => inventory.DestroyItem(10));

            Assert.IsTrue(result);
        }

        #endregion

        #endregion

        public bool GivesError(Action func)
        {
            bool result;

            try
            {
                func();
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}