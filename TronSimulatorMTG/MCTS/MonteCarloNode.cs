using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TronSimulatorMTG.GameInfo;

namespace TronSimulatorMTG.MCTS
{
	public class MonteCarloNode
	{
		public Game gameState { get; set; }

		public MonteCarloNode parent { get; set; }

		public List<MonteCarloNode> children { get; set; } = new List<MonteCarloNode>();

		public int numberOfVisits { get; set; } = 0;

		public double totalScore { get; set; } = 0;

		public Action moveThatCreatedMe { get; set; }
	}
}
