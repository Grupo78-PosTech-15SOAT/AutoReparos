# 📊 Como Executar o SonarQube Localmente

Este guia explica passo a passo como subir o SonarQube local usando Docker e realizar a análise estática do projeto AutoReparos, incluindo a geração e envio do relatório de cobertura de testes.

---

## 🛠️ Pré-requisitos

1. **Docker e Docker Compose** instalados e rodando.
2. **.NET 10 SDK** instalado localmente.
3. Instalar o **Java Runtime Environment (JRE)** ou **JDK** (necessário para executar o SonarScanner localmente via CLI do .NET).
4. Instalar a ferramenta global do SonarScanner para .NET:

   ```bash
   dotnet tool install --global dotnet-sonarscanner
   ```

---

## 🚀 Passo 1: Subir o Container do SonarQube

Criamos um arquivo de configuração específico do Docker Compose ([docker-compose-sonar.yml](file:///mnt/c/Users/joseh/Documents/Github/Fiap/AutoReparos/docker-compose-sonar.yml)). Para iniciar o SonarQube, execute o comando abaixo no terminal dentro do diretório `./AutoReparos`:

```bash
docker compose -f docker-compose-sonar.yml up -d
```

> [!NOTE]
> O SonarQube pode demorar cerca de 1 a 2 minutos para inicializar completamente todos os seus serviços internos (incluindo o banco de dados embutido e o Elasticsearch).

---

## 🔑 Passo 2: Acessar e Configurar o SonarQube

1. Acesse o painel web em seu navegador: **[http://localhost:9000](http://localhost:9000)**.
2. Faça login com as credenciais padrão:
   - **Usuário:** `admin`
   - **Senha:** `admin`
3. Altere a senha padrão conforme solicitado no primeiro acesso.
4. **Criar um Projeto Manualmente:**
   - Clique em **Create a local project** (ou clique no "+" no topo direito -> *New Project* -> *Locally*).
   - Defina o **Project key** como `AutoReparos`.
   - Defina o **Display name** como `AutoReparos` (ou o nome que preferir).
   - Clique em **Set Up**.
5. **Gerar um Token de Acesso:**
   - Na tela "How do you want to analyze your repository?", escolha **Locally**.
   - Em "Provide a token", dê um nome para o token (ex: `local-token`) e selecione o tipo do token como **User Token** ou **Project Analysis Token**.
   - Clique em **Generate** e **copie o token gerado** (você precisará dele para rodar a análise).

---

## 🧪 Passo 3: Executar a Análise de Código e Testes

Para garantir que a análise do SonarQube colete os dados de cobertura de testes corretamente, execute o fluxo completo de comandos abaixo.

### Opção A: No Linux / macOS (Bash)

Abra o terminal na pasta `./AutoReparos`, configure as variáveis de ambiente necessárias para os testes e execute os comandos:

```bash
# 1. Definir as variáveis de ambiente exigidas pelos testes para evitar falhas
export ConnectionStrings__DbConnection="Host=localhost;Database=autoreparos_test;Username=admin;Password=admin123"
export Jwt__Secret="FBQOvEaUYAlmdilnGOk7vKzO9xUHiLgb8QCFUrk6af9"
export Jwt__ExpiryHours="2"
export SeedUser__Email="admin@autoreparos.com"
export SeedUser__Password="Admin@123"
export AprovacaoToken__Secret="another_super_secret_key_for_approval_tokens_with_enough_length"
export SendGrid__ApiKey="SG.dummy_key"
export SendGrid__FromEmail="noreply@autoreparos.com"
export SendGrid__FromName="AutoReparos"
export App__BaseUrl="http://localhost:8080"

# 2. Iniciar a sessão do SonarScanner
# Substitua "SEU_TOKEN_AQUI" pelo token copiado no Passo 2
dotnet sonarscanner begin \
  /k:"AutoReparos" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.token="SEU_TOKEN_AQUI" \
  /d:sonar.cs.cobertura.xmlReportPaths="**/TestResults/*/coverage.cobertura.xml"

# 3. Compilar a aplicação
dotnet build --no-incremental

# 4. Executar os testes coletando cobertura de código (coverlet)
dotnet test --no-build --collect:"XPlat Code Coverage"

# 5. Finalizar a sessão do SonarScanner e enviar os relatórios
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN_AQUI"
```

### Opção B: No Windows (PowerShell)

Abra o PowerShell na pasta `./AutoReparos`, configure as variáveis de ambiente e execute os comandos:

```powershell
# 1. Definir as variáveis de ambiente exigidas pelos testes para evitar falhas
$env:ConnectionStrings__DbConnection="Host=localhost;Database=autoreparos_test;Username=admin;Password=admin123"
$env:Jwt__Secret="FBQOvEaUYAlmdilnGOk7vKzO9xUHiLgb8QCFUrk6af9"
$env:Jwt__ExpiryHours="2"
$env:SeedUser__Email="admin@autoreparos.com"
$env:SeedUser__Password="Admin@123"
$env:AprovacaoToken__Secret="another_super_secret_key_for_approval_tokens_with_enough_length"
$env:SendGrid__ApiKey="SG.dummy_key"
$env:SendGrid__FromEmail="noreply@autoreparos.com"
$env:SendGrid__FromName="AutoReparos"
$env:App__BaseUrl="http://localhost:8080"

# 2. Iniciar a sessão do SonarScanner
# Substitua "SEU_TOKEN_AQUI" pelo token copiado no Passo 2
dotnet sonarscanner begin `
  /k:"AutoReparos" `
  /d:sonar.host.url="http://localhost:9000" `
  /d:sonar.token="SEU_TOKEN_AQUI" `
  /d:sonar.cs.cobertura.xmlReportPaths="**/TestResults/*/coverage.cobertura.xml"

# 3. Compilar a aplicação
dotnet build --no-incremental

# 4. Executar os testes coletando cobertura de código (coverlet)
dotnet test --no-build --collect:"XPlat Code Coverage"

# 5. Finalizar a sessão do SonarScanner e enviar os relatórios
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN_AQUI"
```

---

## 🛑 Parar o SonarQube

Quando terminar de utilizar o SonarQube, você pode parar o container executando:

```bash
docker compose -f docker-compose-sonar.yml down
```
