// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace PerformanceCalculator.LocalScores
{
    public class LocalScoresParser
    {
        private const int minimum_possible_lines = 7;

        private readonly Regex scoreLineRegex = new Regex("║[0-9 ]+│");

        private readonly Regex numberRegex = new Regex("[0-9]+");

        private double previousPP { get; set; }

        private List<LocalReplayInfo> replays { get; } = new List<LocalReplayInfo>();

        private DateTime calcTime { get; set; }

        public List<LocalReplayInfo> parseLocalScores(string inputPath)
        {
            string[] lines = File.ReadAllLines(inputPath);

            if (lines.Length < minimum_possible_lines)
            {
                Console.WriteLine("Failed to parse local scores, invalid format");
                return replays;
            }

            parseCalcTime(lines[0]);
            parsePreviousPP(lines[1]);

            for (int i = 2; i < lines.Length; i++)
            {
                if (isScoreLine(lines[i]))
                {
                    parseScoreLine(lines[i]);
                }
            }

            return replays;
        }

        private void parseCalcTime(string line)
        {
            calcTime = DateTime.Parse(line);
        }

        private void parsePreviousPP(string line)
        {
            Regex ppRegex = new Regex("[0-9]+,[0-9]+");
            MatchCollection matches = ppRegex.Matches(line);
            previousPP = double.Parse(matches[0].Value);
        }

        private bool isScoreLine(string line)
        {
            return scoreLineRegex.Matches(line).Count != 0;
        }

        private void parseScoreLine(string line)
        {
            string[] cells = line.Split("|");
            MatchCollection comboMatches = numberRegex.Matches(cells[6]);
            ScoreInfo scoreInfo = new ScoreInfo
            {
                // Remove %
                Accuracy = double.Parse(cells[4].Substring(0, cells[4].Length - 1)),
                Statistics =
                {
                    [HitResult.Miss] = int.Parse(cells[5])
                },
                // Read MaxCombo / Combo
                MaxCombo = int.Parse(comboMatches[0].Value),
                Combo = int.Parse(comboMatches[1].Value),
            };
            // LocalReplayInfo replayInfo = new LocalReplayInfo
            // {
            //     ScoreInfo = scoreInfo,
            //     MapName = cells[1].Trim(),
            //     //ModString = cells[2].Trim(),
            //     TotalPP = double.Parse(cells[3]),
            //     TimeSet = DateTime.Parse(cells[7])
            // };
            // replayInfo.ScoreInfo = scoreInfo;
            // replays.Add(replayInfo);
        }
    }
}
