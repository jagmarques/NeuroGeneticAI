using NeuroGeneticAI.Infrastructure.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroGeneticAI.Scoring
{
    
    // Central scoreboard responsible for tracking the goals scored by each team.
    
    public class ScoreKeeper : MonoBehaviour
    {
        private readonly ILogger _logger = LogManager.Logger;

        // The score for both players (blue at index 0, red at index 1).
        public int[] score;

        // Identifiers associated with each controller instance.
        public string[] inds;

        // UI text field that displays the current score.
        public Text text;

        private void Start()
        {
            score = new int[2];
            UpdateScoreText();
        }

        // Registers a new goal for the supplied team index.
        // whichgoal: 0 for blue, 1 for red.
        public void ScoreGoal(int whichgoal)
        {
            score[whichgoal]++;
            UpdateScoreText();
            _logger.LogInformation($"Score updated: {score[0]} - {score[1]}");
        }

        // Configures the textual identifiers used for logging and UI.
        public void setIds(string indexIndBlue, string indexIndRed)
        {
            inds = new string[2];
            inds[0] = indexIndBlue;
            inds[1] = indexIndRed;
            UpdateScoreText();
        }

        private void UpdateScoreText()
        {
            string toWrite = $"Blue {inds?[0]}: {score[0]} \tRed {inds?[1]}: {score[1]}";
            text.text = toWrite;
        }
    }
}
