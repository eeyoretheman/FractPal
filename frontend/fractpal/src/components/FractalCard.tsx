import React from 'react';
import { Link } from 'react-router-dom';
import './FractalCard.css';

export interface FractalCardFractal {
  id: string;
  name: string;
  username: string;
  userId: string;
  imageUrl?: string | null;
  likeCount: number;
  isLikedByCurrentUser: boolean;
  createdAt?: string | null;
  // postId is set when this card represents a post, so likes go to the right endpoint
  postId?: string;
}

interface FractalCardProps {
  fractal: FractalCardFractal;
  /** True when this fractal has an active post (set by Gallery from postApi data) */
  isPosted?: boolean;
  onLike?: (id: string) => void;
  onDelete?: (id: string) => void;
  onPublish?: (id: string) => void;
  onUnpublish?: (id: string) => void;
  showActions?: boolean;
}

const FractalCard: React.FC<FractalCardProps> = ({
  fractal,
  isPosted = false,
  onLike,
  onDelete,
  onPublish,
  onUnpublish,
  showActions = false,
}) => {
  const formatDate = (dateString?: string | null) => {
    if (!dateString) return '';
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric',
    });
  };

  const imageUrl = fractal.imageUrl ?? undefined;

  return (
    <div className="fractal-card card">
      <Link to={`/workbench/${fractal.id}`} className="fractal-preview">
        {imageUrl ? (
          <img src={imageUrl} alt={fractal.name} className="fractal-image" />
        ) : (
          <div className="fractal-placeholder">
            <span>No preview</span>
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

        {fractal.createdAt && (
          <div className="fractal-date text-muted">
            {formatDate(fractal.createdAt)}
          </div>
        )}

        <div className="fractal-actions">
          {onLike && (
            <button
              onClick={() => onLike(fractal.postId ?? fractal.id)}
              className={`like-button ${fractal.isLikedByCurrentUser ? 'liked' : ''}`}
            >
              <span className="heart-icon">{fractal.isLikedByCurrentUser ? '❤️' : '♡'}</span>
              <span>{fractal.likeCount}</span>
            </button>
          )}

          {showActions && (
            <div className="fractal-owner-actions">
              {onPublish && !isPosted && (
                <button onClick={() => onPublish(fractal.id)} className="secondary small">
                  Post
                </button>
              )}
              {onUnpublish && isPosted && (
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
