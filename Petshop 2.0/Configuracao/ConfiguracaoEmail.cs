namespace PetShopMensageria.Configuracao;

public class ConfiguracaoEmail
{
    public string Host { get; init; } = string.Empty;
    public int Porta { get; init; } = 587;
    public string Usuario { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
    public string Remetente { get; init; } = string.Empty;
    public bool UsarSsl { get; init; } = true;

    public string? Validar()
    {
        if (string.IsNullOrWhiteSpace(Host)) return "SMTP_HOST não informado.";
        if (string.IsNullOrWhiteSpace(Usuario)) return "SMTP_USER não informado.";
        if (string.IsNullOrWhiteSpace(Senha)) return "SMTP_PASS não informado (no Gmail use uma Senha de app).";
        if (!PetShopMensageria.Servicos.ValidadorEmail.EhValido(Remetente)) return "Remetente inválido (SMTP_FROM).";
        return null;
    }

    public static ConfiguracaoEmail DeVariaveisDeAmbiente()
    {
        string usuario = Ler("SMTP_USER");
        string remetente = Ler("SMTP_FROM");

        return new ConfiguracaoEmail
        {
            Host = Ler("SMTP_HOST"),
            Porta = int.TryParse(Ler("SMTP_PORT"), out var porta) ? porta : 587,
            Usuario = usuario,
            Senha = Ler("SMTP_PASS"),
            Remetente = remetente.Length > 0 ? remetente : usuario,
            UsarSsl = !string.Equals(Ler("SMTP_SSL"), "false", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string Ler(string nome) => Environment.GetEnvironmentVariable(nome) ?? string.Empty;
}
