using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TronSimulatorMTG.GameInfo;

namespace TronSimulatorMTG.MCTS
{
	public class MonteCarloTree
	{
		public MonteCarloNode current { get; set; } = new MonteCarloNode();

		public MonteCarloNode root { get; set; } = new MonteCarloNode();

		//public List<Action> initialMoves { get; set; } = new List<Action>();

		public double C { get; set; } = Math.Sqrt(2);

		public double UCB1(MonteCarloNode theNode)
		{

			if (theNode.numberOfVisits == 0)
			{
				//return 100000;
				return 0;
			}
			else
			{
				double avgScore = theNode.totalScore / theNode.numberOfVisits;

				double exploration = C * (Math.Sqrt(Math.Log(root.numberOfVisits) / theNode.numberOfVisits));

				//return avgScore + exploration;
				return avgScore - exploration;
			}

		}

		//May want to prevent nodes that have reached a final state from being selected again!
		public MonteCarloNode Selection(MonteCarloNode startNode)
		{
			current = startNode;

            MonteCarloNode chosenNode = root; //Just needs to be initialized to something.

			while (current.children.Any())
			{
				//If only one child is present, just choose it without finding UCB1 value.
				if (current.children.Count() == 1)
				{
					chosenNode = current.children.First();
					//"One Choice".Dump();
				}
				else
				{
					//double highScore = 0;
					double highScore = 10000;

					foreach (var x in current.children)
					{
						var score = UCB1(x);

						//if (score >= highScore)
						if (score <= highScore)
						{
							highScore = score;
							chosenNode = x;
						}
					}
				}


				current = chosenNode;
			}

			if (chosenNode.numberOfVisits == 0)
			{
				return chosenNode;
			}
			else
			{
				Expansion(chosenNode);

				//return Selection(chosenNode);
				return chosenNode.children.FirstOrDefault();
			}


		}


		public void Expansion(MonteCarloNode theNode)
		{
			var legalMoves = theNode.gameState.GetLegalActions();

			while (!legalMoves.Any())
			{
				theNode.gameState.PassTurn();

				//"Pass Turn!".Dump();

				legalMoves = theNode.gameState.GetLegalActions();
			}

			foreach (var x in legalMoves)
			{
				var newGame = theNode.gameState.CloneJson();

				var newMove = newGame.FetchMove(x);

				newGame.ShuffleDeck();

				newMove();

				theNode.children.Add(new MonteCarloNode { gameState = newGame, parent = theNode, moveThatCreatedMe = x });

			}
		}

		public void BackPropagate(MonteCarloNode theNode, int tronTurn)
		{
			var temp = theNode;

			while (temp.parent != null)
			{
				//temp.totalScore += 10 - tronTurn;
				temp.totalScore += tronTurn;

				temp.numberOfVisits++;

				temp = temp.parent;
			}

			root.numberOfVisits++;
			//root.totalScore += 10 - tronTurn;
			root.totalScore += tronTurn;
		}

		public int PerformRollout(MonteCarloNode theNode)
		{
			if (theNode.gameState.ManaFromLands.Count() < 7)
			{
				var newGame = theNode.gameState.CloneJson();

				return newGame.PlayToCompletion(newGame.PlayTurnRandomly);
			}
			else
			{
				return theNode.gameState.Turn;
			}
		}

		public MonteCarloTree(Game initial)
		{
			root.gameState = initial;

			var initialMoves = initial.GetLegalActions();

			foreach (var x in initialMoves)
			{

				var newGame = initial.CloneJson();

				var newMove = newGame.FetchMove(x);

				newGame.ShuffleDeck();

				newMove();

                root.children.Add(new MonteCarloNode { gameState = newGame, parent = root, moveThatCreatedMe = x });

			}

			foreach (var x in root.children)
			{
				int tronTurn = PerformRollout(x);

				BackPropagate(x, tronTurn);
			}
		}

		public void MonteCarloIteration()
		{
			var nodeToRollout = Selection(root);

			if (nodeToRollout == null)
			{
				//"NULL".Dump();
			}

			int tronTurn = PerformRollout(nodeToRollout);

			BackPropagate(nodeToRollout, tronTurn);
		}

		//public Action ChooseMoveMCTS()
		//{
		//	
		//}

	}
}
