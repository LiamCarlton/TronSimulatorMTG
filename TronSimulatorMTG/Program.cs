using CsvHelper;
using CsvHelper.Configuration;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TronSimulatorMTG.CardInfo;
using TronSimulatorMTG.GameInfo;
using TronSimulatorMTG.MCTS;

namespace TronSimulatorMTG
{

	public class Program
    {
        static void Main()
		{

			List<Card> Deck1 = new List<Card>();

			Deck1.AddRange(Enumerable.Range(0, 4).Select(e => new Mine()).ToList());
			Deck1.AddRange(Enumerable.Range(0, 4).Select(e => new Plant()).ToList());
			Deck1.AddRange(Enumerable.Range(0, 4).Select(e => new Tower()).ToList());
			Deck1.AddRange(Enumerable.Range(0, 22).Select(e => new WinCondition() { Name = "Some Spell" }).ToList());
			Deck1.AddRange(Enumerable.Range(0, 6).Select(e => new Forest()).ToList());
			Deck1.AddRange(Enumerable.Range(0, 4).Select(e => new SylvanScrying()).ToList());
			Deck1.AddRange(Enumerable.Range(0, 4).Select(e => new ExpeditionMap()).ToList());
			Deck1.AddRange(Enumerable.Range(0, 8).Select(e => new ChromaticWhatever()).ToList());
			Deck1.AddRange(Enumerable.Range(0, 4).Select(e => new AncientStirrings()).ToList());
			//Deck1.AddRange(Enumerable.Range(0, 4).Select(e => new TempleOfFalseGod()).ToList());
			//Deck1.AddRange(Enumerable.Range(0, 4).Select(e => new OnceUponATime()).ToList());

			var gm = new Game();
			gm.Deck = Deck1.ToList();

			//Performance Comparison:

			Helpers.RunSimulationsBasic(Deck1, 1000, @"D:\Documents\ComparisonData.csv");

			Helpers.RunSimulations(Deck1, 1000, new Game().PlayTurnTrulyRandomly, false, 100, 5, 0.3, @"D:\Documents\ComparisonData.csv");

			Helpers.RunSimulationsParallel(1000, new Game().PlayTurnUCT, 30, false, 100, 5, 0.3, @"D:\Documents\ComparisonData.csv");

			Helpers.RunSimulationsParallel(1000, new Game().PlayTurnPMCS, 100, false, 100, 5, 0.3, @"D:\Documents\ComparisonData.csv");

			Helpers.RunSimulationsParallel(1000, new Game().PlayTurnPMCS, 100, true, 100, 5, 0.3, @"D:\Documents\ComparisonData.csv");

		}
	}

}