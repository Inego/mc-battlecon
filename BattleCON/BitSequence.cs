using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCON
{
    public class BitSequence : IComparable<BitSequence>
    {
        public List<uint> bits = new List<uint>(1);
        private int currentBit = 0;

        public BitSequence()
        {
        }

        public BitSequence(uint container, byte bits)
        {
            this.bits.Add(container);
            currentBit = bits;
        }

        private string IntToBit(byte pos)
        {
            return Convert.ToString(bits[pos], 2).PadLeft(32, '0');
        }

        public int CompareTo(BitSequence y)
        {
            int result = this.currentBit.CompareTo(y.currentBit);

            if (result != 0)
                return result;

            int blocksCount = this.bits.Count;

            result = blocksCount.CompareTo(y.bits.Count);

            if (result != 0)
                return result;

            for (int i = 0; i < blocksCount; i++)
            {
                result = this.bits[i].CompareTo(y.bits[i]);
                if (result != 0)
                    return result;
            }

            return 0;
        }

        public void AddBits(uint bitContainer, int choices)
        {
            if (currentBit == 0)
                bits.Add(bitContainer);
            else
                bits[bits.Count - 1] |= bitContainer << currentBit;

            byte bitsToAdd;

            if (choices < 3)
                bitsToAdd = 1;
            else if (choices < 5)
                bitsToAdd = 2;
            else if (choices < 9)
                bitsToAdd = 3;
            else if (choices < 17)
                bitsToAdd = 4;
            else
                bitsToAdd = 5;

            currentBit += bitsToAdd;

            if (currentBit >= 32)
            {
                currentBit -= 32;
                if (currentBit > 0)
                    bits.Add(bitContainer >> (bitsToAdd - currentBit));
            }

        }

        public override string ToString()
        {
            string[] ints = new string[bits.Count];

            for (byte i = 0; i < bits.Count - 1; i++)
                ints[i] = IntToBit(i);
            if (currentBit > 0)
                ints[bits.Count - 1] = Convert.ToString(bits[bits.Count - 1], 2).PadLeft(currentBit, '0');

            Array.Reverse(ints);

            return '[' + String.Join(" ", ints) + "] " + GetHashCode().ToString("X8");
        }
    }
}
