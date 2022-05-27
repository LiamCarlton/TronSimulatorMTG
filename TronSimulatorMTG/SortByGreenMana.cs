using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG
{
	public class SortByGreenMana : IComparer<Mana>
	{
		public int Compare(Mana mana1, Mana mana2)
		{
			if (mana1.manaColor == Color.Green && mana2.manaColor != Color.Green)
				return -1;

			if (mana1.manaColor != Color.Green && mana2.manaColor == Color.Green)
				return 1;

			else
				return 0;
		}
	}
}
