using System.Net;
using System.Net.Mail;
using System.Text;
using PetShopMensageria.Configuracao;
using PetShopMensageria.Eventos;

namespace PetShopMensageria.Servicos;

public sealed class ServicoEmailSmtp : IServicoEmail
{
    private const int TempoLimiteEmMilissegundos = 10_000;

    private readonly ConfiguracaoEmail _configuracao;

    public ServicoEmailSmtp(ConfiguracaoEmail configuracao) => _configuracao = configuracao;

    private static bool EhRejeicaoDefinitiva(SmtpException ex) => ex.StatusCode is
        SmtpStatusCode.MailboxUnavailable or
        SmtpStatusCode.MailboxNameNotAllowed or
        SmtpStatusCode.UserNotLocalTryAlternatePath;

    public void Enviar(EventoPet evento)
    {
        using var mensagem = new MailMessage(_configuracao.Remetente, evento.EmailTutor)
        {
            Subject = $"Seu pet {evento.NomePet} está pronto!",
            Body = $"Olá! O banho e tosa do(a) {evento.NomePet} terminou. " +
                   "Ele(a) já pode ser retirado(a) na recepção do Pet Shop.",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };

        using var cliente = new SmtpClient(_configuracao.Host, _configuracao.Porta)
        {
            EnableSsl = _configuracao.UsarSsl,
            Timeout = TempoLimiteEmMilissegundos
        };

        if (!string.IsNullOrEmpty(_configuracao.Usuario))
            cliente.Credentials = new NetworkCredential(_configuracao.Usuario, _configuracao.Senha);

        try
        {
            cliente.Send(mensagem);
        }
        catch (SmtpException ex) when (EhRejeicaoDefinitiva(ex))
        {
            throw new EmailRejeitadoException($"Servidor SMTP rejeitou o envio ({ex.StatusCode}): {ex.Message}", ex);
        }
        catch (SmtpException ex) when (ex.InnerException is not null)
        {
            throw new InvalidOperationException($"{ex.Message} Motivo: {ex.InnerException.Message}", ex);
        }
    }
}