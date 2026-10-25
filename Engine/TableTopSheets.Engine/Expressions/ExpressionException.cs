namespace TableTopSheets.Engine.Expressions;

public class ExpressionException(string message, Exception? innerException = null): Exception(message, innerException);
