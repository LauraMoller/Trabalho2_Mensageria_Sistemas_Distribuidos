using RabbitMQ.Client; 

namespace PetShopMensageria.Configuracao;

public class ConfiguracaoRabbitMq
{
    public string Host { get; init; } = "localhost";
    public int Porta { get; init; } = 5672;
    public string Usuario { get; init; } = "guest";
    public string Senha { get; init; } = "guest"; 
    public string VirtualHost { get; init; } = "/";
    public bool UsarTls { get; init; } 

    public static ConfiguracaoRabbitMq DeVariaveisDeAmbiente()
    {
        bool tls = string.Equals(Environment.GetEnvironmentVariable("RABBITMQ_TLS"), "true",
                                 StringComparison.OrdinalIgnoreCase);

        return new ConfiguracaoRabbitMq
        {
            Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Porta = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var porta)
                        ? porta
                        : (tls ? 5671 : 5672),
            Usuario = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Senha = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest",
            VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/",
            UsarTls = tls
        };
    }

    public ConnectionFactory CriarFabrica()
    {

        var fabrica = new ConnectionFactory
        {
            HostName = Host,
            Port = Porta,
            UserName = Usuario,
            Password = Senha,
            VirtualHost = VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        if (UsarTls)
            fabrica.Ssl = new SslOption { Enabled = true, ServerName = Host };

        return fabrica;
    }
}
