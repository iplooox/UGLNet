using NUnit.Framework;
using UGLNet;
using UGLNet.Events.Inventory;
using UGLNet.Instances;

namespace UGLNetTests
{
    public class UGLInventoryTest
    {
        public BaseItem BaseItem = new BaseItem { Id = "1", MaxQuantity = 1, Quantity = 1 };
        public BaseItem stackableItem = new BaseItem { Id = "2", MaxQuantity = 2, Quantity = 1 };

        #region AddItem

        [Test]
        public void AddItemTriggerItemAddedEvent()
        {
            Inventory<BaseItem> inventory = new Inventory<BaseItem>(1);

            inventory.OnItemAdded += (object sender, ItemAddedEvent<BaseItem> _event) =>
            {
                Assert.IsTrue(_event.Successful == true && _event.Item == BaseItem && _event.UpdatedIndexes.Length == 1 && _event.UpdatedIndexes[0] == 0);
            };

            inventory.AddItem(BaseItem);
        }

        #endregion

        #region RemoveItem
        [Test]
        public void RemoveItemTriggerItemRemovedEvent()
        {
            Inventory<BaseItem> inventory = new Inventory<BaseItem>(1);

            inventory.OnItemRemoved += (object sender, ItemRemovedEvent _event) =>
            {
                Assert.IsTrue(_event.Successful && inventory.Items[0] == null && _event.Id == BaseItem.Id && _event.Quantity == BaseItem.Quantity && _event.UpdatedIndexes.Length == 1 && _event.UpdatedIndexes[0] == 0);
            };

            inventory.AddItem(BaseItem);
            inventory.RemoveItem(BaseItem);
        }

        [Test]
        public void RemoveItemTriggerItemDestroyedEvent()
        {
            Inventory<BaseItem> inventory = new Inventory<BaseItem>(1);

            inventory.OnItemDestroyed += (object sender, ItemDestroyedEvent _event) =>
            {
                Assert.IsTrue(_event.Successful && _event.Index == 0);
            };

            inventory.AddItem(BaseItem);
            inventory.RemoveItem(BaseItem);
        }
        #endregion

        #region DestroyItem
        [Test]
        public void DestroyItemTriggerItemDestroyedEvent()
        {
            Inventory<BaseItem> inventory = new Inventory<BaseItem>(1);

            inventory.OnItemDestroyed += (object sender, ItemDestroyedEvent _event) =>
            {
                Assert.IsTrue(_event.Successful && _event.Index == 0);
            };

            inventory.AddItem(BaseItem);
            inventory.DestroyItem(0);
        }
        #endregion

        #region MoveItem
        [Test]
        public void MoveItemTriggerItemMovedEvent()
        {
            Inventory<BaseItem> inventory = new Inventory<BaseItem>(2);

            inventory.OnItemMoved += (object sender, ItemMovedEvent _event) =>
            {
                Assert.IsTrue(_event.Successful && _event.UpdatedIndexes.Length == 2 && _event.UpdatedIndexes[0] == 0 && _event.UpdatedIndexes[1] == 1);
            };

            inventory.AddItem(BaseItem);
            inventory.MoveItemToIndex(0, 1);
        }
        #endregion
    }
}
