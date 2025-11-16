using System;

namespace NeuroGeneticAI.Infrastructure.Diagnostics
{
    
    // Provides a very small logging abstraction so that controllers can report
    // their state without depending on Unity's Debug class.
    
    public interface ILogger
    {
        // Logs the supplied message as information.
        // message: Text to record.
        void LogInformation(string message);

        // Logs the supplied message as a warning.
        // message: Text to record.
        void LogWarning(string message);

        // Logs the supplied message as an error.
        // message: Text to record.
        void LogError(string message);
    }

    
    // Default logger implementation that writes to the console.  Unity users can
    // replace this with a custom adapter that forwards messages to
    // Debug.Log.
    
    public sealed class ConsoleLogger : ILogger
    {
        
        public void LogInformation(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        
        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        
        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }
    }

    
    // Central access point for the logger used throughout the code base.
    
    public static class LogManager
    {
        private static ILogger _logger = new ConsoleLogger();

        
        // Gets or sets the application logger.  A custom implementation can be
        // injected by calling this property at application start.
        
        public static ILogger Logger
        {
            get => _logger;
            set => _logger = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
