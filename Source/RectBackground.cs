using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MenuBuddy;
using ResolutionBuddy;

namespace RoboJets
{
	/// <summary>
	/// this screen sits behind all other screens and has a pattern of shifting blocks
	/// </summary>
	public class BlockScreen : GameScreen
	{
		/// <summary>
		/// little struct for managing the rectangles and their movement
		/// </summary>
		private class BackgroundBlock
		{
			public Rectangle m_Rect;
			public float m_fPosition;
			public float m_fDirection;
			
			public Rectangle Rect
			{
				get { return m_Rect; }
			}
			
			public BackgroundBlock()
			{
				m_Rect = new Rectangle();
				m_fPosition = 0.0f;
				m_fDirection = 0.0f;
			}
		}

		#region Member Variables

		/// <summary>
		/// The dark blocks in teh background
		/// </summary>
		private List<BackgroundBlock> m_BackgroundBlocks;

		/// <summary>
		/// the lighter blocks in the foreground
		/// </summary>
		private List<BackgroundBlock> m_ForegroundBlocks;

		private int m_iMaxNumBlocks;
		private int m_iMinBlockWidth;
		private int m_iMaxBlockWidth;
		private int m_iMinBlockHeight;
		private int m_iMaxBlockHeight;

		static private Random g_Random = new Random(DateTime.Now.Millisecond);

		Rectangle m_View;

		#endregion //Member Variables

		#region Initialization

		/// <summary>
		/// Constructor.
		/// </summary>
		public BlockScreen()
		{
			TransitionOnTime = TimeSpan.FromSeconds(0.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);

			m_BackgroundBlocks = new List<BackgroundBlock>();
			m_ForegroundBlocks = new List<BackgroundBlock>();

			m_iMaxNumBlocks = 80;
			m_iMinBlockWidth = 30;
			m_iMaxBlockWidth = 100;
			m_iMinBlockHeight = 20;
			m_iMaxBlockHeight = 80;
		}

		/// <summary>
		/// Loads graphics content for this screen. The background texture is quite
		/// big, so we use our own local ContentManager to load it. This allows us
		/// to unload before going from the menus into the game itself, wheras if we
		/// used the shared ContentManager provided by the Game class, the content
		/// would remain loaded forever.
		/// </summary>
		public override void LoadContent()
		{
			base.LoadContent();

			//do this here, becase the screen rect isnt set until after base.LoadContent is called

			m_iMinBlockWidth = ScreenRect.Width / 14;
			m_iMaxBlockWidth = ScreenRect.Width / 10;
			m_iMinBlockHeight = ScreenRect.Height / 14;
			m_iMaxBlockHeight = ScreenRect.Height / 10;

			m_View = Resolution.ScreenArea;

			//add a bunch of blocks
			for (int i = 0; i < m_iMaxNumBlocks; i++)
			{
				m_BackgroundBlocks.Add(RandomBlock());
			}
			for (int i = 0; i < m_iMaxNumBlocks; i++)
			{
				m_ForegroundBlocks.Add(RandomBlock());
			}
		}

		/// <summary>
		/// Unloads graphics content for this screen.
		/// </summary>
		public override void UnloadContent()
		{
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
		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, false);

			UpdateRectangles(m_BackgroundBlocks, gameTime);
			UpdateRectangles(m_ForegroundBlocks, gameTime);
		}

		/// <summary>
		/// Draws the background screen.
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

			ScreenManager.SpriteBatchBegin();

			//draw the dark rectangles
			DrawRectangles(m_BackgroundBlocks, spriteBatch, new Color(0.45f, 0.45f, 0.45f, TransitionAlpha)); //darker than the clear color

			//draw the lighter rectangles
			DrawRectangles(m_ForegroundBlocks, spriteBatch, new Color(0.5f, 0.5f, 0.5f, TransitionAlpha)); //the clear color

			ScreenManager.SpriteBatchEnd();
		}

		#endregion //Update and Draw

		#region Rectangle Lists

		private BackgroundBlock RandomBlock()
		{
			BackgroundBlock myBlock = new BackgroundBlock();

			//create a random rectangle that fits in the screen

			//get a width & height between min and max
			myBlock.m_Rect.Width = (m_iMinBlockWidth + (g_Random.Next() % m_iMaxBlockWidth));
			myBlock.m_Rect.Height = (m_iMinBlockHeight + (g_Random.Next() % m_iMaxBlockHeight));

			//get a start x & y between 0 and screen size - maxes
			myBlock.m_Rect.X = m_View.X + (g_Random.Next() % (m_View.Width - m_iMaxBlockWidth));
			myBlock.m_Rect.Y = m_View.Y + (g_Random.Next() % (m_View.Height - m_iMaxBlockHeight));
			myBlock.m_fPosition = (float)myBlock.m_Rect.Y;

			//create a random movement vector
			myBlock.m_fDirection = ((g_Random.Next() % 2) == 0) ? RandomSpeed() : -RandomSpeed();

			return myBlock;
		}

		/// <summary>
		/// update a list of rectanlges
		/// </summary>
		/// <param name="rRectangleList">the list to update</param>
		private void UpdateRectangles(List<BackgroundBlock> rRectangleList, GameTime CurrentTime)
		{
			//update rectangle positions
			float fMilliseconds = CurrentTime.ElapsedGameTime.Milliseconds;
			float fTimeDelta = (fMilliseconds / 1000.0f);
			for (int i = 0; i < rRectangleList.Count; i++)
			{
				BackgroundBlock myBlock = rRectangleList[i];
				myBlock.m_fPosition += myBlock.m_fDirection * fTimeDelta;
				myBlock.m_Rect.Y = (int)myBlock.m_fPosition;

				//if rectangle position is outside screen, create new rectanlge inside screen
				if (((myBlock.Rect.Y + myBlock.Rect.Height) < m_View.X) ||
					(myBlock.Rect.Y > m_View.Height))
				{
					rRectangleList[i] = RandomBlock();
					myBlock = rRectangleList[i];

					if (0 == (g_Random.Next() % 2))
					{
						//start at top
						myBlock.m_Rect.Y = (m_View.Y + 1) - myBlock.m_Rect.Height;
						myBlock.m_fPosition = (float)myBlock.m_Rect.Y;
						myBlock.m_fDirection = RandomSpeed();
					}
					else
					{
						//start at bottom
						myBlock.m_Rect.Y = m_View.Height - 1;
						myBlock.m_fPosition = (float)myBlock.m_Rect.Y;
						myBlock.m_fDirection = -RandomSpeed();
					}
				}
			}
		}

		private float RandomSpeed()
		{
			return (30.0f + (float)(g_Random.Next() % 70));
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
				spriteBatch.Draw(ScreenManager.BlankTexture, myRect.m_Rect, RectColor);
			}
		}

		#endregion //Rectangle Lists
	}
}
