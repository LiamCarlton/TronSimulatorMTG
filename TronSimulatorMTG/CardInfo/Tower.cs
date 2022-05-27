﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TronSimulatorMTG.CardInfo
{
	public class Tower : Tronland
	{
		public override string Name { get; set; } = "Tower";

		public override List<Mana> bonusMana { get; set; } = new List<Mana> { new Mana(Color.Colorless), new Mana(Color.Colorless), new Mana(Color.Colorless) };


	}
}
