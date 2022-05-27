using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public abstract class Permanent : Card
	{
		public override void Cast()
		{

			if (this.isCastable())
			{
				_theGame.PayCosts(manaCost);
				_theGame.Hand.Remove(this);
				_theGame.InPlay.Add(this);
				_theGame.firstSpellCast = true;
			}
		}
	}
}
