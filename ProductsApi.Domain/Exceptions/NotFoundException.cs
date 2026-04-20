namespace ProductsApi.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, int id)
        : base($"{entityName} avec l'id {id} est introuvable.") { }
}
