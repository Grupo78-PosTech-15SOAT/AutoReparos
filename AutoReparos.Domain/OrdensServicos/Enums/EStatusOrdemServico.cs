using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Domain.OrdensServicos.Enums
{
    public enum EStatusOrdemServico
    {
        Recebida = 1,
        EmDiagnostico = 2,
        AguardandoAprovacao = 3,
        EmExecucao = 4,
        Finalizada = 5,
        Entregue = 6
    }
}
