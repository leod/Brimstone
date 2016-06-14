﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimstone
{
	public class QueueActionEventArgs : EventArgs
	{
		public Game Game;
		public QueueAction Action;
		public List<ActionResult> Args;

		public QueueActionEventArgs(Game g, QueueAction a, List<ActionResult> args) {
			Game = g;
			Action = a;
			Args = args;
		}
	}

	public class ActionQueue
	{
		public Game Game { get; private set; }
		public Queue<QueueAction> Queue = new Queue<QueueAction>();
		public Stack<ActionResult> ResultStack = new Stack<ActionResult>();
		public bool Paused { get; set; }

		public event EventHandler<QueueActionEventArgs> OnActionStarting;
		public event EventHandler<QueueActionEventArgs> OnAction;

		public void Attach(Game game) {
			Game = game;
		}

		public void Enqueue(ActionGraph g) {
			// Don't queue unimplemented cards
			if (g != null)
				g.Queue(this);
		}

		public List<ActionResult> Process() {
			if (Paused)
				return null;

			while (Queue.Count > 0) {
				var action = Queue.Dequeue();
				Console.WriteLine(action);
				var args = new List<ActionResult>();
				for (int i = 0; i < action.Args.Count; i++)
					args.Add(ResultStack.Pop());
				if (OnActionStarting != null)
					OnActionStarting(this, new QueueActionEventArgs(Game, action, args));
				// TODO: Replace with async/await later
				if (Paused) {
					foreach (var a in args)
						ResultStack.Push(a);
					args.Reverse();
					return args;
				}
				args.Reverse();
				var result = action.Run(Game, args);
				if (result.HasResult)
					ResultStack.Push(result);
				if (OnAction != null)
					OnAction(this, new QueueActionEventArgs(Game, action, args));
			}
			// Return whatever is left on the stack
			var stack = new List<ActionResult>(ResultStack);
			ResultStack.Clear();
			return stack;
		}
	}
}