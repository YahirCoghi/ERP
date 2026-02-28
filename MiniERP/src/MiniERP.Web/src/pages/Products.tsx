import { useEffect, useState } from 'react';
import productService, { ProductCreate, Product } from '../services/productService';
import transactionService, { ProductTransactionCreate, ProductTransaction } from '../services/productTransactionService';

function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Product | null>(null);
  const [form, setForm] = useState<ProductCreate>({ name: '', price: 0, cost: 0, stock: 0, minStock: 0 });
  const [txModalOpen, setTxModalOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [transactions, setTransactions] = useState<ProductTransaction[]>([]);
  const [txForm, setTxForm] = useState<ProductTransactionCreate>({ type: 'Expense', amount: 0, productId: 0 });

  useEffect(() => { fetchProducts(); }, []);

  const fetchProducts = async () => {
    const data = await productService.getProducts();
    setProducts(data);
  };

  const openNew = () => { setEditing(null); setForm({ name: '', price: 0, cost: 0, stock: 0, minStock: 0 }); setShowModal(true); };

  const handleSubmit = async (e: any) => {
    e.preventDefault();
    if (editing) {
      await productService.updateProduct(editing.id, form);
    } else {
      await productService.createProduct(form);
    }
    setShowModal(false);
    fetchProducts();
  };

  const handleEdit = (p: Product) => { setEditing(p); setForm({ name: p.name, price: p.price, cost: p.cost, stock: p.stock, minStock: p.minStock, description: p.description, category: p.category, unit: p.unit, code: p.code }); setShowModal(true); };
  const openTransactions = async (p: Product) => {
    setSelectedProduct(p);
    setTxForm({ type: 'Expense', amount: 0, productId: p.id });
    const data = await transactionService.getTransactionsByProduct(p.id);
    setTransactions(data);
    setTxModalOpen(true);
  };

  const handleDelete = async (id: number) => { if (confirm('¿Eliminar?')) { await productService.deleteProduct(id); fetchProducts(); } };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>Productos</h2>
        <button onClick={openNew}>+ Nuevo</button>
      </div>
      <table>
        <thead><tr><th>Código</th><th>Nombre</th><th>Precio</th><th>Stock</th><th>Acciones</th></tr></thead>
        <tbody>
          {products.map(p => (
            <tr key={p.id}><td>{p.code}</td><td>{p.name}</td><td>{p.price}</td><td>{p.stock}</td><td><button onClick={() => handleEdit(p)}>Editar</button> <button onClick={() => handleDelete(p.id)}>Borrar</button> <button onClick={() => openTransactions(p)}>Transacciones</button></td></tr>
          ))}
        </tbody>
      </table>

      {showModal && (
        <div className="modal">
          <form onSubmit={handleSubmit}>
            <div>
              <label>Nombre</label>
              <input value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
            </div>
            <div>
              <label>Precio</label>
              <input type="number" value={form.price} onChange={e => setForm({ ...form, price: parseFloat(e.target.value) })} required />
            </div>
            <div>
              <label>Stock</label>
              <input type="number" value={form.stock} onChange={e => setForm({ ...form, stock: parseInt(e.target.value) })} required />
            </div>
            <div style={{ marginTop: 8 }}>
              <button type="button" onClick={() => setShowModal(false)}>Cancelar</button>
              <button type="submit">Guardar</button>
            </div>
          </form>
        </div>
      )}

      {txModalOpen && selectedProduct && (
        <div className="modal">
          <h3>Transacciones - {selectedProduct.name} ({selectedProduct.code})</h3>
          <div>
            <h4>Historial</h4>
            <table>
              <thead><tr><th>Fecha</th><th>Tipo</th><th>Monto</th><th>Descripción</th></tr></thead>
              <tbody>
                {transactions.map(tx => (
                  <tr key={tx.id}><td>{new Date(tx.date).toLocaleString()}</td><td>{tx.type}</td><td>{tx.amount}</td><td>{tx.description}</td></tr>
                ))}
              </tbody>
            </table>
          </div>
          <div>
            <h4>Agregar transacción</h4>
            <form onSubmit={async (e) => {
              e.preventDefault();
              await transactionService.createTransaction(selectedProduct.id, txForm);
              const data = await transactionService.getTransactionsByProduct(selectedProduct.id);
              setTransactions(data);
              setTxForm({ type: 'Expense', amount: 0, productId: selectedProduct.id });
            }}>
              <div>
                <label>Tipo</label>
                <select value={txForm.type} onChange={e => setTxForm({ ...txForm, type: e.target.value })}>
                  <option value="Expense">Gasto</option>
                  <option value="Income">Ingreso</option>
                </select>
              </div>
              <div>
                <label>Monto</label>
                <input type="number" step="0.01" value={txForm.amount} onChange={e => setTxForm({ ...txForm, amount: parseFloat(e.target.value) })} required />
              </div>
              <div>
                <label>Descripción</label>
                <input value={txForm.description || ''} onChange={e => setTxForm({ ...txForm, description: e.target.value })} />
              </div>
              <div style={{ marginTop: 8 }}>
                <button type="button" onClick={() => { setTxModalOpen(false); setSelectedProduct(null); }}>Cerrar</button>
                <button type="submit">Guardar transacción</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default ProductsPage;
