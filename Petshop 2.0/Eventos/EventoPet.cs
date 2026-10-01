namespace PetShopMensageria.Eventos;

public class EventoPet
{
    public string Evento { get; set; } = string.Empty;
    public string PetId { get; set; } = string.Empty;
    public string NomePet { get; set; } = string.Empty;
    public string EmailTutor { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
