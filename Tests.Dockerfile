FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["CashFlow.CashierApi.Tests/CashFlow.CashierApi.Tests.csproj", "CashFlow.CashierApi.Tests/"]
COPY ["CashFlow.LoadTests/CashFlow.LoadTests.csproj", "CashFlow.LoadTests/"]
COPY ["CashierApi/CashierApi.csproj", "CashierApi/"]
COPY ["CashReportApi/CashReportApi.csproj", "CashReportApi/"]
COPY ["CashierWorker/CashierWorker.csproj", "CashierWorker/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infra/Infra.csproj", "Infra/"]

RUN dotnet restore "CashFlow.CashierApi.Tests/CashFlow.CashierApi.Tests.csproj"
RUN dotnet restore "CashFlow.LoadTests/CashFlow.LoadTests.csproj"

# Copiar todo o código fonte
COPY . .

# Manter o arquivo de configuração original e usar variáveis de ambiente no entrypoint
COPY CashFlow.CashierApi.Tests/appsettings.test.json /src/CashFlow.CashierApi.Tests/appsettings.test.json

# Criar diretório para resultados de testes
RUN mkdir -p /src/TestResults

# Etapa final para executar os testes
FROM build AS final

# Definir diretório de trabalho
WORKDIR /src

# Instalar ferramentas adicionais para teste
RUN dotnet tool install -g dotnet-reportgenerator-globaltool

# Configurar variáveis de ambiente
ENV PATH="$PATH:/root/.dotnet/tools"

# Comando padrão para executar testes (pode ser substituído no docker-compose)
ENTRYPOINT ["dotnet", "test"]
