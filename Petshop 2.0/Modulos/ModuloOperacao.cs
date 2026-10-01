using PetShopMensageria.Eventos;
using PetShopMensageria.Infraestrutura;
using PetShopMensageria.Publicadores;

namespace PetShopMensageria.Modulos;

public sealed class ModuloOperacao
{
    private readonly PublicadorEventos _publicador;

    public ModuloOperacao(PublicadorEventos publicador) => _publicador = publicador;

    public void IniciarServico(string petId, string nomePet, string emailTutor) =>
        _publicador.Publicar(Topologia.RkIniciado,
            new ServicoIniciado { PetId = petId, NomePet = nomePet, EmailTutor = emailTutor });

    public void FinalizarServico(string petId, string nomePet, string emailTutor) =>
        _publicador.Publicar(Topologia.RkFinalizado,
            new ServicoFinalizado { PetId = petId, NomePet = nomePet, EmailTutor = emailTutor });
}
