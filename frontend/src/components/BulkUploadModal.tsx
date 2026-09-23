import React, { useState } from 'react';
import { productApi, BulkCreateProductsResult } from '../api/productApi';
import { UploadCloud, CheckCircle2, Clock, X, AlertTriangle } from 'lucide-react';

interface BulkUploadModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const BulkUploadModal: React.FC<BulkUploadModalProps> = ({ isOpen, onClose, onSuccess }) => {
  const [count, setCount] = useState<number>(5000);
  const [batchSize, setBatchSize] = useState<number>(1000);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<BulkCreateProductsResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleExecute = async () => {
    setLoading(true);
    setError(null);
    setResult(null);

    try {
      const res = await productApi.bulkCreateProducts({
        generateRandomCount: count,
        batchSize
      });
      setResult(res);
      onSuccess();
    } catch (err: any) {
      console.error(err);
      setError(err.response?.data?.detail || 'Bulk ingestion failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <UploadCloud size={24} color="var(--primary)" />
            <h2 style={{ fontSize: '1.25rem', fontWeight: 600 }}>Streaming Batch Ingestion</h2>
          </div>
          <button onClick={onClose} className="btn btn-outline btn-sm" style={{ padding: '0.3rem' }}>
            <X size={18} />
          </button>
        </div>

        <div className="modal-body">
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '1.5rem' }}>
            Benchmark high-volume catalog ingestion using transactional streaming batches.
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
                style={{ width: '100%' }}
                disabled={loading}
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
                style={{ width: '100%' }}
                disabled={loading}
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

          {result && (
            <div style={{
              marginTop: '1.5rem',
              padding: '1.2rem',
              borderRadius: 'var(--radius-sm)',
              backgroundColor: 'rgba(16, 185, 129, 0.1)',
              border: '1px solid rgba(16, 185, 129, 0.3)'
            }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--success)', fontWeight: 600, marginBottom: '0.5rem' }}>
                <CheckCircle2 size={20} />
                <span>Ingestion Successful!</span>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.75rem', fontSize: '0.85rem', marginTop: '0.8rem' }}>
                <div>
                  <span style={{ color: 'var(--text-muted)', display: 'block' }}>Processed</span>
                  <strong style={{ fontSize: '1rem' }}>{result.totalProcessed.toLocaleString()}</strong>
                </div>
                <div>
                  <span style={{ color: 'var(--text-muted)', display: 'block' }}>Committed</span>
                  <strong style={{ fontSize: '1rem', color: 'var(--success)' }}>{result.successfulImports.toLocaleString()}</strong>
                </div>
                <div>
                  <span style={{ color: 'var(--text-muted)', display: 'block' }}>Execution Time</span>
                  <strong style={{ fontSize: '1rem', display: 'flex', alignItems: 'center', gap: '0.2rem' }}>
                    <Clock size={14} /> {result.elapsedMilliseconds} ms
                  </strong>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="modal-footer">
          <button onClick={onClose} disabled={loading} className="btn btn-secondary">
            Cancel
          </button>
          <button onClick={handleExecute} disabled={loading} className="btn btn-primary">
            {loading ? 'Processing Batch...' : 'Start Bulk Import'}
          </button>
        </div>
      </div>
    </div>
  );
};
