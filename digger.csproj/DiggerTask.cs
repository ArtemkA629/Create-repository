using System;
using System.Windows.Forms;

namespace Digger
{
	public class Terrain : ICreature
	{
		public string GetImageFileName() => "Terrain.png";

		public int GetDrawingPriority() => 0;

		public CreatureCommand Act(int x, int y) => new CreatureCommand { };

		public bool DeadInConflict(ICreature conflictedObject) => true;
	}

	public class Player : ICreature
	{
		public string GetImageFileName() => "Digger.png";

		public int GetDrawingPriority() => 1;

		public CreatureCommand Act(int x, int y)
		{
			var myCommand = new CreatureCommand
			{
				DeltaX = 0,
				DeltaY = 0,
				TransformTo = null
			};

			switch (Game.KeyPressed)
			{
				case Keys.Up:
					if (y > 0 && !(Game.Map[x, y - 1] is Sack)) myCommand.DeltaY--;
					break;
				case Keys.Down:
					if (y < Game.MapHeight - 1
						&& !(Game.Map[x, y + 1] is Sack)) myCommand.DeltaY++;
					break;
				case Keys.Right:
					if (x < Game.MapWidth - 1
						&& !(Game.Map[x + 1, y] is Sack)) myCommand.DeltaX++;
					break;
				case Keys.Left:
					if (x > 0 && !(Game.Map[x - 1, y] is Sack)) myCommand.DeltaX--;
					break;
			}
			return myCommand;
		}

		public bool DeadInConflict(ICreature conflictedObject)
		{
			if (conflictedObject is Gold) Game.Scores += 10;
			else if (conflictedObject is Monster) return true;
			return conflictedObject.ToString() == "Terrain.png";
		}
	}

	public class Sack : ICreature
	{
		public string GetImageFileName() => "Sack.png";

		public int GetDrawingPriority() => 2;

		public int Counter;

		public CreatureCommand Act(int x, int y)
		{
			var myCommand = new CreatureCommand
			{
				DeltaX = 0,
				DeltaY = 0,
				TransformTo = null
			};

			if (y < Game.MapHeight - 1)
			{
				var nextPoint = Game.Map[x, y + 1];
				if (Game.Map[x, y + 1] == null
				|| (Counter > 0 && nextPoint is Player) || (Counter > 0 && nextPoint is Monster))
				{
					Game.Map[x, y + 1] = null;
					Counter++;
					myCommand.DeltaY++;
					return myCommand;
				}
			}
			if (Counter > 1)
			{
				Counter = 0;
				myCommand.TransformTo = new Gold();
			}

			Counter = 0;
			return myCommand;
		}

		public bool DeadInConflict(ICreature conflictedObject) => !(conflictedObject is Player);
	}

	public class Gold : ICreature
	{
		public string GetImageFileName() => "Gold.png";

		public int GetDrawingPriority() => 3;

		public CreatureCommand Act(int x, int y) => new CreatureCommand { };

		public bool DeadInConflict(ICreature conflictedObject) => true;
	}

	public class Monster : ICreature
	{
		public string GetImageFileName() => "Monster.png";

		public int GetDrawingPriority() => 4;

		public void FindPlayer(ref int digX, ref int digY, ref bool isDigger)
		{
			for (int i = 0; i < Game.MapWidth; i++)
				for (int j = 0; j < Game.MapHeight; j++)
					if (Game.Map[i, j] is Player)
					{
						isDigger = true;
						digX = i;
						digY = j;
					}
		}

		public bool CheckCondition(ICreature point)
		{
			return point is null || point is Player || point is Gold;
		}

		public CreatureCommand Act(int x, int y)
		{
			var digX = 0;
			var digY = 0;
			bool isDigger = false;
			FindPlayer(ref digX, ref digY, ref isDigger);

			if (isDigger)
				if (x < digX && x < Game.MapWidth - 1 && CheckCondition(Game.Map[x + 1, y]))
				{
					return new CreatureCommand { DeltaX = 1 };
					Game.Map[x + 1, y] = null;
				}
				else if (x > digX && x > 0 && CheckCondition(Game.Map[x - 1, y]))
				{
					return new CreatureCommand { DeltaX = -1 };
					Game.Map[x - 1, y] = null;
				}
				else if (y < digY && y < Game.MapHeight - 1 && CheckCondition(Game.Map[x, y + 1]))
				{
					return new CreatureCommand { DeltaY = 1 };
					Game.Map[x, y + 1] = null;
				}
				else if (y > digY && y > 0 && CheckCondition(Game.Map[x, y - 1]))
				{
					return new CreatureCommand { DeltaY = -1 };
					Game.Map[x, y - 1] = null;
				}
			return new CreatureCommand { };
		}

		public bool DeadInConflict(ICreature conflictedObject)
		{
			return !(conflictedObject is Gold || conflictedObject is Player);
		}
	}
}