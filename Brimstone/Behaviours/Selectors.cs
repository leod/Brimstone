﻿using System.Collections.Generic;
using System.Linq;
using Brimstone.Actions;
using static Brimstone.Behaviours;

namespace Brimstone
{
	// All of the selectors you can use
	public partial class Behaviours
	{
		// Identity selector
		public static Selector Self => Select(e => e);

		// Current target selector
		public static Selector Target => Select(e => ((ICanTarget)e).Target);

		// Player selectors
		public static Selector Controller => Select(e => e.Controller);
		public static Selector Opponent => Select(e => e.Controller.Opponent);
		public static Selector Players => Select(e => e.Game.Players);
		public static Selector CurrentPlayer => Select(e => e.Game.CurrentPlayer);
		public static Selector CurrentInactivePlayer => Select(e => e.Game.CurrentOpponent);

		// Hero selectors (including mortally wounded)
		public static Selector FriendlyHero => Select(e => e.Controller.Hero);
		public static Selector OpponentHero => Select(e => e.Controller.Opponent.Hero);

		// Hero selectors (excluding mortally wounded)
		public static Selector FriendlyHealthyHero => Select(e => !e.Controller.Hero.MortallyWounded? e.Controller.Hero : null);
		public static Selector OpponentHealthyHero => Select(e => !e.Controller.Opponent.Hero.MortallyWounded? e.Controller.Opponent.Hero : null);

		// Weapon selectors
		//public static Selector FriendlyWeapon { get { throw new NotImplementedException(); } } // TODO: implement
		//public static Selector OpponentWeapon { get { throw new NotImplementedException(); } } // TODO: implement

		// Minion selectors (including mortally wounded)
		public static Selector AllMinions => Select(e => e.Game.Player1.Board.Concat(e.Game.Player2.Board));
		public static Selector FriendlyMinions => Select(e => e.Controller.Board);
		public static Selector OpponentMinions => Select(e => e.Controller.Opponent.Board);

		// Minion selectors (including mortally wounded)
		public static Selector AllHealthyMinions => Select(e => e.Game.Player1.Board.Concat(e.Game.Player2.Board).Where(x => !x.MortallyWounded));
		public static Selector FriendlyHealthyMinions => Select(e => e.Controller.Board.Where(x => !x.MortallyWounded));
		public static Selector OpponentHealthyMinions => Select(e => e.Controller.Opponent.Board.Where(x => !x.MortallyWounded));

		// Character selectors (including mortally wounded)
		public static Selector AllCharacters => Select(e => e.Game.Characters);
		public static Selector FriendlyCharacters => Combine(FriendlyMinions, FriendlyHero);
		public static Selector OpponentCharacters => Combine(OpponentMinions, OpponentHero);

		// Character selectors (excluding mortally wounded)
		public static Selector AllHealthyCharacters => Select(e => e.Game.Characters.Where(x => !x.MortallyWounded));
		public static Selector FriendlyHealthyCharacters => Combine(FriendlyHealthyMinions, FriendlyHealthyHero);
		public static Selector OpponentHealthyCharacters => Combine(OpponentHealthyMinions, OpponentHealthyHero);

		// Minion-in-deck selectors
		public static Selector FriendlyMinionsInDeck => Select(e => e.Controller.Deck.Where(x => x is Minion));
		public static Selector OpponentMinionsInDeck => Select(e => e.Controller.Opponent.Deck.Where(x => x is Minion));

		// Minion-in-hand selectors
		public static Selector FriendlyMinionsInHand => Select(e => e.Controller.Hand.Where(x => x is Minion));

		// Board adjacency selectors
		public static Selector AdjacentMinions => Select(e => new List<IEntity> { e.Controller.Board[e.ZonePosition - 1], e.Controller.Board[e.ZonePosition + 1] }.Where(x => x != null));

		// Random selectors
		public static ActionGraph RandomMinion => Random(AllMinions);
		public static ActionGraph RandomFriendlyMinion => Random(FriendlyMinions);
		public static ActionGraph RandomOpponentMinion => Random(OpponentMinions);

		public static ActionGraph RandomCharacter => Random(AllCharacters);
		public static ActionGraph RandomFriendlyCharacter => Random(FriendlyCharacters);
		public static ActionGraph RandomOpponentCharacter => Random(OpponentCharacters);

		public static ActionGraph RandomFriendlyMinionInDeck => Random(FriendlyMinionsInDeck);
		public static ActionGraph RandomOpponentMinionInDeck => Random(OpponentMinionsInDeck);
		public static ActionGraph RandomFriendlyMinionInHand => Random(FriendlyMinionsInHand);

		public static ActionGraph RandomHealthyMinion => Random(AllHealthyMinions);
		public static ActionGraph RandomFriendlyHealthyMinion => Random(FriendlyHealthyMinions);
		public static ActionGraph RandomOpponentHealthyMinion => Random(OpponentHealthyMinions);

		public static ActionGraph RandomHealthyCharacter => Random(AllHealthyCharacters);
		public static ActionGraph RandomFriendlyHealthyCharacter => Random(FriendlyHealthyCharacters);
		public static ActionGraph RandomOpponentHealthyCharacter => Random(OpponentHealthyCharacters);
	}
}
