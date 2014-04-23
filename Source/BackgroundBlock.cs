using System;
using GameTimer;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ShiftingRectangles
{
	/// <summary>
	/// little struct for managing the rectangles and their movement
	/// </summary>
	public class BackgroundBlock
	{
		#region Fields

		private Rectangle _rect;

		#endregion //Fields

		#region Properties

		/// <summary>
		/// The rectangle of this dude
		/// </summary>
		public Rectangle Rect
		{
			get
			{
				return _rect;
			}
		}

		public int X
		{
			set
			{
				_rect.X = value;
			}
		}

		public int Y
		{
			set
			{
				_rect.Y = value;
			}
		}

		/// <summary>
		/// The direction this guy is going
		/// </summary>
		public Vector2 Direction { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="dir"></param>
		public BackgroundBlock(Rectangle rect, Vector2 dir)
		{
			_rect = rect;
			Direction = dir;
		}

		public void Update(GameClock CurrentTime)
		{
			//add the direction to the location
			_rect.X += (int)(Direction.X * CurrentTime.TimeDelta);
			_rect.Y += (int)(Direction.Y * CurrentTime.TimeDelta);
		}

		#endregion //Methods
	}
}
