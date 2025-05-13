# CashFlow - Sistema de Gestão de Fluxo de Caixa

O CashFlow é uma solução completa de microserviços para gerenciamento de fluxo de caixa, projetada para processar e registrar transações financeiras com alta confiabilidade e escalabilidade.

## Arquitetura

A solução é composta por três microserviços principais:

1. **CashierApi**: API para recebimento e processamento de lançamentos financeiros (créditos e débitos)
2. **CashReportApi**: API para consulta de relatórios e saldos consolidados
3. **CashierWorker**: Serviço de processamento assíncrono de transações via fila

### Tecnologias utilizadas

- **Backend**: .NET 8.0, ASP.NET Core, Entity Framework Core
- **Persistência**: PostgreSQL
- **Cache**: Redis
- **Mensageria**: RabbitMQ
- **Containerização**: Docker e Docker Compose
- **Validação**: FluentValidation
- **Testes**: xUnit, NBomber (testes de carga)

## Requisitos

- Docker e Docker Compose
- .NET 8.0 SDK (para desenvolvimento local)
- Visual Studio 2022 ou VS Code com extensões C# (para desenvolvimento local)

## Iniciando a aplicação

### Via Docker Compose

```bash
# Construir e iniciar todos os containers
docker-compose up -d

# Verificar status dos containers
docker-compose ps

# Visualizar logs de um serviço específico
docker-compose logs -f cashierapi
```

### Em ambiente de desenvolvimento

```bash
# Executar CashierApi
cd CashierApi
dotnet run

# Executar CashReportApi
cd CashReportApi
dotnet run

# Executar CashierWorker
cd CashierWorker
dotnet run
```

## Estrutura do Projeto

```
CashFlow/
├── CashierApi/            # API de lançamentos financeiros
├── CashReportApi/         # API de relatórios e saldos
├── CashierWorker/         # Serviço processador de transações
├── Domain/                # Classes de domínio compartilhadas
├── Infra/                 # Infraestrutura compartilhada
├── CashFlow.CashierApi.Tests/  # Testes de validação e integração
└── CashFlow.LoadTests/    # Testes de carga
```

## Endpoints de API

### CashierApi

- **POST /api/Cash** - Registra um novo lançamento financeiro
  ```json
  {
    "amount": 100.50,
    "postingType": "C",  // C para crédito, D para débito
    "description": "Pagamento de salário"
  }
  ```

- **GET /api/Cash** - Retorna todos os lançamentos registrados

### CashReportApi

- **GET /api/Report/{data}** - Retorna o saldo consolidado para a data especificada
  - Formato de data: yyyy-MM-dd (ex: 2025-05-13)

## Configuração

A configuração de todos os serviços é feita via arquivos `appsettings.json` em cada projeto, inclusive no ambiente Docker. Já no ambiente de desenvolvimento as configurações estão no `appsettings.Development.json`.

### Redis

O Redis é utilizado para cache de saldos consolidados, melhorando o desempenho das consultas frequentes. A configuração inclui o parâmetro `abortConnect=false` para aumentar a resiliência da conexão.

### RabbitMQ

O RabbitMQ é utilizado para processamento assíncrono dos lançamentos, garantindo maior escalabilidade e tolerância a falhas. A fila principal é a `cash_posting_queue`.

### PostgreSQL

O PostgreSQL armazena todas as transações e dados consolidados. O esquema de banco é criado automaticamente via Entity Framework Core.
Antes de rodar a aplicação é preciso rodar o script de criação da tabela. O arquivo `create_cash_posting_table.sql` está no diretório Database do projeto Infra.

## Testes

O CashFlow inclui uma suite completa de testes para garantir a qualidade e robustez da aplicação. Os testes são organizados em três categorias principais:

### 1. Testes de Validação

Realizados com FluentValidation + xUnit para garantir que os dados de entrada atendam às regras de negócio:
- Amount (valor) deve ser maior que zero
- PostingType (tipo de lançamento) deve ser 'C' para crédito ou 'D' para débito
- Description (descrição) é opcional, mas não pode exceder 255 caracteres (conforme restrição do banco de dados)

```bash
# Executar localmente
cd CashFlow.CashierApi.Tests
dotnet test --filter "ValidationTests"
```

### 2. Testes de Integração

Verificam a saúde e conectividade com serviços externos:
- Conexão com Redis (cache)
- Conexão com RabbitMQ (mensageria)
- Conexão com PostgreSQL (banco de dados)
- Endpoints da API funcionando corretamente

```bash
# Executar localmente
cd CashFlow.CashierApi.Tests
dotnet test --filter "IntegrationTests"

# Executar via Docker com infraestrutura isolada
docker-compose -f docker-compose.test.yml up integration-tests
```

### 3. Testes de Carga

Simulam alta concorrência e estresse na aplicação:
- Teste de 500 requisições por segundo para validar a escalabilidade
- Teste com carga crescente para identificar pontos de saturação
- Medição de tempos de resposta sob diferentes cargas

```bash
# Executar localmente
cd CashFlow.LoadTests
dotnet test

# Executar via Docker com ambiente completo
docker-compose -f docker-compose.test.yml up load-tests
```

### Executando todos os testes em ambiente isolado

O CashFlow inclui um ambiente de testes isolado usando Docker, que configura automaticamente as dependências necessárias para todos os tipos de teste:

```bash
# Construir e executar todos os testes
docker-compose -f docker-compose.test.yml up --build

# Executar apenas testes específicos
docker-compose -f docker-compose.test.yml up integration-tests
docker-compose -f docker-compose.test.yml up load-tests
```

### Arquivos de configuração de testes

- **Tests.Dockerfile**: Configuração do container de testes
- **docker-compose.test.yml**: Orquestração completa do ambiente de teste
- **appsettings.test.json**: Configurações específicas para ambiente de teste

Os resultados dos testes são armazenados no diretório `TestResults` e podem ser visualizados em ferramentas como o Visual Studio ou convertidos em relatórios HTML usando ReportGenerator.

## Monitoramento

- **RabbitMQ**: Acesse o painel de administração em http://localhost:15672 (usuário: guest, senha: guest)
- **Logs**: Disponíveis via `docker-compose logs`

## Licença

Este projeto está licenciado sob a [MIT License](LICENSE).
