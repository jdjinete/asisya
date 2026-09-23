import React, { useState } from 'react';
import { productApi } from '../api/productApi';
import { UploadCloud, X, AlertTriangle } from 'lucide-react';
import { toast } from 'sonner';

interface BulkUploadModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const BulkUploadModal: React.FC<BulkUploadModalProps> = ({ isOpen, onClose, onSuccess }) => {
  const [count, setCount] = useState<number>(5000);
  const [batchSize, setBatchSize] = useState<number>(1000);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleExecute = async () => {
    if (isSubmitting) return;

    setIsSubmitting(true);
    setError(null);

    try {
      const res = await productApi.bulkCreateProducts({
        generateRandomCount: count,
        batchSize
      });

      const batchId = res.batchId || 'N/A';
      toast.success(`Bulk ingestion enqueued successfully with ID: ${batchId}`);
      onSuccess();
      onClose();
    } catch (err: any) {
      console.error('Bulk ingestion submission failed:', err);
      const errorMessage =
        err.response?.data?.detail ||
        err.response?.data?.title ||
        err.message ||
        'Bulk ingestion request was rejected by server.';

      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={isSubmitting ? undefined : onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <UploadCloud size={24} color="var(--primary)" />
            <h2 style={{ fontSize: '1.25rem', fontWeight: 600 }}>Streaming Batch Ingestion</h2>
          </div>
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="btn btn-outline btn-sm"
            style={{ padding: '0.3rem', cursor: isSubmitting ? 'not-allowed' : 'pointer', opacity: isSubmitting ? 0.6 : 1 }}
          >
            <X size={18} />
          </button>
        </div>

        <div className="modal-body">
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '1.5rem' }}>
            Benchmark high-volume catalog ingestion using transactional streaming batches via RabbitMQ.
            Items are categorized into <strong>'SERVIDORES'</strong> and <strong>'CLOUD'</strong>.
          </p>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', fontWeight: 500, marginBottom: '0.4rem' }}>
                Total Products to Generate
              </label>
              <select
                value={count}
                onChange={(e) => setCount(Number(e.target.value))}
                style={{ width: '100%', cursor: isSubmitting ? 'not-allowed' : 'pointer' }}
                disabled={isSubmitting}
              >
                <option value={1000}>1,000 Products (Quick verification)</option>
                <option value={5000}>5,000 Products (~1 second)</option>
                <option value={10000}>10,000 Products (~2 seconds)</option>
                <option value={50000}>50,000 Products (~10 seconds)</option>
                <option value={100000}>100,000 Products (Full Stress Benchmark)</option>
              </select>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', fontWeight: 500, marginBottom: '0.4rem' }}>
                Batch Chunk Size (Items per DB roundtrip)
              </label>
              <input
                type="number"
                value={batchSize}
                onChange={(e) => setBatchSize(Number(e.target.value))}
                min={100}
                max={5000}
                style={{ width: '100%', cursor: isSubmitting ? 'not-allowed' : 'text' }}
                disabled={isSubmitting}
              />
              <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                Recommended: 1,000. Clears EF Core change tracker after each batch to prevent memory spikes.
              </span>
            </div>
          </div>

          {error && (
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              padding: '0.8rem',
              marginTop: '1rem',
              backgroundColor: 'rgba(239, 68, 68, 0.15)',
              borderRadius: 'var(--radius-sm)',
              color: '#f87171',
              fontSize: '0.85rem'
            }}>
              <AlertTriangle size={18} />
              <span>{error}</span>
            </div>
          )}
        </div>

        <div className="modal-footer">
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="btn btn-secondary"
            style={{ cursor: isSubmitting ? 'not-allowed' : 'pointer', opacity: isSubmitting ? 0.6 : 1 }}
          >
            Cancel
          </button>
          <button
            onClick={handleExecute}
            disabled={isSubmitting}
            className="btn btn-primary"
            style={{
              minWidth: '150px',
              cursor: isSubmitting ? 'not-allowed' : 'pointer',
              opacity: isSubmitting ? 0.8 : 1
            }}
          >
            {isSubmitting ? 'Iniciando...' : 'Start Bulk Import'}
          </button>
        </div>
      </div>
    </div>
  );
};
