using System;
using UGLNet.Interfaces.Inventory;

namespace UGLNet.Instances
{
    public class BaseItem : IItem, IEquatable<IItem>
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            return Equals((BaseItem)obj);
        }

        public bool Equals(IItem other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetType() != other.GetType())
            {
                return false;
            }

            return Id == other.Id && Quantity == other.Quantity && MaxQuantity == other.MaxQuantity;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() * 0x00010000 * Quantity * MaxQuantity;
        }

        public static bool operator ==(BaseItem lhs, BaseItem rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(BaseItem lhs, BaseItem rhs)
        {
            return !(lhs == rhs);
        }
    }
}
