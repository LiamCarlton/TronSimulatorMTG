using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class Land : Card
	{
		public override string CardType { get; set; } = "Land";

		public override string Name { get; set; } = "GenericLand";

		public override Color CardColor { get; set; } = Color.Colorless;

		public override List<Mana> manaCost { get; set; } = new List<Mana>();

		public bool isTapped = false;

		public virtual List<Mana> ManaMade { get; set; } = new List<Mana> { new Mana(Color.Colorless) };

		public virtual List<Mana> Tap(bool test = false)
		{
			//List<Mana> theMana = new List<Mana>();

			if (test)
			{
				return ManaMade;
			}
			else
			{
				if (isTapped) return new List<Mana>();

				isTapped = !test;

				return ManaMade;
			}


		}

		public bool IsPlayable()
		{
			return !_theGame.PlayedLandThisTurn;
		}

		public void Play()
		{

			_theGame.Hand.Remove(this);
			_theGame.InPlay.Add(this);
			this.isTapped = false;
			_theGame.PlayedLandThisTurn = true;
			//this.Tap();
			_theGame.TapAllLands();


		}

		//Hand.Remove(landChoice);
		//

		public override void Cast()
		{
			//"dont cast me bro".Dump();
		}
	}
}
