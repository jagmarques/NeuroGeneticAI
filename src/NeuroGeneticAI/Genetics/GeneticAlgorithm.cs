using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NeuroGeneticAI.Genetics
{
    
    // Implements a simple generational genetic algorithm for co-evolving red and blue
    // agents.
    
    public class GeneticAlgorithm : MetaHeuristic
    {
        public int tournamentSize;

        [Header("Red Population Parameters")]
        public float mutationProbabilityRedPopulation;
        public float crossoverProbabilityRedPopulation;
        public bool elitistRed = true;

        [Header("Blue Population Parameters")]
        public float mutationProbabilityBluePopulation;
        public float crossoverProbabilityBluePopulation;
        public bool elitistBlue = true;

        // Initializes the populations either randomly or from persisted files.
        public override void InitPopulation()
        {
            GamesPerIndividualForEvaluation = Mathf.Min(GamesPerIndividualForEvaluation, populationSize);
            populationRed = new List<Individual>();
            populationBlue = new List<Individual>();

            List<string> redfiles = new List<string>();
            int redIndex = 0;

            if (seedRedPopulationFromFolder)
            {
                foreach (string file in Directory.EnumerateFiles(redFolderPath, "*.dat"))
                {
                    redfiles.Add(file);
                }

                Debug.Log(string.Join(" ", redfiles));
                if (redfiles.Count == 0)
                {
                    Debug.LogError("RED FILES NOT FOUND");
                }
            }

            List<string> bluefiles = new List<string>();
            int blueIndex = 0;
            if (seedBluePopulationFromFolder)
            {
                foreach (string file in Directory.EnumerateFiles(blueFolderPath, "*.dat"))
                {
                    bluefiles.Add(file);
                }

                Debug.Log(string.Join(" ", bluefiles));
                if (bluefiles.Count == 0)
                {
                    Debug.LogError("BLUE FILES NOT FOUND");
                }
            }

            while (populationRed.Count < populationSize)
            {
                GeneticIndividual newIndRed = new GeneticIndividual(NNTopology, GamesPerIndividualForEvaluation, mutationMethod);
                GeneticIndividual newIndBlue = new GeneticIndividual(NNTopology, GamesPerIndividualForEvaluation, mutationMethod);

                if (seedPopulationFromFile)
                {
                    NeuralNetwork nnRed = getRedIndividualFromFile();
                    NeuralNetwork nnBlue = getBlueIndividualFromFile();
                    newIndRed.Initialize(nnRed);
                    newIndBlue.Initialize(nnBlue);
                    if (populationRed.Count != 0 && populationBlue.Count != 0)
                    {
                        newIndRed.Mutate(mutationProbabilityRedPopulation);
                        newIndBlue.Mutate(mutationProbabilityBluePopulation);
                    }
                }
                else
                {
                    if (seedRedPopulationFromFolder)
                    {
                        Debug.Log($"Loading Red :{redfiles[redIndex % redfiles.Count]}");
                        NeuralNetwork nnRed = getIndividualFromFile(redfiles[redIndex % redfiles.Count]);
                        newIndRed.Initialize(nnRed);
                        if (redIndex >= redfiles.Count)
                        {
                            newIndRed.Mutate(mutationProbabilityRedPopulation);
                            Debug.Log($"Mutated Red Index{redIndex} : {newIndRed}");
                        }
                        else
                        {
                            Debug.Log($"Original Red Index{redIndex} : {newIndRed}");
                        }

                        redIndex++;
                    }
                    else
                    {
                        newIndRed.Initialize();
                    }

                    if (seedBluePopulationFromFolder)
                    {
                        Debug.Log($"Loading Blue:{bluefiles[blueIndex % bluefiles.Count]}");
                        NeuralNetwork nnBlue = getIndividualFromFile(bluefiles[blueIndex % bluefiles.Count]);
                        newIndBlue.Initialize(nnBlue);
                        if (blueIndex >= bluefiles.Count)
                        {
                            newIndBlue.Mutate(mutationProbabilityBluePopulation);
                            Debug.Log($"Mutated Blue Index{blueIndex} : {newIndBlue}");
                        }
                        else
                        {
                            Debug.Log($"Original Blue Index{blueIndex} : {newIndBlue}");
                        }

                        blueIndex++;
                    }
                    else
                    {
                        newIndBlue.Initialize();
                    }
                }

                populationRed.Add(newIndRed);
                populationBlue.Add(newIndBlue);
            }

            switch (selectionMethod)
            {
                case SelectionType.Tournament:
                    selection = new TournamentSelection(tournamentSize);
                    break;
            }
        }

        // Runs one generation: selection, crossover, mutation, and elitism.
        public override void Step()
        {
            updateReport();
            generation++;
            if (generation == numberOfGenerations)
            {
                return;
            }

            List<Individual> newPopRed = selection.selectIndividuals(populationRed, populationSize);
            List<Individual> newPopBlue = selection.selectIndividuals(populationBlue, populationSize);

            for (int i = 0; i < populationSize - 1; i += 2)
            {
                Individual parent1Red = newPopRed[i];
                Individual parent2Red = newPopRed[i + 1];
                Individual parent1Blue = newPopBlue[i];
                Individual parent2Blue = newPopBlue[i + 1];
                parent1Red.Crossover(parent2Red, crossoverProbabilityRedPopulation);
                parent1Blue.Crossover(parent2Blue, crossoverProbabilityBluePopulation);
            }

            for (int i = 0; i < populationSize; i++)
            {
                newPopRed[i].Mutate(mutationProbabilityRedPopulation);
                newPopBlue[i].Mutate(mutationProbabilityBluePopulation);
            }

            if (elitistRed)
            {
                Individual tmpRed = overallBestRed.Clone();
                newPopRed[0] = tmpRed;
            }

            if (elitistBlue)
            {
                Individual tmpBlue = overallBestBlue.Clone();
                newPopBlue[0] = tmpBlue;
            }

            populationRed = newPopRed;
            populationBlue = newPopBlue;
        }
    }
}
