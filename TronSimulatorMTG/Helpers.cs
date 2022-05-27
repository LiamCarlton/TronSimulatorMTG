using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TronSimulatorMTG.CardInfo;
using TronSimulatorMTG.GameInfo;

namespace TronSimulatorMTG
{
	public static class Helpers
	{

		public static Random rndSeed = new Random();

		public static long TaskCount { get; set; } = 0;

		//Compares two lists of cards to see if they contain the same elements.
		public static bool CompareTo(this List<Card> source, List<Card> other)
		{
			var sourceCopy = source.ToList();
			var otherCopy = other.ToList();

			//sourceCopy.RemoveAll(x => x is Card);
			foreach (Card x in source)
			{
				if (otherCopy.Any(y => y.Name == x.Name))
				{
					otherCopy.Remove(otherCopy.Find(y => y.Name == x.Name));
				}
				else
				{
					return false;
				}

			}

			foreach (Card x in other)
			{
				if (sourceCopy.Any(y => y.Name == x.Name))
				{
					sourceCopy.Remove(sourceCopy.Find(y => y.Name == x.Name));
				}
				else
				{
					return false;
				}
			}

			return true;
		}



		/// <summary>
		/// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T CloneJson<T>(this T source)
		{

			// Don't serialize a null object, simply return the default for that object
			if (ReferenceEquals(source, null)) return default;

			// initialize inner objects individually
			// for example in default constructor some list property initialized with some values,
			// but in 'source' these items are cleaned -
			// without ObjectCreationHandling.Replace default constructor values will be added to result
			var serializeSettings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto,
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				//ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
				//ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				MaxDepth = 1,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects
				//NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
			};
			var thejson = JsonConvert.SerializeObject(source, serializeSettings);

			//thejson.Dump("JSON");


			var deserializeSettings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto,
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				//ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
				//ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				//MaxDepth = 1,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects
				//NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
			};

			return JsonConvert.DeserializeObject<T>(thejson, deserializeSettings);
		}

		public static bool HasMethod(this object objectToCheck, string methodName)
		{
			var type = objectToCheck.GetType();
			return type.GetMethod(methodName) != null;
		}

		public static T PickRandom<T>(this IEnumerable<T> source)
		{
			return source.PickRandom(1)
						 .Single();
		}


		public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
		{
			return source.Shuffle()
						 .Take(count);
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.OrderBy(x => Guid.NewGuid());
		}

		public static IEnumerable<T> ShuffleGood<T>(this IEnumerable<T> source)
		{
			lock (Helpers.rndSeed)
			{
				return MoreLinq.MoreEnumerable.Shuffle(source, Helpers.rndSeed).ToList();
			}
		}

		//Simulation method for algorithms with zero parameters (ie, playturnrandom)
		public static void RunSimulations(List<Card> theDeck, int simCount, Action turnPlayingMethod, bool enableMulligans, int mulliganSimCount, double mulliganThreshold, double thresholdIncrement, string outputFile)
		{
			var count = 0;

			var watch3 = new Stopwatch();
			watch3.Start();

			var method = turnPlayingMethod.GetMethodInfo();
			var nameOfMethod = method.Name;

			var res = Enumerable.Range(0, simCount)
		.ToList()
		//.Dump()
		.Select(x =>
		{

			var gm = new Game();

			foreach (Card card in theDeck)
			{
				gm.Deck.Add(card);
			}

			var TotalCards = gm.Deck.Count;

			gm.Start(enableMulligans, mulliganSimCount, mulliganThreshold, thresholdIncrement);

			Action action = (Action)method.CreateDelegate(typeof(Action), gm);

			gm.PlayToCompletion(action);


			count++;
			Console.WriteLine(count);

			return new
			{
				gm.ManaFromLands,
				gm.Turn,
				Method = nameOfMethod,
				TheMaps = gm.InPlay.Where(y => y is ExpeditionMap).ToList(),
				Permanents = gm.InPlay,
				Lands = gm.InPlay.Where(y => y is Land).ToList(),
				PermanentCount = gm.InPlay.Count(),
				LandCount = gm.InPlay.Where(y => y is Land).ToList().Count(),
				gm.Hand,
				gm.Graveyard

			};


		})//.Dump()
		.Where(x => x != null)
		.ToList()
		//.Dump(0)
		//.Dump(true)
		;


			watch3.Stop();
			var time = (watch3.ElapsedMilliseconds / (long)60000);//.Dump("Minutes Elapsed");

			var output = res
			.GroupBy(x => x.Method)
			.Select(x => new
			{
				x.Key,
				SimulationCount = "N/A",
				avg = x.Average(q => q.Turn),
				NumberOfGames = x.Count(),
				Mulligans = enableMulligans,
				mulliganSimCount,
				mulliganThreshold,
				thresholdIncrement,
				MinutesElapsed = time


			})
			.ToList();
			//.Dump(nameOfMethod);

			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				// Don't write the header again.
				HasHeaderRecord = false,
			};
			using (var stream = File.Open(outputFile, FileMode.Append))
			using (var writer = new StreamWriter(stream))
			using (var csv = new CsvWriter(writer, config))
			{
				csv.WriteRecords(output);
			}


		}



		public static void RunSimulations(List<Card> theDeck, int simCount, Action<int> turnPlayingMethod, int simCountMonteCarlo, bool enableMulligans, int mulliganSimCount, double mulliganThreshold, double thresholdIncrement, string outputFile)
		{
			var watch3 = new Stopwatch();
			watch3.Start();

			var count = 0;

			var method = turnPlayingMethod.GetMethodInfo();
			var nameOfMethod = method.Name;

			var res = Enumerable.Range(0, simCount)
		.ToList()
		//.Dump()
		.Select(x =>
		{

			var gm = new Game();

			foreach (Card card in theDeck)
			{
				gm.Deck.Add(card);
			}

			gm.Start(enableMulligans, mulliganSimCount, mulliganThreshold, thresholdIncrement);

			do
			{
				//turnPlayingMethod.Invoke();
				method.Invoke(gm, new object[] { simCountMonteCarlo });
			}
			while (gm.ManaFromLands.Count() < 7);

			count++;
			Console.WriteLine(count);

			return new
			{
				gm.ManaFromLands,
				gm.Turn,
				Method = nameOfMethod,
				TheMaps = gm.InPlay.Where(y => y is ExpeditionMap).ToList(),
				Permanents = gm.InPlay,
				Lands = gm.InPlay.Where(y => y is Land).ToList(),
				PermanentCount = gm.InPlay.Count(),
				LandCount = gm.InPlay.Where(y => y is Land).ToList().Count(),
				gm.Hand,
				gm.Graveyard

			};


		})//.Dump()
		.Where(x => x != null)
		.ToList()
		//.Dump(0)
		//.Dump(true)
		;

			watch3.Stop();
			var time = watch3.ElapsedMilliseconds;

			var output = res
			.GroupBy(x => x.Method)
			.Select(x => new
			{
				x.Key,
				SimulationCount = simCountMonteCarlo,
				avg = x.Average(q => q.Turn),
				NumberOfGames = x.Count(),
				Mulligans = enableMulligans,
				mulliganSimCount,
				mulliganThreshold,
				thresholdIncrement,
				Mode = "Sequential",
				MinutesElapsed = time


			})
			.ToList();
			//.Dump(nameOfMethod);


			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				// Don't write the header again.
				HasHeaderRecord = false,
			};
			using (var stream = File.Open(outputFile, FileMode.Append))
			using (var writer = new StreamWriter(stream))
			using (var csv = new CsvWriter(writer, config))
			{
				csv.WriteRecords(output);
			}

		}

		public static void RunSimulationsParallel(int simCount, Action<int> turnPlayingMethod, int simCountMonteCarlo, bool enableMulligans, int mulliganSimCount, double mulliganThreshold, double thresholdIncrement, string outputFile)
		{

			var watch3 = new Stopwatch();
			watch3.Start();

			var method = turnPlayingMethod.GetMethodInfo();
			var nameOfMethod = method.Name;

			List<Tuple<string, int>> turnCounts = new List<Tuple<string, int>>();

			Parallel.For(0, simCount, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
			{
				//("Task " + i + " Start").Dump();
				//Console.WriteLine("Task " + i + " Start");
				var gm = new Game();

				gm.Deck.AddRange(Enumerable.Range(0, 4).Select(e => new Mine()).ToList());
				gm.Deck.AddRange(Enumerable.Range(0, 4).Select(e => new Plant()).ToList());
				gm.Deck.AddRange(Enumerable.Range(0, 4).Select(e => new Tower()).ToList());
				gm.Deck.AddRange(Enumerable.Range(0, 22).Select(e => new WinCondition() { Name = "Some Spell" }).ToList());
				gm.Deck.AddRange(Enumerable.Range(0, 6).Select(e => new Forest()).ToList());
				gm.Deck.AddRange(Enumerable.Range(0, 4).Select(e => new SylvanScrying()).ToList());
				gm.Deck.AddRange(Enumerable.Range(0, 4).Select(e => new ExpeditionMap()).ToList());
				gm.Deck.AddRange(Enumerable.Range(0, 8).Select(e => new ChromaticWhatever()).ToList());
				gm.Deck.AddRange(Enumerable.Range(0, 4).Select(e => new AncientStirrings()).ToList());
				//gm.Deck.AddRange(Enumerable.Range(0, 4).Select(e => new TempleOfFalseGod()).ToList());
				//gm.Deck.AddRange(Enumerable.Range(0, 4).Select(e => new OnceUponATime()).ToList());

				gm.Start(enableMulligans, mulliganSimCount, mulliganThreshold, thresholdIncrement);

				do
				{
					method.Invoke(gm, new object[] { simCountMonteCarlo });
				}
				while (gm.ManaFromLands.Count() < 7);

				//("Task " + i + " Finish").Dump();

				turnCounts.Add(new Tuple<string, int>(nameOfMethod, gm.Turn));

				TaskCount++;
				Console.WriteLine(TaskCount);

			});
			//turnCounts.Dump("TurnCounts");
			watch3.Stop();
			var time = watch3.ElapsedMilliseconds;

			var output = turnCounts
			.GroupBy(x => x.Item1)
			.Select(x => new
			{
				Method = nameOfMethod,
				RolloutCount = simCountMonteCarlo,
				avg = x.Average(q => q.Item2),
				NumberOfGames = simCount,
				Mulligans = enableMulligans,
				mulliganSimCount,
				mulliganThreshold,
				thresholdIncrement,
				Mode = "Parallel",
				HorizontalAxis = "" + mulliganThreshold + ", " + thresholdIncrement,
				//MinutesElapsed = time,
				//FixedThreshold = true,

			})
			.ToList();
			//.Dump(nameOfMethod);



			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				// Don't write the header again.
				HasHeaderRecord = false,
			};
			//using (var stream = File.Open(@"D:\Documents\MethodComparisonData.csv", FileMode.Append))
			using (var stream = File.Open(outputFile, FileMode.Append))
			using (var writer = new StreamWriter(stream))
			using (var csv = new CsvWriter(writer, config))
			{
				//csv.WriteRecords(turnCounts);

				csv.WriteRecords(output);
			}

			TaskCount = 0;

		}

		public static void RunSimulationsBasic(List<Card> theDeck, int simCount, string outputFile)
		{

			//var theCopy = theDeck.ToList();

			var res = Enumerable.Range(0, simCount)
		.ToList()
		//.Dump()
		.Select(x =>
		{

			var gm = new Game();

			foreach (Card card in theDeck)
			{
				gm.Deck.Add(card);
			}

			var TotalCards = gm.Deck.Count;

			gm.Start();

			do
			{
				//gm.PlayTurnRandomly();
				gm.PlayTurnCool();
			}
			while (gm.ManaFromLands.Count() < 7);

			return new
			{
				gm.ManaFromLands,
				gm.Turn,
				Style = "Rules-Based",
				TheMaps = gm.InPlay.Where(y => y is ExpeditionMap).ToList(),
				Permanents = gm.InPlay,
				Lands = gm.InPlay.Where(y => y is Land).ToList(),
				PermanentCount = gm.InPlay.Count(),
				LandCount = gm.InPlay.Where(y => y is Land).ToList().Count(),
				gm.Hand,
				gm.Graveyard

			};


		})//.Dump()
		.Where(x => x != null)
		.ToList()
		//.Dump(0)
		//.Dump(true)
		;

			var output = res
				.GroupBy(x => x.Style)
				.Select(x => new
				{
					x.Key,
					SimulationCount = "N/A",
					avg = x.Average(q => q.Turn),
					NumberOfGames = x.Count(),
				});
			//.Dump("Original");

			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				// Don't write the header again.
				HasHeaderRecord = false,
			};
			//using (var stream = File.Open(@"D:\Documents\MethodComparisonData.csv", FileMode.Append))
			using (var stream = File.Open(outputFile, FileMode.Append))
			using (var writer = new StreamWriter(stream))
			using (var csv = new CsvWriter(writer, config))
			{
				csv.WriteRecords(output);
			}

		}
	}
}
