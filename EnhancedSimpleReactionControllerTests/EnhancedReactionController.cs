using System;
using System.IO;

namespace SimpleReactionMachine
{
    public class EnhancedReactionController : IController
    {
        // Game timing constants
        private const int MIN_REACTION_DURATION = 100; // Minimum random wait duration: 1 second (in ticks)
        private const int MAX_READY_DURATION = 1000; // Max time in 'Ready' state before timeout
        private const int MAX_REACTION_DURATION = 250; // Maximum random wait duration: 2.5 seconds (in ticks)
        private const int MAX_GAME_DURATION = 200; // Maximum allowed reaction time: 2 seconds (in ticks)
        private const int GAMEOVER_DURATION = 300; // Duration to display result: 3 seconds (in ticks)
        private const double TICKS_PER_SECOND = 100.0; // Conversion factor: 10ms per tick
        private const int RESULT_DURATION = 500; // Duration to display average result: 5 seconds (in ticks)
        private const int MAX_GAMES = 3; // Maximum number of games allowed per coin

        // State and dependencies
    
        private ControllerMode currentMode;  // Holds the current state of the controller
        private IGui guiInterface { get; set; } // Interface for interacting with the graphical user interface
        private IRandom randomGenerator { get; set; }  // Interface for generating random numbers
        private int TickCounter { get; set; }  // Tracks the number of ticks elapsed since a particular event started
        private int gamesPlayedCount { get; set; }  // Keeps track of the number of games played by the user
        private int FinalReactionTime { get; set; } // Stores the cumulative reaction time of the user across multiple games

        // Establish connections to the GUI and RNG
        public void Connect(IGui gui, IRandom rng)
        {
            this.guiInterface = gui;
            this.randomGenerator = rng;
            Init();
        }

        // Initialize controller state
        public void Init()
        {
            currentMode = new IdleMode(this); // Set initial mode to Idle
            guiInterface.SetDisplay("Insert Coin"); // Set GUI display message
            gamesPlayedCount = 0;  // Initialize games played count
        } 

        // Handle coin insertion event
        public void CoinInserted()
        {
            currentMode.CoinInserted();  // Delegate to current mode to handle coin insertion
        }

        // Handle Go/Stop button press event
        public void GoStopPressed()
        {
            currentMode.GoStopPressed();  // Delegate to current mode to handle Go/Stop button press
        }

        // Handle timer tick event
        public void Tick()
        {
            currentMode.Tick(); // Delegate to current mode to handle timer tick
        }

        // Change the controller's current state
        void ChangeState(ControllerMode newState)
        {
            currentMode = newState; // Update current mode to the new state
        }

        // Abstract base class for different game states
        abstract class ControllerMode
        {
            protected EnhancedReactionController controller;

            // Constructor to initialize the controller reference
            public ControllerMode(EnhancedReactionController controller)
            {
                this.controller = controller;
            }

            // Abstract methods to handle events in different states
            public abstract void CoinInserted();
            public abstract void GoStopPressed();
            public abstract void Tick();
        }

        // State for displaying the result after all games are played
        class ResultMode : ControllerMode
        {
            public ResultMode(EnhancedReactionController controller) : base(controller)
            {
                // Calculate and display average reaction time
                double averageReactionTime = (double)controller.FinalReactionTime / controller.gamesPlayedCount * 0.01;
                // Set the display to show the average reaction time
                controller.guiInterface.SetDisplay("Average: " + averageReactionTime.ToString("0.00"));
                // Reset the tick counter
                controller.TickCounter = 0;
            }

            // Transition to IdleMode when a coin is inserted
            public override void CoinInserted()
            {
                controller.ChangeState(new IdleMode(controller));
            }

            // Transition to IdleMode when Go/Stop button is pressed
            public override void GoStopPressed()
            {
                controller.ChangeState(new IdleMode(controller));
            }

            // Increment tick counter and transition to IdleMode after result duration
            public override void Tick()
            {
                controller.TickCounter++;
                if (controller.TickCounter == RESULT_DURATION)
                {
                    controller.ChangeState(new IdleMode(controller));
                }
            }
        }

        // State for waiting after a coin is inserted
        class ReadyMode : ControllerMode
        {
            public ReadyMode(EnhancedReactionController controller) : base(controller)
            {
                // Set the display message to prompt the user to press Go
                base.controller.guiInterface.SetDisplay("Press Go!");
            }

            public override void CoinInserted() { } // No action when coin is inserted
                                            
            // Transition to WaitingMode when Go/Stop button is pressed
            public override void GoStopPressed()
            {
                controller.ChangeState(new WaitingMode(controller));
            }

            // Increment tick counter and transition to IdleMode if timeout reached
            public override void Tick()
            {
                controller.TickCounter++;
                if (controller.TickCounter == MAX_READY_DURATION)
                    controller.ChangeState(new IdleMode(controller));
            }
        }

        // State for waiting for a coin to be inserted
        class IdleMode : ControllerMode
        {
            public IdleMode(EnhancedReactionController controller) : base(controller)
            {
                // Set the display message to prompt the user to insert a coin
                base.controller.guiInterface.SetDisplay("Insert Coin");
            }

            // Transition to ReadyMode when a coin is inserted
            public override void CoinInserted()
            {
                if (controller.gamesPlayedCount < MAX_GAMES)
                {
                    controller.ChangeState(new ReadyMode(controller));
                }
                else
                {
                    // Display max games played message and transition to GameOverMode
                    controller.guiInterface.SetDisplay("Max games played");
                    controller.ChangeState(new GameOverMode(controller));
                }
            }
            // No action when Go/Stop button is pressed
            public override void GoStopPressed() { }
            // No action during ticks in IdleMode
            public override void Tick() { }
        }

        // State for waiting for a random time before user can react
        class WaitingMode : ControllerMode
        {
            private int waitTime;

            public WaitingMode(EnhancedReactionController controller) : base(controller)
            {
                // Set the display message to indicate waiting
                base.controller.guiInterface.SetDisplay("Wait...");
                base.controller.TickCounter = 0;
                // Generate random wait time within specified range
                waitTime = base.controller.randomGenerator.GetRandom(MIN_REACTION_DURATION, MAX_REACTION_DURATION);
            }

            // No action when coin is inserted during waiting
            public override void CoinInserted() { }

            // Transition to IdleMode when Go/Stop button is pressed
            public override void GoStopPressed()
            {
                controller.ChangeState(new IdleMode(controller));
            }

            // Increment tick counter and transition to RunningMode after wait time elapses
            public override void Tick()
            {
                controller.TickCounter++;
                if (controller.TickCounter == waitTime)
                {
                    controller.gamesPlayedCount++;
                    controller.ChangeState(new RunningMode(controller));
                }
            }
        }

        // State for handling game over logic
        class GameOverMode : ControllerMode
        {
            public GameOverMode(EnhancedReactionController controller) : base(controller)
            {
                // Reset tick counter when entering GameOverMode
                base.controller.TickCounter = 0;
            }

            // No action when coin is inserted during game over
            public override void CoinInserted() { }

            // Transition to next state when Go/Stop button is pressed
            public override void GoStopPressed()
            {
                TransitionToNextState();
            }

            // Increment tick counter and transition to next state after timeout
            public override void Tick()
            {
                controller.TickCounter++;
                if (controller.TickCounter == GAMEOVER_DURATION)
                {
                    TransitionToNextState();
                }
            }

            // Helper method to transition to next state based on games played count
            private void TransitionToNextState()
            {
                if (controller.gamesPlayedCount == MAX_GAMES)
                {
                    controller.ChangeState(new ResultMode(controller));
                }
                else
                {
                    controller.ChangeState(new WaitingMode(controller));
                }
            }
        }

        // State for running the game, waiting for user reaction
        class RunningMode : ControllerMode
        {
            public RunningMode(EnhancedReactionController controller) : base(controller)
            {
                // Set initial display to show reaction time
                base.controller.guiInterface.SetDisplay("0.00");
                base.controller.TickCounter = 0;
            }

            // No action when coin is inserted during running mode
            public override void CoinInserted() { }

            // Update final reaction time and transition to GameOverMode when Go/Stop button is pressed
            public override void GoStopPressed()
            {
                controller.FinalReactionTime += controller.TickCounter;
                controller.ChangeState(new GameOverMode(controller));
            }

            // Update display with current time, transition to GameOverMode after maximum game duration
            public override void Tick()
            {
                controller.TickCounter++;
                // Update display with current time
                controller.guiInterface.SetDisplay((controller.TickCounter / TICKS_PER_SECOND).ToString("0.00"));
                // Transition to GameOverMode after maximum game duration
                if (controller.TickCounter == MAX_GAME_DURATION)
                {
                    controller.ChangeState(new GameOverMode(controller));
                }
            }
        }

        
    }
}
