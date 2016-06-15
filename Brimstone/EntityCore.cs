﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Brimstone
{
	public interface IEntity : ICloneable
	{
		int Id { get; set; }
		// Allow owner game and controller to be changed for state cloning
		Game Game { get; set; }
		Entity Controller { get; set; }
		Card Card { get; }

		int? this[GameTag t] { get; set; }

		IEntity CloneState();
	}

	public interface ICopyOnWrite
	{
		void CopyOnWrite();
	}

	public interface IPlayable : IEntity
	{
		IPlayable Play();
	}

	public interface IMinion : IPlayable
	{
		void Damage(int amount);
	}

	public class BaseEntityData : ICloneable
	{
		public int Id { get; set; }
		public Card Card { get; }
		public Dictionary<GameTag, int?> Tags { get; }

		public int? this[GameTag t] {
			get {
				// Use TryGetValue for safety
				return Tags[t];
			}
			set {
				Tags[t] = value;
			}
		}

		public BaseEntityData(Game game, Card card, Dictionary<GameTag, int?> tags = null) {
			Card = card;
			if (tags != null)
				Tags = tags;
			else
				Tags = new Dictionary<GameTag, int?>((int)GameTag._COUNT);
		}

		// Cloning copy constructor
		public BaseEntityData(BaseEntityData cloneFrom) {
			Card = cloneFrom.Card;
			Id = cloneFrom.Id;
			Tags = new Dictionary<GameTag, int?>(cloneFrom.Tags);
		}

		public virtual object Clone() {
			return new BaseEntityData(this);
		}
	}

	public class ReferenceCount
	{
		public ReferenceCount() {
			Count = 1;
		}

		public int Count { get; set; }
	}

	public class Entity : IEntity, IEnumerable<KeyValuePair<GameTag, int?>>, ICopyOnWrite
	{
		private BaseEntityData _entity;
		private ReferenceCount _referenceCount;

		public int ReferenceCount { get { return _referenceCount.Count; } }
		public BaseEntityData BaseEntityData { get { return _entity; } }

		public Game Game { get; set; }
		public Entity Controller { get; set; }

		public Entity(Entity cloneFrom) {
			_entity = cloneFrom._entity;
			_referenceCount = cloneFrom._referenceCount;
			_referenceCount.Count++;
			Game = cloneFrom.Game;
			Controller = Game.Entities[cloneFrom.Controller.Id];
		}

		public Entity(Game game, Entity controller, Card card, Dictionary<GameTag, int?> tags = null) {
			_entity = new BaseEntityData(game, card, tags);
			_referenceCount = new ReferenceCount();
			Controller = controller;
			if (game != null)
				game.Entities.Add(this);
		}

		public int? this[GameTag t] {
			get {
				if (t == GameTag.ENTITY_ID)
					return _entity.Id;
				if (t == GameTag.CONTROLLER)
					return Controller.Id;

				if (!_entity.Tags.ContainsKey(t))
					return 0;
				return _entity[t];
			}
			set {
				// Ignore unchanged data
				if (_entity.Tags.ContainsKey(t) && _entity[t] == value)
					return;
				else if (t == GameTag.CONTROLLER && value != null) {
					Controller = Game.Entities[(int)value];
				}
				else if (t == GameTag.ENTITY_ID && value != null) {
					CopyOnWrite();
					_entity.Id = (int)value;
				} else {
					CopyOnWrite();
					_entity[t] = value;
				}
				if (Game != null)
					Game.PowerHistory.Add(new TagChange(this) { Key = t, Value = value });
			}
		}

		public Card Card {
			get {
				return _entity.Card;
			}
		}

		public int Id {
			get {
				return _entity.Id;
			}

			set {
				CopyOnWrite();
				_entity.Id = value;
			}
		}

		public virtual object Clone() {
			return new Entity(this);
		}

		public virtual IEntity CloneState() {
			return Clone() as IEntity;
		}

		public void CopyOnWrite() {
			if (_referenceCount.Count > 1) {
				_entity = (BaseEntityData)_entity.Clone();
				_referenceCount.Count--;
				_referenceCount = new ReferenceCount();
			}
		}

		public Dictionary<GameTag, int?> Tags {
			get {
				var allTags = new Dictionary<GameTag, int?>(_entity.Tags);
				allTags[GameTag.CONTROLLER] = Controller.Id;
				allTags[GameTag.ENTITY_ID] = _entity.Id;
				return allTags;
			}
		}

		public IEnumerator<KeyValuePair<GameTag, int?>> GetEnumerator() {
			// Hopefully we're only iterating through tags in test code
			// so it doesn't matter that we are making a deep clone of the dictionary
			return Tags.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public override string ToString() {
			string s = Card.Name + " - ";
			foreach (var tag in this) {
				s += tag.Key + ": " + tag.Value + ", ";
			}
			return s.Substring(0, s.Length - 2);
		}
	}

	public class EntityController : IEnumerable<Entity>, ICloneable {
		public Game Game { get; }
		public int NextEntityId = 1;

		private Dictionary<int, Entity> Entities = new Dictionary<int, Entity>();

		public Entity this[int id] {
			get {
				return Entities[id];
			}
		}

		public int Count {
			get {
				return Entities.Count;
			}
		}

		public ICollection<int> Keys {
			get {
				return Entities.Keys;
			}
		}

		public bool ContainsKey(int key) {
			return Entities.ContainsKey(key);
		}

		public EntityController(Game game) {
			Game = game;
		}

		public EntityController(EntityController es) {
			NextEntityId = es.NextEntityId;
			foreach (var entity in es) {
				Entities.Add(entity.Id, (Entity) entity.Clone());
			}
			// Change ownership
			Game = FindGame();
			foreach (var entity in Entities)
				entity.Value.Game = Game;
			foreach (var entity in Entities)
				entity.Value.Controller = Entities[es.Entities[entity.Key].Controller.Id];
		}

		public int Add(Entity entity) {
			entity.Game = Game;
			entity.Id = NextEntityId++;
			Entities[entity.Id] = entity;
			Game.PowerHistory.Add(new CreateEntity(entity));
			return entity.Id;
		}

		public int Remove(Entity entity) {
			Entities.Remove(entity.Id);
			return entity.Id;
		}

		public Game FindGame() {
			// Game is always entity ID 1
			return (Game)Entities[1];
		}

		public Player FindPlayer(int p) {
			// Player is always p+1
			return (Player)Entities[p + 1];
		}

		public IEnumerator<Entity> GetEnumerator() {
			return Entities.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public object Clone() {
			return new EntityController(this);
		}
	}
}