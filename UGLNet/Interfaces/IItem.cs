using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGLNet.Interfaces.Inventory
{
    public interface IItem : ICloneable
    {
        string Id { get; set; }
        int Quantity { get; set; }
        int MaxQuantity { get; set; }
    }
}
