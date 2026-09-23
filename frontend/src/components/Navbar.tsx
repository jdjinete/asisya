import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Server, LogOut, Package, Database, ClipboardList } from 'lucide-react';

export const Navbar: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

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

        <nav style={{ display: 'flex', gap: '1rem' }}>
          <Link to="/products" className="btn btn-outline btn-sm">
            <Package size={16} /> Products
          </Link>
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
    </header>
  );
};
