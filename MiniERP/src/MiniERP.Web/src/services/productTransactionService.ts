import api from './api';

export interface ProductTransactionCreate {
  type: string; // 'Expense' | 'Income'
  amount: number;
  description?: string | null;
  date?: string | null;
  productId: number;
}

export interface ProductTransaction {
  id: number;
  type: string;
  amount: number;
  description?: string | null;
  date: string;
  productId: number;
}

export const getTransactionsByProduct = async (productId: number): Promise<ProductTransaction[]> => {
  const res = await api.get(`/products/${productId}/transactions`);
  return res.data;
};

export const createTransaction = async (productId: number, payload: ProductTransactionCreate): Promise<ProductTransaction> => {
  const res = await api.post(`/products/${productId}/transactions`, payload);
  return res.data;
};

export default { getTransactionsByProduct, createTransaction };
