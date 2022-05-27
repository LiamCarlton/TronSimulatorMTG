using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG
{
	public enum Color { Colorless, White, Black, Red, Green, Blue }

	public class Mana : IComparable
	{
		public Color manaColor { get; set; }

		public Mana(Color theColor)
		{
			manaColor = theColor;
		}

		public int CompareTo(object obj)
		{
			var x = (Mana)obj;
			return ((IComparable)manaColor).CompareTo(x.manaColor);
		}

	}
}
