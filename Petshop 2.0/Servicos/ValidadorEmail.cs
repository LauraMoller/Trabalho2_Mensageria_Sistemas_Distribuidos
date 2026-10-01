using System.Net.Mail;

namespace PetShopMensageria.Servicos;

public static class ValidadorEmail
{
    public static bool EhValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var texto = email.Trim();
        return MailAddress.TryCreate(texto, out var endereco)
               && string.Equals(endereco.Address, texto, StringComparison.OrdinalIgnoreCase)
               && endereco.Host.Contains('.');
    }
}
