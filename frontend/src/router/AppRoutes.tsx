import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from '../pages/LoginPage';
import { ProductsPage } from '../pages/ProductsPage';
import { ProductFormPage } from '../pages/ProductFormPage';
import { AuditLogsPage } from '../pages/AuditLogsPage';
import { AuthGuard } from './AuthGuard';
import { AdminGuard } from './AdminGuard';

/**
 * Main application routing module (simulates Angular AppRoutingModule in React).
 * Defines public authentication endpoints and private guarded application features.
 */
export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Public Routes */}
      <Route path="/login" element={<LoginPage />} />

      {/* Private Guarded Routes (Protected by AuthGuard) */}
      <Route element={<AuthGuard />}>
        <Route path="/products" element={<ProductsPage />} />
        <Route path="/audit-logs" element={<AuditLogsPage />} />
        <Route path="/" element={<Navigate to="/products" replace />} />

        {/* Administrator-only Routes (Protected by AdminGuard) */}
        <Route element={<AdminGuard />}>
          <Route path="/products/new" element={<ProductFormPage />} />
          <Route path="/products/edit/:id" element={<ProductFormPage />} />
        </Route>
      </Route>

      {/* Fallback Catch-all Route */}
      <Route path="*" element={<Navigate to="/products" replace />} />
    </Routes>
  );
};
