# NeuroGeneticAI

NeuroGeneticAI is a cleaned-up Unity/AI research sandbox for evolving neural controllers with either a genetic algorithm or a hill climber. The Unity specific components were retained while the core logic was reorganized into a conventional `src/` tree with a `.sln`/`.csproj` so the code base can be linted and unit tested outside of Unity.

## Repository layout

```
src/NeuroGeneticAI/
├── Controllers        # Unity behaviours that orchestrate matches and evaluations
├── Genetics           # GA/Hill Climber implementations plus the neural network
├── Infrastructure     # Logging abstraction and Unity stubs for .NET builds
├── Robotics           # Physical robot representation
├── Scoring            # Goals, hit counters, and score keeping logic
├── Sensors            # Ray-casting detector
└── Utils              # Camera/sensor helpers
```

Additional documentation (including the original academic report) lives in `docs/`.

## Getting started

1. Install the .NET 6 SDK (or newer) if you want to lint/format the code outside of Unity.
2. Restore/build the solution:

   ```bash
   dotnet build NeuroGeneticAI.sln
   ```

   The solution uses lightweight Unity stubs so the build succeeds without the Unity editor installed.
3. Open the Unity project that references these scripts and assign the controllers to your prefabs as usual.

## Testing

Unit tests can be added under `tests/` (not provided here) and executed via `dotnet test`. The logging abstraction in `Infrastructure/Diagnostics` makes it trivial to redirect log messages to Unity's `Debug.Log` or any other sink.

## Debugging aids

* Every controller now writes structured messages via `LogManager.Logger`; replace the default `ConsoleLogger` if you want to forward logs elsewhere.
* The neural controller (`D31NeuralControler`) sanitizes detector output and logs missing sensor data so it is easier to diagnose null references.

## Cleaning legacy assets

All `.meta`, `.zip`, and other Unity generated artifacts were removed from the git history. The compressed `LearningAlgorithms`, `Sensors`, and `Utils` directories were expanded into normal source folders so the files can be versioned and reviewed individually.
