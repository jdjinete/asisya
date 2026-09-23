import React, { useState, useEffect, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { productApi } from '../api/productApi';
import { categoryApi, CategoryDto } from '../api/categoryApi';
import { Navbar } from '../components/Navbar';
import { CategoryManagementModal } from '../components/CategoryManagementModal';
import { Box, Save, ArrowLeft, AlertCircle, Settings } from 'lucide-react';

interface ProductFormData {
  productName: string;
  categoryId: number;
  unitPrice: number;
  unitsInStock: number;
  quantityPerUnit: string;
  discontinued: boolean;
}

/**
 * Product creation and edition form using react-hook-form.
 * Implements strict schema validation and error feedback simulating Angular Reactive Forms.
 */
export const ProductFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors }
  } = useForm<ProductFormData>({
    defaultValues: {
      productName: '',
      categoryId: 1,
      unitPrice: 199.99,
      unitsInStock: 20,
      quantityPerUnit: '1 Unit standard',
      discontinued: false
    }
  });

  const loadCategories = useCallback(async () => {
    try {
      const data = await categoryApi.getCategories();
      setCategories(data);
      if (data.length > 0) {
        setValue('categoryId', data[0].categoryId);
      }
    } catch (err) {
      console.error('Failed to load categories', err);
    }
  }, [setValue]);

  useEffect(() => {
    loadCategories();
  }, [loadCategories]);

  const onSubmit = async (data: ProductFormData) => {
    setSubmitting(true);
    setServerError(null);

    try {
      await productApi.bulkCreateProducts({
        products: [
          {
            productName: data.productName,
            categoryId: Number(data.categoryId),
            unitPrice: Number(data.unitPrice),
            unitsInStock: Number(data.unitsInStock),
            quantityPerUnit: data.quantityPerUnit,
            discontinued: data.discontinued
          }
        ],
        batchSize: 1
      });

      navigate('/products');
    } catch (err: any) {
      console.error(err);
      setServerError(err.response?.data?.detail || 'Failed to save product.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="app-container">
      <Navbar />
      <main className="main-content">
        <div style={{ maxWidth: '680px', margin: '0 auto' }}>
          <button
            onClick={() => navigate('/products')}
            className="btn btn-outline btn-sm"
            style={{ marginBottom: '1.5rem' }}
          >
            <ArrowLeft size={16} /> Back to Catalog
          </button>

          <div className="card">
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '1rem' }}>
              <Box size={24} color="var(--primary)" />
              <div>
                <h1 style={{ fontSize: '1.3rem', fontWeight: 700 }}>New Catalog Product</h1>
                <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                  Register commercial hardware or cloud subscription
                </p>
              </div>
            </div>

            {serverError && (
              <div style={{
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem',
                padding: '0.8rem',
                backgroundColor: 'rgba(239, 68, 68, 0.15)',
                borderRadius: 'var(--radius-sm)',
                color: '#f87171',
                fontSize: '0.85rem',
                marginBottom: '1.5rem'
              }}>
                <AlertCircle size={18} />
                <span>{serverError}</span>
              </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} style={{ display: 'flex', flexDirection: 'column', gap: '1.2rem' }}>
              {/* Product Name */}
              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', fontWeight: 500, marginBottom: '0.4rem' }}>
                  Product Name *
                </label>
                <input
                  type="text"
                  placeholder="e.g. Dell PowerEdge R650xs Server"
                  style={{ width: '100%' }}
                  {...register('productName', {
                    required: 'Product name is strictly required.',
                    minLength: { value: 3, message: 'Must contain at least 3 characters.' },
                    maxLength: { value: 100, message: 'Cannot exceed 100 characters.' }
                  })}
                />
                {errors.productName && (
                  <span style={{ color: 'var(--danger)', fontSize: '0.8rem', marginTop: '0.3rem', display: 'block' }}>
                    {errors.productName.message}
                  </span>
                )}
              </div>

              {/* Category & Unit Price in 2 Columns */}
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
                <div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.4rem' }}>
                    <label style={{ fontSize: '0.85rem', fontWeight: 500 }}>
                      Category *
                    </label>
                    <button
                      type="button"
                      onClick={() => setIsCategoryModalOpen(true)}
                      className="btn btn-outline btn-sm"
                      style={{ fontSize: '0.75rem', padding: '0.15rem 0.4rem', display: 'flex', alignItems: 'center', gap: '0.25rem' }}
                      title="Manage categories"
                    >
                      <Settings size={12} /> Manage
                    </button>
                  </div>
                  <select
                    style={{ width: '100%' }}
                    {...register('categoryId', { required: 'Please select a valid category.' })}
                  >
                    {categories.length === 0 ? (
                      <option value="">Loading categories...</option>
                    ) : (
                      categories.map((cat) => (
                        <option key={cat.categoryId} value={cat.categoryId}>
                          {cat.categoryName} {cat.description ? `(${cat.description})` : ''}
                        </option>
                      ))
                    )}
                  </select>
                  {errors.categoryId && (
                    <span style={{ color: 'var(--danger)', fontSize: '0.8rem', marginTop: '0.3rem', display: 'block' }}>
                      {errors.categoryId.message}
                    </span>
                  )}
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', fontWeight: 500, marginBottom: '0.4rem' }}>
                    Unit Price ($ USD) *
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    placeholder="0.00"
                    style={{ width: '100%' }}
                    {...register('unitPrice', {
                      required: 'Unit price is required.',
                      min: { value: 0.01, message: 'Price must be greater than zero.' }
                    })}
                  />
                  {errors.unitPrice && (
                    <span style={{ color: 'var(--danger)', fontSize: '0.8rem', marginTop: '0.3rem', display: 'block' }}>
                      {errors.unitPrice.message}
                    </span>
                  )}
                </div>
              </div>

              {/* Stock and Packaging */}
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', fontWeight: 500, marginBottom: '0.4rem' }}>
                    Units In Stock *
                  </label>
                  <input
                    type="number"
                    placeholder="0"
                    style={{ width: '100%' }}
                    {...register('unitsInStock', {
                      required: 'Inventory quantity is required.',
                      min: { value: 0, message: 'Stock quantity cannot be negative.' }
                    })}
                  />
                  {errors.unitsInStock && (
                    <span style={{ color: 'var(--danger)', fontSize: '0.8rem', marginTop: '0.3rem', display: 'block' }}>
                      {errors.unitsInStock.message}
                    </span>
                  )}
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', fontWeight: 500, marginBottom: '0.4rem' }}>
                    Packaging Specification
                  </label>
                  <input
                    type="text"
                    placeholder="e.g. Rack 1U chassis"
                    style={{ width: '100%' }}
                    {...register('quantityPerUnit', {
                      maxLength: { value: 50, message: 'Cannot exceed 50 characters.' }
                    })}
                  />
                </div>
              </div>

              {/* Discontinued checkbox */}
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', marginTop: '0.5rem' }}>
                <input
                  type="checkbox"
                  id="discontinued"
                  {...register('discontinued')}
                  style={{ width: '18px', height: '18px', cursor: 'pointer' }}
                />
                <label htmlFor="discontinued" style={{ fontSize: '0.9rem', cursor: 'pointer' }}>
                  Mark item as discontinued (hidden from active catalog default searches)
                </label>
              </div>

              {/* Actions */}
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '1rem', marginTop: '1.5rem' }}>
                <button
                  type="button"
                  onClick={() => navigate('/products')}
                  className="btn btn-secondary"
                  disabled={submitting}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={submitting}
                >
                  <Save size={18} />
                  {submitting ? 'Registering...' : 'Save Product'}
                </button>
              </div>
            </form>
          </div>
        </div>
      </main>

      <CategoryManagementModal
        isOpen={isCategoryModalOpen}
        onClose={() => setIsCategoryModalOpen(false)}
        onCategoriesChanged={loadCategories}
      />
    </div>
  );
};
