import React, { useEffect, useState } from 'react';
import { Navbar } from '../components/Navbar';
import { auditApi, AuditLogDto } from '../api/auditApi';
import { AuditLogDetailModal } from '../components/AuditLogDetailModal';
import {
  ShieldAlert,
  RotateCw,
  Eye,
  ChevronLeft,
  ChevronRight,
  Filter,
  Calendar,
  User,
  Database,
  Tag
} from 'lucide-react';

export const AuditLogsPage: React.FC = () => {
  const [logs, setLogs] = useState<AuditLogDto[]>([]);
  const [pageIndex, setPageIndex] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [totalItems, setTotalItems] = useState<number>(0);
  const [totalPages, setTotalPages] = useState<number>(1);
  const [tableName, setTableName] = useState<string>('');
  const [action, setAction] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [selectedLog, setSelectedLog] = useState<AuditLogDto | null>(null);

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const data = await auditApi.getAuditLogs({
        pageIndex,
        pageSize,
        tableName: tableName.trim() || undefined,
        action: action.trim() || undefined
      });

      setLogs(data.items);
      setPageIndex(data.pageIndex);
      setPageSize(data.pageSize);
      setTotalItems(data.totalItems);
      setTotalPages(data.totalPages);
    } catch (err) {
      console.error('Failed to fetch audit logs', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLogs();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageIndex, pageSize, action]);

  const handleFilterSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    fetchLogs();
  };

  const getActionBadgeClass = (act: string) => {
    const lower = act.toLowerCase();
    if (lower === 'insert') return 'badge-insert';
    if (lower === 'update') return 'badge-update';
    if (lower === 'delete') return 'badge-delete';
    return '';
  };

  return (
    <div className="app-container">
      <Navbar />

      <main className="main-content">
        {/* Top Header */}
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem',
          marginBottom: '1.5rem'
        }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
              <ShieldAlert size={28} color="#3b82f6" />
              <h1 style={{ fontSize: '1.75rem', fontWeight: 700 }}>Security & Audit Trail</h1>
            </div>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
              Showing {totalItems.toLocaleString()} mutation events recorded via EF Core ChangeTracker Interceptors
            </p>
          </div>

          <div>
            <button
              onClick={() => fetchLogs()}
              className="btn btn-secondary"
              disabled={loading}
              title="Refresh audit events"
            >
              <RotateCw size={16} className={loading ? 'spin' : ''} />
              Refresh
            </button>
          </div>
        </div>

        {/* Filters Card */}
        <div className="card" style={{ marginBottom: '1.5rem', padding: '1rem 1.25rem' }}>
          <form onSubmit={handleFilterSubmit} style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
            gap: '1rem',
            alignItems: 'end'
          }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, marginBottom: '0.3rem', color: 'var(--text-secondary)' }}>
                Filter by Table Name
              </label>
              <div style={{ position: 'relative' }}>
                <input
                  type="text"
                  placeholder="e.g. Products, Categories..."
                  value={tableName}
                  onChange={(e) => setTableName(e.target.value)}
                  style={{ width: '100%', paddingLeft: '2.2rem' }}
                />
                <Database size={16} color="var(--text-muted)" style={{ position: 'absolute', left: '0.75rem', top: '50%', transform: 'translateY(-50%)' }} />
              </div>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, marginBottom: '0.3rem', color: 'var(--text-secondary)' }}>
                Filter by Action Type
              </label>
              <div style={{ position: 'relative' }}>
                <select
                  value={action}
                  onChange={(e) => {
                    setAction(e.target.value);
                    setPageIndex(1);
                  }}
                  style={{ width: '100%', paddingLeft: '2.2rem' }}
                >
                  <option value="">All Actions</option>
                  <option value="Insert">INSERT</option>
                  <option value="Update">UPDATE</option>
                  <option value="Delete">DELETE</option>
                </select>
                <Tag size={16} color="var(--text-muted)" style={{ position: 'absolute', left: '0.75rem', top: '50%', transform: 'translateY(-50%)' }} />
              </div>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 500, marginBottom: '0.3rem', color: 'var(--text-secondary)' }}>
                Page Size
              </label>
              <select
                value={pageSize}
                onChange={(e) => {
                  setPageSize(Number(e.target.value));
                  setPageIndex(1);
                }}
                style={{ width: '100%' }}
              >
                <option value={10}>10 items</option>
                <option value={25}>25 items</option>
                <option value={50}>50 items</option>
              </select>
            </div>

            <div>
              <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
                <Filter size={16} /> Filter Logs
              </button>
            </div>
          </form>
        </div>

        {/* Data Table */}
        <div className="table-responsive card" style={{ padding: 0 }}>
          <table className="data-table">
            <thead>
              <tr>
                <th>Log ID</th>
                <th><div style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}><Calendar size={14} /> Timestamp (UTC)</div></th>
                <th><div style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}><User size={14} /> Responsible User</div></th>
                <th>Action</th>
                <th><div style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}><Database size={14} /> Affected Table</div></th>
                <th>Primary Key</th>
                <th style={{ textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
                    Loading audit trail events from server...
                  </td>
                </tr>
              ) : logs.length === 0 ? (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
                    No audit records match the selected filters.
                  </td>
                </tr>
              ) : (
                logs.map((log) => (
                  <tr key={log.id}>
                    <td>
                      <code style={{ color: 'var(--primary)', fontWeight: 600 }}>#{log.id}</code>
                    </td>
                    <td style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
                      {new Date(log.timestampUtc).toLocaleString()}
                    </td>
                    <td>
                      <span style={{ fontWeight: 500 }}>{log.userId || 'System'}</span>
                    </td>
                    <td>
                      <span className={`badge ${getActionBadgeClass(log.action)}`}>
                        {log.action.toUpperCase()}
                      </span>
                    </td>
                    <td>
                      <strong style={{ color: '#e2e8f0' }}>{log.tableName}</strong>
                    </td>
                    <td>
                      <code style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>{log.primaryKey || '—'}</code>
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <button
                        onClick={() => setSelectedLog(log)}
                        className="btn btn-outline btn-sm"
                        title="View JSON modification details"
                      >
                        <Eye size={15} /> View Details
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Bar */}
        {totalPages > 1 && (
          <div style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginTop: '1.5rem',
            flexWrap: 'wrap',
            gap: '1rem'
          }}>
            <span style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
              Page <strong>{pageIndex}</strong> of <strong>{totalPages}</strong> ({totalItems} total events)
            </span>

            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button
                onClick={() => setPageIndex((prev) => Math.max(prev - 1, 1))}
                disabled={pageIndex <= 1}
                className="btn btn-secondary btn-sm"
              >
                <ChevronLeft size={16} /> Previous
              </button>
              <button
                onClick={() => setPageIndex((prev) => Math.min(prev + 1, totalPages))}
                disabled={pageIndex >= totalPages}
                className="btn btn-secondary btn-sm"
              >
                Next <ChevronRight size={16} />
              </button>
            </div>
          </div>
        )}
      </main>

      {/* Audit Log JSON Inspection Modal */}
      <AuditLogDetailModal
        log={selectedLog}
        onClose={() => setSelectedLog(null)}
      />
    </div>
  );
};
