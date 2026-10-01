using System.Text.Json; 
using PetShopMensageria.Eventos;
using PetShopMensageria.Infraestrutura; 
using PetShopMensageria.Publicadores; 
using PetShopMensageria.Servicos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PetShopMensageria.Consumidores;

public sealed class ConsumidorNotificacao : ConsumidorBase
{
    private const string HeaderRetries = "x-retry-count";

    private readonly IServicoEmail _servicoEmail;

    private readonly PublicadorEventos _publicador;

    private readonly int _maxTentativas;

    private readonly TimeSpan _espera;

    public ConsumidorNotificacao(IConnection conexao, IServicoEmail servicoEmail, PublicadorEventos publicador,
                                 int maxTentativas = 3, TimeSpan? espera = null) : base(conexao)
    {
        _servicoEmail = servicoEmail;
        _publicador = publicador;
        _maxTentativas = maxTentativas;
        _espera = espera ?? TimeSpan.FromSeconds(2);
    }

    protected override string Fila => Topologia.FilaNotificacaoEmail;

    protected override string Nome => "Notificação";

    protected override void Processar(BasicDeliverEventArgs ea)
    {
        var evento = SerializadorJson.Desserializar<EventoPet>(ea.Body);

        if (!ValidadorEmail.EhValido(evento.EmailTutor))
            throw new EmailInvalidoException(evento.EmailTutor);

        Log($"Enviando e-mail para {evento.EmailTutor} (tutor do pet {evento.NomePet} #{evento.PetId}) - tentativa {TentativaAtual(ea)} de {_maxTentativas}...");
        _servicoEmail.Enviar(evento);
        Log("Sucesso! Tutor notificado.", "sucesso");

        _publicador.Publicar(Topologia.RkNotificacaoEnviada,
            new NotificacaoEnviada { PetId = evento.PetId, NomePet = evento.NomePet, EmailTutor = evento.EmailTutor });
    }

    protected override void AoFalhar(BasicDeliverEventArgs ea, Exception ex)
    {
        int tentativa = TentativaAtual(ea);
        bool erroPermanente = ex is JsonException or EmailInvalidoException or EmailRejeitadoException;

        Log($"Falha na tentativa {tentativa}: {ex.Message}", "erro");

        if (!erroPermanente && tentativa < _maxTentativas)
        {
            Log($"Nova tentativa em {_espera.TotalSeconds:0}s...", "retry");
            Thread.Sleep(_espera);

            var props = Canal.CreateBasicProperties();
            props.Persistent = true;
            props.MessageId = ea.BasicProperties.MessageId;
            props.ContentType = "application/json";
            props.Headers = new Dictionary<string, object> { { HeaderRetries, tentativa } };

            Canal.BasicPublish(exchange: "", routingKey: Topologia.FilaNotificacaoEmail, props, ea.Body);
            Canal.BasicAck(ea.DeliveryTag, multiple: false);
        }
        else 
        {

            if (erroPermanente)
                Log("ERRO PERMANENTE: repetir não resolve, sem novas tentativas. Enviando direto para a Dead Letter Queue...", "dlq");
            else
                Log($"Limite de {_maxTentativas} tentativas excedido. Enviando para a Dead Letter Queue...", "dlq");
            Canal.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);

        }
    }

    private static int TentativaAtual(BasicDeliverEventArgs ea)
    {
        var headers = ea.BasicProperties.Headers;
        return headers != null && headers.TryGetValue(HeaderRetries, out var valor)
            ? Convert.ToInt32(valor) + 1
            : 1;
    }
}
