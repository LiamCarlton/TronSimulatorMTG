using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class ChromaticWhatever : Artifact
	{
		public override List<Mana> manaCost { get; set; } = new List<Mana>() { new Mana(Color.Colorless) };

		public override List<Mana> abilityCost { get; set; } = new List<Mana>() { new Mana(Color.Colorless) };

		public override Color CardColor { get; set; } = Color.Colorless;

		public override string Name { get; set; } = "Chromatic Whatever";


		public override void Activate()
		{

			if (this.isActivatable())
			{

				_theGame.PayCosts(abilityCost);
				_theGame.ManaPool.Add(new Mana(Color.Green));

				_theGame.DrawCard();

				_theGame.InPlay.Remove(this);
				_theGame.Graveyard.Add(this);

			}
		}
	}
}
