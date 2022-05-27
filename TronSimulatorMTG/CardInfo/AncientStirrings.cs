using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class AncientStirrings : Sorcery, IRandomizable
	{
		public override string Name { get; set; } = "Ancient Stirrings";

		public override Color CardColor { get; set; } = Color.Green;

		public override List<Mana> manaCost { get; set; } = new List<Mana> { new Mana(Color.Green) };

		//NOTE:
		//It is very difficult to hard code a method of casting Ancient Stirrings that is truly optimal in EVERY possible scenario.
		//This could potentially become easier once AI is implemented in the project, but for now all this spell is really good for is finding tronlands in the top 5 cards.
		//As a result, the simulation will likely undervalue the power of ancient stirrings simply because the bot doesn't know how to use it very well.
		public override void Cast()
		{

			if (this.isCastable())
			{

				_theGame.PayCosts(manaCost);

				var options = _theGame.Deck
				.Take(5)
				.OrderBy(x => x.CardColor)
				.ThenByDescending(x => x is Tronland && !_theGame.InPlay.Any(ip => ip.Name == x.Name) && !_theGame.Hand.Any(ip => ip.Name == x.Name))
				.ThenBy(x => x is WinCondition)
				//.ThenByDescending(x => x._theGame.isCostPayableNextTurn(x.manaCost))
				.ToList()
				//.Dump("stir", 1)
				;

				var selection = options.FirstOrDefault();

				if (selection.CardColor == Color.Colorless)
				{
					options.Remove(selection);
					_theGame.Deck.Remove(selection);
					_theGame.Hand.Add(selection);
				}


				foreach (Card x in options)
				{
					_theGame.Deck.Remove(x);
					_theGame.Deck.Insert(_theGame.Deck.Count - 1, x);
				}

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

				var options = _theGame.Deck.Take(5).ToList();

				var legalOptions = options.Where(x => x.CardColor == Color.Colorless).ToList();

				var selection = Helpers.ShuffleGood(legalOptions).FirstOrDefault();

				if (selection != null)
				{
					options.Remove(selection);
					_theGame.Deck.Remove(selection);
					_theGame.Hand.Add(selection);
				}


				foreach (Card x in options)
				{
					_theGame.Deck.Remove(x);
					_theGame.Deck.Insert(_theGame.Deck.Count - 1, x);
				}

				_theGame.Hand.Remove(this);
				_theGame.Graveyard.Add(this);

				_theGame.firstSpellCast = true;
			}
		}
	}
}
