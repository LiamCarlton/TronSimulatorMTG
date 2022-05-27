using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class SylvanScrying : Sorcery, IRandomizable
	{
		public override List<Mana> manaCost { get; set; } = new List<Mana>() { new Mana(Color.Colorless), new Mana(Color.Green) };

		override public string Name { get; set; } = "Sylvan Scrying";

		public override Color CardColor { get; set; } = Color.Green;

		public override void Cast()
		{

			if (this.isCastable())
			{
				_theGame.PayCosts(manaCost);


				var landChoices = _theGame.Deck
						.Where(x => x is Land)
						.ToList()
						.OrderBy(x => (_theGame.InPlay.Any(ip => ip.Name == x.Name) || _theGame.Hand.Any(ip => ip.Name == x.Name)))
						.ThenByDescending(x => x is Tronland)
						.ToList();
				;



				if (landChoices.Any())
				{
					var landChoice = landChoices
						.FirstOrDefault()
						//	 .Dump("pick")
						;

					_theGame.Deck.Remove(landChoice);
					_theGame.Hand.Add(landChoice);
				}

				_theGame.ShuffleDeck();

				_theGame.Hand.Remove(this);
				_theGame.Graveyard.Add(this);

				_theGame.firstSpellCast = true;
			}


		}

		public void UseRandomly()
		{
			if (this.isCastable())
			{

				_theGame.PayCosts(manaCost);


				var landChoices = _theGame.Deck
						.Where(x => x is Land)
						.ToList()
				;



				if (landChoices.Any())
				{
					var landChoice = Helpers.ShuffleGood(landChoices).FirstOrDefault();

					_theGame.Deck.Remove(landChoice);
					_theGame.Hand.Add(landChoice);
				}

				_theGame.ShuffleDeck();

				_theGame.Hand.Remove(this);
				_theGame.Graveyard.Add(this);

				_theGame.firstSpellCast = true;
			}
		}


	}
}
