/*
 *   Module Name: Menu.cs
 *   Purpose: This module is the main menu class that is responsible for displaying the main menu and handling user input.
 *   Inputs: None
 *   Output: None
 *   Additional code sources: None
 *   Developers: Derek Norton, Ethan Berkley, Jacob Wilkus, Mo Morgan, and Richard Moser
 *   Date: 09/15/2024
 *   Last Modified: 09/15/2024
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Battleship;

/// <summary>
/// This class is responsible for displaying the main menu and handling user input.
/// </summary>
public class Menu
{
    /// <summary>
    /// Rectangle for all buttons
    /// </summary>
    private Dictionary<GameState, Rectangle> buttonRects;

    /// <summary>
    /// Mouse state to detect input
    /// </summary>
    private MouseState mouseState;

    /// <summary>
    /// Font for the button text
    /// </summary>
    private SpriteFont font;

    /// <summary>
    /// Button colors (changes on hover)
    /// </summary>
    private Dictionary<GameState, Color> buttonColors;

    /// <summary>
    /// Holds the text for each button
    /// </summary>
    private Dictionary<GameState, string> buttonTexts;

    /// <summary>
    /// Holds the selected game state
    /// </summary>
    public GameState SelectedState { get; private set; }

    /// <summary>
    /// Back button initialization for the instructions tab
    /// </summary>
    private Rectangle backButtonRect;
    private Color backButtonColor = Color.White;
    private string backButtonText = "Back";

    /// <summary>
    /// Initializes a new instance of the Menu class.
    /// </summary>
    /// <param name="font"></param>
    public Menu(SpriteFont font)
    {
        this.font = font;  // Set the font

        // Initialize button rectangles for the main menu
        buttonRects = new Dictionary<GameState, Rectangle>
        {
            { GameState.ShipSelection, new Rectangle(580, 100, 300, 100) },   // "Play Game"
            { GameState.Settings, new Rectangle(580, 250, 300, 100) },   // "Settings"
            { GameState.Instructions, new Rectangle(580, 400, 300, 100) },  // "Instructions"
            { GameState.Exit, new Rectangle(580, 550, 300, 100) }        // "Exit"
        };

        // Initialize button colors for each menu state
        buttonColors = new Dictionary<GameState, Color>
        {
            { GameState.ShipSelection, Color.White },
            { GameState.Settings, Color.White },
            { GameState.Instructions, Color.White },
            { GameState.Exit, Color.White }
        };

        // Button texts for the main menu
        buttonTexts = new Dictionary<GameState, string>
        {
            { GameState.ShipSelection, "Play Game" },
            { GameState.Settings, "AI Difficulty" },
            { GameState.Instructions, "Instructions" },
            { GameState.Exit, "Exit" }
        };

        SelectedState = GameState.MainMenu; // Default to main menu

        // Initialize back button rectangle
        int backButtonWidth = 200;
        int backButtonHeight = 50;
        int screenWidth = 1366; // Assume 1366 is the screen width or get it from the graphics device
        backButtonRect = new Rectangle((screenWidth / 2) - (backButtonWidth / 2), 650, backButtonWidth, backButtonHeight);
    }

    /// <summary>
    /// Updates the main menu based on user input.
    /// </summary>
    public void Update()
    {
        mouseState = Mouse.GetState(); // Get the current mouse state

        // Handle back button click
        if (backButtonRect.Contains(mouseState.Position))
        {
            backButtonColor = Color.Gray;  // Change color on hover

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                SelectedState = GameState.MainMenu;
            }
        }
        else
        {
            backButtonColor = Color.White;  // Reset color
        }
        

        // Main Menu handling
        foreach (var button in buttonRects)
        {
            // Change button color on hover
            if (button.Value.Contains(mouseState.Position))
            {
                buttonColors[button.Key] = Color.Gray;

                // Handle button clicks
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                        if (button.Key == GameState.ShipSelection)
                        {
                            SelectedState = GameState.ShipSelection;  // Transition to Ship Selection
                        }
                        else if (button.Key == GameState.Exit)
                        {
                            SelectedState = GameState.Exit;  // Exit the game
                        }
                        else if (button.Key == GameState.Settings)
                        {
                            SelectedState = GameState.Settings;  // Go to settings menu
                        }
                        else if (button.Key == GameState.Instructions)
                        {
                            SelectedState = GameState.Instructions;  // Go to instructions page
                        }
                }
            }
            else
            {
                buttonColors[button.Key] = Color.White; // Reset button color
            }
        }
    }

    private string instructionsText = "Welcome to Battleship!\n\n" +
    "Objective:\n" +
    "Try to sink all of your opponent's ships before they sink all of yours.\n\n" +
    "Setup:\n" +
    "Select the amount of ships you want to play with. Place your ships on the grid.\n\n" +
    "Playing the Game:\n" +
    "- Players take turns firing at the opponent's grid.\n" +
    "- A hit on a ship is marked with an explosion. A miss is marked with a splash.\n\n" +
    "Special Shots:\n" +
    "- Each player has a single use of a 3x3 bomb.\n" +
    "- Each player has a single use carpet bomb.\n\n" +
    "Winning the Game:\n" +
    "The first player to sink all of the opponent's ships wins the game.\n\n" +
    "Controls:\n" +
    "- Use the mouse to place a ship, rotate the ship by pressing 'r'.\n" +
    "- Click to select a grid square to fire upon during your turn.\n" +
    "- To use the 3x3 bomb, hold down 'b' can click a grid square.\n" +
    "- To use the carpet bomb, hold down 'c'. This defaults to a horizontal shot, this can be rotated by holding the buttons 'c' and 'r'.\n\n";


    /// <summary>
    /// Draws the main menu.
    /// </summary>
    /// <param name="spriteBatch"></param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (SelectedState == GameState.Instructions)
        {
            Texture2D backTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1); // Create a 1x1 texture
            backTexture.SetData(new[] { Color.White }); // Set the color of the rectangle to white

            // Clear the screen or draw an instructions background
            spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the instructions text
            Vector2 position = new Vector2(50, 50);  // Adjust this based on your UI layout
            spriteBatch.DrawString(font, instructionsText, position, Color.White);

            // Draws the back button and its text
            spriteBatch.Draw(backTexture, backButtonRect, backButtonColor);

            string backText = "Return to Main Menu";
            Vector2 backTextSize = font.MeasureString(backText);
            Vector2 backTextPosition = new Vector2(
                backButtonRect.X + (backButtonRect.Width / 2) - (backTextSize.X / 2),
                backButtonRect.Y + (backButtonRect.Height / 2) - (backTextSize.Y / 2)
            );

            spriteBatch.DrawString(font, backText, backTextPosition, Color.Black);
        }
        else
        {

            /// Draw the main menu buttons
            Texture2D rectangleTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1); // Create a 1x1 texture
            rectangleTexture.SetData(new[] { Color.White }); // Set the color of the rectangle to white

            foreach (var button in buttonRects)
            {
                // Draw the button rectangle
                spriteBatch.Draw(rectangleTexture, button.Value, buttonColors[button.Key]);

                // Center the button text on the button rectangle
                Vector2 textSize = font.MeasureString(buttonTexts[button.Key]);
                Vector2 textPosition = new Vector2(
                    button.Value.X + (button.Value.Width / 2) - (textSize.X / 2),
                    button.Value.Y + (button.Value.Height / 2) - (textSize.Y / 2)
                );

                spriteBatch.DrawString(font, buttonTexts[button.Key], textPosition, Color.Black); // Draw the button text
            }
        }
    }
}