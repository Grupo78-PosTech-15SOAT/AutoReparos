# AutoReparos - Sistema Integrado de Oficina Mecânica

<div>
<img src="https://img.shields.io/badge/.NET-10.0-512bd4" alt=".NET 10.0">
<img src="https://img.shields.io/badge/Docker-Enabled-2496ed" alt="Docker Enabled"></div>
<img src="https://img.shields.io/badge/PostgreSQL-16-336791" alt="PostgreSQL 16">
<img src="https://img.shields.io/badge/Architecture-DDD%20%2F%20Clean-blue" alt="Architecture DDD/Clean">
</div>

## 📌 Sobre o Projeto

O **AutoReparos** é um Sistema Integrado de Atendimento e Execução de Serviços desenvolvido para oficinas mecânicas de médio porte. O objetivo é modernizar o processo de atendimento, diagnóstico, execução de serviços e entrega de veículos, eliminando anotações manuais e planilhas ineficientes.

Este projeto faz parte do **Tech Challenge - Fase 1** do curso de Pós-Graduação em Software Architecture da FIAP (SOAT).

## 🚀 Funcionalidades Principais

- **Gestão de Ordens de Serviço:**
  - Abertura de Ordens de Serviço com identificação de cliente (CPF/CNPJ).
  - Cadastro e vínculo de veículos.
  - Inclusão de serviços e insumos/peças.
  - Geração automática de orçamento.
  - Acompanhamento de status em tempo real.
- **Gestão Administrativa (CRUDs):**
  - Clientes, Veículos, Serviços e Insumos (Peças).
  - Controle de estoque de insumos.
- **Segurança:**
  - Autenticação JWT para acesso administrativo.
  - Validação de dados sensíveis.
- **Qualidade:**
  - Testes unitários e de integração.

## 🛠️ Tecnologias Utilizadas

- **Linguagem:** C# (.NET 10)
- **Framework Web:** ASP.NET Core
- **Banco de Dados:** PostgreSQL 16
- **ORM:** Entity Framework Core
- **Segurança:** ASP.NET Core Identity & JWT
- **Documentação:** Swagger (OpenAPI)

### Justificativa da escolha do Banco de Dados

O **PostgreSQL** foi escolhido por ser um banco de dados relacional de código aberto, robusto e altamente confiável. Sua escolha para o projeto AutoReparos justifica-se por:

1. **Consistência ACID:** Essencial para garantir a integridade dos dados de estoque, orçamentos e peças.
2. **Suporte Avançado:** Excelente integração com o Entity Framework Core via o provedor Npgsql.
3. **Escalabilidade:** Capacidade de lidar com o crescimento da oficina e volume de dados a longo prazo.
4. **Ecossistema:** Ferramentas como o pgAdmin facilitam a gestão e monitoramento, conforme solicitado nos requisitos de gestão administrativa.

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**:

- **AutoReparos.Domain:** Núcleo da aplicação contendo Entidades, Value Objects, Enums, Exceções de Domínio e Interfaces de Repositório.
- **AutoReparos.Application:** Camada de lógica de aplicação, DTOs e serviços.
- **AutoReparos.Infra:** Implementação de persistência (EF Core), Repositórios, Identidade e configurações de infraestrutura.
- **AutoReparos.API:** Ponto de entrada da aplicação, contendo os Endpoints e Handlers de exceção global.

## ▶️ Como Executar

### Pré-requisitos

- [Docker](https://www.docker.com/) e [Docker Compose](https://docs.docker.com/compose/) instalados.
- .NET 10 SDK (opcional, apenas para desenvolvimento local sem Docker).

### Execução usando Docker

1. Clone o repositório.
2. Certifique-se de configurar as variáveis de ambiente necessárias (pode ser via arquivo `.env` na raiz ou variáveis de sistema):
   - `DB_PASSWORD`: Senha do banco de dados PostgreSQL.
   - `JWT_SECRET`: Chave secreta para geração dos tokens JWT.
   - `JWT_EXPIRY_HOURS`: Tempo (em horas) para expiração do Token
   - `SEED_USER_EMAIL` e `SEED_USER_PASSWORD`: Credenciais do usuário administrativo inicial.
   - 
3. No terminal, execute:

   ```bash
   docker-compose up -d --build
   ```

4. A API estará disponível em: `http://localhost:8080`
5. Acesse a documentação Swagger em: `http://localhost:8080/swagger`

### Scripts de Desenvolvimento

Existem scripts auxiliares para facilitar tarefas comuns:

- **Linux/macOS:** `./dev.sh {run|watch|db-update|mig-add|restore}`
- **Windows (PowerShell):** `./dev.ps1 -Action {run|watch|db-update|mig-add|restore}`

## 🧪 Testes

Para executar o conjunto de testes (Unitários e Integração):

```bash
dotnet test
```

## 📄 Documentação de Entrega

- **DDD:** A documentação estratégica (Event Storming, Linguagem Ubíqua e Diagramas) pode ser consultada pelo [Miro](https://miro.com/app/board/uXjVGw2wAXY=/?share_link_id=246372446405).
- **Análise de Vulnerabilidades:** Relatórios de segurança incluídos na documentação de entrega.

## 👥 Grupo

- **Participantes:**
  - Enrico Gollner - rm370737
  - José Dotta - rm372959
  - Júlia Santos - rm370364
  - Lucas Bastos - rm370749
  - Mateus Lecchi - rm371085

---
Desenvolvido para fins educacionais - FIAP SOAT.
