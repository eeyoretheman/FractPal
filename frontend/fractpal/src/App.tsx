import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import Login from './pages/Login';
import Register from './pages/Register';
import Home from './pages/Home';
import Gallery from './pages/Gallery';
import Workbench from './pages/Workbench';
import Profile from './pages/Profile';
import PostDetail from './pages/PostDetail';
import Layout from './components/Layout';
import './App.css';

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" />;
};

const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/" element={<ProtectedRoute><Layout><Home /></Layout></ProtectedRoute>} />
          <Route path="/post/:postId" element={<ProtectedRoute><Layout><PostDetail /></Layout></ProtectedRoute>} />
          <Route path="/gallery" element={<ProtectedRoute><Layout><Gallery /></Layout></ProtectedRoute>} />
          <Route path="/workbench" element={<ProtectedRoute><Layout><Workbench /></Layout></ProtectedRoute>} />
          <Route path="/workbench/:id" element={<ProtectedRoute><Layout><Workbench /></Layout></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><Layout><Profile /></Layout></ProtectedRoute>} />
          <Route path="/profile/:userId" element={<ProtectedRoute><Layout><Profile /></Layout></ProtectedRoute>} />
        </Routes>
      </Router>
    </AuthProvider>
  );
};

export default App;
