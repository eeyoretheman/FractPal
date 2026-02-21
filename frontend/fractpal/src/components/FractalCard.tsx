import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { fractalApi } from '../services/api';
import './FractalCard.css';

export interface FractalCardFractal {
  id: string;
  name: string;
  username: string;
  userId: string;
  thumbnail?: string | null;
  likeCount: number;
  forkCount?: number;
  viewCount?: number;
  isLikedByCurrentUser: boolean;
  createdAt?: string | null;
  publishedAt?: string | null;
  postId?: string;
}

interface FractalCardProps {
  fractal: FractalCardFractal;
  isPosted?: boolean;
  onLike?: (id: string) => void;
  onDelete?: (id: string) => void;
  onPublish?: (id: string) => void;
  onUnpublish?: (id: string) => void;
  showActions?: boolean;
  showFork?: boolean;
}

const FractalCard: React.FC<FractalCardProps> = ({
  fractal,
  isPosted = false,
  onLike,
  onDelete,
  onPublish,
  onUnpublish,
  showActions = false,
  showFork = false,
}) => {
  const navigate = useNavigate();
  const [imageLoaded, setImageLoaded] = useState(false);
  const [imageError, setImageError] = useState(false);
  const [liking, setLiking] = useState(false);
  const [forking, setForking] = useState(false);

  const formatDate = (dateString?: string | null) => {
    if (!dateString) return '';
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
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

  const handleLike = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (onLike) {
      setLiking(true);
      try {
        await Promise.resolve(onLike(fractal.postId ?? fractal.id));
      } finally {
        setLiking(false);
      }
    }
  };

  const hasThumbnail = !!fractal.thumbnail && !imageError;

  return (
    <div className="fractal-card card">
      <Link
        to={`/workbench/${fractal.id}`}
        className="fractal-preview"
        aria-labelledby={`fractal-name-${fractal.id}`}
      >
        <div className="fractal-preview-container">
          {hasThumbnail ? (
            <>
              {!imageLoaded && <div className="skeleton-loader" aria-hidden="true" />}
              <img
                src={fractal.thumbnail!}
                alt={`Preview of ${fractal.name}`}
                className={`fractal-image ${imageLoaded ? 'visible' : 'hidden'}`}
                onLoad={() => setImageLoaded(true)}
                onError={() => setImageError(true)}
                style={{ objectFit: 'contain', width: '100%', height: '100%' }}
              />
            </>
          ) : (
            <div className="fractal-placeholder" aria-label="No preview available">
              <span className="placeholder-icon">✨</span>
            </div>
          )}
        </div>
      </Link>

      <div className="fractal-info">
        <div className="fractal-header">
          <Link
            to={`/workbench/${fractal.id}`}
            className="fractal-name"
            id={`fractal-name-${fractal.id}`}
          >
            {fractal.name}
          </Link>
          <Link
            to={`/profile/${fractal.userId}`}
            className="fractal-author"
            title={`View ${fractal.username}'s profile`}
          >
            @{fractal.username}
          </Link>
          {fractal.publishedAt && (
            <div className="fractal-date">{formatDate(fractal.publishedAt)}</div>
          )}
        </div>

        <div className="fractal-metrics">
          {fractal.likeCount > 0 && (
            <span
              className="metric"
              title={`${fractal.likeCount} like${fractal.likeCount !== 1 ? 's' : ''}`}
            >
              <span role="img" aria-label="likes">
                ❤️
              </span>{' '}
              {fractal.likeCount}
            </span>
          )}
          {fractal.forkCount && fractal.forkCount > 0 && (
            <span
              className="metric"
              title={`${fractal.forkCount} fork${fractal.forkCount !== 1 ? 's' : ''}`}
            >
              <span role="img" aria-label="forks">
                🔀
              </span>{' '}
              {fractal.forkCount}
            </span>
          )}
          {fractal.viewCount && fractal.viewCount > 0 && (
            <span
              className="metric"
              title={`${fractal.viewCount} view${fractal.viewCount !== 1 ? 's' : ''}`}
            >
              <span role="img" aria-label="views">
                👁️
              </span>{' '}
              {fractal.viewCount}
            </span>
          )}
        </div>

        <div className="fractal-actions">
          <div className="fractal-action-buttons">
            {onLike && (
              <button
                onClick={handleLike}
                disabled={liking}
                className={`action-button like-button ${
                  fractal.isLikedByCurrentUser ? 'liked' : ''
                }`}
                title={fractal.isLikedByCurrentUser ? 'Unlike' : 'Like'}
                aria-label={`Like this fractal. Currently ${
                  fractal.isLikedByCurrentUser ? 'liked' : 'not liked'
                }.`}
                aria-pressed={fractal.isLikedByCurrentUser}
              >
                <span className="icon">{fractal.isLikedByCurrentUser ? '❤️' : '♡'}</span>
              </button>
            )}

            {showFork && (
              <button
                onClick={handleFork}
                disabled={forking}
                className="action-button fork-button"
                title="Fork this fractal"
                aria-label="Fork this fractal"
              >
                {forking ? <span className="loading"></span> : <span className="icon">🔀</span>}
              </button>
            )}
          </div>

          {showActions && (
            <div className="fractal-owner-actions">
              {!fractal.publishedAt && onPublish && (
                <button onClick={() => onPublish(fractal.id)} className="secondary small">
                  Publish
                </button>
              )}
              {isPosted && onUnpublish && (
                <button onClick={() => onUnpublish(fractal.id)} className="secondary small">
                  Unpost
                </button>
              )}
              <Link to={`/workbench/${fractal.id}`} className="button secondary small">
                ✏️ Edit
              </Link>
              {onDelete && (
                <button onClick={() => onDelete(fractal.id)} className="danger small">
                  🗑️ Delete
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
