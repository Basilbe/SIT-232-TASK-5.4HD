using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleReactionMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedSimpleReactionControllerTests
{
    // Unit test class for EnhancedReactionController
    [TestClass]
    public class EnhancedReactionConrtollerTests
    {
        // Define static variables to hold instances of test objects
        private static IController testcontroller;
        private static IGui guiInterface;
        private static IRandom randomGenerator;
        private static string currentDisplayText;
        private static int RandomValue { get; set; }


        private void InitialiseToIdleMode(IController controller, IGui gui, IRandom rng)
        {
            // This method initializes the controller to the Idle mode.
            gui.Connect(controller);  // connect Gui to the controller
            controller.Connect(gui, rng); // Connect controller to GUI and random number generator
            gui.Init();  // Initialize the GUI
            controller.Init();  // Initialize the controller
        }

        // Method to initialize the controller to the Ready mode
        private void InitialiseToReadyMode(IController controller, IGui gui, IRandom rng)
        {
            // This method initializes the controller to the Ready mode.
            InitialiseToIdleMode(controller, gui, rng);
            controller.CoinInserted();  // Simulate coin insertion
        }

        // Method to initialize the controller to the Waiting mode
        private void InitialiseToWaitngMode(IController controller, IGui gui, IRandom rng)
        {
            // This method initializes the controller to the Waiting mode.
            InitialiseToReadyMode(controller, gui, rng);
            controller.GoStopPressed(); // Simulate GoStop button press
        }

        private void InitialiseToRunningMode(IController controller, IGui gui, IRandom rng)
        {
            // This method initializes the controller to the Running mode.
            // It first initializes the controller to the Waiting mode, then simulates ticks on the controller for a specified number of times.
            InitialiseToWaitngMode(controller, gui, rng);
            for (int t = 0; t < RandomValue; t++)
            {
                controller.Tick();    // Simulate ticks on the controller
            }
        }

        // Implementation of the IGui interface for testing purposes
        private class TestGui : IGui
        {
            private IController _controller;
            // connects  GUI to the controller.
            public void Connect(IController controller)
            {
                _controller = controller;
            }

            // Initializes the GUI with a default display text
            public void Init()
            {
                currentDisplayText = "?reset?";
            }

            // Updates the display text on the GUI.
            public void SetDisplay(string msg)
            {
                currentDisplayText = msg;
            }

        }

        // Implementation of the IRandom interface for testing purposes
        private class RndGenerator : IRandom
        {
            Random rnd = new Random(42);  // Random number generator

            // Generates a random number within a given range.
            public int GetRandom(int from, int to)
            {
                int generatedValue = rnd.Next(from) + to;
                RandomValue = generatedValue;   // Update RandomValue property with the generated value
                return RandomValue;
            }
        }

        // Method to prepare test objects before each test method
        [TestInitialize]
        public void Initialize()
        {    
            // Creates instances of the controller, GUI interface, and connects them
            testcontroller = new EnhancedReactionController();
            guiInterface = new TestGui();
            testcontroller.Connect(guiInterface, new RndGenerator());
        }

        // Test method to verify the creation of the controller
        [TestMethod]
        public void Test_ControllerCreation()
        {
            // Verifies that the controller is created successfully and is not null
            Assert.IsNotNull(testcontroller);
        }

        // Test method to check connection and initialization of the controller
        [TestMethod]
        public void Test_ControllerConnectAndInitialize()
        {

            // Initializes the controller, ensuring it starts in the IdleMode and displays "Insert coin"
            testcontroller.Init();
            Assert.AreEqual("Insert Coin", currentDisplayText);

        }

        [TestMethod]
        public void Test_OnMode_GoStopPressed()
        {
            testcontroller.Init();  // Initializes the controller
            // GoStopPressed has no effect in IdleMode
            Assert.AreEqual("Insert Coin", currentDisplayText); // Checks if the display text is still "Insert Coin" after pressing GoStop in IdleMode
            testcontroller.GoStopPressed(); // Presses GoStop button
            Assert.AreEqual("Insert Coin", currentDisplayText); // Checks if the display text remains "Insert Coin"
        }
    

        [TestMethod]
        public void Test_OnState_Tick()
        {
            testcontroller.Init();
            // Tick has no effect in IdleMode
            Assert.AreEqual("Insert Coin", currentDisplayText); 
            testcontroller.Tick();
            Assert.AreEqual("Insert Coin", currentDisplayText);
        }

        [TestMethod]
        public void Test_OnMode_CoinInserted()
        {
            testcontroller.Init();
            // Inserting a coin sets the state to ReadyMode, display should then be "Press Go!"
            Assert.AreEqual("Insert Coin", currentDisplayText);
            testcontroller.CoinInserted();
            Assert.AreEqual("Press Go!", currentDisplayText);
        }

        [TestMethod]
        public void Test_ReadyMode_CoinInserted()
        {
            InitialiseToReadyMode(testcontroller, guiInterface, randomGenerator);
            // Inserting a coin has no effect in ReadyMode
            Assert.AreEqual("Press Go!", currentDisplayText);
            testcontroller.CoinInserted();
            Assert.AreEqual("Press Go!", currentDisplayText);
        }

        [TestMethod]
        public void Test_ReadyState_Tick()
        {

            InitialiseToReadyMode(testcontroller, guiInterface, randomGenerator);
            // Tick has no effect in ReadyMode
            Assert.AreEqual("Press Go!", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("Press Go!", currentDisplayText);
        }

        [TestMethod]
        public void Test_ReadyMode_GoStopPressed()
        {
            
            InitialiseToReadyMode(testcontroller, guiInterface, randomGenerator);

            // Pressing GoStop sets the state to WaitingMode, display should then be "Wait..."
            Assert.AreEqual("Press Go!", currentDisplayText);
            testcontroller.GoStopPressed();
            Assert.AreEqual("Wait...", currentDisplayText);
        }

        [TestMethod]
        public void Test_ReadyMode_Too_Long()
        { 
            InitialiseToReadyMode(testcontroller, guiInterface, randomGenerator);

            // Waiting for 10 seconds in WaitState resets the controller back to OnState
            // Display should then be "Insert coin"
            for (int t = 0; t < 999; t++) testcontroller.Tick();
            Assert.AreEqual("Press Go!", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("Insert Coin", currentDisplayText);
        }

        [TestMethod]
        public void Test_WaitMode_CoinInserted()
        {
            InitialiseToWaitngMode(testcontroller, guiInterface, randomGenerator);

            // Inserting a coin has no effect in WaitingMode
            Assert.AreEqual("Wait...", currentDisplayText);
            testcontroller.CoinInserted();
            Assert.AreEqual("Wait...", currentDisplayText);
        }

        [TestMethod]
        public void Test_WaitMode_GoStopPressed()
        {
            InitialiseToWaitngMode(testcontroller, guiInterface, randomGenerator);

            // GoStopPressed in the WaitingMode is considered cheating and it sets the game back to the OnState
            // Display should then be "Insert coin"
            Assert.AreEqual("Wait...", currentDisplayText);
            testcontroller.GoStopPressed();
            Assert.AreEqual("Insert Coin", currentDisplayText);
        }

        [TestMethod]
        public void Test_WaitMode_Tick()
        {
            InitialiseToWaitngMode(testcontroller, guiInterface, randomGenerator);

            // After the random wait time, the controller should be set to the RunningMode
            // Display should then be "0.00"
            for (int t = 0; t < RandomValue - 1; t++) testcontroller.Tick();
            Assert.AreEqual("Wait...", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("0.00", currentDisplayText);
        }

        [TestMethod]
        public void Test_RunningMode_CoinInserted()
        {

            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // CoinInserted has no effect in the RunningMode
            Assert.AreEqual("0.00", currentDisplayText);
            testcontroller.CoinInserted();
            Assert.AreEqual("0.00", currentDisplayText);
        }

        [TestMethod]
        public void Test_RunningMode_Tick()
        {

            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // Ticks advance the time display in the RunningMode
            Assert.AreEqual("0.00", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("0.01", currentDisplayText);

            for (int t = 0; t < 10; t++) testcontroller.Tick();
            Assert.AreEqual("0.11", currentDisplayText);

            for (int t = 0; t < 100; t++) testcontroller.Tick();
            Assert.AreEqual("1.11", currentDisplayText);

            // GoStopPressed should advance to the GameOverMode and no further update to the display
            testcontroller.GoStopPressed();
            Assert.AreEqual("1.11", currentDisplayText);
        }

        [TestMethod]
        public void Test_RunningMode_GoStopPressed()
        {
            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // GoStopPressed records the reaction time in the RunningMode
            // and advances the controller to the GameOverMode
            // Display should be the same as the reaction time when GoStop is pressed;
            for (int t = 0; t < 164; t++) testcontroller.Tick();
            Assert.AreEqual("1.64", currentDisplayText);
            testcontroller.GoStopPressed();
            Assert.AreEqual("1.64", currentDisplayText);
        }

        [TestMethod]
        public void Test_RunningMode_Tick_Two_Seconds()
        {

            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // Not reacting in 2 seconds automatically ends the game
            // Display should show 2.00 seconds
            for (int t = 0; t < 199; t++) testcontroller.Tick();
            Assert.AreEqual("1.99", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("2.00", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("2.00", currentDisplayText);
        }

        [TestMethod]
        public void Test_GameOverMode_CoinInserted()
        {
            testcontroller = new EnhancedReactionController();
            guiInterface = new TestGui();
            randomGenerator = new RndGenerator();
            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // Inserting a coin has no effect in GameOverMode
            for (int t = 0; t < 22; t++) testcontroller.Tick();
            Assert.AreEqual("0.22", currentDisplayText);
            testcontroller.CoinInserted();
            Assert.AreEqual("0.22", currentDisplayText);
        }

        [TestMethod]
        public void Test_GameOverMode_Tick()
        {
            testcontroller = new EnhancedReactionController();
            guiInterface = new TestGui();
            randomGenerator = new RndGenerator();
            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // Tick shows the reaction time and then sets the controller to the WaitMode
            // NOTE: This test does not test the transition to the ResultMode. That is tested in Test_Play_Three_Games_And_Wait_Ticks
            for (int t = 0; t < 50; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            Assert.AreEqual("0.50", currentDisplayText);
            for (int t = 0; t < 299; t++) testcontroller.Tick();
            Assert.AreEqual("0.50", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("Wait...", currentDisplayText);
        }

        [TestMethod]
        public void Test_GameOverMode_GoStopPressed()
        {
            testcontroller = new EnhancedReactionController();
            guiInterface = new TestGui();
            randomGenerator = new RndGenerator();
            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

           // When GoStopPressed is called, it promptly transitions from the GameOverMode to the WaitingMode.
           // Note: This test does not verify the transition to the ResultMode after three games, which is tested in Test_Play_Three_Games_And_GoStopPressed.
            for (int t = 0; t < 56; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            Assert.AreEqual("0.56", currentDisplayText);
            testcontroller.GoStopPressed();
            Assert.AreEqual("Wait...", currentDisplayText);
        }

        [TestMethod]
        public void Test_Play_Three_Games_And_Wait_Ticks()
        {

            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // Run three games and then wait the final 3 seconds
            // State should advance to ResultMode
            // Display should then show the average reaction time
            for (int t = 0; t < 20; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            Assert.AreEqual("0.20", currentDisplayText);
            for (int t = 0; t < 299; t++) testcontroller.Tick();
            Assert.AreEqual("0.20", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("Wait...", currentDisplayText);

            for (int t = 0; t < RandomValue + 30; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            Assert.AreEqual("0.30", currentDisplayText);
            for (int t = 0; t < 299; t++) testcontroller.Tick();
            Assert.AreEqual("0.30", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("Wait...", currentDisplayText);

            for (int t = 0; t < RandomValue + 40; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            Assert.AreEqual("0.40", currentDisplayText);
            for (int t = 0; t < 299; t++) testcontroller.Tick();
            testcontroller.Tick();
            Assert.AreEqual("Average: 0.30", currentDisplayText);
        }

        [TestMethod]
        public void Test_Play_Three_Games_And_GoStopPressed()
        {
            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // Run three games and then press GoStop
            // State should advance to ResultMode immediately
            // Display should then show the average reaction time
            for (int t = 0; t < 155; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            Assert.AreEqual("1.55", currentDisplayText);
            testcontroller.GoStopPressed();
            Assert.AreEqual("Wait...", currentDisplayText);

            for (int t = 0; t < RandomValue + 160; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            Assert.AreEqual("1.60", currentDisplayText);
            testcontroller.GoStopPressed();
            Assert.AreEqual("Wait...", currentDisplayText);

            for (int t = 0; t < RandomValue + 165; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            Assert.AreEqual("1.65", currentDisplayText);
            Assert.AreEqual("1.65", currentDisplayText);
        }

        [TestMethod]
        public void Test_ResultMode_CoinInserted()
        {

            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            //To play 3 games
            for (int t = 0; t < 10; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            for (int t = 0; t < RandomValue + 15; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            for (int t = 0; t < RandomValue + 20; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            // Inserting a coin in the ResultMode has no effect
            Assert.AreEqual("Average: 0.15", currentDisplayText);
            testcontroller.CoinInserted();
            Assert.AreEqual("Insert Coin", currentDisplayText);
        }

        [TestMethod]
        public void Test_ResultMode_Ticks()
        {

            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // to play 3 games
            for (int t = 0; t < 10; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            for (int t = 0; t < RandomValue + 15; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            for (int t = 0; t < RandomValue + 20; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            // Play three games
            // Ticks displays the average reaction time for 5 seconds
            // and then the controller is set to IdleMode
            // Display should then be "Insertcoin"
            Assert.AreEqual("Average: 0.15", currentDisplayText);
            for (int i = 0; i < 499; i++) testcontroller.Tick();
            Assert.AreEqual("Average: 0.15", currentDisplayText);
            testcontroller.Tick();
            Assert.AreEqual("Insert Coin", currentDisplayText);
        }

        [TestMethod]
        public void Test_ResultMode_GoStopPressed()
        {

            InitialiseToRunningMode(testcontroller, guiInterface, randomGenerator);

            // to play 3 games
            for (int t = 0; t < 10; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            for (int t = 0; t < RandomValue + 15; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            for (int t = 0; t < RandomValue + 20; t++) testcontroller.Tick();
            testcontroller.GoStopPressed();
            testcontroller.GoStopPressed();

            // GoStopPressed displays the average reaction time for 5 seconds
            // and then the controller is set to IdleMode
            // Display should then be "Insertcoin"
            Assert.AreEqual("Average: 0.15", currentDisplayText);
            testcontroller.GoStopPressed();
            Assert.AreEqual("Insert Coin", currentDisplayText);
        }
    }
}
