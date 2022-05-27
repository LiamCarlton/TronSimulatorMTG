using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class TempleOfFalseGod : Land
	{

		public override string Name { get; set; } = "TempleOfFalseGod";

		public virtual List<Mana> bonusMana { get; set; } = new List<Mana> { new Mana(Color.Colorless), new Mana(Color.Colorless) };

		public override List<Mana> Tap(bool test = false)
		{
			//var landsInPlay = _theGame.InPlay.Where(x => x is Land).ToList();

			if (_theGame.LandsInPlay.Count() >= 5)
			{
				ManaMade = bonusMana;
			}
			else
			{
				ManaMade = new List<Mana>();
			}

			return base.Tap(test);
		}

	}
}
