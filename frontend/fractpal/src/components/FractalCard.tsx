import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { fractalApi } from '../services/api';
import './FractalCard.css';

interface FractalCardProps {
  fractal: {
    id: string;
    name: string;
    username: string;
    userId: string;
    imageUrl?: string;
    likeCount: number;
    isLikedByCurrentUser: boolean;
    publishedAt?: string;
  };
  onLike?: (id: string) => void;
  onDelete?: (id: string) => void;
  onPublish?: (id: string) => void;
  onUnpublish?: (id: string) => void;
  showActions?: boolean;
  showFork?: boolean;
}

const FractalCard: React.FC<FractalCardProps> = ({
  fractal,
  onLike,
  onDelete,
  onPublish,
  onUnpublish,
  showActions = false,
  showFork = false,
}) => {
  const navigate = useNavigate();
  const [forking, setForking] = useState(false);

  const formatDate = (dateString?: string) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  };

  const handleFork = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    try {
      setForking(true);
      const forked = await fractalApi.forkFractal(fractal.id);
      navigate(`/workbench/${forked.id}`);
    } catch (error) {
      console.error('Failed to fork:', error);
      alert('Failed to fork fractal');
    } finally {
      setForking(false);
    }
  };

  return (
    <div className="fractal-card card">
      <Link to={`/workbench/${fractal.id}`} className="fractal-preview">
        {fractal.imageUrl ? (
          <img src={fractal.imageUrl} alt={fractal.name} className="fractal-image" />
        ) : (
          <div className="fractal-placeholder shimmer">
            <span className="placeholder-icon">✨</span>
          </div>
        )}
      </Link>

      <div className="fractal-info">
        <div className="fractal-header">
          <Link to={`/workbench/${fractal.id}`} className="fractal-name">
            {fractal.name}
          </Link>
          <Link to={`/profile/${fractal.userId}`} className="fractal-author">
            by {fractal.username}
          </Link>
        </div>

        {fractal.publishedAt && (
          <div className="fractal-date text-muted">
            {formatDate(fractal.publishedAt)}
          </div>
        )}

        <div className="fractal-actions">
          {onLike && (
            <button
              onClick={() => onLike(fractal.id)}
              className={`like-button ${fractal.isLikedByCurrentUser ? 'liked' : ''}`}
            >
              <span className="heart-icon">{fractal.isLikedByCurrentUser ? '❤️' : '♡'}</span>
              <span>{fractal.likeCount}</span>
            </button>
          )}

          {showFork && (
            <button
              onClick={handleFork}
              disabled={forking}
              className="fork-button"
              title="Fork this fractal"
            >
              {forking ? <span className="loading small"></span> : '🔀'}
              <span>{forking ? 'Forking...' : 'Fork'}</span>
            </button>
          )}

          {showActions && (
            <div className="fractal-owner-actions">
              {onPublish && !fractal.publishedAt && (
                <button onClick={() => onPublish(fractal.id)} className="secondary small">
                  Post
                </button>
              )}
              {onUnpublish && fractal.publishedAt && (
                <button onClick={() => onUnpublish(fractal.id)} className="secondary small">
                  Unpost
                </button>
              )}
              <Link to={`/workbench/${fractal.id}`} className="button secondary small">
                Edit
              </Link>
              {onDelete && (
                <button onClick={() => onDelete(fractal.id)} className="danger small">
                  Delete
                </button>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default FractalCard;
