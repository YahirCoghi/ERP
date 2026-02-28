import api from './api';

export interface ProductCreate {
  code?: string | null;
  name: string;
  description?: string | null;
  price: number;
  cost: number;
  stock: number;
  minStock: number;
  category?: string | null;
  unit?: string | null;
}

export interface Product {
  id: number;
  code: string;
  name: string;
  description?: string | null;
  price: number;
  cost: number;
  stock: number;
  minStock: number;
  category?: string | null;
  unit?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export const getProducts = async (): Promise<Product[]> => {
  const res = await api.get('/products');
  return res.data;
};

export const getProduct = async (id: number): Promise<Product> => {
  const res = await api.get(`/products/${id}`);
  return res.data;
};

export const createProduct = async (payload: ProductCreate): Promise<Product> => {
  const res = await api.post('/products', payload);
  return res.data;
};

export const updateProduct = async (id: number, payload: ProductCreate) => {
  await api.put(`/products/${id}`, { ...payload, id });
};

export const deleteProduct = async (id: number) => {
  await api.delete(`/products/${id}`);
};

export default { getProducts, getProduct, createProduct, updateProduct, deleteProduct };
