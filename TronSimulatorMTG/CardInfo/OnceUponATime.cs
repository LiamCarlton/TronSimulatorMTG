﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class OnceUponATime : Sorcery, IRandomizable
	{
		public override string Name { get; set; } = "Once Upon A Time";

		public override Color CardColor { get; set; } = Color.Green;

		//public override List<Mana> manaCost { get; set; } = new List<Mana> { new Mana(Color.Green), new Mana(Color.Green) };

		public List<Mana> _manaCost = new List<Mana> { new Mana(Color.Green), new Mana(Color.Colorless) };

		public override List<Mana> manaCost
		{
			get
			{
				if (!_theGame.firstSpellCast)
				{
					return new List<Mana>();
				}
				else
				{
					return new List<Mana> { new Mana(Color.Green), new Mana(Color.Colorless) };
				}
			}

			set => this._manaCost = value;


		}

		public override void Cast()
		{
			//if(this.isCastable() && _theGame.firstSpellCast)
			//{
			//	manaCost.Dump("manaCost");
			//	_theGame.ManaPool.Dump("ManaPool");
			//}

			if (this.isCastable())
			{

				_theGame.PayCosts(manaCost);

				var options = _theGame.Deck
				.Take(5)
				.OrderByDescending(x => x is Land)
				.ThenByDescending(x => x is Tronland && !_theGame.InPlay.Any(ip => ip.Name == x.Name) && !_theGame.Hand.Any(ip => ip.Name == x.Name))
				.ThenBy(x => x is WinCondition)
				//.ThenByDescending(x => x._theGame.isCostPayableNextTurn(x.manaCost))
				.ToList()
				//.Dump("stir", 1)
				;

				var selection = options.FirstOrDefault();


				if (selection is Land)
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

				var legalOptions = options.Where(x => x is Land).ToList();

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
