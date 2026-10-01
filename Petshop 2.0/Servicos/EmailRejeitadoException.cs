namespace PetShopMensageria.Servicos;

public sealed class EmailRejeitadoException : Exception
{
    public EmailRejeitadoException(string mensagem, Exception? causa = null) : base(mensagem, causa) { }
}