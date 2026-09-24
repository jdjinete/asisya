import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/**
 * Route protection guard for Administrator-only endpoints.
 * Redirects non-admin authenticated users back to the read-only catalog view.
 */
export const AdminGuard: React.FC = () => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  const isAdmin = user?.role?.toLowerCase() === 'admin' || user?.role?.toLowerCase() === 'administrator';
  if (!isAdmin) {
    return <Navigate to="/products" replace />;
  }

  return <Outlet />;
};
