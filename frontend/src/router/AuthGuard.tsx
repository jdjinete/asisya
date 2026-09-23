import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/**
 * Route protection guard (equivalent to Angular CanActivate / AuthGuard).
 * Intercepts navigation to private routes and redirects unauthenticated users to the Login view.
 */
export const AuthGuard: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
};
