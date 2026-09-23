import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { productApi, ProductSummaryDto } from '../api/productApi';
import { Navbar } from '../components/Navbar';
import { ProductDetailModal } from '../components/ProductDetailModal';
import { BulkUploadModal } from '../components/BulkUploadModal';
import {
  Search,
  Plus,
  UploadCloud,
  ChevronLeft,
  ChevronRight,
  Eye,
  RefreshCw,
  Server,
  Cloud
} from 'lucide-react';

export const ProductsPage: React.FC = () => {
  const navigate = useNavigate();

  // State
  const [products, setProducts] = useState<ProductSummaryDto[]>([]);
  const [pageIndex, setPageIndex] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [totalItems, setTotalItems] = useState<number>(0);
  const [totalPages, setTotalPages] = useState<number>(1);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [categoryId, setCategoryId] = useState<number | undefined>(undefined);
  const [sortBy, setSortBy] = useState<string>('name');
  const [sortOrder, setSortOrder] = useState<string>('asc');
  const [loading, setLoading] = useState<boolean>(false);

  // Modals
  const [selectedProductId, setSelectedProductId] = useState<number | null>(null);
  const [isBulkModalOpen, setIsBulkModalOpen] = useState<boolean>(false);

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
                  <option value={1}>SERVIDORES (Physical Hardware)</option>
                  <option value={2}>CLOUD (Virtual Infrastructure)</option>
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
                <option value="stock-desc">Stock (Highest First)</option>
              </select>
            </div>

            {/* Refresh button */}
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button
                type="button"
                onClick={fetchProducts}
                className="btn btn-outline"
                style={{ height: '40px', width: '100%', justifyContent: 'center' }}
                title="Refresh table"
              >
                <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
                Refresh
              </button>
            </div>
          </div>
        </div>

        {/* Product Table */}
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Product Name</th>
                <th>Category</th>
                <th>Packaging</th>
                <th style={{ textAlign: 'right' }}>Unit Price</th>
                <th style={{ textAlign: 'right' }}>Stock</th>
                <th>Status</th>
                <th style={{ textAlign: 'center' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading && products.length === 0 ? (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
                    Loading products from database...
                  </td>
                </tr>
              ) : products.length === 0 ? (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-muted)' }}>
                    No products matched your search criteria.
                  </td>
                </tr>
              ) : (
                products.map((p) => (
                  <tr key={p.productId}>
                    <td style={{ color: 'var(--text-muted)' }}>#{p.productId}</td>
                    <td style={{ fontWeight: 600 }}>{p.productName}</td>
                    <td>
                      {p.categoryName === 'SERVIDORES' ? (
                        <span className="badge badge-servidores" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.3rem' }}>
                          <Server size={12} /> {p.categoryName}
                        </span>
                      ) : p.categoryName === 'CLOUD' ? (
                        <span className="badge badge-cloud" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.3rem' }}>
                          <Cloud size={12} /> {p.categoryName}
                        </span>
                      ) : (
                        <span className="badge" style={{ backgroundColor: 'rgba(148, 163, 184, 0.2)', color: 'var(--text-secondary)' }}>
                          {p.categoryName || 'General'}
                        </span>
                      )}
                    </td>
                    <td style={{ color: 'var(--text-secondary)' }}>{p.quantityPerUnit || '-'}</td>
                    <td style={{ textAlign: 'right', fontWeight: 600, color: 'var(--success)' }}>
                      ${p.unitPrice?.toFixed(2) || '0.00'}
                    </td>
                    <td style={{ textAlign: 'right' }}>{p.unitsInStock ?? 0}</td>
                    <td>
                      <span className={`badge ${p.discontinued ? 'badge-discontinued' : 'badge-active'}`}>
                        {p.discontinued ? 'Discontinued' : 'Active'}
                      </span>
                    </td>
                    <td style={{ textAlign: 'center' }}>
                      <button
                        onClick={() => setSelectedProductId(p.productId)}
                        className="btn btn-outline btn-sm"
                        style={{ padding: '0.3rem 0.6rem' }}
                        title="View product specifications and category picture"
                      >
                        <Eye size={15} />
                        View
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Server-Side Pagination Bar */}
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem',
          marginTop: '1.5rem',
          padding: '0.5rem 0'
        }}>
          <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
            Page <strong>{pageIndex}</strong> of <strong>{totalPages}</strong> ({totalItems.toLocaleString()} total items)
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <select
              value={pageSize}
              onChange={(e) => {
                setPageSize(Number(e.target.value));
                setPageIndex(1);
              }}
              style={{ padding: '0.4rem 0.6rem', fontSize: '0.85rem' }}
            >
              <option value={10}>10 per page</option>
              <option value={25}>25 per page</option>
              <option value={50}>50 per page</option>
              <option value={100}>100 per page</option>
            </select>

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
    </div>
  );
};
