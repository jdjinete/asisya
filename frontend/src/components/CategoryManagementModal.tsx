import React, { useState, useEffect } from 'react';
import { categoryApi, CategoryDto } from '../api/categoryApi';
import { useAuth } from '../context/AuthContext';
import { Tags, X, Plus, Edit, Trash2, AlertCircle, CheckCircle, Package } from 'lucide-react';
import { toast } from 'sonner';

interface CategoryManagementModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCategoriesChanged?: () => void;
}

export const CategoryManagementModal: React.FC<CategoryManagementModalProps> = ({
  isOpen,
  onClose,
  onCategoriesChanged
}) => {
  const { user } = useAuth();
  const isAdmin = user?.role?.toLowerCase() === 'admin' || user?.role?.toLowerCase() === 'administrator';

  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [editingCategory, setEditingCategory] = useState<CategoryDto | null>(null);

  // Form states
  const [categoryName, setCategoryName] = useState('');
  const [description, setDescription] = useState('');
  const [formError, setFormError] = useState<string | null>(null);

  const fetchCategories = async () => {
    setLoading(true);
    try {
      const data = await categoryApi.getCategories();
      setCategories(data);
    } catch (err: any) {
      console.error('Failed to load categories', err);
      toast.error(err.response?.data?.detail || 'Failed to load categories.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isOpen) {
      fetchCategories();
      resetForm();
    }
  }, [isOpen]);

  const resetForm = () => {
    setEditingCategory(null);
    setCategoryName('');
    setDescription('');
    setFormError(null);
  };

  const handleStartEdit = (cat: CategoryDto) => {
    setEditingCategory(cat);
    setCategoryName(cat.categoryName);
    setDescription(cat.description || '');
    setFormError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!categoryName.trim()) {
      setFormError('Category name is required.');
      return;
    }

    setSubmitting(true);
    setFormError(null);

    try {
      if (editingCategory) {
        await categoryApi.updateCategory(editingCategory.categoryId, {
          categoryId: editingCategory.categoryId,
          categoryName: categoryName.trim(),
          description: description.trim() || undefined
        });
        toast.success(`Category "${categoryName}" updated successfully.`);
      } else {
        await categoryApi.createCategory({
          categoryName: categoryName.trim(),
          description: description.trim() || undefined
        });
        toast.success(`Category "${categoryName}" created successfully.`);
      }

      resetForm();
      await fetchCategories();
      onCategoriesChanged?.();
    } catch (err: any) {
      console.error('Failed to save category:', err);
      const msg = err.response?.data?.detail || err.response?.data?.title || 'Failed to save category.';
      setFormError(msg);
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (cat: CategoryDto) => {
    if (cat.productCount > 0) {
      toast.error(
        `Cannot delete "${cat.categoryName}" because it has ${cat.productCount} associated product(s). Remove or reassign them first.`
      );
      return;
    }

    const confirmDelete = window.confirm(
      `Are you sure you want to permanently delete category "${cat.categoryName}"?`
    );
    if (!confirmDelete) return;

    try {
      await categoryApi.deleteCategory(cat.categoryId);
      toast.success(`Category "${cat.categoryName}" deleted successfully.`);
      if (editingCategory?.categoryId === cat.categoryId) {
        resetForm();
      }
      await fetchCategories();
      onCategoriesChanged?.();
    } catch (err: any) {
      console.error('Failed to delete category:', err);
      const msg = err.response?.data?.detail || err.response?.data?.title || 'Failed to delete category.';
      toast.error(msg);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content"
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: '650px', width: '95%' }}
      >
        <div className="modal-header">
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <Tags size={24} color="var(--primary)" />
            <h2 style={{ fontSize: '1.25rem', fontWeight: 600 }}>Category Management</h2>
          </div>
          <button onClick={onClose} className="btn-icon" title="Close modal">
            <X size={20} />
          </button>
        </div>

        <div className="modal-body" style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          {/* Create / Edit Form Card - Admin Only */}
          {isAdmin && (
            <div className="card" style={{ padding: '1rem', backgroundColor: 'var(--surface-color)' }}>
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '0.75rem' }}>
                <h3 style={{ fontSize: '0.95rem', fontWeight: 600, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  {editingCategory ? <Edit size={16} color="var(--primary)" /> : <Plus size={16} color="var(--primary)" />}
                  {editingCategory ? `Edit Category: ${editingCategory.categoryName}` : 'Add New Category'}
                </h3>
                {editingCategory && (
                  <button
                    type="button"
                    onClick={resetForm}
                    className="btn btn-outline btn-sm"
                    style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem' }}
                  >
                    Cancel Edit
                  </button>
                )}
              </div>

              {formError && (
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.5rem',
                  padding: '0.6rem 0.8rem',
                  backgroundColor: 'rgba(239, 68, 68, 0.15)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#f87171',
                  fontSize: '0.82rem',
                  marginBottom: '0.75rem'
                }}>
                  <AlertCircle size={16} />
                  <span>{formError}</span>
                </div>
              )}

              <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, marginBottom: '0.25rem' }}>
                    Category Name *
                  </label>
                  <input
                    type="text"
                    placeholder="e.g. STORAGE, NETWORKING"
                    value={categoryName}
                    onChange={(e) => setCategoryName(e.target.value)}
                    maxLength={50}
                    style={{ width: '100%' }}
                  />
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, marginBottom: '0.25rem' }}>
                    Description (Optional)
                  </label>
                  <input
                    type="text"
                    placeholder="Brief description of the catalog segment..."
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    maxLength={500}
                    style={{ width: '100%' }}
                  />
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '0.5rem' }}>
                  <button
                    type="submit"
                    className="btn btn-primary btn-sm"
                    disabled={submitting}
                  >
                    {editingCategory ? <CheckCircle size={16} /> : <Plus size={16} />}
                    {submitting ? 'Saving...' : editingCategory ? 'Save Changes' : 'Create Category'}
                  </button>
                </div>
              </form>
            </div>
          )}

          {/* Existing Categories Table */}
          <div>
            <h3 style={{ fontSize: '0.95rem', fontWeight: 600, marginBottom: '0.75rem' }}>
              Existing Catalog Categories ({categories.length})
            </h3>

            {loading ? (
              <div style={{ textAlign: 'center', padding: '1.5rem', color: 'var(--text-secondary)' }}>
                Loading categories...
              </div>
            ) : categories.length === 0 ? (
              <div style={{ textAlign: 'center', padding: '1.5rem', color: 'var(--text-secondary)' }}>
                No categories found.
              </div>
            ) : (
              <div className="table-responsive" style={{ maxHeight: '280px', overflowY: 'auto' }}>
                <table className="data-table" style={{ width: '100%', fontSize: '0.85rem' }}>
                  <thead>
                    <tr>
                      <th style={{ width: isAdmin ? '30%' : '35%' }}>Name</th>
                      <th style={{ width: isAdmin ? '40%' : '50%' }}>Description</th>
                      <th style={{ width: '15%', textAlign: 'center' }}>Products</th>
                      {isAdmin && <th style={{ width: '15%', textAlign: 'right' }}>Actions</th>}
                    </tr>
                  </thead>
                  <tbody>
                    {categories.map((cat) => (
                      <tr key={cat.categoryId}>
                        <td style={{ fontWeight: 600, color: 'var(--text-primary)' }}>
                          {cat.categoryName}
                        </td>
                        <td style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>
                          {cat.description || '—'}
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          <span
                            className="badge"
                            style={{
                              display: 'inline-flex',
                              alignItems: 'center',
                              gap: '0.25rem',
                              backgroundColor: cat.productCount > 0 ? 'rgba(59, 130, 246, 0.15)' : 'rgba(107, 114, 128, 0.15)',
                              color: cat.productCount > 0 ? 'var(--primary)' : 'var(--text-secondary)',
                              padding: '0.2rem 0.5rem',
                              borderRadius: '12px',
                              fontSize: '0.75rem'
                            }}
                          >
                            <Package size={12} />
                            {cat.productCount}
                          </span>
                        </td>
                        {isAdmin && (
                          <td style={{ textAlign: 'right' }}>
                            <div style={{ display: 'inline-flex', gap: '0.4rem', justifyContent: 'flex-end' }}>
                              <button
                                onClick={() => handleStartEdit(cat)}
                                className="btn-icon"
                                title="Edit Category"
                              >
                                <Edit size={16} color="var(--primary)" />
                              </button>
                              <button
                                onClick={() => handleDelete(cat)}
                                className="btn-icon"
                                title={
                                  cat.productCount > 0
                                    ? 'Cannot delete: category has assigned products'
                                    : 'Delete Category'
                                }
                                disabled={cat.productCount > 0}
                              >
                                <Trash2 size={16} color="var(--danger)" />
                              </button>
                            </div>
                          </td>
                        )}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>

        <div className="modal-footer" style={{ borderTop: '1px solid var(--border-color)', paddingTop: '0.75rem' }}>
          <button onClick={onClose} className="btn btn-secondary btn-sm">
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
