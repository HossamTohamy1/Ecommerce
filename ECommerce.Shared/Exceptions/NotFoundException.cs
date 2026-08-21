namespace ECommerce.Shared.Exceptions;

public class NotFoundException : Exception
{
    public string EntityName { get; }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.")
    {
        EntityName = entityName;
    }

    public NotFoundException(string message) : base(message)
    {
        EntityName = string.Empty;
    }
}
