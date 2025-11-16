using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeuroGeneticAI.Genetics
{
    
    // Standard tournament selection implementation used by the GA.
    
    public class TournamentSelection : SelectionMethod
    {
        private readonly int tournamentSize;

        public TournamentSelection(int tournamentSize)
        {
            this.tournamentSize = tournamentSize;
        }

        
        public override List<Individual> selectIndividuals(List<Individual> oldpop, int num)
        {
            if (oldpop.Count < tournamentSize)
            {
                throw new Exception("The population size is smaller than the tournament size.");
            }

            List<Individual> selectedInds = new List<Individual>();
            for (int i = 0; i < num; i++)
            {
                selectedInds.Add(Tournament(oldpop, tournamentSize).Clone());
            }

            return selectedInds;
        }

        private Individual Tournament(List<Individual> population, int size)
        {
            Individual best = null;
            for (int i = 0; i < size; i++)
            {
                Individual ind = population[UnityEngine.Random.Range(0, population.Count)];
                if (best == null || ind.Fitness > best.Fitness)
                {
                    best = ind.Clone();
                }
            }

            return best;
        }
    }
}
