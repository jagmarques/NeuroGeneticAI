# NeuroGeneticAI


## 1. Introduction
This work describes the tests conducted and the results obtained in a Practical Work focused on neural networks and genetic algorithms to train an agent to attack and defend a ball in a soccer game. The neural network consisted of 3 neurons, with 18 input parameters and 2 outputs (force intensity and application angle). Genetic algorithms were used to optimize the agent's behavior.

## 3. Parameter Modeling
Tests were conducted to determine the best parameters for the genetic algorithm. The tested parameters included mutation probability, crossover probability, tournament size, population size, and the use of elitism. Based on the results, the following values were selected:
- Mutation: 0.05%
- Crossover: 0.7%
- Tournament Size: 5
- Population Size: 70
- Elitist: Enabled

## 4. Map Resolution
The work included solving various maps, such as "ControlTheBallToAdversaryGoal" and "Defense." Specific fitness functions were developed for each scenario and optimized to train the agent to act effectively.

## 5. Results and Conclusions
The work involved the analysis of different agent evolution scenarios and the selection of the best fitness functions and parameters for each scenario. The results and conclusions obtained for each map were discussed.

The report provides an overview of the work carried out and the choices made to successfully train the agents in different scenarios. Specific details and in-depth analyses are available in the source code documents and full reports.
