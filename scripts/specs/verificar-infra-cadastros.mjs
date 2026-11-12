// Provas de infraestrutura da feature `cadastros` (portáveis: cmd.exe e bash; só exigem node e helm).
//
// Uso:
//   node scripts/specs/verificar-infra-cadastros.mjs rds-sem-lambda
//   node scripts/specs/verificar-infra-cadastros.mjs networkpolicy
//
// rds-sem-lambda: nenhum workflow passa o security group da Lambda ao RDS e a camada de banco exige só o SG do EKS.
// networkpolicy: o chart do Cadastros renderiza uma NetworkPolicy que libera só as origens previstas.
import { readFileSync, readdirSync } from 'node:fs';
import { spawnSync } from 'node:child_process';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const raiz = join(dirname(fileURLToPath(import.meta.url)), '..', '..');

function falhar(mensagem) {
  console.error(`FALHA: ${mensagem}`);
  process.exit(1);
}

function rdsSemLambda() {
  const pasta = join(raiz, '.github', 'workflows');
  for (const arquivo of readdirSync(pasta).filter((f) => f.endsWith('.yml'))) {
    readFileSync(join(pasta, arquivo), 'utf8').split(/\r?\n/).forEach((linha, i) => {
      if (linha.includes('TF_VAR_allowed_security_group_ids') && /\$lambda|lambda_sg|lambda_security_group/.test(linha)) {
        falhar(`${arquivo}:${i + 1} ainda libera a Lambda no SG do RDS`);
      }
    });
  }

  const variaveis = readFileSync(join(raiz, 'submodules', 'AutoReparos.Infra.Database', 'terraform', 'variables.tf'), 'utf8');
  const bloco = variaveis.split('variable "allowed_security_group_ids"')[1]?.split('variable ')[0] ?? '';
  if (!bloco) falhar('variables.tf não declara allowed_security_group_ids');
  if (bloco.includes('>= 2')) falhar('variables.tf ainda exige dois security groups (EKS e Lambda)');
  console.log('ok: o RDS só libera o security group do EKS');
}

function networkPolicy() {
  const chart = join(raiz, 'submodules', 'AutoReparos.Infra.K8s', 'k8s');
  const r = spawnSync('helm', ['template', 'autoreparos', chart, '--set', 'api.networkPolicy.allowedCidrs[0]=10.0.0.0/16'], { encoding: 'utf8', shell: process.platform === 'win32' });
  if (r.status !== 0) falhar(`helm template falhou: ${(r.stderr || '').slice(0, 300)}`);
  const partes = r.stdout.split('kind: NetworkPolicy');
  if (partes.length < 2) falhar('o chart não renderiza NetworkPolicy');
  const trecho = partes[1].split('\n---')[0];
  for (const esperado of ['policyTypes:', '- Ingress', 'ingress-nginx', '10.0.0.0/16', 'podSelector: {}']) {
    if (!trecho.includes(esperado)) falhar(`a NetworkPolicy não contém '${esperado}'`);
  }
  console.log('ok: NetworkPolicy renderizada com origens restritas');
}

const comandos = { 'rds-sem-lambda': rdsSemLambda, networkpolicy: networkPolicy };
const comando = comandos[process.argv[2]];
if (!comando) falhar(`uso: node verificar-infra-cadastros.mjs ${Object.keys(comandos).join('|')}`);
comando();
