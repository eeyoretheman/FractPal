import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Layout.css';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { user, logout } = useAuth();
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="layout">
      <nav className="navbar">
        <div className="navbar-container">
          <Link to="/" className="logo">
            <span className="logo-text">FractPal</span>
          </Link>

          <div className="nav-links">
            <Link to="/" className={`nav-link ${isActive('/') ? 'active' : ''}`}>
              Home
            </Link>
            <Link to="/gallery" className={`nav-link ${isActive('/gallery') ? 'active' : ''}`}>
              Gallery
            </Link>
            <Link to="/workbench" className={`nav-link ${isActive('/workbench') ? 'active' : ''}`}>
              Workbench
            </Link>
            <Link to="/profile" className={`nav-link ${isActive('/profile') ? 'active' : ''}`}>
              Profile
            </Link>
          </div>

          <div className="nav-user">
            <span className="username">{user?.username}</span>
            <button onClick={logout} className="logout-btn">
              Logout
            </button>
          </div>
        </div>
      </nav>

      <main className="main-content">
        {children}
      </main>
    </div>
  );
};

export default Layout;
