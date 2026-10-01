namespace PetShopMensageria.Servicos;

public sealed class EmailInvalidoException : Exception
{
    public EmailInvalidoException(string? email) : base($"E-mail do tutor inválido: '{email}'.") { }
}
