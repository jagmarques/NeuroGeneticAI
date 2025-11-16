using System.Collections.Generic;

namespace NeuroGeneticAI.Genetics
{
    
    // Base class for population selection strategies.
    
    public abstract class SelectionMethod
    {
        // Selects the next generation from the supplied population.
        // oldpop: Population to sample from.
        // num: Number of individuals to return.
        public abstract List<Individual> selectIndividuals(List<Individual> oldpop, int num);
    }
}
