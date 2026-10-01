# Pet Shop — Mensageria com RabbitMQ

Sistema de banho e tosa em C# (ASP.NET Core, .NET 10). Recepção, Operação e Notificação se comunicam por eventos no RabbitMQ. Ao finalizar o serviço, o tutor recebe um e-mail real via SMTP.

Equipe: Ana Carolina Fanderuff Mellek, João Paulo de Lima, Laura Soares Möller, Tiago Segatti, Yuri Ricardo Pacher Rabelo.

**Mudança em relação às Etapas 1 e 2:** a notificação ao tutor, prevista pela **API do WhatsApp**, passou a ser feita por **e-mail (SMTP)**. A API do WhatsApp Business exige um telefone dedicado exclusivamente a uma conta business, o que não era viável. Por isso a fila `fila.notificacao.whatsapp` agora se chama `fila.notificacao.email`. O restante da arquitetura não mudou.

---

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version`)
- Docker (ou um RabbitMQ instalado localmente)
- Conta Google com verificação em duas etapas (para gerar a Senha de app)

## Configuração

### 1. RabbitMQ

```bash
docker run -d --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Painel: <http://localhost:15672> (usuário `guest`, senha `guest`).

### 2. Senha de app do Gmail

1. Ative a verificação em duas etapas na conta Google.
2. Acesse <https://myaccount.google.com/apppasswords> e crie uma senha de app (16 caracteres).
3. Use essa senha em `SMTP_PASS`. **Não** é a senha normal da conta.

### 3. Variáveis de ambiente

| Variável | Obrigatória | Descrição |
|---|---|---|
| `SMTP_HOST` | Sim | `smtp.gmail.com` |
| `SMTP_USER` | Sim | Seu e-mail do Gmail |
| `SMTP_PASS` | Sim | Senha de app de 16 caracteres |
| `SMTP_PORT` | Não | `587` (padrão) |
| `SMTP_FROM` | Não | Remetente; se vazio, usa `SMTP_USER` |
| `SMTP_SSL` | Não | `true` (padrão) |
| `RETRY_ESPERA_SEGUNDOS` | Não | Espera entre tentativas de envio (padrão `2`) |
| `RABBITMQ_HOST` / `_PORT` / `_USER` / `_PASS` / `_VHOST` | Não | `localhost` / `5672` / `guest` / `guest` / `/` |

Sem as variáveis SMTP obrigatórias, a aplicação **não inicia** (não existe envio simulado).

### 4. Script de execução (`iniciar.ps1`)

```powershell
$env:SMTP_HOST="smtp.gmail.com"
$env:SMTP_PORT="587"
$env:SMTP_USER="seuemail@gmail.com"
$env:SMTP_FROM="seuemail@gmail.com"
$env:SMTP_PASS="xxxxxxxxxxxxxxxx"
$env:RETRY_ESPERA_SEGUNDOS="15"
dotnet run
```

## Executando

1. Confirme que o RabbitMQ está rodando (`docker ps`).
2. Defina as variáveis de ambiente no terminal conforme o script de execução anterior.
3. Na pasta do projeto, compile:
```powershell
   dotnet build
```
4. Execute:
```powershell
   dotnet run
```
5. Abra, cada uma em uma aba:
   - Recepção: <http://localhost:5080>
   - Operação: <http://localhost:5080/operacao.html>

Fluxo: **check-in** (Recepção) → **iniciar** e **finalizar serviço** (Operação) → e-mail enviado ao tutor → **check-out** (Recepção).
