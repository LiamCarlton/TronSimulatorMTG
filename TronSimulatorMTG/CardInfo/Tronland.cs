using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public abstract class Tronland : Land
	{
		public virtual List<Mana> bonusMana { get; set; }

		public bool HaveTron()
		{

			return _theGame.InPlay.Any(ip => ip is Tower) && _theGame.InPlay.Any(ip => ip is Mine) && _theGame.InPlay.Any(ip => ip is Plant);

		}

		public override List<Mana> Tap(bool test = false)
		{
			if (HaveTron())
			{
				ManaMade = bonusMana;
			}
			else
			{
				ManaMade = new List<Mana> { new Mana(Color.Colorless) };
			}

			return base.Tap(test);
		}

	}
}
