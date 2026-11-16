namespace backend.Exceptions;

public class DistanceUnavailableException : Exception
{
    public DistanceUnavailableException(string message, Exception? innerException = null) : base(message, innerException) { }
}