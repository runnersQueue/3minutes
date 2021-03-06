﻿/*
 * File: GameController.cs
 * Author: Michael Johannes and Carlos Santana
 * Desc: Contains game logic for manipulating game elements
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CC_X.Model
{
    public enum Difficulty { Easy, Medium, Hard } // Sets the levels' difficulty
    public enum Level { One, Two, Three} // Sets the level

    //Controls game play
    class GameController
    {
        public Dictionary<uint, GameObj> GameObjCollection { get; set; } // Contains Nature and Enemy objects      
        public MainCharacter MainChar = new MainCharacter(new Vector3(75, -0.50523f, 1.62f)); // Instance of MainCharacter
        public Enemy foe = new Enemy(new Vector3(0,0,0)); // Instance of foe
        public Difficulty DifficutlySelected { get; set; } // Returns Difficulty selected
        public Vector3 EndGameZone { get; set; } // Location of the finish line
        public bool GameOver { get; set; } // Ends the game when the character dies (or crosses the final finish line)
        public int CurrentTime { get; set; } // Time counter for the level
        public int Level1QualTime { get; set; } // Time the character needs to meet to unlock level 2
        public int Level2QualTime { get; set; } // Time the character needs to meet to unlock level 3

        public int Level3QualTime { get; set; } // Time the character needs to meet to win the game
        public int LastLevelTime { get; set; } // Time the character achieves in the last level
        public bool Level1Complete { get; set; } // Determines whether Level 1 has been successfully completed
        public bool Level2Complete { get; set; } // Determines whether Level 2 has been successfully completed
        public bool Level3Complete { get; set; } // Determines whether Level 3 has been successfully completed
        public bool CheatModeEn = false; // Determines whether Cheat Mode is active


        public Level CurrentLevel = Level.One; // Retrieves the current level. Default set to 1 for the beginning
        public HighScore highscore = new HighScore(); // Returns the total points accumulated

        //View will set this to user input name
        public string MainCharName = "";

        //IObserver object
        public IObserver gui { get; set; }

        //Receives selected difficulty from view and generates level according to difficulty
        public GameController(Difficulty difficulty)
        {
            GameOver = false;
            GameObjCollection = new Dictionary<uint, GameObj>();
            DifficutlySelected = difficulty;
            Level1Complete = false;
            Level2Complete = false;
            Level3Complete = false;
        }

        //Returns true when level is over
        public bool EndLevel()
        {
            if(Math.Abs(MainChar.Position.X - EndGameZone.X) <= 5 && Math.Abs(MainChar.Position.Z - EndGameZone.Z) <= 1)
            {
                GameOver = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        // Toggles Cheat Mode
        public void EnableCheat()
        {
            CheatModeEn = !CheatModeEn;
            MainChar.Invinsible = !MainChar.Invinsible;
        }

        // Switches the Qualifying Time and Level Number according to the level number
        // Also, advances the level if the qualifications are met, while calculating experience gained
        public bool PassLevel()
        {
            int QualTime = 0;
            Level currLevel = Level.One;
            if(CurrentLevel == Level.One) { QualTime = Level1QualTime; }
            else if (CurrentLevel == Level.Two) { currLevel = Level.Two; QualTime = Level2QualTime; }
            else if (CurrentLevel == Level.Three) { currLevel = Level.Three; QualTime = Level3QualTime; }
            if (CurrentTime <= QualTime && MainChar.IsDead == false)
            {
                CalcExperience();
                if (currLevel == Level.One) { Level1Complete = true; }
                if (currLevel == Level.Two) { Level2Complete = true; }
                if (currLevel == Level.Three) { Level3Complete = true; }
                return true;
            }
            else { return false; }
        }

        // Returns a completed/failed message depending on whether or not qualifications were met
        public string GetLevelStatus()
        {
            if (PassLevel()) { return "Completed"; }
            else { return "Failed"; }
        }

        // Returns true if collided with other GameObj objects.
        // If (GameObj object == Enemy) && (MainChar.PositionSinceLastCollide != MainChar.Position), subtracts enemy damage (power) from health
        public List<object> DetectCollision()
        {
            if (MainChar != null)
            {
                UpdateCurrentTime();
                foreach (GameObj obj in GameObjCollection.Values)
                {
                    if (obj is Enemy)
                    {
                        if (MainChar.persnlBubble.IntersectsWith(((Enemy)(obj)).persnlBubble) && MainChar.TimeSinceLastCollide != CurrentTime)
                        {
                            MainChar.TimeSinceLastCollide = CurrentTime;
                            MainChar.ReceiveDamage(((Enemy)obj).Strength);
                            return new List<object>() { true, obj.Position };
                        }
                    }
                }
                return new List<object>() { false, new Vector3(-1000000, -1000000, -1000000) };
            }
            else
            {
                return new List<object>() { false, new Vector3(-1000000, -1000000, -1000000) };
            }
        }

        // Makes the GUI update the time remotely via the observer
        public void UpdateCurrentTime()
        {
            gui.UpdateCurrentTime();
        }
        
        // Retrieves the qualifying time necessary for each level
        public int GetQualTime()
        {
            if(CurrentLevel == Level.One) { return Level1QualTime; }
            if (CurrentLevel == Level.Two) { return Level2QualTime; }
            if (CurrentLevel == Level.Three) { return Level3QualTime; }
            else { return -1; }
        }

        //Calculates player's experience
        public void CalcExperience()
        {
            int QualTime = 0;
            bool currLevelPass = false;
            if (CurrentLevel == Level.One) { currLevelPass = Level1Complete; QualTime = Level1QualTime; }
            if (CurrentLevel == Level.Two) { currLevelPass = Level2Complete; QualTime = Level2QualTime; }
            if (CurrentLevel == Level.Three) { currLevelPass = Level3Complete; QualTime = Level3QualTime; }
            if (LastLevelTime != null)
            {
                if(CurrentTime <= QualTime && MainChar.IsDead == false && currLevelPass == false)
                {                   
                    int experience = (int)(Math.Round((double)((QualTime - CurrentTime)/10), 0)*1000 + 1000);
                    MainChar.Experience += experience;
                }
            }
        }

        // Calculates the points achieved at the end of the level
        public void CalcPoints(Level level, Difficulty difficulty, int time)
        {
            throw new NotImplementedException();
        }


        //Populate WorldCollection with level 1 world objects/coordinates according to difficutly
        public void SetUpLevel1(Difficulty difficulty)
        {
            EndGameZone = new Vector3(75, 0, 124);
            MainChar.Position = new Vector3(75, -0.50523f, 1.62f);
            SetUpDifficulty(difficulty, Level.One);
            gui.SetUpLevel(Level.One, difficulty);
            
        }

        //Populate WorldCollection with level 2 world objects/coordinates according to difficutly
        private void SetUpLevel2(Difficulty difficulty)
        {
            EndGameZone = new Vector3(75, 0, 124);
            MainChar.Position = new Vector3(75, -0.50523f, 1.62f);
            CurrentLevel = Level.Two;
            SetUpDifficulty(difficulty, Level.Two);
            gui.SetUpLevel(Level.Two, difficulty);
        }

        //Populate WorldCollection with level 3 world objects/coordinates according to difficutly
        private void SetUpLevel3(Difficulty difficulty)
        {
            EndGameZone = new Vector3(75, 0, 124);
            MainChar.Position = new Vector3(75, -0.50523f, 1.62f);
            CurrentLevel = Level.Three;
            SetUpDifficulty(difficulty, Level.Three);
            gui.SetUpLevel(Level.Three, difficulty);
        }

        // Calls the above "SetUpLevelX" methods according to which level is active.
        public void SetUpLevel(Level level)
        {
            switch(level)
            {
                case Level.One:
                    {
                        SetUpLevel1(DifficutlySelected);
                        break;
                    }
                case Level.Two:
                    {
                        SetUpLevel2(DifficutlySelected);
                        break;
                    }
                case Level.Three:
                    {
                        SetUpLevel3(DifficutlySelected);
                        break;
                    }
            }
        }

        // Erase the level, restore original settings
        public void ResetLevel()
        {
            GameObjCollection = new Dictionary<uint, GameObj>();

            //Reset Main Character
            MainChar.Position = new Vector3(75, -0.50523f, 1.62f);

            GameOver = false;
            MainChar.Health = 100;
            MainChar.IsDead = false;
            CurrentTime = 0;

            gui.ResetLevel();
        }

        // Set qualifying time for the three levels under Easy difficulty
        public void SetUpDifficultyEasy(Level level)
        {
            if(level == Level.One) { Level1QualTime = 90; }
            if(level == Level.Two) { Level2QualTime = 100; }
            if(level == Level.Three) { Level3QualTime = 110; }
        }

        // Set qualifying time for the three levels under Medium difficulty
        public void SetUpDifficultyMedium(Level level)
        {
            if (level == Level.One) { Level1QualTime = 70; }
            if (level == Level.Two) { Level2QualTime = 80; }
            if (level == Level.Three) { Level3QualTime = 90; }
        }

        // Set qualifying time for the three levels under Hard difficulty
        public void SetUpDifficultyHard(Level level)
        {
            if (level == Level.One) { Level1QualTime = 62; }
            if (level == Level.Two) { Level2QualTime = 72; }
            if (level == Level.Three) { Level3QualTime = 82; }
        }

        // Calls each of the above "SetUpDifficultyX" methods according to the difficulty setting
        public void SetUpDifficulty(Difficulty difficulty, Level level)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    {
                        SetUpDifficultyEasy(level);
                        break;
                    }
                case Difficulty.Medium:
                    {
                        SetUpDifficultyMedium(level);
                        break;
                    }
                case Difficulty.Hard:
                    {
                        SetUpDifficultyHard(level);
                        break;
                    }
            }
        }

        // Load/save mechanism
        // Uploads the file, parses its contents into a string array, and deserializes each line.
        public void Load(string filepath)
        {
            string[] temp = File.ReadAllLines(filepath);
            MainChar.DeSerialize(temp[0]);

            for (int i = 1; i < temp.Count(); ++i) // Aid found at: http://stackoverflow.com/questions/3878820/c-how-to-get-first-char-of-a-string
            {
                string TempString = temp[i];
                if (TempString[0] == '+')
                {
                    TempString = TempString.Substring(1);
                    foe.DeSerialize(TempString);
                }
                else if (TempString[0] == '-')
                {
                    TempString = TempString.Substring(1);
                    Nature.NatureType tempType = Nature.NatureType.Plane;
                    Vector3 num = new Vector3(0, 0, 0);
                    var tempNature = new Nature(tempType, num);
                    tempNature.DeSerialize(TempString);
                }

            }
        }

        // Serializes the data, adds it as a line to a StringBuilder, and then writes all lines in the csv to the Save Data File
        public void Save(string filepath) // Aid found at: http://stackoverflow.com/questions/18757097/writing-data-into-csv-file
        {
            // Use a StringBuilder
            var csv = new StringBuilder();
            csv.AppendLine(MainChar.Serialize());
            foreach (KeyValuePair<uint, GameObj> entry in GameObjCollection) // Aid found at: http://stackoverflow.com/questions/141088/what-is-the-best-way-to-iterate-over-a-dictionary-in-c
            {
                if (entry.Value is Enemy)
                {
                    var tempEnemy = (Enemy)entry.Value;
                    csv.AppendLine('+' + tempEnemy.Serialize());
                }
                else if (entry.Value is Nature)
                {
                    var tempNature = (Nature)entry.Value;
                    csv.AppendLine('-' + tempNature.Serialize());
                }
                else
                    continue;
            }
            File.WriteAllText(filepath, csv.ToString());
        }
    }
}
