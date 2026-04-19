import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import type { PostDto } from '../services/types';
import './PostCard.css';

interface PostCardProps {
  post: PostDto;
  onLike?: (id: string) => void;
}

const PostCard: React.FC<PostCardProps> = ({ post, onLike }) => {
  const [imageLoaded, setImageLoaded] = useState(false);
  const [imageError, setImageError] = useState(false);
  const [liking, setLiking] = useState(false);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const handleLike = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (onLike) {
      setLiking(true);
      try {
        await Promise.resolve(onLike(post.id));
      } finally {
        setLiking(false);
      }
    }
  };

  const hasThumbnail = !!post.thumbnail && !imageError;

  return (
    <article className="post-card card">
      {/* Post Preview Link */}
      <Link to={`/post/${post.id}`} className="post-preview">
        <div className="post-preview-container">
          {hasThumbnail ? (
            <>
              {!imageLoaded && <div className="skeleton-loader" />}
              <img
                src={post.thumbnail || ''}
                alt={`Preview of ${post.name}`}
                onLoad={() => setImageLoaded(true)}
                onError={() => setImageError(true)}
                className={imageLoaded ? 'loaded' : ''}
              />
            </>
          ) : (
            <div className="no-preview">
              <span>🌿</span>
              <span>Fractal Preview</span>
            </div>
          )}
        </div>
      </Link>

      {/* Post Info */}
      <div className="post-info">
        <Link to={`/post/${post.id}`} className="post-title-link">
          <h3 id={`post-name-${post.id}`} className="post-title">
            {post.name}
          </h3>
        </Link>

        {post.description && (
          <p className="post-description">{post.description}</p>
        )}

        <div className="post-meta">
          <Link to={`/profile/${post.authorId}`} className="post-author">
            {post.username}
          </Link>
          <time dateTime={post.createdAt} className="post-date">
            {formatDate(post.createdAt)}
          </time>
        </div>

        {/* Engagement Footer */}
        <div className="post-footer">
          <button
            className={`engagement-btn like-btn ${post.isLikedByCurrentUser ? 'liked' : ''}`}
            onClick={handleLike}
            disabled={liking}
            aria-label={post.isLikedByCurrentUser ? 'Unlike' : 'Like'}
          >
            <span className="heart-icon">♥</span>
            <span className="count">{post.likeCount}</span>
          </button>

          <Link to={`/post/${post.id}`} className="engagement-btn comments-btn">
            <span className="comment-icon">💬</span>
            <span className="label">View Post</span>
          </Link>
        </div>
      </div>
    </article>
  );
};

export default PostCard;
