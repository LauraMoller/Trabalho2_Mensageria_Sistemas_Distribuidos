using RabbitMQ.Client;

namespace PetShopMensageria.Infraestrutura;

public static class ConfiguradorTopologia
{
    public static void Configurar(IModel canal)
    {

        canal.ExchangeDeclare(Topologia.ExchangePrincipal, ExchangeType.Topic, durable: true);
        canal.ExchangeDeclare(Topologia.ExchangeDlx, ExchangeType.Topic, durable: true);

        var argumentosEmail = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", Topologia.ExchangeDlx },
            { "x-dead-letter-routing-key", Topologia.RkDlx }
        };

        canal.QueueDeclare(Topologia.FilaOperacionalCheckin, durable: true, exclusive: false, autoDelete: false);
        canal.QueueBind(Topologia.FilaOperacionalCheckin, Topologia.ExchangePrincipal, Topologia.RkCheckin);

        canal.QueueDeclare(Topologia.FilaRecepcaoStatus, durable: true, exclusive: false, autoDelete: false);
        canal.QueueBind(Topologia.FilaRecepcaoStatus, Topologia.ExchangePrincipal, Topologia.RkTodosEventosPet);

        canal.QueueDeclare(Topologia.FilaNotificacaoEmail, durable: true, exclusive: false, autoDelete: false,
                           arguments: argumentosEmail);
        canal.QueueBind(Topologia.FilaNotificacaoEmail, Topologia.ExchangePrincipal, Topologia.RkFinalizado);



        canal.QueueDeclare(Topologia.FilaNotificacaoDlq, durable: true, exclusive: false, autoDelete: false);
        canal.QueueBind(Topologia.FilaNotificacaoDlq, Topologia.ExchangeDlx, Topologia.RkDlx);

        Console.WriteLine("[Infra] Exchanges, filas e bindings configurados.");
    }
}
