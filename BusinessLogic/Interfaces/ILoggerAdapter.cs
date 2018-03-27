using System;

namespace BusinessLogic.Interfaces
{
    public interface ILoggerAdapter<T>
    {
        void Information(string message);
        void Warning(string message);
        void Error(Exception ex, string message, params object[] args);
    }
}
