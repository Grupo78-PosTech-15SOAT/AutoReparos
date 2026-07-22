export interface Insumo {
  id?: string;
  nome: string;
  descricao?: string;
  quantidadeEstoque: number;
  quantidadeMinima: number;
  precoUnitario: number;
  abaixoEstoqueMinimo?: boolean;
}
