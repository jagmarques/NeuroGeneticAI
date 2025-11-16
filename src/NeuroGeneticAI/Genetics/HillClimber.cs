using System.Collections.Generic;
using UnityEngine;

namespace NeuroGeneticAI.Genetics
{
    
    // Simplified hill climber that repeatedly mutates the current best individual in
    // the population.
    
    public class HillClimber : MetaHeuristic
    {
        [Header("Red Population Parameters")]
        public float mutationProbabilityRedPopulation;

        [Header("Blue Population Parameters")]
        public float mutationProbabilityBluePopulation;

        
        public override void InitPopulation()
        {
            GamesPerIndividualForEvaluation = Mathf.Min(GamesPerIndividualForEvaluation, populationSize);
            populationRed = new List<Individual>();
            populationBlue = new List<Individual>();

            while (populationRed.Count < populationSize)
            {
                HillClimberIndividual newRed = new HillClimberIndividual(NNTopology, GamesPerIndividualForEvaluation, mutationMethod);
                HillClimberIndividual newBlue = new HillClimberIndividual(NNTopology, GamesPerIndividualForEvaluation, mutationMethod);

                if (seedPopulationFromFile)
                {
                    NeuralNetwork nnRed = getRedIndividualFromFile();
                    NeuralNetwork nnBlue = getBlueIndividualFromFile();
                    newRed.Initialize(nnRed);
                    newBlue.Initialize(nnBlue);
                    if (populationRed.Count != 0 && populationBlue.Count != 0)
                    {
                        newRed.Mutate(mutationProbabilityRedPopulation);
                        newBlue.Mutate(mutationProbabilityBluePopulation);
                    }
                }
                else
                {
                    newRed.Initialize();
                    newBlue.Initialize();
                }

                populationRed.Add(newRed);
                populationBlue.Add(newBlue);
            }
        }

        
        public override void Step()
        {
            List<Individual> newPopRed = new List<Individual>();
            List<Individual> newPopBlue = new List<Individual>();
            updateReport();

            for (int i = 0; i < populationSize; i++)
            {
                HillClimberIndividual tmpRed = (HillClimberIndividual)overallBestRed.Clone();
                HillClimberIndividual tmpBlue = (HillClimberIndividual)overallBestBlue.Clone();
                tmpRed.Mutate(mutationProbabilityRedPopulation);
                tmpBlue.Mutate(mutationProbabilityBluePopulation);
                newPopRed.Add(tmpRed.Clone());
                newPopBlue.Add(tmpBlue.Clone());
            }

            populationRed = newPopRed;
            populationBlue = newPopBlue;
            generation++;
        }
    }
}
