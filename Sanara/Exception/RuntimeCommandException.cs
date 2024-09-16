namespace Sanara.Exception;

public class RuntimeCommandException(string message, System.Exception innerException) : System.Exception(message, innerException)
{ }
