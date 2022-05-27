using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class WinCondition : Permanent
	{
		public override string Name { get; set; } = "WinCondition";
		public override string CardType { get; set; } = "SomeType";
		public override Color CardColor { get; set; } = Color.Colorless;

		public override List<Mana> manaCost { get; set; } = new List<Mana>(){new Mana(Color.Colorless),new Mana(Color.Colorless),
														new Mana(Color.Colorless),new Mana(Color.Colorless),new Mana(Color.Colorless),new Mana(Color.Colorless),
														new Mana(Color.Colorless),new Mana(Color.Colorless),new Mana(Color.Colorless),new Mana(Color.Colorless)};

		//public override void Cast()
		//{
		//	throw new NotImplementedException();
		//}

	}
}
