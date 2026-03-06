import { useEffect, useState } from 'react'
import axios from 'axios'

interface Product {
  id: number
  code: string
  name: string
  description?: string
  price: number
  cost: number
  stock: number
  minStock: number
  category?: string
  unit?: string
}

function Products() {
  const [products, setProducts] = useState<Product[]>([])
  const [showModal, setShowModal] = useState(false)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null)
  const [formData, setFormData] = useState({
    code: '',
    name: '',
    description: '',
    price: '',
    cost: '',
    stock: '',
    minStock: '',
    category: '',
    unit: ''
  })

  useEffect(() => {
    fetchProducts()
  }, [])

  const fetchProducts = async () => {
    try {
      const response = await axios.get('/api/products')
      setProducts(response.data)
    } catch (error) {
      console.error('Error fetching products:', error)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const data = {
        ...formData,
        price: parseFloat(formData.price),
        cost: parseFloat(formData.cost),
        stock: parseInt(formData.stock),
        minStock: parseInt(formData.minStock)
      }

      if (editingProduct) {
        await axios.put(`/api/products/${editingProduct.id}`, { ...data, id: editingProduct.id })
      } else {
        await axios.post('/api/products', data)
      }

      setShowModal(false)
      setEditingProduct(null)
      setFormData({ code: '', name: '', description: '', price: '', cost: '', stock: '', minStock: '', category: '', unit: '' })
      fetchProducts()
    } catch (error) {
      console.error('Error saving product:', error)
      alert('Error al guardar el producto')
    }
  }

  const handleEdit = (product: Product) => {
    setEditingProduct(product)
    setFormData({
      code: product.code,
      name: product.name,
      description: product.description || '',
      price: product.price.toString(),
      cost: product.cost.toString(),
      stock: product.stock.toString(),
      minStock: product.minStock.toString(),
      category: product.category || '',
      unit: product.unit || ''
    })
    setShowModal(true)
  }

  const handleDelete = async (id: number) => {
    if (confirm('¿Está seguro de eliminar este producto?')) {
      try {
        await axios.delete(`/api/products/${id}`)
        fetchProducts()
      } catch (error) {
        console.error('Error deleting product:', error)
        alert('Error al eliminar el producto')
      }
    }
  }

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>Productos</h2>
        <button className="btn btn-success" onClick={() => setShowModal(true)}>
          + Nuevo Producto
        </button>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Código</th>
              <th>Nombre</th>
              <th>Categoría</th>
              <th>Precio</th>
              <th>Stock</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {products.map(product => (
              <tr key={product.id}>
                <td>{product.code}</td>
                <td>{product.name}</td>
                <td>{product.category || '-'}</td>
                <td>${product.price.toFixed(2)}</td>
                <td>{product.stock}</td>
                <td>
                  <button className="btn btn-primary" style={{ marginRight: '5px' }} onClick={() => handleEdit(product)}>
                    Editar
                  </button>
                  <button className="btn btn-danger" onClick={() => handleDelete(product.id)}>
                    Eliminar
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>{editingProduct ? 'Editar Producto' : 'Nuevo Producto'}</h3>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Código</label>
                <input
                  type="text"
                  className="form-control"
                  value={editingProduct ? formData.code : 'Auto-generated'}
                  readOnly
                  placeholder={editingProduct ? '' : 'Auto-generated'}
                  required={false}
                />
              </div>
              <div className="form-group">
                <label>Nombre</label>
                <input type="text" className="form-control" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} required />
              </div>
              <div className="form-group">
                <label>Descripción</label>
                <input type="text" className="form-control" value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})} />
              </div>
              <div className="form-group">
                <label>Precio</label>
                <input type="number" step="0.01" className="form-control" value={formData.price} onChange={e => setFormData({...formData, price: e.target.value})} required />
              </div>
              <div className="form-group">
                <label>Costo</label>
                <input type="number" step="0.01" className="form-control" value={formData.cost} onChange={e => setFormData({...formData, cost: e.target.value})} required />
              </div>
              <div className="form-group">
                <label>Stock</label>
                <input type="number" className="form-control" value={formData.stock} onChange={e => setFormData({...formData, stock: e.target.value})} required />
              </div>
              <div className="form-group">
                <label>Stock Mínimo</label>
                <input type="number" className="form-control" value={formData.minStock} onChange={e => setFormData({...formData, minStock: e.target.value})} required />
              </div>
              <div className="form-group">
                <label>Categoría</label>
                <input type="text" className="form-control" value={formData.category} onChange={e => setFormData({...formData, category: e.target.value})} />
              </div>
              <div className="form-group">
                <label>Unidad</label>
                <input type="text" className="form-control" value={formData.unit} onChange={e => setFormData({...formData, unit: e.target.value})} />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn" style={{ background: '#95a5a6', color: 'white' }} onClick={() => setShowModal(false)}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-success">
                  Guardar
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default Products
