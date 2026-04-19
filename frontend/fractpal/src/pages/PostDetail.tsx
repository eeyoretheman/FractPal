import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { postApi, fractalApi } from '../services/api';
import type { PostDto, FractalDto } from '../services/types';
import Comments from '../components/Comments';
import './PostDetail.css';

const PostDetail: React.FC = () => {
  const { postId } = useParams<{ postId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();

  const [post, setPost] = useState<PostDto | null>(null);
  const [fractal, setFractal] = useState<FractalDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (postId) {
      loadPost();
    }
  }, [postId]);

  const loadPost = async () => {
    if (!postId) return;
    try {
      setLoading(true);
      const postData = await postApi.getPostById(postId);
      setPost(postData);

      // Load the fractal details for additional info (optional)
      try {
        const fractalData = await fractalApi.getFractalById(postData.fractalId);
        setFractal(fractalData);
      } catch (err) {
        console.error('Failed to load fractal details:', err);
        // This is optional, so we don't fail the whole page load
      }
    } catch (err: any) {
      setError(err?.message ?? 'Failed to load post');
    } finally {
      setLoading(false);
    }
  };

  const handleLike = async () => {
    if (!post) return;
    try {
      const result = await postApi.toggleLike(post.id);
      setPost(prev =>
        prev ? {
          ...prev,
          isLikedByCurrentUser: result.isLiked,
          likeCount: result.isLiked ? prev.likeCount + 1 : prev.likeCount - 1,
        } : null
      );
    } catch (error) {
      console.error('Failed to toggle like:', error);
    }
  };

  const handleDelete = async () => {
    if (!post || user?.id !== post.authorId) return;

    if (!window.confirm('Are you sure you want to delete this post?')) return;

    try {
      await postApi.deletePost(post.id);
      navigate('/');
    } catch (error) {
      console.error('Failed to delete post:', error);
      setError('Failed to delete post');
    }
  };

  if (loading) {
    return (
      <div className="post-detail-page">
        <div className="loading-container">
          <div className="loading" />
        </div>
      </div>
    );
  }

  if (error || !post) {
    return (
      <div className="post-detail-page">
        <div className="error-container">
          <p>{error || 'Post not found'}</p>
          <Link to="/" className="back-link">← Back to Feed</Link>
        </div>
      </div>
    );
  }

  const createdDate = new Date(post.createdAt);
  const isAuthor = user?.id === post.authorId;

  return (
    <div className="post-detail-page">
      <div className="post-detail-container">
        {/* Back Link */}
        <Link to="/" className="back-link">← Back to Feed</Link>

        {/* Post Header */}
        <header className="post-detail-header">
          <div className="post-detail-title-section">
            <h1 className="post-detail-title">{post.name}</h1>
            <div className="post-detail-meta">
              <Link to={`/profile/${post.authorId}`} className="post-author">
                {post.username}
              </Link>
              <span className="post-date">
                {createdDate.toLocaleDateString()} at {createdDate.toLocaleTimeString()}
              </span>
            </div>
          </div>

          {isAuthor && (
            <div className="post-detail-actions">
              <button onClick={handleDelete} className="delete-btn" title="Delete">
                Delete
              </button>
            </div>
          )}
        </header>

        {/* Post Description */}
        {post.description && (
          <section className="post-detail-description">
            {post.description}
          </section>
        )}

        {/* Fractal Image */}
        {post.thumbnail && (
          <section className="post-detail-fractal">
            <div className="fractal-display">
              <img
                src={post.thumbnail}
                alt={`Fractal: ${fractal?.name || post.name}`}
                className="fractal-image"
              />
              {fractal && (
                <div className="fractal-info">
                  <h3>{fractal.name}</h3>
                  <div className="fractal-meta">
                    <span>by {fractal.username}</span>
                    <span>{fractal.generations} generations</span>
                    <span>{fractal.likeCount} likes</span>
                  </div>
                  <Link to={`/workbench/${fractal.id}`} className="view-fractal-btn">
                    View Fractal Details
                  </Link>
                </div>
              )}
            </div>
          </section>
        )}

        {/* Engagement Stats */}
        <div className="post-detail-stats">
          <button
            className={`stat-btn like-btn ${post.isLikedByCurrentUser ? 'liked' : ''}`}
            onClick={handleLike}
            disabled={!user}
          >
            <span className="heart-icon">♥</span>
            <span className="stat-count">{post.likeCount}</span>
          </button>
          <div className="stat-item">
            <span className="stat-label">Fractal: </span>
            <Link to={`/gallery`} className="fractal-link">
              {fractal?.name || 'View Fractal'}
            </Link>
          </div>
        </div>

        {/* Comments Section */}
        <Comments postId={post.id} />
      </div>
    </div>
  );
};

export default PostDetail;
