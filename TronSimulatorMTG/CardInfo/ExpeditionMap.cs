using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class ExpeditionMap : Artifact, IRandomizable
	{
		public override List<Mana> manaCost { get; set; } = new List<Mana>() { new Mana(Color.Colorless) };

		override public string Name { get; set; } = "Expedition Map";

		public override Color CardColor { get; set; } = Color.Colorless;

		public override List<Mana> abilityCost { get; set; } = new List<Mana>() { new Mana(Color.Colorless), new Mana(Color.Colorless) };

		//public override void Cast()
		//{
		//	//if(_theGame.manaPool >= 1)
		//	if(this.isCastable())
		//	{
		//		_theGame.PayCosts(manaCost);
		//		_theGame.Hand.Remove(this);
		//		_theGame.InPlay.Add(this);
		//	}
		//}

		public override void Activate()
		{


			//if(_theGame.manaPool >= 2)
			if (this.isActivatable())
			{

				_theGame.PayCosts(abilityCost);


				var landChoices = _theGame.Deck
						 .Where(x => x is Land)
						 .ToList()
						//					 .Select((x, i) => new
						//					 {
						//						 x,
						//						 x.Name,
						//						 handorPlay = (_theGame.InPlay.Any(ip => ip.Name == x.Name) || _theGame.Hand.Any(ip => ip.Name == x.Name)),
						//						 aTron = x is Tronland,
						//						 rnk = i
						//					 })
						//					 .ToList()
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

				_theGame.InPlay.Remove(this);
				_theGame.Graveyard.Add(this);
			}



		}

		public void UseRandomly()
		{
			if (this.isActivatable())
			{

				_theGame.PayCosts(abilityCost);


				var landChoices = _theGame.Deck.Where(x => x is Land).ToList();


				if (landChoices.Any())
				{
					var landChoice = Helpers.ShuffleGood(landChoices).FirstOrDefault();

					_theGame.Deck.Remove(landChoice);
					_theGame.Hand.Add(landChoice);
				}

				_theGame.ShuffleDeck();

				_theGame.InPlay.Remove(this);
				_theGame.Graveyard.Add(this);

			}


		}
	}
}
