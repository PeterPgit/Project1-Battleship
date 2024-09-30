/*
 *   Module Name: BattleshipGame.cs
 *   Purpose: This module is the main game class for the Battleship game.
 *            It is responsible for managing all other subordinate manager objects needed to run the game.
 *   Inputs: None
 *   Output: None
 *   Additional code sources: ChatGPT for getting the size of an array
 *   Developers: Derek Norton, Ethan Berkley, Jacob Wilkus, Mo Morgan, Richard Moser, Michael Oliver, Peter Pham
 *   Date: 09/03/2024
 *   Last Modified: 09/23/2024
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO.Pipes;

namespace Battleship
{
    /// <summary>
    /// The main game class for the Battleship game.
    /// Manages all game logic and object management.
    /// </summary>
    public class BattleshipGame : Game
    {
        /// <summary>
        /// The MonoGame Graphics Device Manager.
        /// </summary>
        private GraphicsDeviceManager _graphics;

        /// <summary>
        /// The MonoGame sprit batch object.
        /// </summary>
        private SpriteBatch? _spriteBatch;

        /// <summary>
        /// The player's cursor.
        /// </summary>
        private Cursor _cursor = new();

        /// <summary>
        /// Player 1 grid object.
        /// </summary>
        private Grid? _player1grid;

        /// <summary>
        /// Player 2 grid object.
        /// </summary>
        private Grid? _player2grid;

        /// <summary>
        /// The internal ship manager object.
        /// </summary>
        private ShipManager? _shipManager;

        /// <summary>
        /// The internal turn manager object.
        /// </summary>
        private TurnManager? _turnManager;

        /// <summary>
        /// Boolean representing if user is in game or in the menu.
        /// </summary>
        private bool inGame = false;

        /// <summary>
        /// Game state object.
        /// </summary>
        private GameState currentGameState;

        /// <summary>
        /// Main menu object.
        /// </summary>
        private Menu menu;

        /// <summary>
        /// Ship selection menu object.
        /// </summary>
        private ShipSelectionMenu shipSelectionMenu;

        /// <summary>
        /// Settings menu object.
        /// </summary>
        private SettingsMenu SettingsMenu;

        private InstructionsMenu instructionsMenu;

        /// <summary>
        /// Creates object to store the selected difficulty for the AI; Sets the default state to disabled
        /// </summary>
        public DifficultyState selectedDifficulty = DifficultyState.Disabled;

        //private Postgame postgame;

        /// <summary>
        /// Object containing the font used in menu
        /// </summary>
        private SpriteFont font;

        /// <summary>
        /// Variable containing the number of ships used in game.
        /// </summary>
        public int shipCount;

        /// <summary>
        /// The total number of hits needed to sink all ships for player 1.
        ///</summary>
        public int P1HitLimit;

        /// <summary>
        /// The total number of hits needed to sink all ships for player 2.
        ///</summary>
        public int P2HitLimit;

        /// <summary>
        /// Initializes a new instance of the <see cref="BattleshipGame"/> class.
        ///</summary>
        public BattleshipGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Textures and fonts used while switching players - Added by Mikey (Sep 22)
        ///</summary>
        private Texture2D SwapTexture;
        private SpriteFont feedbackFont; 
        private Texture2D backgroundTexture; 

        /// <summary>
        /// Array for holding coords which should be priorited in attacks by the AI
        ///</summary>
        public List<(int,int)> priorityAttacks;

        /// <summary>
        /// Initializes the relevant objects and window. 
        /// Called once at startup.
        /// </summary>
        protected override void Initialize()
        {
            // Set the window size.
            _graphics.IsFullScreen = false; // Set to true to enable fullscreen mode.
            _graphics.PreferredBackBufferWidth = Constants.SQUARE_SIZE * Constants.GRID_SIZE * 2 * Constants.SCALE; // Set the window width to fit two grids. Scaled by Constants.SCALE.
            _graphics.PreferredBackBufferHeight = Constants.SQUARE_SIZE * Constants.GRID_SIZE * Constants.SCALE; // Set the window height to fit one grid. Scaled by Constants.SCALE.
            _graphics.ApplyChanges(); // Apply the changes to the window size.

            Window.Title = "Battleship"; // Set the window title.

            _player1grid = new Grid(Constants.GRID_SIZE, Constants.PLAYER_1_OFFSET); // Initialize the player 1 grid object.
            _player2grid = new Grid(Constants.GRID_SIZE, Constants.PLAYER_2_OFFSET); // Initialize the player 2 grid object.
            _shipManager = new ShipManager(5);  /* Initialize the ship manager with the number of ships.
                                                 * The parameter will eventually be a constant int property whose value
                                                 * is determined by the number of ships chosen at the main menu.
                                                 */

            _turnManager = new TurnManager(); // Initialize the turn manager object.

            // add the event handlers for ship placement, tile adjustment, and ship placement validation for both players.
            _shipManager.OnPlayer1ShipPlaced = _player1grid.ShipPlaced;
            _shipManager.OnPlayer2ShipPlaced = _player2grid.ShipPlaced;
            _shipManager.OnPlayer1AdjustedTileRequested = _player1grid.GetAdjustedCurrentTile;
            _shipManager.OnPlayer2AdjustedTileRequested = _player2grid.GetAdjustedCurrentTile;
            _shipManager.IsPlayer1PlacementValid = _player1grid.IsShipPlacementValid;
            _shipManager.IsPlayer2PlacementValid = _player2grid.IsShipPlacementValid;
            _shipManager.OnPlayerChange = _turnManager.NextTurn;

            priorityAttacks = new List<(int, int)>(); // Initializes the empty list
            base.Initialize(); // Ensures the framerwork-level logic in the base class is initialized.
        }

        /// <summary>
        /// Loads all texture content.
        /// Called once at startup.
        /// </summary>
        protected override void LoadContent()
        {
            // If the game hasn't started, load the font and content for the main menu and ship selection menu.
            if (!inGame)
            {
                _spriteBatch = new SpriteBatch(GraphicsDevice);
                font = Content.Load<SpriteFont>("defaultFont");

                // Initialize the main menu, ship selection menu, and instructions menu
                menu = new Menu(font);
                SettingsMenu = new SettingsMenu(font);
                shipSelectionMenu = new ShipSelectionMenu(font);
                instructionsMenu = new InstructionsMenu(font);
                return;
            }
            _player1grid = new Grid(Constants.GRID_SIZE, Constants.PLAYER_1_OFFSET);
            _player2grid = new Grid(Constants.GRID_SIZE, Constants.PLAYER_2_OFFSET);
            _shipManager = new ShipManager(shipCount);
            _turnManager = new TurnManager();
            // add event handlers
            _shipManager.OnPlayer1ShipPlaced = _player1grid.ShipPlaced;
            _shipManager.OnPlayer2ShipPlaced = _player2grid.ShipPlaced;
            _shipManager.OnPlayer1AdjustedTileRequested = _player1grid.GetAdjustedCurrentTile;
            _shipManager.OnPlayer2AdjustedTileRequested = _player2grid.GetAdjustedCurrentTile;
            _shipManager.IsPlayer1PlacementValid = _player1grid.IsShipPlacementValid;
            _shipManager.IsPlayer2PlacementValid = _player2grid.IsShipPlacementValid;
            _shipManager.OnPlayerChange = _turnManager.NextTurn;
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            /* Load the content for the grid objects, cursor, and ship manager objects.
             * The Content property is inherited from the base Game class. It is used to load content from the Content.mgcb file.
             */
            _player1grid!.LoadContent(Content);
            _player2grid!.LoadContent(Content);
            _shipManager!.LoadContent(Content);
            _cursor.LoadContent(Content);
            _turnManager!.LoadContent(Content);

            SwapTexture = Content.Load<Texture2D>("swap"); // extra textures for when switching players
            feedbackFont = Content.Load<SpriteFont>("feedbackFont"); 
            backgroundTexture = Content.Load<Texture2D>("clear"); 
            
        }

        /// <summary>
        /// Checks if any game logic has updated. Called constantly in a loop.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        protected override void Update(GameTime gameTime)
        {
            // Exit the game if the back button is pressed or the escape key is pressed.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // If the game hasn't started, update the main menu and ship selection menu.
            if (!inGame)
            {
                switch (currentGameState)
                {
                    // Update the main menu if the current game state is the main menu.
                    case GameState.MainMenu:
                        menu.Update(); // Update the main menu.
                        if (menu.SelectedState == GameState.ShipSelection) // If the "Play Game" button is clicked, transition to ship selection menu.
                        {
                            currentGameState = GameState.ShipSelection; // Transition to the ship selection menu.
                        }
                        else if (menu.SelectedState == GameState.Settings) // If the "Settings" button is clicked, go to the settings menu.
                        {
                            currentGameState = GameState.Settings; // Transition to the settings menu.
                        }
                        else if (menu.SelectedState == GameState.Exit) // If the "Exit" button is clicked, exit the game.
                        {
                            Exit();
                        }
                        break;

                    // Update the ship selection menu if the current game state is the ship selection menu.
                    case GameState.ShipSelection:
                        shipSelectionMenu.Update(); // Update the ship selection menu.

                        // When "Start Game" is clicked, transition to playing
                        if (shipSelectionMenu.IsSelectionMade)
                        {
                            shipCount = shipSelectionMenu.SelectedShipCount;  // Store the selected ship count
                            P1HitLimit = shipCount * (shipCount + 1) / 2; // Calculate the hit limit for player 1. This equation gives the total number of hits needed to sink all ships.
                            P2HitLimit = shipCount * (shipCount + 1) / 2; // Calculate the hit limit for player 2.
                            inGame = true; // Set the game to be in progress. This will skip the main menu and ship selection menu logic from all subsequent calls to Update().
                            currentGameState = GameState.Playing;  // Transition to the gameplay state
                            base.Initialize();
                            _shipManager!.ReadClick = false; // Set the read click to false ensure catching the positive end of the next click.
                        }
                        else if (shipSelectionMenu.back && Mouse.GetState().LeftButton == ButtonState.Released) // If the back button is clicked, return to the main menu.
                        {
                            currentGameState = GameState.MainMenu; // Transition back to main menu
                            base.Initialize();
                        }
                        break;

                    case GameState.Playing:
                        // Add your game logic here
                        // When the game is over, reset to main menu
                        break;

                    // When "Settings" is clicked, transition to the settings menu
                    case GameState.Settings:
                        SettingsMenu.Update();
                        
                        selectedDifficulty = SettingsMenu.SelectedDifficulty; // Updates the global difficulty to what was chosen in the settings menu
                        if (SettingsMenu.back && Mouse.GetState().LeftButton == ButtonState.Released) // If the back button is clicked, return to the main menu.
                        {
                            // Update the game's difficulty based on what was selected within the settings menu when the player returns to the main menu.
                            currentGameState = GameState.MainMenu; // Transition back to main menu
                            base.Initialize();
                        }
                        break;

                    case GameState.Instructions:
                        instructionsMenu.Update();
                        break;

                    case GameState.Exit:
                        Exit();
                        break;
                }

                base.Update(gameTime);
                return;
            }

            // If the start game button is pressed without choosing a ship count, return to the main menu.
            if (P1HitLimit == 0 || P2HitLimit == 0)
            {
                inGame = false; // reset the inGame variable to false to enable the main menu and ship selection menu to be displayed.
                currentGameState = GameState.MainMenu; // Transition back to the main menu.
                base.Initialize();
                return;
            }

            // Uses system random class to randomly pick grids to place ships for the AI
            Random random = new Random();

            // Update the grid objects
            _player1grid!.Update();
            _player2grid!.Update();

            // Get the current tile location for each player.
            Tuple<int, int> currentPlayer1TileLocation = _player1grid.GridArray.CoordinatesOf(_player1grid.CurrentTile);
            Tuple<int, int> currentPlayer2TileLocation = _player2grid.GridArray.CoordinatesOf(_player2grid.CurrentTile);

            // If the AI is disabled, continue the game as originally coded
            if (selectedDifficulty == DifficultyState.Disabled)
            {

                // Update the cursor object depending on if player 1 is placing ships or shooting tiles.
                if (_shipManager!.IsPlayer1Placing && _player1grid.CurrentTile is not null)
                    _cursor.UpdateWhilePlacing(_player1grid.CurrentTile, currentPlayer1TileLocation, _shipManager.CurrentShipSize);
                else if (_player1grid.CurrentTile is not null)
                    _cursor.UpdateWhilePlaying(_player1grid.CurrentTile, currentPlayer1TileLocation.Item1);

                // Update the cursor object depending on if player 2 is placing ships or shooting tiles.
                if (_shipManager!.IsPlayer2Placing && _player2grid.CurrentTile is not null)
                    _cursor.UpdateWhilePlacing(_player2grid.CurrentTile, currentPlayer2TileLocation, _shipManager.CurrentShipSize);
                else if (_player2grid.CurrentTile is not null)
                    _cursor.UpdateWhilePlaying(_player2grid.CurrentTile, currentPlayer2TileLocation.Item1);

                // Check if the left mouse button is released. If it is, indicate that the click has been read.
                if (Mouse.GetState().LeftButton == ButtonState.Released) // If the left mouse button is released, set the read click to true.
                {
                    _shipManager!.ReadClick = true;
                }
                // Check if the game is waiting for the players to swap turns and if the read click is true.
                // If so, progress the game by acknowledging the turn swap has been completed.
                else if (_turnManager!.SwapWaiting && _shipManager!.ReadClick)
                {
                    _shipManager!.ReadClick = false;
                    _turnManager.SwapWaiting = false; // Setting this to false ends the turn swap delay.
                }
                else
                {
                    // Update the ship manager object while the players are in ship placing mode.
                    if (_shipManager!.IsPlayer1Placing && _player1grid.CurrentTile is not null)
                        _shipManager.UpdateWhilePlacing(_player1grid.CurrentTile, _cursor.Orientation, 1);
                    if (_shipManager!.IsPlayer2Placing && _player2grid.CurrentTile is not null)
                        _shipManager.UpdateWhilePlacing(_player2grid.CurrentTile, _cursor.Orientation, 2);

                    // Handle shooting logic if the players are not in ship placing mode.
                    if (!_shipManager.IsPlacingShips)
                    {
                        HandleShooting();
                    }
                }

                // Hide both players ships if transitioning between player turns 
                // or hide the ships of the player who is not currently taking their turn.
                _shipManager!.HideP1Ships = _turnManager!.SwapWaiting || !_turnManager.IsP1sTurn;
                _shipManager.HideP2Ships = _turnManager!.SwapWaiting || _turnManager.IsP1sTurn;
            }
            // If the AI is enabled, proceed with randomly placing the ships
            else
            {
                // Update the cursor object depending on if player 1 is placing ships or shooting tiles.
                if (_shipManager!.IsPlayer1Placing && _player1grid.CurrentTile is not null)
                    _cursor.UpdateWhilePlacing(_player1grid.CurrentTile, currentPlayer1TileLocation, _shipManager.CurrentShipSize);
                else if (_player1grid.CurrentTile is not null)
                    _cursor.UpdateWhilePlaying(_player1grid.CurrentTile, currentPlayer1TileLocation.Item1);

                // Update the cursor object depending on if player 2 is placing ships or shooting tiles.
                if (_shipManager!.IsPlayer2Placing)
                {
                    // Gets random X and Y coords and modifies player 2's CurrentTile to the randomly chosen one
                    int gridSize = _player2grid.GridArray.GetLength(0); // Used ChatGPT here to know how to get the size of an array
                    int randomTileX = random.Next(1, gridSize);
                    int randomTileY = random.Next(1, gridSize);

                    _player2grid.CurrentTile = _player2grid.GridArray[randomTileX,randomTileY];
                    // Shows randomized ship placement
                    // _cursor.UpdateWhilePlacing(_player2grid.CurrentTile, currentPlayer2TileLocation, _shipManager.CurrentShipSize);
                    
                    // Changes settings to proceed without needing an input for the AI 
                    _turnManager.SwapWaiting = false;
                    _shipManager!.ReadClick = true;
                }
                else if (_player2grid.CurrentTile is not null)
                    _cursor.UpdateWhilePlaying(_player2grid.CurrentTile, currentPlayer2TileLocation.Item1);

                // Check if the left mouse button is released. If it is, indicate that the click has been read.
                if (Mouse.GetState().LeftButton == ButtonState.Released) // If the left mouse button is released, set the read click to true.
                {
                    _shipManager!.ReadClick = true;
                }
                // Check if the game is waiting for the players to swap turns and if the read click is true.
                // If so, progress the game by acknowledging the turn swap has been completed.
                else if (_turnManager!.SwapWaiting)
                {
                    _shipManager!.ReadClick = false;
                    _turnManager.SwapWaiting = false; // Setting this to false ends the turn swap delay.
                }
                else
                {
                    // Update the ship manager object while the players are in ship placing mode.
                    if (_shipManager!.IsPlayer1Placing && _player1grid.CurrentTile is not null)
                        _shipManager.UpdateWhilePlacing(_player1grid.CurrentTile, _cursor.Orientation, 1);
                    if (_shipManager!.IsPlayer2Placing && _player2grid.CurrentTile is not null)
                        _shipManager.UpdateWhilePlacing(_player2grid.CurrentTile, _cursor.Orientation, 2);

                    // Handle shooting logic if the players are not in ship placing mode.
                    if (!_shipManager.IsPlacingShips)
                    {
                        HandleShooting();
                    }
                }

                _shipManager!.HideP1Ships = _turnManager!.SwapWaiting || !_turnManager.IsP1sTurn;
                _shipManager.HideP2Ships = true;
            }

            base.Update(gameTime); // Ensures the framework-level logic in the base class is updated.
        }

        /// <summary>
        /// Draws objects to the screen. Called constantly in a loop.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        protected override void Draw(GameTime gameTime)
        {
            // If the game hasn't started, draw the main menu and ship selection menu.
            if (!inGame)
            {
                GraphicsDevice.Clear(Color.CornflowerBlue); // Clear the screen with a blue color.
                _spriteBatch!.Begin(); // Begin the sprite batch for drawing.

                if (currentGameState! == GameState.MainMenu)
                {
                    menu.Draw(_spriteBatch);
                }
                else if (currentGameState == GameState.ShipSelection)
                {
                    shipSelectionMenu.Draw(_spriteBatch);
                }
                else if (currentGameState == GameState.Settings)
                {
                    SettingsMenu.Draw(_spriteBatch);
                    SettingsMenu.SelectedDifficulty = selectedDifficulty;
                }
                else if (currentGameState == GameState.Instructions)
                {
                    instructionsMenu.Draw(_spriteBatch);
                }

                _spriteBatch.End(); // End the sprite batch for drawing.
                base.Draw(gameTime); // Ensures the framework-level logic in the base class is drawn.
                return;
            }

            // If the game has started, clear the screen and draw the game elements.
            GraphicsDevice.Clear(Color.CornflowerBlue); // Clear the screen with a blue color.
            _spriteBatch!.Begin(samplerState: SamplerState.PointClamp);

            // Draw the grid objects and other elements
            _player1grid!.DrawBackground(_spriteBatch);
            _player2grid!.DrawBackground(_spriteBatch);
            _shipManager!.Draw(_spriteBatch);
            _player1grid!.DrawForeground(_spriteBatch);
            _player2grid!.DrawForeground(_spriteBatch);
            _cursor.Draw(_spriteBatch);
            _turnManager!.Draw(_spriteBatch);

            // Check if a turn swap is waiting and draw the texture and feedback message
            if (_turnManager.SwapWaiting && selectedDifficulty == DifficultyState.Disabled)
            {
                _spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                //_spriteBatch.Draw(SwapTexture, new Vector2((GraphicsDevice.Viewport.Width - SwapTexture.Width) / 2,
                //                                            (GraphicsDevice.Viewport.Height - SwapTexture.Height) / 2), 
                //                Color.White);

                string feedbackMessage = _turnManager.IsP1sTurn ? "Player 2's Turn Finished!\nClick to Switch Player" : "Player 1's Turn Finished!\nClick to Switch Player";
                Vector2 messageSize = feedbackFont.MeasureString(feedbackMessage);
                Vector2 messagePosition = new Vector2((GraphicsDevice.Viewport.Width - messageSize.X) / 2,
                                                    (GraphicsDevice.Viewport.Height - SwapTexture.Height) / 2 + SwapTexture.Height);
                _spriteBatch.DrawString(feedbackFont, feedbackMessage, messagePosition, Color.Red);
            }
            _spriteBatch.End(); // End the sprite batch for drawing.
            base.Draw(gameTime); // Ensures the framework-level logic in the base class is drawn.
        }


private void BombShoot(Grid grid, ref int hitLimit)
{
    if (grid.CurrentTile == null)
    {
        return; // No valid tile selected
    }

    // Get the current tile's coordinates
    Tuple<int, int> tileCoordinates = grid.GridArray.CoordinatesOf(grid.CurrentTile);
    int centerX = tileCoordinates.Item1;
    int centerY = tileCoordinates.Item2;

    // Track the number of hits for this bomb shot
    int hitsMade = 0;

    // Loop over a 3x3 area centered around the clicked tile
    for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
    {
        for (int colOffset = -1; colOffset <= 1; colOffset++)
        {
            int newX = centerX + colOffset;
            int newY = centerY + rowOffset;

            // Ensure we're within bounds of the grid
            if (newX >= 0 && newX < grid.Size && newY >= 0 && newY < grid.Size)
            {
                // Temporarily set the current tile to the one in the 3x3 grid
                GridTile tileToShoot = grid.GridArray[newY, newX];

                if (!tileToShoot.IsShot) // Only shoot if the tile hasn't been shot already
                {
                    grid.CurrentTile = tileToShoot;
                    bool? success = grid.Shoot();

                    // If the shot was a hit, update the hit limit
                    if (success == true)
                    {
                        hitsMade++;
                    }
                }
            }
        }
    }

    // Adjust the hit limit by the number of hits made
    hitLimit -= hitsMade;

    // After the bomb shot, reset CurrentTile to avoid lingering selection issues
    grid.CurrentTile = null;
}

private void CarpetBombShoot(Grid grid, ref int hitLimit, CursorOrientation orientation)
{
    if (grid.CurrentTile == null)
    {
        return; // No valid tile selected
    }

    // Get the current tile's coordinates
    Tuple<int, int> tileCoordinates = grid.GridArray.CoordinatesOf(grid.CurrentTile);
    int centerX = tileCoordinates.Item1;
    int centerY = tileCoordinates.Item2;

    // Track the number of hits for this carpet bomb shot
    int hitsMade = 0;

    if (orientation == CursorOrientation.HORIZONTAL)
    {
        // Blow up the entire row (centerY row)
        for (int col = 0; col < grid.Size; col++)
        {
            GridTile tileToShoot = grid.GridArray[centerY, col];

            if (!tileToShoot.IsShot) // Only shoot if the tile hasn't been shot already
            {
                grid.CurrentTile = tileToShoot;
                bool? success = grid.Shoot();

                // If the shot was a hit, update the hit limit
                if (success == true)
                {
                    hitsMade++;
                }
            }
        }
    }
    else if (orientation == CursorOrientation.VERTICAL)
    {
        // Blow up the entire column (centerX column)
        for (int row = 0; row < grid.Size; row++)
        {
            GridTile tileToShoot = grid.GridArray[row, centerX];

            if (!tileToShoot.IsShot) // Only shoot if the tile hasn't been shot already
            {
                grid.CurrentTile = tileToShoot;
                bool? success = grid.Shoot();

                // If the shot was a hit, update the hit limit
                if (success == true)
                {
                    hitsMade++;
                }
            }
        }
    }

    // Adjust the hit limit by the number of hits made
    hitLimit -= hitsMade;

    // After the carpet bomb shot, reset CurrentTile to avoid lingering selection issues
    grid.CurrentTile = null;
}
        private ShotType selectedShot = ShotType.Normal; // Add shot selection: Normal or Bomb
        private int P1BombAmmo = 1; // Add a variable for Player 1 bomb ammo 
        private int P2BombAmmo = 1; // Add a variable for Player 2 bomb ammo 
        private int P1CarpetBombAmmo = 1; // carpet bomb ammo for Player 1
        private int P2CarpetBombAmmo = 1; // carpet bomb ammo for Player 2
        private ShotType P1Shot = ShotType.Normal; // Add a variable for Player 1
        private ShotType P2Shot = ShotType.Normal; // Add a variable for Player 2

        // Declare Player 1 and Player 2 carpet bomb orientations
        private CursorOrientation P1CarpetBombOrientation = CursorOrientation.HORIZONTAL;
        private CursorOrientation P2CarpetBombOrientation = CursorOrientation.HORIZONTAL;
        private Timer? _rotateTimeout; // Timer to debounce the rotation key


        private enum ShotType
        {
            Normal,
            Bomb,
            CarpetBomb
        }

        /// <summary>
        /// Handles shooting logic for the game.
        /// </summary>
        private void HandleShooting()
{
    // If the game is not in progress, return because there's nothing to shoot.
    if (!inGame)
    {
        return;
    }

    MouseState mouseState = Mouse.GetState(); // Get the current mouse state.
    KeyboardState keyboardState = Keyboard.GetState(); // Get the current keyboard state.

    // Handle Player's Shot Selection
    if (selectedDifficulty == DifficultyState.Disabled) // When the AI is disabled
    {
        if (_turnManager!.IsP1sTurn)
        {
            // Player 1 Shot Handling
            HandlePlayerShot(_player2grid, ref P2HitLimit, ref P1Shot, ref P1BombAmmo, ref P1CarpetBombAmmo, ref P1CarpetBombOrientation, keyboardState, mouseState);
        }
        else
        {
            // Player 2 Shot Handling
            HandlePlayerShot(_player1grid, ref P1HitLimit, ref P2Shot, ref P2BombAmmo, ref P2CarpetBombAmmo, ref P2CarpetBombOrientation, keyboardState, mouseState);
        }
    }
    else
    {
        // AI Shot Handling
        if (_turnManager!.IsP1sTurn)
        {
            // Player 1's turn logic (human)
            HandlePlayerShot(_player2grid, ref P2HitLimit, ref P1Shot, ref P1BombAmmo, ref P1CarpetBombAmmo, ref P1CarpetBombOrientation, keyboardState, mouseState);
        }
        else
        {
            // AI's turn logic
            HandleAIShot();
        }
    }

    // Check for Game Over
    if (P1HitLimit == 0 || P2HitLimit == 0)
    {
        EndGame();
    }
}

// Handle AI shooting logic
private void HandleAIShot()
{
    bool? success = false;
    
    // The AI logic for different difficulty levels
    if (selectedDifficulty == DifficultyState.Easy)
    {
        success = RandomAIAttack(_player1grid);
    }
    else if (selectedDifficulty == DifficultyState.Medium)
    {
        success = PriorityAIAttack(_player1grid, priorityAttacks);
    }
    else if (selectedDifficulty == DifficultyState.Hard)
    {
        success = GuaranteedHitAIAttack(_player1grid);
    }

    // Check if AI's shot was successful
    if (success != null)
    {
        // Decrement Player 1's hit limit if the AI hit a ship
        if (success == true)
        {
            P1HitLimit--;
        }

        // Check for win condition after AI's shot
        if (P1HitLimit == 0)
        {
            EndGame();
            return; // Ensure no further actions are taken after the game ends
        }

        // If the game hasn't ended, progress to the next turn
        _turnManager.NextTurn();
        _shipManager!.HideP1Ships = !_turnManager.IsP1sTurn;
        _shipManager.HideP2Ships = _turnManager.IsP1sTurn;
    }
}


private void HandlePlayerShot(Grid opponentGrid, ref int opponentHitLimit, ref ShotType selectedShot, ref int bombAmmo, ref int carpetBombAmmo, ref CursorOrientation carpetBombOrientation, KeyboardState keyboardState, MouseState mouseState)
{
    // Set default shot to normal
    selectedShot = ShotType.Normal;

    // Switch shot type if the player presses B (for Bomb) or C (for Carpet Bomb)
    if (keyboardState.IsKeyDown(Keys.B) && bombAmmo > 0)
    {
        selectedShot = ShotType.Bomb;
    }
    else if (keyboardState.IsKeyDown(Keys.C) && carpetBombAmmo > 0)
    {
        selectedShot = ShotType.CarpetBomb;
    }

    // Rotate Carpet Bomb if R is pressed
    if (selectedShot == ShotType.CarpetBomb && keyboardState.IsKeyDown(Keys.R))
    {
        carpetBombOrientation = (carpetBombOrientation == CursorOrientation.HORIZONTAL) ? CursorOrientation.VERTICAL : CursorOrientation.HORIZONTAL;
    }

    // Handle shot when the left mouse button is clicked
    if (_shipManager!.ReadClick && mouseState.LeftButton == ButtonState.Pressed)
    {
        _shipManager.ReadClick = false;
        bool? success = null;

        if (selectedShot == ShotType.Normal)
        {
            success = opponentGrid.Shoot();
            if (success == true)
            {
                opponentHitLimit--;
            }
        }
        else if (selectedShot == ShotType.Bomb && bombAmmo > 0)
        {
            BombShoot(opponentGrid, ref opponentHitLimit);
            bombAmmo--;
            success = true;
        }
        else if (selectedShot == ShotType.CarpetBomb && carpetBombAmmo > 0)
        {
            CarpetBombShoot(opponentGrid, ref opponentHitLimit, carpetBombOrientation);
            carpetBombAmmo--;
            success = true;
        }

        // Check for win condition after the player's shot
        if (opponentHitLimit == 0)
        {
            EndGame();
            return; // Ensure no further actions are taken after the game ends
        }

        // Progress the turn if a valid shot was made
        if (success != null)
        {
            _turnManager.NextTurn();
            _shipManager!.HideP1Ships = !_turnManager.IsP1sTurn;
            _shipManager.HideP2Ships = _turnManager.IsP1sTurn;
        }
    }
}


private void EndGame()
{
    inGame = false;
    currentGameState = GameState.MainMenu; // Transition to main menu or you could use a GameOver screen if you have one
    _turnManager.SwapWaiting = true;
    _shipManager.ReadClick = false;

    // Reset any necessary game variables
    P1BombAmmo = 1;
    P2BombAmmo = 1;
    P1CarpetBombAmmo = 1;
    P2CarpetBombAmmo = 1;

    base.Initialize(); // Reset the game to its initial state
}


private bool? RandomAIAttack(Grid grid)
{
    Random random = new Random();
    int gridSize = grid.GridArray.GetLength(0); // Get grid size
    int randomTileX, randomTileY;
    bool? success = null;

    // Repeats until a valid tile is randomly selected to be attacked
    while (success == null)
    {
        randomTileX = random.Next(1, gridSize);
        randomTileY = random.Next(1, gridSize);

        // Set the current tile to the randomly chosen one
        grid.CurrentTile = grid.GridArray[randomTileX, randomTileY];
        success = grid.Shoot();
    }

    return success;
}

private bool? PriorityAIAttack(Grid grid, List<(int, int)> priorityAttacks)
{
    Random random = new Random();
    int gridSize = grid.GridArray.GetLength(0); // Get grid size
    bool? success = null;

    // If priority attacks exist, use them, otherwise fall back to random attacks
    if (priorityAttacks.Count > 0)
    {
        // Process priority attack targets
        var priorityAttack = priorityAttacks[0];
        int tileX = priorityAttack.Item1;
        int tileY = priorityAttack.Item2;

        // Remove the target from the priority list
        priorityAttacks.RemoveAt(0);

        // Set the AI's current tile to the priority target
        grid.CurrentTile = grid.GridArray[tileX, tileY];
        success = grid.Shoot();

        // If a ship was hit, add the adjacent tiles to the priority list
        if (success == true)
        {
            AddAdjacentTilesToPriorityList(priorityAttacks, tileX, tileY, gridSize);
        }
    }
    else
    {
        // If no priority targets remain, fall back to random attack
        success = RandomAIAttack(grid);

        // If the AI randomly hits a ship, add the adjacent tiles to the priority list
        if (success == true)
        {
            // Get the coordinates of the random hit tile
            var hitTileCoordinates = grid.GridArray.CoordinatesOf(grid.CurrentTile);
            int tileX = hitTileCoordinates.Item1;
            int tileY = hitTileCoordinates.Item2;

            AddAdjacentTilesToPriorityList(priorityAttacks, tileX, tileY, gridSize);
        }
    }

    return success;
}


private void AddAdjacentTilesToPriorityList(List<(int, int)> priorityAttacks, int tileX, int tileY, int gridSize)
{
    // Add the adjacent tiles to the priority list, ensuring they are within the bounds of the grid
    if (tileX - 1 >= 0 && !priorityAttacks.Contains((tileX - 1, tileY))) // Left
    {
        priorityAttacks.Add((tileX - 1, tileY));
    }
    if (tileX + 1 < gridSize && !priorityAttacks.Contains((tileX + 1, tileY))) // Right
    {
        priorityAttacks.Add((tileX + 1, tileY));
    }
    if (tileY - 1 >= 0 && !priorityAttacks.Contains((tileX, tileY - 1))) // Up
    {
        priorityAttacks.Add((tileX, tileY - 1));
    }
    if (tileY + 1 < gridSize && !priorityAttacks.Contains((tileX, tileY + 1))) // Down
    {
        priorityAttacks.Add((tileX, tileY + 1));
    }
}


private bool? GuaranteedHitAIAttack(Grid grid)
{
    Random random = new Random();
    int gridSize = grid.GridArray.GetLength(0); // Get grid size
    bool? success = null;

    // Repeats until a valid ship tile is found to be attacked
    while (success == null)
    {
        int randomTileX = random.Next(1, gridSize);
        int randomTileY = random.Next(1, gridSize);

        grid.CurrentTile = grid.GridArray[randomTileX, randomTileY];

        // Only attack if the tile has a ship
        if (grid.CurrentTile.HasShip)
        {
            success = grid.Shoot();
        }
    }

    return success;
}
    }
}
