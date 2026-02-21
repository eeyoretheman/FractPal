import React, { useState, useEffect } from 'react';
import { fractalApi } from '../services/api';
import FractalCard from '../components/FractalCard';
import './Home.css';

interface Fractal {
  id: string;
  name: string;
  username: string;
  userId: string;
  createdAt: string;
  publishedAt: string;
  imageUrl: string;
  likeCount: number;
  isLikedByCurrentUser: boolean;
}

const Home: React.FC = () => {
  const [fractals, setFractals] = useState<Fractal[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);

  useEffect(() => {
    loadFeed();
  }, [page]);

  const loadFeed = async () => {
    try {
      setLoading(true);
      const response = await fractalApi.getFeed(page, 20);

      if (page === 1) {
        setFractals(response.fractals);
      } else {
        setFractals(prev => [...prev, ...response.fractals]);
      }

      setHasMore(response.fractals.length === 20);
    } catch (error) {
      console.error('Failed to load feed:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleLike = async (id: string) => {
    try {
      const result = await fractalApi.toggleLike(id);
      setFractals(prev =>
        prev.map(f =>
          f.id === id
            ? {
                ...f,
                isLikedByCurrentUser: result.isLiked,
                likeCount: result.isLiked ? f.likeCount + 1 : f.likeCount - 1,
              }
            : f
        )
      );
    } catch (error) {
      console.error('Failed to toggle like:', error);
    }
  };

  const loadMore = () => {
    if (!loading && hasMore) {
      setPage(prev => prev + 1);
    }
  };

  return (
    <div className="home-page">
      <header className="page-header">
        <h1>Home</h1>
        <p className="text-muted">Discover fractals from the community</p>
      </header>

      {loading && page === 1 ? (
        <div className="loading-container">
          <div className="loading"></div>
        </div>
      ) : fractals.length === 0 ? (
        <div className="empty-state">
          <p>No fractals yet. Be the first to share!</p>
        </div>
      ) : (
        <>
          <div className="fractals-grid">
            {fractals.map((fractal, index) => (
              <div key={fractal.id} className="fade-in" style={{ animationDelay: `${index * 0.05}s` }}>
                <FractalCard fractal={fractal} onLike={handleLike} showFork />
              </div>
            ))}
          </div>

          {hasMore && (
            <div className="load-more-container">
              <button onClick={loadMore} disabled={loading} className="secondary">
                {loading ? <span className="loading"></span> : 'Load More'}
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default Home;
