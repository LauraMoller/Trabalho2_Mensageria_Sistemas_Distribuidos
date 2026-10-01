namespace PetShopMensageria.Eventos;

public class ServicoFinalizado : EventoPet
{
    public ServicoFinalizado() => Evento = nameof(ServicoFinalizado);
}
