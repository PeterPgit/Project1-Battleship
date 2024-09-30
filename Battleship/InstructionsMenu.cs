/*
 *   Module Name: SettingsMenu.cs
 *   Purpose: This module contains the code for drawing the settings menu and selecting an AI difficulty
 *   Inputs: None
 *   Output: None
 *   Additional code sources: None
 *   Developers: Peter Pham, Reused for of previous group's code
 *   Date: 09/19/2024
 *   Last Modified: 09/20/2024
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for displaying the settings menu and getting/storing the user's chosen AI difficulty.
/// </summary>
public class InstructionsMenu
{
    // Object for storing the button rectangles
    private Dictionary<DifficultyState, Rectangle> buttonRects;

    // Object for storing the mouse state to detect input
    private MouseState mouseState;

    // Object for storing the button text
    private SpriteFont font;

    // Object for storing the button colors
    private Dictionary<DifficultyState, Color> buttonColors;

    // Temp object for checking whether the game should return back to the main menu
    public bool back { get; private set; }

    // Object for storing the back button rectangle
    private Rectangle backButtonRect;

    // Object for storing the back button color and setting the default color to white.
    private Color backButtonColor = Color.White;

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

    // Initializes the SettingsMenu class
    public InstructionsMenu(SpriteFont font)
    {
        this.font = font;
        back = false; 

        // Also initializes the back button rectangle
        backButtonRect = new  Rectangle(580, 500, 300, 75);
    }

    // Updates the menu based upon the user input
    public void Update()
    {
        // Gets the current mouse state
        mouseState = Mouse.GetState();

        foreach (var button in buttonRects)
        {
            // Checks whether the mouse is currently positioned on any button and changes the color
            if (button.Value.Contains(mouseState.Position))
            {
                buttonColors[button.Key] = Color.Gray;
            }
            // If mouse is not selecting any button, return all button colors to their default state
            else
            {
                buttonColors[button.Key] = Color.White;
            }
        }

        // Checks whether the back button is hovered and selected and updates the back variable accordingly
        if (backButtonRect.Contains(mouseState.Position))
        {
            backButtonColor = Color.Gray; 

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                back = true; 
            }
        }
        else
        {
            backButtonColor = Color.White; 
        }
    }

    // Draws the UI
    public void Draw(SpriteBatch spriteBatch)
    {
        // Clear the screen or draw an instructions background
        spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

        // Draw the instructions text
        Vector2 position = new Vector2(100, 100);  // Adjust this based on your UI layout
        spriteBatch.DrawString(font, instructionsText, position, Color.White);

        Texture2D rectangleTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        rectangleTexture.SetData(new[] { Color.White });

        // Draws the back button and its text
        spriteBatch.Draw(rectangleTexture, backButtonRect, backButtonColor);

        string backText = "Return to Main Menu";
        Vector2 backTextSize = font.MeasureString(backText);
        Vector2 backTextPosition = new Vector2(
            backButtonRect.X + (backButtonRect.Width / 2) - (backTextSize.X / 2),
            backButtonRect.Y + (backButtonRect.Height / 2) - (backTextSize.Y / 2)
        );

        spriteBatch.DrawString(font, backText, backTextPosition, Color.Black);
    }
}