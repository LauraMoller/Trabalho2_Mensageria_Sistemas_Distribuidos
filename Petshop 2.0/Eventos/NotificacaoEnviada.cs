namespace PetShopMensageria.Eventos;


public class NotificacaoEnviada : EventoPet
{
    public string Canal { get; set; } = "E-mail";

    public NotificacaoEnviada() => Evento = nameof(NotificacaoEnviada);
}
