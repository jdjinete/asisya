import React, { useEffect, useState } from 'react';
import { productApi, ProductDetailDto } from '../api/productApi';
import { X, Image as ImageIcon, Box, Layers, DollarSign, AlertCircle } from 'lucide-react';

interface ProductDetailModalProps {
  productId: number | null;
  onClose: () => void;
}

export const ProductDetailModal: React.FC<ProductDetailModalProps> = ({ productId, onClose }) => {
  const [product, setProduct] = useState<ProductDetailDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!productId) {
      setProduct(null);
      return;
    }

    setLoading(true);
    setError(null);

    productApi.getProductById(productId)
      .then((data) => setProduct(data))
      .catch((err) => {
        console.error(err);
        setError('Failed to load product details.');
      })
      .finally(() => setLoading(false));
  }, [productId]);

  if (!productId) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <Box size={22} color="var(--primary)" />
            <h2 style={{ fontSize: '1.2rem', fontWeight: 600 }}>Product Specification</h2>
          </div>
          <button onClick={onClose} className="btn btn-outline btn-sm" style={{ padding: '0.3rem' }}>
            <X size={18} />
          </button>
        </div>

        <div className="modal-body">
          {loading && (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
              Loading product specifications and category media...
            </div>
          )}

          {error && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--danger)', padding: '1rem' }}>
              <AlertCircle size={20} />
              <span>{error}</span>
            </div>
          )}

          {product && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
              <div>
                <span className={`badge ${product.discontinued ? 'badge-discontinued' : 'badge-active'}`} style={{ marginBottom: '0.5rem' }}>
                  {product.discontinued ? 'Discontinued' : 'Active Catalog'}
                </span>
                <h3 style={{ fontSize: '1.4rem', fontWeight: 700 }}>{product.productName}</h3>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>SKU ID: #{product.productId}</p>
              </div>

              {/* Category Info with Picture */}
              <div style={{
                backgroundColor: 'rgba(15, 23, 42, 0.6)',
                borderRadius: 'var(--radius-sm)',
                padding: '1rem',
                border: '1px solid var(--border-color)',
                display: 'flex',
                gap: '1rem',
                alignItems: 'center'
              }}>
                <div style={{
                  width: '70px',
                  height: '70px',
                  borderRadius: 'var(--radius-sm)',
                  backgroundColor: 'var(--bg-input)',
                  border: '1px solid var(--border-color)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  overflow: 'hidden',
                  flexShrink: 0
                }}>
                  {product.category?.pictureBase64 ? (
                    <img
                      src={product.category.pictureBase64}
                      alt={product.category.categoryName}
                      style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                    />
                  ) : (
                    <ImageIcon size={28} color="var(--text-muted)" />
                  )}
                </div>

                <div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <Layers size={16} color="var(--primary)" />
                    <span style={{ fontWeight: 600, fontSize: '0.95rem' }}>
                      Category: {product.category?.categoryName || 'Uncategorized'}
                    </span>
                  </div>
                  <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginTop: '0.2rem' }}>
                    {product.category?.description || 'No descriptive metadata provided for this category.'}
                  </p>
                </div>
              </div>

              {/* Metric Grid */}
              <div style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(2, 1fr)',
                gap: '1rem'
              }}>
                <div style={{ padding: '0.8rem', backgroundColor: 'rgba(15, 23, 42, 0.4)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase' }}>Unit Price</span>
                  <div style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--success)', display: 'flex', alignItems: 'center', gap: '0.2rem' }}>
                    <DollarSign size={18} />
                    {product.unitPrice?.toFixed(2) || '0.00'}
                  </div>
                </div>

                <div style={{ padding: '0.8rem', backgroundColor: 'rgba(15, 23, 42, 0.4)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase' }}>Packaging Unit</span>
                  <div style={{ fontSize: '1rem', fontWeight: 600, marginTop: '0.2rem' }}>
                    {product.quantityPerUnit || 'Standard unit'}
                  </div>
                </div>

                <div style={{ padding: '0.8rem', backgroundColor: 'rgba(15, 23, 42, 0.4)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase' }}>Units In Stock</span>
                  <div style={{ fontSize: '1.1rem', fontWeight: 600, marginTop: '0.2rem' }}>
                    {product.unitsInStock ?? 0} units
                  </div>
                </div>

                <div style={{ padding: '0.8rem', backgroundColor: 'rgba(15, 23, 42, 0.4)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase' }}>Units On Order</span>
                  <div style={{ fontSize: '1.1rem', fontWeight: 600, marginTop: '0.2rem' }}>
                    {product.unitsOnOrder ?? 0} units
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="modal-footer">
          <button onClick={onClose} className="btn btn-secondary">
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
