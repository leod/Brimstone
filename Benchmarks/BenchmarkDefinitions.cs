﻿using System.Collections.Generic;
using System.Linq;
using Brimstone;

namespace Brimstone.Benchmark
{
	internal partial class Benchmarks {
		// ======================================================
		// =============== BENCHMARK DEFINITIONS ================
		// ======================================================

		public const int DefaultIterations = 100000;

		public void LoadDefinitions() {
			Tests = new Dictionary<string, Test>() {
				{ "RawClone", new Test("Raw cloning speed (full game)", Test_RawClone) },
				{ "BoomBotPreHit", new Test("Boom Bot pre-hit cloning test; RC + 2 BB per side", Test_BoomBotPreHit) },
				{ "BoomBotPreDeathrattle", new Test("Boom Bot pre-deathrattle cloning test; 5 RC + 2 BB per side", Test_BoomBotPreDeathrattle) },
				{ "BoomBotUniqueStatesNS", new Test("Boom Bot hit; fuzzy unique states; Naive; 5 BR + 2 BB per side", Test_BoomBotUniqueStatesNS, Default_Setup2, 1) },
				{ "BoomBotUniqueStatesDSF", new Test("Boom Bot hit; fuzzy unique states; DSF; 5 BR + 2 BB per side", Test_BoomBotUniqueStatesDSF, Default_Setup2, 1) },
				{ "BoomBotUniqueStatesBSF", new Test("Boom Bot hit; fuzzy unique states; BSF; 5 BR + 2 BB per side", Test_BoomBotUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles2UniqueStatesDSF", new Test("Arcane Missiles (2); fuzzy unique game states; DSF; 5 BR + 2 BB per side", Test_2AMUniqueStatesDSF, Default_Setup2, 1) },
				{ "ArcaneMissiles1UniqueStatesBSF", new Test("Arcane Missiles (1); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_1AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles2UniqueStatesBSF", new Test("Arcane Missiles (2); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_2AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles3UniqueStatesBSF", new Test("Arcane Missiles (3); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_3AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles4UniqueStatesBSF", new Test("Arcane Missiles (4); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_4AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles5UniqueStatesBSF", new Test("Arcane Missiles (5); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_5AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles6UniqueStatesBSF", new Test("Arcane Missiles (6); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_6AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles7UniqueStatesBSF", new Test("Arcane Missiles (7); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_7AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles8UniqueStatesBSF", new Test("Arcane Missiles (8); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_8AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles9UniqueStatesBSF", new Test("Arcane Missiles (9); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_9AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles10UniqueStatesBSF", new Test("Arcane Missiles (10); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_10AMUniqueStatesBSF, Default_Setup2, 1) },
			};
		}

		// ======================================================
		// ===============    BENCHMARK CODE    =================
		// ======================================================

		public static Game Default_Setup() {
			return NewScenarioGame(MaxMinions: 7, NumBoomBots: 2, FillMinion: "River Crocolisk");
		}
		public static Game Default_Setup2() {
			return NewScenarioGame(MaxMinions: 7, NumBoomBots: 2, FillMinion: "Bloodfen Raptor", FillDeck: false);
		}

		public void Test_RawClone(Game g, int it) {
			for (int i = 0; i < it; i++)
				g.CloneState();
		}

		public void Test_BoomBotPreHit(Game g, int it) {
			var BoomBotId = g.Player1.Board.First(t => t.Card.Name == "Boom Bot").Id;
			for (int i = 0; i < it; i++) {
				Game cloned = (Game)g.CloneState();
				((Minion)cloned.Entities[BoomBotId]).Hit(1);
			}
		}

		public void Test_BoomBotPreDeathrattle(Game g, int it) {
			// Capture after Boom Bot has died but before Deathrattle executes
			var BoomBot = g.Player1.Board.First(t => t.Card.Name == "Boom Bot") as Minion;
			g.ActionQueue.OnAction += (o, e) => {
				ActionQueue queue = o as ActionQueue;
				if (e.Action is Death && e.Source == BoomBot) {
					for (int i = 0; i < it; i++) {
						Game cloned = (Game)g.CloneState();
						cloned.ActionQueue.ProcessAll();
					}
				}
			};
			BoomBot.Hit(1);
		}

		private void _boomBotUniqueStates(Game g, int it, ITreeSearcher search) {
			var BoomBot = g.CurrentPlayer.Board.First(t => t.Card.Name == "Boom Bot") as Minion;
			var tree = GameTree.Build(
				Game: g,
				SearchMode: search,
				Action: () => {
					BoomBot.Hit(1);
				}
			);
		}
		public void Test_BoomBotUniqueStatesNS(Game g, int it) {
			_boomBotUniqueStates(g, it, new NaiveTreeSearch());
		}

		public void Test_BoomBotUniqueStatesDSF(Game g, int it) {
			_boomBotUniqueStates(g, it, new DepthFirstTreeSearch());
		}

		public void Test_BoomBotUniqueStatesBSF(Game g, int it) {
			_boomBotUniqueStates(g, it, new BreadthFirstTreeSearch());
		}

		private void _missilesUniqueStates(Game g, int it, int missiles, ITreeSearcher search) {
			Cards.FromName("Arcane Missiles").Behaviour.Battlecry = Actions.Damage(Actions.RandomOpponentCharacter, 1) * missiles;
			Cards.FromName("Boom Bot").Behaviour.Deathrattle = Actions.Damage(Actions.RandomOpponentMinion, Actions.RandomAmount(1, 4));

			var ArcaneMissiles = g.CurrentPlayer.Give("Arcane Missiles");
			var tree = GameTree.Build(
				Game: g,
				SearchMode: search,
				Action: () => {
					ArcaneMissiles.Play();
				}
			);
		}

		public void Test_2AMUniqueStatesDSF(Game g, int it) {
			_missilesUniqueStates(g, it, 2, new DepthFirstTreeSearch());
		}

		public void Test_1AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 1, new BreadthFirstTreeSearch());
		}

		public void Test_2AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 2, new BreadthFirstTreeSearch());
		}

		public void Test_3AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 3, new BreadthFirstTreeSearch());
		}

		public void Test_4AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 4, new BreadthFirstTreeSearch());
		}
		public void Test_5AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 5, new BreadthFirstTreeSearch());
		}
		public void Test_6AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 6, new BreadthFirstTreeSearch());
		}
		public void Test_7AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 7, new BreadthFirstTreeSearch());
		}
		public void Test_8AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 8, new BreadthFirstTreeSearch());
		}
		public void Test_9AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 9, new BreadthFirstTreeSearch());
		}
		public void Test_10AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 10, new BreadthFirstTreeSearch());
		}
	}
}