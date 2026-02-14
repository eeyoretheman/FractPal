import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import '../App.css'; // Ensure we use the global styles

export const NavBar = () => {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="nav-bar">
      <div className="logo">
        <div className="triangle-icon"></div>
        <Link to="/" style={{ textDecoration: 'none', color: 'inherit' }}>FractPal</Link>
      </div>
      <div className="nav-links">
        <Link to="/">Home</Link>
        <Link to="/workbench">Workbench</Link>
        <Link to="/gallery">Gallery</Link>
        <Link to="/profile">Profile</Link>
        <button onClick={handleLogout} className="logout-btn">Logout</button>
      </div>
    </nav>
  );
};
