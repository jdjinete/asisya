import React, { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { productApi, ProductSummaryDto } from '../api/productApi';
import { categoryApi, CategoryDto } from '../api/categoryApi';
import { Navbar } from '../components/Navbar';
import { ProductDetailModal } from '../components/ProductDetailModal';
import { BulkUploadModal } from '../components/BulkUploadModal';
import { CategoryManagementModal } from '../components/CategoryManagementModal';
import {
  Search,
  Plus,
  UploadCloud,
  ChevronLeft,
  ChevronRight,
  Eye,
  Edit,
  Trash2,
  AlertTriangle,
  RefreshCw,
  Server,
  Cloud,
  Tags
} from 'lucide-react';
import { toast } from 'sonner';

export const ProductsPage: React.FC = () => {
  const navigate = useNavigate();

  // State
  const [products, setProducts] = useState<ProductSummaryDto[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [pageIndex, setPageIndex] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [totalItems, setTotalItems] = useState<number>(0);
  const [totalPages, setTotalPages] = useState<number>(1);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [categoryId, setCategoryId] = useState<number | undefined>(undefined);
  const [sortBy, setSortBy] = useState<string>('name');
  const [sortOrder, setSortOrder] = useState<string>('asc');
  const [loading, setLoading] = useState<boolean>(false);

  // Modals & Actions
  const [selectedProductId, setSelectedProductId] = useState<number | null>(null);
  const [isBulkModalOpen, setIsBulkModalOpen] = useState<boolean>(false);
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState<boolean>(false);
  const [productToDelete, setProductToDelete] = useState<ProductSummaryDto | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  const handleDeleteConfirm = async () => {
    if (!productToDelete) return;
    setIsDeleting(true);
    try {
      await productApi.deleteProduct(productToDelete.productId);
      toast.success(`Product "${productToDelete.productName}" deleted successfully.`);
      setProductToDelete(null);
      fetchProducts();
    } catch (err: any) {
      console.error(err);
      toast.error(err.response?.data?.detail || 'Failed to delete product.');
    } finally {
      setIsDeleting(false);
    }
  };

  const fetchProducts = async () => {
    setLoading(true);
    try {
      const data = await productApi.getProducts({
        pageIndex,
        pageSize,
        searchTerm: searchTerm.trim() || undefined,
        categoryId,
        sortBy,
        sortOrder
      });

      setProducts(data.items);
      setPageIndex(data.pageIndex);
      setPageSize(data.pageSize);
      setTotalItems(data.totalItems);
      setTotalPages(data.totalPages);
    } catch (err) {
      console.error('Failed to fetch products', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchCategories = useCallback(async () => {
    try {
      const data = await categoryApi.getCategories();
      setCategories(data);
    } catch (err) {
      console.error('Failed to fetch categories', err);
    }
  }, []);

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  useEffect(() => {
    fetchProducts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageIndex, pageSize, categoryId, sortBy, sortOrder]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    fetchProducts();
  };

  return (
    <div className="app-container">
      <Navbar />

      <main className="main-content">
        {/* Top Header & Action Bar */}
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem',
          marginBottom: '1.5rem'
        }}>
          <div>
            <h1 style={{ fontSize: '1.75rem', fontWeight: 700 }}>Commercial Catalog</h1>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
              Showing {totalItems.toLocaleString()} total items in PostgreSQL
            </p>
          </div>

          <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
            <button
              onClick={() => setIsCategoryModalOpen(true)}
              className="btn btn-outline"
            >
              <Tags size={18} />
              Categories
            </button>
            <button
              onClick={() => setIsBulkModalOpen(true)}
              className="btn btn-secondary"
            >
              <UploadCloud size={18} />
              Bulk Streaming Ingest
            </button>
            <button
              onClick={() => navigate('/products/new')}
              className="btn btn-primary"
            >
              <Plus size={18} />
              New Product
            </button>
          </div>
        </div>

        {/* Filters and Search Bar */}
        <div className="card" style={{ marginBottom: '1.5rem', padding: '1rem 1.25rem' }}>
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
            gap: '1rem',
            alignItems: 'end'
          }}>
            {/* Search Input */}
            <form onSubmit={handleSearchSubmit} style={{ position: 'relative' }}>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, marginBottom: '0.3rem', color: 'var(--text-secondary)' }}>
                Search by Name
              </label>
              <div style={{ position: 'relative' }}>
                <input
                  type="text"
                  placeholder="e.g. PowerEdge, AWS..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  style={{ width: '100%', paddingLeft: '2.4rem' }}
                />
                <Search size={16} color="var(--text-muted)" style={{ position: 'absolute', left: '0.8rem', top: '50%', transform: 'translateY(-50%)' }} />
              </div>
            </form>

            {/* Category Filter */}
            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, marginBottom: '0.3rem', color: 'var(--text-secondary)' }}>
                Filter by Category
              </label>
              <div style={{ position: 'relative' }}>
                <select
                  value={categoryId ?? ''}
                  onChange={(e) => {
                    const val = e.target.value;
                    setCategoryId(val ? Number(val) : undefined);
                    setPageIndex(1);
                  }}
                  style={{ width: '100%' }}
                >
                  <option value="">All Categories</option>
                  {categories.map((cat) => (
                    <option key={cat.categoryId} value={cat.categoryId}>
                      {cat.categoryName} ({cat.productCount})
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* Sort Dropdown */}
            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, marginBottom: '0.3rem', color: 'var(--text-secondary)' }}>
                Sort Order
              </label>
              <select
                value={`${sortBy}-${sortOrder}`}
                onChange={(e) => {
                  const [field, order] = e.target.value.split('-');
                  setSortBy(field);
                  setSortOrder(order);
                }}
                style={{ width: '100%' }}
              >
                <option value="name-asc">Product Name (A - Z)</option>
                <option value="name-desc">Product Name (Z - A)</option>
                <option value="price-asc">Price (Low to High)</option>
                <option value="price-desc">Price (High to Low)</option>
                <option value="stock-asc">Stock (Low to High)</option>
                <option value="stock-desc">Stock (High to Low)</option>
              </select>
            </div>

            {/* Refresh Button */}
            <div>
              <button
                type="button"
                onClick={() => fetchProducts()}
                className="btn btn-secondary"
                style={{ width: '100%' }}
                disabled={loading}
              >
                <RefreshCw size={16} className={loading ? 'spin' : ''} />
                Refresh
              </button>
            </div>
          </div>
        </div>

        {/* Data Table */}
        <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th style={{ width: '80px' }}>ID</th>
                  <th>Product Name</th>
                  <th>Category</th>
                  <th>Unit Price</th>
                  <th>Stock</th>
                  <th>Packaging</th>
                  <th>Status</th>
                  <th style={{ textAlign: 'right' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {loading && products.length === 0 ? (
                  <tr>
                    <td colSpan={8} style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
                      Loading catalog from PostgreSQL...
                    </td>
                  </tr>
                ) : products.length === 0 ? (
                  <tr>
                    <td colSpan={8} style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
                      No products found matching the criteria.
                    </td>
                  </tr>
                ) : (
                  products.map((p) => (
                    <tr key={p.productId}>
                      <td style={{ color: 'var(--text-muted)', fontFamily: 'monospace' }}>
                        #{p.productId}
                      </td>
                      <td style={{ fontWeight: 600, color: 'var(--text-primary)' }}>
                        {p.productName}
                      </td>
                      <td>
                        <span className="badge" style={{
                          backgroundColor: p.categoryName === 'CLOUD' ? 'rgba(59, 130, 246, 0.15)' : 'rgba(16, 185, 129, 0.15)',
                          color: p.categoryName === 'CLOUD' ? '#60a5fa' : '#34d399',
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: '0.35rem'
                        }}>
                          {p.categoryName === 'CLOUD' ? <Cloud size={12} /> : <Server size={12} />}
                          {p.categoryName || 'Unassigned'}
                        </span>
                      </td>
                      <td style={{ fontFamily: 'monospace', fontWeight: 600 }}>
                        ${p.unitPrice?.toFixed(2) ?? '0.00'}
                      </td>
                      <td>
                        <span style={{
                          color: (p.unitsInStock ?? 0) <= 5 ? 'var(--danger)' : 'inherit',
                          fontWeight: (p.unitsInStock ?? 0) <= 5 ? 700 : 'normal'
                        }}>
                          {p.unitsInStock ?? 0} units
                        </span>
                      </td>
                      <td style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                        {p.quantityPerUnit || 'N/A'}
                      </td>
                      <td>
                        {p.discontinued ? (
                          <span className="badge badge-danger">Discontinued</span>
                        ) : (
                          <span className="badge badge-success">Active</span>
                        )}
                      </td>
                      <td style={{ textAlign: 'right' }}>
                        <div style={{ display: 'inline-flex', gap: '0.4rem', justifyContent: 'flex-end' }}>
                          <button
                            onClick={() => setSelectedProductId(p.productId)}
                            className="btn-icon"
                            title="Inspect Product & Category Details"
                          >
                            <Eye size={16} />
                          </button>
                          <button
                            onClick={() => navigate(`/products/edit/${p.productId}`)}
                            className="btn-icon"
                            title="Edit Product"
                          >
                            <Edit size={16} color="var(--primary)" />
                          </button>
                          <button
                            onClick={() => setProductToDelete(p)}
                            className="btn-icon"
                            title="Delete Product"
                          >
                            <Trash2 size={16} color="var(--danger)" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination Controls */}
          <div className="pagination">
            <div className="pagination-info">
              Showing page <strong>{pageIndex}</strong> of <strong>{totalPages}</strong> ({totalItems.toLocaleString()} total items)
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Per page:</label>
              <select
                value={pageSize}
                onChange={(e) => {
                  setPageSize(Number(e.target.value));
                  setPageIndex(1);
                }}
                style={{ padding: '0.2rem 0.5rem', fontSize: '0.8rem' }}
              >
                <option value={10}>10</option>
                <option value={25}>25</option>
                <option value={50}>50</option>
                <option value={100}>100</option>
              </select>
            </div>

            <div className="pagination-controls">
              <button
                onClick={() => setPageIndex((p) => Math.max(1, p - 1))}
                disabled={pageIndex <= 1 || loading}
                className="btn btn-outline btn-sm"
              >
                <ChevronLeft size={16} /> Prev
              </button>

              <button
                onClick={() => setPageIndex((p) => Math.min(totalPages, p + 1))}
                disabled={pageIndex >= totalPages || loading}
                className="btn btn-outline btn-sm"
              >
                Next <ChevronRight size={16} />
              </button>
            </div>
          </div>
        </div>
      </main>

      {/* Product Detail Modal */}
      <ProductDetailModal
        productId={selectedProductId}
        onClose={() => setSelectedProductId(null)}
      />

      {/* Bulk Upload Modal */}
      <BulkUploadModal
        isOpen={isBulkModalOpen}
        onClose={() => setIsBulkModalOpen(false)}
        onSuccess={() => {
          fetchProducts();
        }}
      />

      {/* Delete Confirmation Modal */}
      {productToDelete && (
        <div className="modal-overlay" onClick={() => !isDeleting && setProductToDelete(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '480px' }}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
                <AlertTriangle size={20} color="var(--danger)" />
                <h3 style={{ fontSize: '1.1rem', fontWeight: 600 }}>Confirm Product Deletion</h3>
              </div>
            </div>
            <div className="modal-body">
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '1rem' }}>
                Are you sure you want to delete product <strong>"{productToDelete.productName}"</strong> (ID: #{productToDelete.productId})?
              </p>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                This action cannot be undone and will record a permanent <code>DELETE</code> mutation event in the Audit Logs trail.
              </p>
            </div>
            <div className="modal-footer">
              <button
                type="button"
                onClick={() => setProductToDelete(null)}
                className="btn btn-secondary btn-sm"
                disabled={isDeleting}
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleDeleteConfirm}
                className="btn btn-danger btn-sm"
                disabled={isDeleting}
              >
                {isDeleting ? 'Deleting...' : 'Confirm Delete'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Category Management Modal */}
      <CategoryManagementModal
        isOpen={isCategoryModalOpen}
        onClose={() => setIsCategoryModalOpen(false)}
        onCategoriesChanged={() => {
          fetchCategories();
          fetchProducts();
        }}
      />
    </div>
  );
};
