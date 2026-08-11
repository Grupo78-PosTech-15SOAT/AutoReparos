## 📋 Summary of Changes
<!-- Provide a clear, concise overview of what changes are introduced in this PR and why. -->

- 

## 🛠️ Type of Change
<!-- Mark the relevant options with an 'x' -->
- [ ] 🚀 New Feature (non-breaking change adding functionality)
- [ ] 🐛 Bug Fix (non-breaking change fixing an issue)
- [ ] 🔄 Refactoring / Code Cleanup (no functional or logic changes)
- [ ] 🗄️ Database Migration / Schema Update
- [ ] ☁️ Infrastructure / DevOps (Docker, Kubernetes, Terraform, Helm)
- [ ] 📚 Documentation Update

## 🏗️ Architectural & Database Impact
- **Layer(s) Modified:** `[ ] Domain` `[ ] Application` `[ ] Infra` `[ ] API` `[ ] Web (Angular)`
- **Clean Architecture Boundaries Preserved:** Yes / No
- **EF Core Migrations Included:** (List migration name or 'N/A')
- **Infrastructure Changes:** (Kubernetes `k8s/`, Terraform `infra/`, Docker Compose)

## 🧪 Testing Matrix & Verification
- [ ] **Domain Unit Tests:** `dotnet test AutoReparos.Domain.Tests`
- [ ] **Application Unit Tests:** `dotnet test AutoReparos.Application.Tests`
- [ ] **Integration Tests:** `dotnet test AutoReparos.IntegrationTests`
- [ ] **Angular Frontend Build & Verification:** `yarn build` (AutoReparos.Web)
- [ ] **Manual End-to-End Testing:** Tested via Swagger / Postman / Browser UI

## 🔒 Security & Breaking Changes Checklist
- [ ] **Secrets Check:** No hardcoded secrets, connection strings, or JWT keys committed.
- [ ] **Authorization:** Endpoints appropriately protected with `[Authorize]` attributes.
- [ ] **Breaking Changes:** No breaking changes to existing REST contracts or DB tables.
- [ ] **Input Validation:** Domain invariants and DTO validations enforced.
