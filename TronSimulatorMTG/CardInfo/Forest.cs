using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class Forest : Land
	{
		public override string Name { get; set; } = "Forest";

		public override List<Mana> ManaMade { get; set; } = new List<Mana> { new Mana(Color.Green) };

		public override List<Mana> Tap(bool test = false)
		{
			//ManaMade = new List<Mana> {new Mana(Color.Green)};

			return base.Tap(test);
		}
	}
}
