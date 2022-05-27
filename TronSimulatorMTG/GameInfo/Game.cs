using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TronSimulatorMTG.CardInfo;
using TronSimulatorMTG.MCTS;

namespace TronSimulatorMTG.GameInfo
{
    public class Game
    {
        public List<Card> Hand { get; set; } = new List<Card>();
        public List<Card> InPlay { get; set; } = new List<Card>();
        public List<Card> Graveyard { get; set; } = new List<Card>();
        public List<Card> Deck { get; set; } = new List<Card>();

        public bool PlayedLandThisTurn { get; set; } = false;

        public int Turn { get; set; } = 0;

        public bool firstSpellCast { get; set; } = false;

        public List<Mana> ManaPool { get; set; } = new List<Mana>();

        public List<Mana> ManaFromLands
        {
            get
            {

                var tMana = this
                            .InPlay
                            .Select(ip => ip as Land)
                            .Where(x => x != null)
                            .SelectMany(ip => ip.Tap(true))
                            .ToList()
                            //.AddRange(TemporaryManaPool)
                            ;

                return tMana;

            }
        }

        public List<Land> LandsInPlay
        {
            get
            {
                var theLands = this.InPlay
                                    .Select(ip => ip as Land)
                                    .Where(x => x != null)
                                    .ToList();
                return theLands;
            }
        }

        public int PlayToCompletion(Action decisionAlgorithm)
        {

            do
            {
                decisionAlgorithm();
            }
            while (ManaFromLands.Count() < 7);



            return Turn;
        }

        public int PlayToCompletion(Action<int> decisionAlgorithm, int simCount)
        {
            do
            {
                decisionAlgorithm(simCount);

                //dc.Dump("After turn " + (Turn), 3);
            }
            while (ManaFromLands.Count() < 7);



            return Turn;
        }

        public void PassTurn()
        {
            ManaPool.Clear();

            Turn++;

            PlayedLandThisTurn = false;

            LandsInPlay.ForEach(x => x.isTapped = false);

            if (Turn != 1)
            {
                DrawCard();
            }


            TapAllLands();
        }

        //I wanna be able to pass 
        public void ReadDecklist(Dictionary<Type, int> decklist)
        {
            foreach (var x in decklist)
            {
                var cardToAdd = (Card)Activator.CreateInstance(x.Key);
                Deck.AddRange(Enumerable.Range(0, x.Value).Select(e => cardToAdd).ToList());
            }

        }

        public void ReadDecklist2(List<Card> theDeck)
        {
            foreach (var x in theDeck)
            {
                //var cardToAdd = (Card)Activator.CreateInstance(x.GetType());
                Deck.Add((Card)Activator.CreateInstance(x.GetType()));
            }
        }

        public bool CompareBoardstate(Game otherGame)
        {
            var handsTheSame = Hand.CompareTo(otherGame.Hand);

            var permanentsTheSame = InPlay.CompareTo(otherGame.InPlay);

            return handsTheSame && permanentsTheSame;
        }

        public void DrawCard()
        {
            try
            {
                var drw = Deck[0];

                Deck.Remove(drw);
                Hand.Add(drw);
            }
            catch (ArgumentOutOfRangeException)
            {
                //"oops, no cards".Dump();
            }

            //var drw = Deck.PickRandom();

        }


        public void TapAllLands()
        {
            var generatedMana = this
                            .InPlay
                            .Select(ip => ip as Land)
                            .Where(x => x != null)
                            .SelectMany(x => x.Tap())
                            .ToList();

            ManaPool.AddRange(generatedMana);
        }

        public void ShuffleDeck()
        {
            lock (Helpers.rndSeed)
            {
                Deck = MoreLinq.MoreEnumerable.Shuffle(Deck, Helpers.rndSeed).ToList();
            }

            //Deck = Deck.Shuffle().ToList();
        }

        public void Start()
        {
            Deck.ForEach(d =>
            {
                d._theGame = this;
                if (d is Land)
                {
                    (d as Land).isTapped = false;
                }
            });

            ShuffleDeck();

            //New
            for (int i = 0; i < 7; i++)
            {
                DrawCard();
            }
        }

        public void Start(int handSize)
        {
            Deck.ForEach(d =>
            {
                d._theGame = this;
                if (d is Land)
                {
                    (d as Land).isTapped = false;
                }
            });

            ShuffleDeck();

            //New
            for (int i = 0; i < handSize; i++)
            {
                DrawCard();
            }
        }

        public void Start(bool enableMulligans, int mulliganSimCount, double mulliganThreshold, double thresholdIncrement)
        {
            var watch2 = Stopwatch.StartNew();
            watch2.Start();

            Deck.ForEach(d =>
            {
                d._theGame = this;
                if (d is Land)
                {
                    (d as Land).isTapped = false;
                }
            });

            ShuffleDeck();

            int handSize = 7;


            //New
            for (int i = 0; i < handSize; i++)
            {
                DrawCard();
            }

            while (enableMulligans)
            {
                var watch = Stopwatch.StartNew();

                var newGame = this.CloneJson();



                List<Game> simulatedGames = new List<Game>();

                double avg;

                //mulliganThreshold++;

                //simulatedGames.Clear();



                for (int i = 0; i < mulliganSimCount; i++)
                {
                    var gm = new Game();

                    gm.Deck.AddRange(newGame.Deck);
                    gm.Hand.AddRange(newGame.Hand);

                    gm.Deck.ForEach(x => x._theGame = gm);
                    gm.Hand.ForEach(x => x._theGame = gm);



                    gm.PlayToCompletion(gm.PlayTurnRandomly);



                    simulatedGames.Add(gm);
                }



                avg = simulatedGames.Average(x => x.Turn);

                if (avg > mulliganThreshold)
                {
                    //Hand.Dump("BeforeMulligan", 1);

                    Hand.ForEach(x => Deck.Add(x));
                    Hand.Clear();

                    handSize--;
                    mulliganThreshold += thresholdIncrement;

                    ShuffleDeck();

                    if (handSize <= 0)
                    {
                        enableMulligans = false;
                    }

                    for (int i = 0; i < handSize; i++)
                    {
                        DrawCard();
                    }
                    //Hand.Dump("AfterMulligan", 1);

                }
                else
                {
                    enableMulligans = false;
                }


            }

        }

        public void Start(bool enableMulligans, int mulliganSimCount)
        {

            Deck.ForEach(d =>
            {
                d._theGame = this;
                if (d is Land)
                {
                    (d as Land).isTapped = false;
                }
            });

            ShuffleDeck();

            int handSize = 7;

            var thresholdList = new List<double>
        {
            4.85,
            5.1215,
            5.364333333,
            5.6865,
            6.056,
            6.467166667,
            6.900271467,
            100
        };

            int index = 1;


            //New
            for (int i = 0; i < handSize; i++)
            {
                DrawCard();
            }

            while (enableMulligans)
            {
                var newGame = this.CloneJson();

                List<Game> simulatedGames = new List<Game>();

                double avg;

                for (int i = 0; i < mulliganSimCount; i++)
                {
                    var gm = new Game();

                    gm.Deck.AddRange(newGame.Deck);
                    gm.Hand.AddRange(newGame.Hand);

                    gm.Deck.ForEach(x => x._theGame = gm);
                    gm.Hand.ForEach(x => x._theGame = gm);



                    gm.PlayToCompletion(gm.PlayTurnRandomly);



                    simulatedGames.Add(gm);
                }

                avg = simulatedGames.Average(x => x.Turn);

                if (avg > thresholdList[index])
                {
                    //Hand.Dump("BeforeMulligan", 1);

                    Hand.ForEach(x => Deck.Add(x));
                    Hand.Clear();

                    handSize--;
                    index++;

                    ShuffleDeck();

                    if (handSize <= 0)
                    {
                        enableMulligans = false;
                    }

                    for (int i = 0; i < handSize; i++)
                    {
                        DrawCard();
                    }
                    //Hand.Dump("AfterMulligan", 1);

                }
                else
                {
                    enableMulligans = false;
                }


            }

        }

        public bool isCostPayable(List<Mana> manaCost)
        {


            var tempManaPool = new List<Mana>(ManaPool);
            tempManaPool.Sort();

            foreach (Mana x in manaCost)
            {
                if (x.manaColor == Color.Green)
                {
                    if (tempManaPool.Exists(y => y.manaColor == Color.Green))
                    {
                        tempManaPool
                            .Remove
                            (tempManaPool.Find(y => y.manaColor == Color.Green));
                    }

                    else
                    {
                        return false;
                    }

                }

                else
                {
                    if (tempManaPool.Any())
                    {
                        tempManaPool.RemoveAt(0);
                    }

                    else
                    {
                        return false;
                    }
                }

            }


            return true;
            //return manaCost.isSubset(_theGame.ManaPool);
        }

        //Still don't know why you can't just do ManaPool.OrderBy(x => x.manaColor)
        public void PayCosts(List<Mana> theManaCost)
        {

            ManaPool.Sort();
            theManaCost.Sort();

            //Method 1:

            foreach (Mana x in theManaCost)
            {
                if (x.manaColor != Color.Colorless)
                {
                    //ManaPool.Sort(new SortByGreenMana());
                    ManaPool
                        .Remove(
                        ManaPool.Find(y => y.manaColor == x.manaColor)
                            );
                }

                else
                {
                    ManaPool.RemoveAt(0);
                }

                //try
                //{
                //	ManaPool.RemoveAt(0);
                //}
                //catch(ArgumentOutOfRangeException)
                //{
                //	this.Dump("ME CHEAT");
                //}


            }



            //Method 2:

            //foreach (Mana x in theManaCost)
            //{
            ////	ManaPool
            ////		.Remove(
            ////		ManaPool.Find(y => y.manaColor == x.manaColor)
            ////			);
            //	
            //}

            //No idea why these don't work:
            //ManaPool.OrderBy(x => x.manaColor);
            //ManaPool.OrderBy(x => x.manaColor, new SortByGreenMana());


        }

        //Doesn't return copies; invoking the actions returned by this method will actually mutate the game state.
        public List<Action> GetLegalActions()
        {

            var legalActions = new List<Action>();

            var artifactsInPlay = InPlay.Where(x => x is Artifact).ToList().Distinct(new CardComparer()).ToList();
            var nonLandsInHand = Hand.Where(x => !(x is Land)).ToList().Distinct(new CardComparer()).ToList();
            var landsInHand = Hand.Where(x => (x is Land)).ToList().Distinct(new CardComparer()).ToList();

            foreach (Land x in landsInHand)
            {
                if (x.IsPlayable())
                {
                    legalActions.Add(x.Play);
                }
            }

            foreach (Card x in nonLandsInHand)
            {
                try
                {
                    if (x.isCastable())
                    {
                        legalActions.Add(x.Cast);
                    }
                }
                catch (NullReferenceException e)
                {
                    //"MISTAKE".Dump();
                }

            }

            foreach (Artifact x in artifactsInPlay)
            {
                if (x.isActivatable())
                {
                    legalActions.Add(x.Activate);
                }

            }


            return legalActions;//.Dump("All Actions", 0);


        }

        //This method of determining legal moves also ensures that the effects will be executed in a random fashion, not a hard-coded one.
        public List<Action> GetLegalActions2()
        {

            var legalActions = new List<Action>();

            var artifactsInPlay = InPlay.Where(x => x is Artifact).ToList().Distinct(new CardComparer()).ToList();
            var nonLandsInHand = Hand.Where(x => !(x is Land)).ToList().Distinct(new CardComparer()).ToList();
            var landsInHand = Hand.Where(x => (x is Land)).ToList().Distinct(new CardComparer()).ToList();

            foreach (Land x in landsInHand)
            {
                if (x.IsPlayable())
                {
                    legalActions.Add(x.Play);
                }
            }

            foreach (Card x in nonLandsInHand)
            {
                if (x.isCastable())
                {
                    if (x is IRandomizable && !(x is ExpeditionMap))
                    {
                        legalActions.Add((x as IRandomizable).UseRandomly);
                    }
                    else
                    {
                        legalActions.Add(x.Cast);
                    }

                }
            }

            foreach (Artifact x in artifactsInPlay)
            {
                if (x.isActivatable())
                {
                    if (x is IRandomizable)
                    {
                        legalActions.Add((x as IRandomizable).UseRandomly);
                    }
                    else
                    {
                        legalActions.Add(x.Activate);
                    }
                }

            }


            return legalActions;//.Dump("All Actions", 0);


        }

        //This method only passes the turn at the start if there are no legal moves available.
        public void PlayTurnRandomly()
        {
            List<Action> legalMoves = GetLegalActions();

            //if (midTurn)
            //{
            //	legalMoves = GetLegalActions();
            //	midTurn = false;
            //}
            if (!legalMoves.Any())
            {
                PassTurn();

                legalMoves = GetLegalActions();
            }

            //		if (!legalMoves.Any() || Turn == 0)
            //		{
            //			PassTurn();
            //
            //			legalMoves = GetLegalActions();
            //		}

            Action moveToExecute;

            while (legalMoves.Count > 0)
            {

                moveToExecute = legalMoves.PickRandom();

                moveToExecute();

                legalMoves = GetLegalActions();

                //legalMoves.Dump("The Moves",1);
            }

        }

        public void PlayTurnTrulyRandomly()
        {
            List<Action> legalMoves = GetLegalActions2();

            if (!legalMoves.Any())
            {
                PassTurn();

                legalMoves = GetLegalActions2();
            }

            Action moveToExecute;

            while (legalMoves.Count > 0)
            {

                moveToExecute = legalMoves.PickRandom();

                moveToExecute();

                legalMoves = GetLegalActions2();

                //legalMoves.Dump("The Moves",1);
            }

        }

        public Action ChooseMoveUCT(int simCount)
        {
            var theTree = new MonteCarloTree(this);

            for (int i = 0; i < simCount; i++)
            {
                theTree.MonteCarloIteration();
            }

            var chosenMove = theTree.root.children
                            .OrderByDescending(x => x.numberOfVisits)
                            .FirstOrDefault()
                            .moveThatCreatedMe
                            //.Dump();
                            ;

            //theTree.Dump("The Tree", 1);

            return chosenMove;
        }

        public void PlayTurnUCT(int simCount)
        {
            PassTurn();

            var legalMoves = GetLegalActions();
            //legalMoves.Dump("The Moves",1);

            Action moveToExecute;

            while (legalMoves.Count > 0)
            {

                moveToExecute = ChooseMoveUCT(simCount);

                moveToExecute();

                //dc.Dump("After Move: " + (moveToExecute.Target), 3);

                //legalMoves.Dump("The Moves",1);
                //moveToExecute.Dump("Chosen Move",1);

                legalMoves = GetLegalActions();
                //legalMoves.Dump("The Moves",1);
            }
        }

        public Action ChooseMovePMCS(int simCount, List<Action> legalMoves)
        {

            if (legalMoves.Count == 1)
            {
                return legalMoves.First()//.Dump("Only One Choice", 1)
                ;
            }

            var simulatedGames = new List<Game>();

            var scores = new Dictionary<Action, double>();

            foreach (Action move in legalMoves)
            {// && 
             //if(move.Target.GetType() == (new ChromaticWhatever()).GetType() && move.Target.HasMethod("Activate"))
             //{
             //	"wow".Dump();
             //}
                var newGame = this.CloneJson();

                for (int i = 0; i < simCount; i++)
                {
                    var gm = new Game();

                    gm.PlayedLandThisTurn = newGame.PlayedLandThisTurn;

                    gm.Turn = newGame.Turn;

                    gm.firstSpellCast = newGame.firstSpellCast;

                    gm.ManaPool = newGame.ManaPool.ToList();

                    gm.Deck.AddRange(newGame.Deck);
                    gm.Hand.AddRange(newGame.Hand);
                    gm.InPlay.AddRange(newGame.InPlay);
                    gm.Graveyard.AddRange(newGame.Graveyard);

                    gm.Deck.ForEach(x => x._theGame = gm);
                    gm.Hand.ForEach(x => x._theGame = gm);
                    gm.InPlay.ForEach(x => x._theGame = gm);
                    gm.Graveyard.ForEach(x => x._theGame = gm);

                    var newMove = gm.FetchMove(move);

                    gm.ShuffleDeck();

                    newMove();

                    gm.PlayToCompletion(gm.PlayTurnRandomly);

                    simulatedGames.Add(gm);

                }

                scores.Add(move, simulatedGames.Average(x => x.Turn));

                simulatedGames.Clear();

            }


            var orderedResults = scores.OrderBy(x => x.Value).ToList()//.Dump("Ordered Results", 1)
            ;

            var topResult = orderedResults.First();

            return topResult.Key;

        }


        public void PlayTurnPMCS(int simCount)
        {
            PassTurn();

            var legalMoves = GetLegalActions();
            //legalMoves.Dump("The Moves",1);

            Action moveToExecute;

            while (legalMoves.Count > 0)
            {

                moveToExecute = ChooseMovePMCS(simCount, legalMoves);

                moveToExecute();

                //dc.Dump("After Move: " + (moveToExecute.Target), 3);

                legalMoves = GetLegalActions();
                //legalMoves.Dump("The Moves",1);
            }

        }

        public Action FetchMove(Action moveToFind)
        {
            Action locatedMove = null;

            Card theTarget;

            var moveName = moveToFind.GetMethodInfo().Name;

            var targetType = moveToFind.Target.GetType();

            switch (moveName)
            {
                case "Play":
                    theTarget = Hand.Find(x => x.HasMethod("Play") && x.GetType() == targetType);

                    try
                    {
                        locatedMove = ((Land)theTarget).Play;
                    }
                    catch (ArgumentException e)
                    {
                        //"MISTAKE".Dump();
                    }

                    //GetType().GetMethod("Play").GetMethod; //.Invoke(theTarget,null)
                    break;

                case "Cast":
                    theTarget = Hand.Find(x => x.HasMethod("Cast") && x.GetType() == targetType);

                    try
                    {
                        locatedMove = theTarget.Cast;
                    }
                    catch (NullReferenceException e)
                    {
                        //"MISTAKE".Dump();
                    }

                    break;

                case "Activate":
                    theTarget = InPlay.Find(x => x.HasMethod("Activate") && x.GetType() == targetType);
                    locatedMove = ((Artifact)theTarget).Activate;
                    break;

            }

            return locatedMove;
        }


        //Upon further progress with this project, ChooseMoveCool is unfortunately no longer the coolest way of choosing moves
        public Action ChooseMoveCool(List<Action> theActions)
        {
            Action chosenMove = null;

            var eggInHand = theActions.Find(x =>
                                            Hand.Contains(x.Target)
                                            &&
                                            x.Target.GetType() == new ChromaticWhatever().GetType()
                                            );
            if (eggInHand != null)
            {
                chosenMove = eggInHand;
            }



            var eggInPlay = theActions.Find(x =>
                            InPlay.Contains(x.Target)
                            &&
                            x.Target.GetType() == new ChromaticWhatever().GetType()
                            );

            if (eggInPlay != null)
            {
                chosenMove = eggInPlay;
            }

            var stir = theActions.Find(x =>
                            Hand.Contains(x.Target)
                            &&
                            x.Target.GetType() == new AncientStirrings().GetType()
                            );

            if (stir != null)
            {
                chosenMove = stir;
            }

            var mapInHand = theActions.Find(x =>
                            Hand.Contains(x.Target)
                            &&
                            x.Target.GetType() == new ExpeditionMap().GetType()
                            );

            if (mapInHand != null)
            {
                chosenMove = mapInHand;
            }

            var mapInPlay = theActions.Find(x =>
                            InPlay.Contains(x.Target)
                            &&
                            x.Target.GetType() == new ExpeditionMap().GetType()
                            );

            if (mapInPlay != null)
            {
                chosenMove = mapInPlay;
            }

            var scry = theActions.Find(x =>
                            Hand.Contains(x.Target)
                            &&
                            x.Target.GetType() == new SylvanScrying().GetType()
                            );

            if (scry != null)
            {
                chosenMove = scry;
            }

            var landChoices = Hand
                                 .Where(x => x is Land)
                                 //						 	 .Dump()
                                 .Select(x => new
                                 {
                                     x,
                                     already = InPlay.Any(ip => ip.Name == x.Name),
                                     trn = x is Tronland
                                 })
                                 //.Dump()
                                 .OrderBy(x => x.already)
                                 // .OrderByDescending(x => x.trn)
                                 .ThenByDescending(x => x.trn)
                                 //	 .Dump("picks")
                                 ;

            var landChoice = landChoices
                     .Select(x => x.x as Land) //~*~
                     .FirstOrDefault()
                     //.Dump("pick")
                     ;




            if (landChoice != null)
            {
                //var theLand = landChoice.PickRandom();
                //Hand.Remove(landChoice);
                //InPlay.Add(landChoice);
                landChoice.Play(); //~*~
            }

            return chosenMove;
        }

        public void PlayTurnCool2()
        {
            //manaPool = ManaFromLands;

            //		var drw = Deck.PickRandom();
            //
            //		Deck.Remove(drw);
            //		Hand.Add(drw);

            PassTurn();

            var eggInHand = Hand
                            .Select(h => h as ChromaticWhatever)
                            .Where(h => h != null)
                            .FirstOrDefault()
                            ;

            if (eggInHand != null)
            {
                eggInHand.Cast();
            }

            var eggInPlay = InPlay
                            .Select(h => h as ChromaticWhatever)
                            .Where(h => h != null)
                            .FirstOrDefault()
                            ;

            if (eggInPlay != null)
            {
                eggInPlay.Activate();
            }

            var stir = Hand
                        .Select(h => h as AncientStirrings)
                        .Where(h => h != null)
                        .FirstOrDefault()
                        ;

            if (stir != null)
            {
                stir.Cast();
            }

            var mapInHand = Hand
                                .Select(h => h as ExpeditionMap)
                                .Where(h => h != null)
                                .FirstOrDefault()
                                ;

            if (mapInHand != null)
            {
                //var theLand = landChoice.PickRandom();
                //Hand.Remove(map);
                mapInHand.Cast();
                //manaPool -= 1;
            }

            var mapInPlay = InPlay
                            .Select(h => h as ExpeditionMap)
                            .Where(h => h != null)
                            .FirstOrDefault()
                            ;

            if (mapInPlay != null)
            {
                mapInPlay.Activate();
            }

            var scry = Hand
                        .Select(h => h as SylvanScrying)
                        .Where(h => h != null)
                        .FirstOrDefault()
                        ;

            if (scry != null)
            {
                scry.Cast();
            }



            //	Util.HorizontalRun(true, this.InPlay, this.Hand).Dump("pre" + this.Turn);

            //

            //	Util.HorizontalRun(true, this.InPlay, this.Hand).Dump("post" + this.Turn);

            //InPlay.Dump("inply");

            var landChoices = Hand
                                 .Where(x => x is Land)
                                 //						 	 .Dump()
                                 .Select(x => new
                                 {
                                     x,
                                     already = InPlay.Any(ip => ip.Name == x.Name),
                                     trn = x is Tronland
                                 })
                                 //.Dump()
                                 .OrderBy(x => x.already)
                                 // .OrderByDescending(x => x.trn)
                                 .ThenByDescending(x => x.trn)
                                 //	 .Dump("picks")
                                 ;

            var landChoice = landChoices
                     .Select(x => x.x as Land) //~*~
                     .FirstOrDefault()
                     //.Dump("pick")
                     ;




            if (landChoice != null)
            {
                //var theLand = landChoice.PickRandom();
                //Hand.Remove(landChoice);
                //InPlay.Add(landChoice);
                landChoice.Play(); //~*~
            }

        }

        public string PlayTurnCool()
        {


            //		DrawCard();
            //
            //		TapAllLands();

            PassTurn();

            if (ManaPool.Count() >= 7)
            {
                this.InPlay.OfType<Land>().ForEach(ip => ip.isTapped = false);
                return "Tron Acquired!";
            }

            var eggInHand = Hand
                            .Select(h => h as ChromaticWhatever)
                            .Where(h => h != null)
                            .FirstOrDefault()
                            ;

            if (eggInHand != null)
            {
                eggInHand.Cast();
            }

            var eggInPlay = InPlay
                            .Select(h => h as ChromaticWhatever)
                            .Where(h => h != null)
                            .FirstOrDefault()
                            ;

            if (eggInPlay != null)
            {
                eggInPlay.Activate();
            }

            var stir = Hand
                        .Select(h => h as AncientStirrings)
                        .Where(h => h != null)
                        .FirstOrDefault()
                        ;

            if (stir != null)
            {
                stir.Cast();
            }

            var mapInHand = Hand
                                .Select(h => h as ExpeditionMap)
                                .Where(h => h != null)
                                .FirstOrDefault()
                                ;

            if (mapInHand != null)
            {
                mapInHand.Cast();
            }

            var mapInPlay = InPlay
                            .Select(h => h as ExpeditionMap)
                            .Where(h => h != null)
                            .FirstOrDefault()
                            ;

            if (mapInPlay != null)
            {
                mapInPlay.Activate();
            }

            var scry = Hand
                        .Select(h => h as SylvanScrying)
                        .Where(h => h != null)
                        .FirstOrDefault()
                        ;

            if (scry != null)
            {
                scry.Cast();
            }

            var landChoices = Hand
                                 .Where(x => x is Land)
                                 //						 	 .Dump()
                                 .Select(x => new
                                 {
                                     x,
                                     already = InPlay.Any(ip => ip.Name == x.Name),
                                     trn = x is Tronland
                                 })
                                 //.Dump()
                                 .OrderBy(x => x.already)
                                 // .OrderByDescending(x => x.trn)
                                 .ThenByDescending(x => x.trn)
                                 //	 .Dump("picks")
                                 ;

            var landChoice = landChoices
                     .Select(x => x.x as Land) //~*~
                     .FirstOrDefault()
                     //.Dump("pick")
                     ;




            if (landChoice != null)
            {
                //var theLand = landChoice.PickRandom();
                //Hand.Remove(landChoice);
                //InPlay.Add(landChoice);
                landChoice.Play(); //~*~
            }

            //		ManaPool.Clear();
            //
            //		this.Turn++;
            //
            //		this.InPlay.OfType<Land>().ForEach(ip => ip.isTapped = false);

            return "COOL";
        }

        public string PlayTurn()
        {

            var drw = Deck.PickRandom();

            Deck.Remove(drw);
            Hand.Add(drw);

            var landChoice = Hand
                        .Where(x => x is Land)
                        ;

            if (landChoice.Any())
            {

                var theLand = landChoice.PickRandom();
                Hand.Remove(theLand);
                InPlay.Add(theLand);

            }


            this.Turn++;

            return "ok";

        }


    }
}
