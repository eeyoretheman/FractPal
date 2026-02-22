import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { fractalApi, commentApi } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import './PostDetail.css';

interface Comment {
  id: string;
  username: string;
  userId: string;
  content: string;
  createdAt: string;
  updatedAt?: string;
}

interface Fractal {
  id: string;
  name: string;
  username: string;
  userId: string;
  imageUrl?: string;
  likeCount: number;
  isLikedByCurrentUser: boolean;
  publishedAt?: string;
  createdAt: string;
}

const PostDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();

  const [fractal, setFractal] = useState<Fractal | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(true);
  const [commentsLoading, setCommentsLoading] = useState(false);
  const [commentText, setCommentText] = useState('');
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
  const [editingText, setEditingText] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadPostData();
  }, [id]);

  const loadPostData = async () => {
    if (!id) return;
    try {
      setLoading(true);
      const fractalData = await fractalApi.getFractalById(id);
      setFractal(fractalData);
      await loadComments();
    } catch (err) {
      setError('Failed to load post');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadComments = async () => {
    if (!id) return;
    try {
      setCommentsLoading(true);
      const commentsData = await commentApi.getCommentsByFractal(id);
      setComments(Array.isArray(commentsData) ? commentsData : []);
    } catch (err) {
      console.error('Failed to load comments:', err);
      setComments([]);
    } finally {
      setCommentsLoading(false);
    }
  };

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!commentText.trim() || !id) return;

    try {
      const newComment = await commentApi.createComment(id, commentText);
      setComments([newComment, ...comments]);
      setCommentText('');
    } catch (err) {
      setError('Failed to add comment');
      console.error(err);
    }
  };

  const handleEditComment = async (commentId: string) => {
    if (!editingText.trim()) return;
    try {
      const updated = await commentApi.updateComment(commentId, editingText);
      setComments(comments.map((c: Comment) => c.id === commentId ? updated : c));
      setEditingCommentId(null);
      setEditingText('');
    } catch (err) {
      setError('Failed to edit comment');
      console.error(err);
    }
  };

  const handleDeleteComment = async (commentId: string) => {
    try {
      await commentApi.deleteComment(commentId);
      setComments(comments.filter((c: Comment) => c.id !== commentId));
    } catch (err) {
      setError('Failed to delete comment');
      console.error(err);
    }
  };

  const handleLike = async () => {
    if (!fractal) return;
    try {
      const result = await fractalApi.toggleLike(fractal.id);
      setFractal({
        ...fractal,
        isLikedByCurrentUser: result.isLiked,
        likeCount: result.isLiked ? fractal.likeCount + 1 : fractal.likeCount - 1,
      });
    } catch (err) {
      console.error('Failed to toggle like:', err);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return (
      <div className="post-detail-page">
        <div className="loading-container">
          <div className="loading"></div>
        </div>
      </div>
    );
  }

  if (!fractal) {
    return (
      <div className="post-detail-page">
        <div className="error-state">
          <p>Post not found</p>
          <button onClick={() => navigate('/')} className="primary">
            Back to Home
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="post-detail-page">
      <button onClick={() => navigate(-1)} className="back-button">
        ← Back
      </button>

      <article className="post-container card">
        <div className="post-header">
          <div className="post-info">
            <h1 className="post-title">{fractal.name}</h1>
            <div className="post-meta">
              <Link to={`/profile/${fractal.userId}`} className="author-link">
                <span className="author-name">{fractal.username}</span>
              </Link>
              <span className="separator">•</span>
              <time className="post-date" dateTime={fractal.publishedAt || fractal.createdAt}>
                {formatDate(fractal.publishedAt || fractal.createdAt)}
              </time>
            </div>
          </div>
          <div className="post-actions">
            <button 
              className={`icon-button like-button ${fractal.isLikedByCurrentUser ? 'liked' : ''}`}
              onClick={handleLike}
              aria-label={fractal.isLikedByCurrentUser ? 'Unlike' : 'Like'}
            >
              <span className="like-icon">❤️</span>
              <span className="like-count">{fractal.likeCount}</span>
            </button>
          </div>
        </div>

        {fractal.imageUrl && (
          <div className="post-image-container">
            <img 
              src={fractal.imageUrl} 
              alt={fractal.name}
              className="post-image"
            />
          </div>
        )}

        <section className="comments-section">
          <h2 className="comments-title">Comments ({comments.length})</h2>

          {user && (
            <form className="comment-form" onSubmit={handleAddComment}>
              <textarea
                className="comment-input"
                placeholder="Share your thoughts..."
                value={commentText}
                onChange={(e) => setCommentText(e.target.value)}
                rows={3}
              />
              <button 
                type="submit" 
                disabled={!commentText.trim()}
                className="primary small"
              >
                Post Comment
              </button>
            </form>
          )}

          {error && <div className="error-message">{error}</div>}

          <div className="comments-list">
            {commentsLoading ? (
              <div className="loading-placeholder">Loading comments...</div>
            ) : comments.length === 0 ? (
              <div className="empty-comments">
                <p>No comments yet. Be the first to share your thoughts!</p>
              </div>
            ) : (
              comments.map((comment) => (
                <div key={comment.id} className="comment-item card">
                  <div className="comment-header">
                    <Link to={`/profile/${comment.userId}`} className="comment-author">
                      <strong>{comment.username}</strong>
                    </Link>
                    <time className="comment-date" dateTime={comment.createdAt}>
                      {formatDate(comment.createdAt)}
                    </time>
                    {comment.updatedAt && comment.updatedAt !== comment.createdAt && (
                      <span className="comment-edited">(edited)</span>
                    )}
                  </div>

                  {editingCommentId === comment.id ? (
                    <div className="comment-edit-form">
                      <textarea
                        className="comment-input"
                        value={editingText}
                        onChange={(e) => setEditingText(e.target.value)}
                        rows={3}
                      />
                      <div className="edit-actions">
                        <button
                          onClick={() => handleEditComment(comment.id)}
                          className="primary small"
                          disabled={!editingText.trim()}
                        >
                          Save
                        </button>
                        <button
                          onClick={() => setEditingCommentId(null)}
                          className="secondary small"
                        >
                          Cancel
                        </button>
                      </div>
                    </div>
                  ) : (
                    <>
                      <p className="comment-content">{comment.content}</p>
                      {user?.id === comment.userId && (
                        <div className="comment-actions">
                          <button
                            onClick={() => {
                              setEditingCommentId(comment.id);
                              setEditingText(comment.content);
                            }}
                            className="secondary small"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleDeleteComment(comment.id)}
                            className="danger small"
                          >
                            Delete
                          </button>
                        </div>
                      )}
                    </>
                  )}
                </div>
              ))
            )}
          </div>
        </section>
      </article>
    </div>
  );
};

export default PostDetail;
