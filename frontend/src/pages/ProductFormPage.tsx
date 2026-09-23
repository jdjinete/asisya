import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import { productApi } from '../api/productApi';
import { Navbar } from '../components/Navbar';
import { Box, Save, ArrowLeft, AlertCircle, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

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
  const { id } = useParams<{ id: string }>();
  const isEditMode = Boolean(id);

  const [serverError, setServerError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [loadingInitial, setLoadingInitial] = useState(isEditMode);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<ProductFormData>({
    defaultValues: {
      productName: '',
      categoryId: 1, // Default to SERVIDORES
      unitPrice: 199.99,
      unitsInStock: 20,
      quantityPerUnit: '1 Unit standard',
      discontinued: false
    }
  });

  useEffect(() => {
    if (isEditMode && id) {
      setLoadingInitial(true);
      productApi.getProductById(Number(id))
        .then((product) => {
          reset({
            productName: product.productName,
            categoryId: product.category?.categoryId ?? 1,
            unitPrice: product.unitPrice ?? 0,
            unitsInStock: product.unitsInStock ?? 0,
            quantityPerUnit: product.quantityPerUnit || '',
            discontinued: product.discontinued
          });
        })
        .catch((err) => {
          console.error('Failed to load product details', err);
          setServerError('Could not load existing product details.');
          toast.error('Could not load existing product details.');
        })
        .finally(() => {
          setLoadingInitial(false);
        });
    }
  }, [id, isEditMode, reset]);

  const onSubmit = async (data: ProductFormData) => {
    setSubmitting(true);
    setServerError(null);

    try {
      if (isEditMode && id) {
        await productApi.updateProduct(Number(id), {
          productId: Number(id),
          productName: data.productName,
          categoryId: Number(data.categoryId),
          unitPrice: Number(data.unitPrice),
          unitsInStock: Number(data.unitsInStock),
          quantityPerUnit: data.quantityPerUnit,
          discontinued: data.discontinued
        });
        toast.success(`Product #${id} updated successfully!`);
      } else {
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
        toast.success('Product created successfully!');
      }

      navigate('/products');
    } catch (err: any) {
      console.error(err);
      const detail = err.response?.data?.detail || 'Failed to save product.';
      setServerError(detail);
      toast.error(detail);
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
                <h1 style={{ fontSize: '1.3rem', fontWeight: 700 }}>
                  {isEditMode ? `Edit Catalog Product #${id}` : 'New Catalog Product'}
                </h1>
                <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                  {isEditMode ? 'Modify catalog specifications, pricing, and status' : 'Register commercial hardware or cloud subscription'}
                </p>
              </div>
            </div>

            {loadingInitial ? (
              <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
                <Loader2 size={24} className="spin" style={{ margin: '0 auto 0.5rem' }} />
                Loading product data...
              </div>
            ) : (
              <>
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
                      <label style={{ display: 'block', fontSize: '0.85rem', fontWeight: 500, marginBottom: '0.4rem' }}>
                        Category *
                      </label>
                      <select
                        style={{ width: '100%' }}
                        {...register('categoryId', { required: 'Please select a valid category.' })}
                      >
                        <option value={1}>SERVIDORES (Physical Hardware)</option>
                        <option value={2}>CLOUD (Virtual Infrastructure)</option>
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
                      {submitting
                        ? (isEditMode ? 'Updating...' : 'Registering...')
                        : (isEditMode ? 'Update Product' : 'Save Product')}
                    </button>
                  </div>
                </form>
              </>
            )}
          </div>
        </div>
      </main>
    </div>
  );
};
