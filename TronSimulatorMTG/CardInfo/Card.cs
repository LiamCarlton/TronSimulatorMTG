using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TronSimulatorMTG.GameInfo;

namespace TronSimulatorMTG.CardInfo
{
	public abstract class Card
	{
		public abstract string Name { get; set; }
		public abstract string CardType { get; set; }
		public abstract Color CardColor { get; set; }


		public Game _theGame { get; set; }


		public abstract List<Mana> manaCost { get; set; }

		public bool isCastable()
		{
			return _theGame.isCostPayable(manaCost);
		}

		public abstract void Cast();


	}
}
