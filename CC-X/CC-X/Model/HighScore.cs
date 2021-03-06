﻿/*
 * File: HighScore.cs
 * Author: Joshua Case
 * Desc: Mechanism for saving and ranking high scores
 */

using System;
using System.IO; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC_X.Model
{
    //Only the top 10 highscores will be saved.
    class HighScore
    {
        //This is the name of the file that the highscores will be written to. 
        const string highScoreFile = "highScore.txt";

        //Holds a list of Score Objects 
        public List<Score> collectionScoreObj = new List<Score>();

        //This method will write all of the high scores to the "highScore.txt" file. 
        //It will take in a list of all the data points and overwrite the contents of the file
        //the data will be stored on separate lines separated by a space
        //player_name " " score
        public void WriteToFile()
        {
            if (File.Exists(@"C:highScore.txt"))   //How to delete a file referenced StackOverflow: http://stackoverflow.com/questions/6391711/how-to-delete-a-file-after-checking-whether-it-exists
            {
                File.Delete(@"C:highScore.txt");
            }
            using (StreamWriter writer = new StreamWriter(highScoreFile))
            {
                string dataToWrite;
                for (int i = 0; i < collectionScoreObj.Count; i++)
                {
                    dataToWrite = collectionScoreObj[i].ToString();
                    writer.WriteLine(dataToWrite);
                }
            }
        }

        //Read the data from "highScore.txt"
        //Add the data to a list containing all the highscores. 
        public void ReadFromFile()
        {
            try
            {
                using (StreamReader reader = new StreamReader(highScoreFile))
                {
                    collectionScoreObj.Clear();
                    string line = reader.ReadLine();
                    while ((line != null))
                    {
                        string[] contents = new string[2];
                        contents = line.Split(' ');
                        Score scoreObj = new Score();
                        if (contents.Length == 2)
                        {
                            scoreObj.Name = contents[0];
                            scoreObj.PlayerScore = Convert.ToInt32(contents[1]);
                            collectionScoreObj.Add(scoreObj);
                        }
                        
                        line = reader.ReadLine();
                    }

                }
            }
            catch(FileNotFoundException e)
            {
                WriteToFile();
                ReadFromFile();
            }
        }

        //Updates the list containing the highscores if the new score is larger than the lowest highscore. 
        public void AddHighScore(string playerName, int newScore)
        {
            ReadFromFile();
            Score newScoreData = new Score();
            newScoreData.Name = playerName;
            newScoreData.PlayerScore = newScore;
            collectionScoreObj.Add(newScoreData);
            collectionScoreObj.Sort((Score S1, Score S2) => { if (S1.PlayerScore < S2.PlayerScore) return -1; else if (S1.PlayerScore > S2.PlayerScore) return 1; else return 0; });
            if (collectionScoreObj.Count() > 10)
            {
                collectionScoreObj.RemoveAt(collectionScoreObj.Count() - 1);
            }
            WriteToFile();       
        }

        //Returns the list of highscores
        public List<string> GetHighScores()
        {
            ReadFromFile();
            List<string> scores = new List<string>();
            for (int item = 0; item < collectionScoreObj.Count(); item++)
            {
                scores.Add(collectionScoreObj[item].ToString());
            }
            return scores;
        }
    }
}
