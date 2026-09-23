import React, { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '../api/authApi';

interface User {
  email: string;
  role: string;
}

interface AuthContextType {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('asisya_token'));
  const [user, setUser] = useState<User | null>(() => {
    const saved = localStorage.getItem('asisya_user');
    return saved ? JSON.parse(saved) : null;
  });

  useEffect(() => {
    if (token) {
      localStorage.setItem('asisya_token', token);
    } else {
      localStorage.removeItem('asisya_token');
    }
  }, [token]);

  useEffect(() => {
    if (user) {
      localStorage.setItem('asisya_user', JSON.stringify(user));
    } else {
      localStorage.removeItem('asisya_user');
    }
  }, [user]);

  const login = async (email: string, password: string) => {
    const data = await authApi.login(email, password);
    setToken(data.token);
    setUser({ email: data.email, role: data.role });
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('asisya_token');
    localStorage.removeItem('asisya_user');
  };

  return (
    <AuthContext.Provider
      value={{
        token,
        user,
        isAuthenticated: !!token,
        login,
        logout
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
