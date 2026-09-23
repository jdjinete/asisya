import React from 'react';
import { AuditLogDto } from '../api/auditApi';
import { X, FileCode, Clock, User, Database, Hash } from 'lucide-react';

interface AuditLogDetailModalProps {
  log: AuditLogDto | null;
  onClose: () => void;
}

export const AuditLogDetailModal: React.FC<AuditLogDetailModalProps> = ({ log, onClose }) => {
  if (!log) return null;

  const formatJson = (jsonString: string | null): string => {
    if (!jsonString) return '';
    try {
      const parsed = JSON.parse(jsonString);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return jsonString;
    }
  };

  const getActionBadgeClass = (action: string) => {
    const act = action.toLowerCase();
    if (act === 'insert') return 'badge-insert';
    if (act === 'update') return 'badge-update';
    if (act === 'delete') return 'badge-delete';
    return '';
  };

  const formattedOldValues = formatJson(log.oldValues);
  const formattedNewValues = formatJson(log.newValues);

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-content-lg" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <FileCode size={22} color="var(--primary)" />
            <h2 style={{ fontSize: '1.2rem', fontWeight: 600 }}>Audit Record Inspection #{log.id}</h2>
          </div>
          <button onClick={onClose} className="btn btn-outline btn-sm" style={{ padding: '0.3rem' }} title="Close">
            <X size={18} />
          </button>
        </div>

        <div className="modal-body" style={{ maxHeight: '75vh', overflowY: 'auto' }}>
          {/* Metadata Card */}
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
            gap: '1rem',
            padding: '1rem',
            backgroundColor: 'rgba(15, 23, 42, 0.5)',
            border: '1px solid var(--border-color)',
            borderRadius: 'var(--radius-sm)',
            marginBottom: '1.5rem'
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <Database size={16} color="var(--text-secondary)" />
              <div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Table Affected</div>
                <strong style={{ fontSize: '0.9rem' }}>{log.tableName}</strong>
              </div>
            </div>

            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.2rem' }}>Action</div>
              <span className={`badge ${getActionBadgeClass(log.action)}`}>
                {log.action.toUpperCase()}
              </span>
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <User size={16} color="var(--text-secondary)" />
              <div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Operator</div>
                <strong style={{ fontSize: '0.9rem' }}>{log.userId || 'System / Anonymous'}</strong>
              </div>
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <Clock size={16} color="var(--text-secondary)" />
              <div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Timestamp (UTC)</div>
                <strong style={{ fontSize: '0.9rem' }}>{new Date(log.timestampUtc).toLocaleString()}</strong>
              </div>
            </div>

            {log.primaryKey && (
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', gridColumn: '1 / -1' }}>
                <Hash size={16} color="var(--text-secondary)" />
                <div style={{ fontSize: '0.85rem' }}>
                  <span style={{ color: 'var(--text-secondary)' }}>Primary Key: </span>
                  <code>{log.primaryKey}</code>
                </div>
              </div>
            )}
          </div>

          {/* JSON Diffs */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '1.25rem' }}>
            {/* Old Values */}
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                <h3 style={{ fontSize: '0.95rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
                  Old Values (Pre-Mutation)
                </h3>
                {formattedOldValues ? (
                  <span className="badge" style={{ backgroundColor: 'rgba(239, 68, 68, 0.1)', color: '#f87171' }}>Original</span>
                ) : (
                  <span className="badge" style={{ backgroundColor: 'rgba(148, 163, 184, 0.1)', color: '#94a3b8' }}>None</span>
                )}
              </div>
              {formattedOldValues ? (
                <pre className="json-viewer" style={{ color: '#fca5a5' }}>
                  <code>{formattedOldValues}</code>
                </pre>
              ) : (
                <div style={{
                  padding: '1.5rem',
                  textAlign: 'center',
                  backgroundColor: '#0b1120',
                  border: '1px dashed var(--border-color)',
                  borderRadius: 'var(--radius-sm)',
                  color: 'var(--text-muted)',
                  fontSize: '0.85rem'
                }}>
                  No previous state recorded (Entity Inserted).
                </div>
              )}
            </div>

            {/* New Values */}
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                <h3 style={{ fontSize: '0.95rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
                  New Values (Post-Mutation)
                </h3>
                {formattedNewValues ? (
                  <span className="badge" style={{ backgroundColor: 'rgba(16, 185, 129, 0.1)', color: '#34d399' }}>Modified</span>
                ) : (
                  <span className="badge" style={{ backgroundColor: 'rgba(148, 163, 184, 0.1)', color: '#94a3b8' }}>None</span>
                )}
              </div>
              {formattedNewValues ? (
                <pre className="json-viewer" style={{ color: '#86efac' }}>
                  <code>{formattedNewValues}</code>
                </pre>
              ) : (
                <div style={{
                  padding: '1.5rem',
                  textAlign: 'center',
                  backgroundColor: '#0b1120',
                  border: '1px dashed var(--border-color)',
                  borderRadius: 'var(--radius-sm)',
                  color: 'var(--text-muted)',
                  fontSize: '0.85rem'
                }}>
                  No new state recorded (Entity Deleted).
                </div>
              )}
            </div>
          </div>
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
