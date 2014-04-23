using System;
using Vector2Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using GameTimer;

namespace ShiftingRectangles
{
	/// <summary>
	/// this is a pattern of shifting blocks
	/// </summary>
	public class RectBackground
	{
		#region Fields

		/// <summary>
		/// The dark blocks in teh background
		/// </summary>
		private List<BackgroundBlock> m_BackgroundBlocks;

		/// <summary>
		/// the lighter blocks in the foreground
		/// </summary>
		private List<BackgroundBlock> m_ForegroundBlocks;

		/// <summary>
		/// 1x1 pixel that creates the shape.
		/// </summary>
		private Texture2D m_Pixel = null;

		static private Random g_Random = new Random(DateTime.Now.Millisecond);

		#endregion //Fields

		#region Properties

		/// <summary>
		/// the color of the background bricks
		/// </summary>
		public Color BackgroundColor { get; set; }

		/// <summary>
		/// the color of the foreground bricks, should be the same as clear color
		/// </summary>
		public Color ForegroundColor { get; set; }

		/// <summary>
		/// The rectangles will float around in this area until they float out and are replaced
		/// </summary>
		public Rectangle Border { get; set; }

		public int MaxNumBlocks { get; set; }
		public int MinBlockWidth { get; set; }
		public int MaxBlockWidth { get; set; }
		public int MinBlockHeight { get; set; }
		public int MaxBlockHeight { get; set; }

		public float MinAbsSpeed { get; set; }
		public float MaxAbsSpeed { get; set; }

		/// <summary>
		/// Add scrolling effects to the rect background
		/// </summary>
		public Vector2 Velocity { get; set; }

		GameClock _timer = new GameClock();

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// Constructor.
		/// </summary>
		public RectBackground(Rectangle border,
			Color background,
			Color foreground,
			int maxBlocks = 80, 
			int minWidth = 64, 
			int maxWidth = 256, 
			int minHeight = 50, 
			int maxHeight = 128, 
			float minAbsSpeed = 30, 
			float maxAbsSpeed = 50)
		{
			m_BackgroundBlocks = new List<BackgroundBlock>();
			m_ForegroundBlocks = new List<BackgroundBlock>();
			BackgroundColor = background;
			ForegroundColor = foreground;

			Border = border;

			MaxNumBlocks = maxBlocks;
			MinBlockWidth = minWidth;
			MaxBlockWidth = maxWidth;
			MinBlockHeight = minHeight;
			MaxBlockHeight = maxHeight;
			MinAbsSpeed = minAbsSpeed;
			MaxAbsSpeed = maxAbsSpeed;

			Velocity = Vector2.Zero;
		}

		/// <summary>
		/// Loads graphics content for this screen. The background texture is quite
		/// big, so we use our own local ContentManager to load it. This allows us
		/// to unload before going from the menus into the game itself, wheras if we
		/// used the shared ContentManager provided by the Game class, the content
		/// would remain loaded forever.
		/// </summary>
		public void LoadContent(GraphicsDevice graphicsDevice)
		{
			//do this here, becase the screen rect isnt set until after base.LoadContent is called

			// Create the pixel texture.
			m_Pixel = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
			m_Pixel.SetData<Color>(new Color[] { Color.White });

			//add a bunch of blocks
			for (int i = 0; i < MaxNumBlocks; i++)
			{
				m_BackgroundBlocks.Add(RandomBlock());
			}
			for (int i = 0; i < MaxNumBlocks; i++)
			{
				m_ForegroundBlocks.Add(RandomBlock());
			}
		}

		#endregion

		#region Update and Draw

		/// <summary>
		/// Updates the background screen. Unlike most screens, this should not
		/// transition off even if it has been covered by another screen: it is
		/// supposed to be covered, after all! This overload forces the
		/// coveredByOtherScreen parameter to false in order to stop the base
		/// Update method wanting to transition off.
		/// </summary>
		public void Update(GameTime gameTime)
		{
			_timer.Update(gameTime);
			UpdateRectangles(m_BackgroundBlocks);
			UpdateRectangles(m_ForegroundBlocks);
		}

		/// <summary>
		/// Draws the background screen.
		/// </summary>
		public void Draw(SpriteBatch spriteBatch)
		{
			//draw the dark rectangles
			DrawRectangles(m_BackgroundBlocks, spriteBatch, BackgroundColor); //darker than the clear color

			//draw the lighter rectangles
			DrawRectangles(m_ForegroundBlocks, spriteBatch, ForegroundColor); //the clear color
		}

		#endregion //Update and Draw

		#region Rectangle Lists

		private BackgroundBlock RandomBlock()
		{
			//create a random rectangle that fits in the border

			//get a width & height between min and max
			int width = g_Random.Next(MinBlockWidth, MaxBlockWidth);
			int height = g_Random.Next(MinBlockHeight, MaxBlockHeight);

			//get a start x & y between 0 and screen size - maxes
			int x = g_Random.Next(Border.Left, Border.Right - width);
			int y = g_Random.Next(Border.Top, Border.Bottom - height);

			//create a random movement vector
			Vector2 dir = RandomSpeed();

			//create the random block
			return new BackgroundBlock(new Rectangle(x, y, width, height), dir);
		}

		/// <summary>
		/// update a list of rectanlges
		/// </summary>
		/// <param name="rRectangleList">the list to update</param>
		private void UpdateRectangles(List<BackgroundBlock> rRectangleList)
		{
			//update rectangle positions
			for (int i = 0; i < rRectangleList.Count; i++)
			{
				BackgroundBlock myBlock = rRectangleList[i];
				myBlock.Update(_timer, Velocity);

				//if rectangle position is outside screen, wrap around the border
				if (myBlock.Rect.Top >= Border.Bottom)
				{
					//move to the top of the screen
					myBlock.Y = Border.Top - myBlock.Rect.Height;
				}
				else if (myBlock.Rect.Bottom < Border.Top)
				{
					//move to the bottom of the screen
					myBlock.Y = Border.Bottom;
				}

				if (myBlock.Rect.Left >= Border.Right)
				{
					//move to the left of the screen
					myBlock.X = Border.Left - myBlock.Rect.Width;
				}
				else if (myBlock.Rect.Right < Border.Left)
				{
					//move to the right of the screen
					myBlock.X = Border.Right;
				}
			}
		}

		private Vector2 RandomSpeed()
		{
			Vector2 dir = g_Random.NextVector2(MinAbsSpeed, MaxAbsSpeed, MinAbsSpeed * 0.5f, MaxAbsSpeed * 0.5f); //use half speed for y dir

			//flip the x direction?
			if ((g_Random.Next() % 2) == 0)
			{
				dir.X *= -1;
			}

			//flip the y direction?
			if ((g_Random.Next() % 2) == 0)
			{
				dir.Y *= -1;
			}

			return dir;
		}

		/// <summary>
		/// draw a list of rectangles
		/// </summary>
		/// <param name="rRectangleList"></param>
		/// <param name="RectColor"></param>
		private void DrawRectangles(List<BackgroundBlock> rRectangleList, SpriteBatch spriteBatch, Color RectColor)
		{
			foreach (BackgroundBlock myRect in rRectangleList)
			{
				spriteBatch.Draw(m_Pixel, myRect.Rect, RectColor);
			}
		}

		#endregion //Rectangle Lists
	}
}
