using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public abstract class Artifact : Permanent
	{
		override public string CardType { get; set; } = "Artifact";

		public virtual List<Mana> abilityCost { get; set; } = new List<Mana>();

		public bool isActivatable()
		{
			return _theGame.isCostPayable(abilityCost);
		}

		public abstract void Activate();

	}
}
