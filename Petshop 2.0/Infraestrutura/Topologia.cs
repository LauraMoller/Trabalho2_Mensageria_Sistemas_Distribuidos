namespace PetShopMensageria.Infraestrutura;

public static class Topologia
{
    public const string ExchangePrincipal = "petshop.events";
    public const string ExchangeDlx = "petshop.events.dlx";

    public const string FilaOperacionalCheckin = "fila.operacional.checkin";
    public const string FilaRecepcaoStatus = "fila.recepcao.status";
    public const string FilaNotificacaoEmail = "fila.notificacao.email";
    public const string FilaNotificacaoDlq = "fila.notificacao.dlq";

    public const string RkCheckin = "pet.checkin";
    public const string RkIniciado = "pet.iniciado";
    public const string RkFinalizado = "pet.finalizado";
    public const string RkCheckout = "pet.checkout";
    public const string RkNotificacaoEnviada = "pet.notificacao.enviada";
    public const string RkTodosEventosPet = "pet.#";
    public const string RkDlx = "pet.events.dlx";
}
