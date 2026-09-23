import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { CategoryManagementModal } from './CategoryManagementModal';
import { Server, LogOut, Package, Database, ClipboardList, Activity, Tags } from 'lucide-react';

export const Navbar: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="navbar">
      <div style={{ display: 'flex', alignItems: 'center', gap: '2rem' }}>
        <Link to="/products" className="navbar-brand" style={{ textDecoration: 'none' }}>
          <Server size={24} color="#3b82f6" />
          <span>ASISYA</span>
          <span className="brand-badge">CATALOG</span>
        </Link>

        <nav style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          <Link to="/products" className="btn btn-outline btn-sm">
            <Package size={16} /> Products
          </Link>
          <button
            type="button"
            onClick={() => setIsCategoryModalOpen(true)}
            className="btn btn-outline btn-sm"
          >
            <Tags size={16} /> Categories
          </button>
          <Link to="/audit-logs" className="btn btn-outline btn-sm">
            <ClipboardList size={16} /> Audit Logs
          </Link>
          <a
            href="http://localhost:5000"
            target="_blank"
            rel="noopener noreferrer"
            className="btn btn-outline btn-sm"
          >
            <Database size={16} /> Swagger API
          </a>
          <a
            href="http://localhost:5000/health-ui"
            target="_blank"
            rel="noopener noreferrer"
            className="btn btn-outline btn-sm"
            title="Inspect Liveness & Readiness Probes (PostgreSQL & RabbitMQ)"
            style={{ display: 'inline-flex', alignItems: 'center', gap: '0.4rem', borderColor: 'rgba(16, 185, 129, 0.4)' }}
          >
            <Activity size={16} color="#10b981" />
            <span>System Status</span>
            <span style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: '#10b981',
              display: 'inline-block',
              boxShadow: '0 0 6px #10b981'
            }} />
          </a>
        </nav>
      </div>

      {user && (
        <div className="navbar-user">
          <div className="user-tag">
            <span className="user-email">{user.email}</span>
            <span className="user-role">{user.role}</span>
          </div>
          <button onClick={handleLogout} className="btn btn-secondary btn-sm" title="Sign out">
            <LogOut size={16} /> Logout
          </button>
        </div>
      )}

      <CategoryManagementModal
        isOpen={isCategoryModalOpen}
        onClose={() => setIsCategoryModalOpen(false)}
      />
    </header>
  );
};
