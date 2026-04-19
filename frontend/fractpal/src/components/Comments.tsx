import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { commentApi } from '../services/api';
import type { CommentDto } from '../services/types';
import CommentItem from './CommentItem';
import './Comments.css';

interface CommentsProps {
  postId: string;
}

const Comments: React.FC<CommentsProps> = ({ postId }) => {
  const { user } = useAuth();
  const [comments, setComments] = useState<CommentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [newComment, setNewComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadComments();
  }, [postId]);

  const loadComments = async () => {
    try {
      setLoading(true);
      const data = await commentApi.getPostComments(postId);
      setComments(data);
    } catch (err) {
      console.error('Failed to load comments:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitComment = async () => {
    if (!newComment.trim()) {
      setError('Comment cannot be empty');
      return;
    }

    try {
      setSubmitting(true);
      setError('');
      const created = await commentApi.createComment(postId, {
        content: newComment.trim(),
      });
      setComments(prev => [created, ...prev]);
      setNewComment('');
    } catch (err: any) {
      setError(err?.message ?? 'Failed to post comment');
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdateComment = (updated: CommentDto) => {
    setComments(prev =>
      prev.map(c => (c.id === updated.id ? updated : c))
    );
  };

  const handleDeleteComment = (id: string) => {
    setComments(prev => prev.filter(c => c.id !== id));
  };

  return (
    <div className="comments-section">
      <div className="comments-header">
        <h3>Comments ({comments.length})</h3>
      </div>

      {user && (
        <div className="comment-composer">
          <textarea
            placeholder="Share your thoughts..."
            value={newComment}
            onChange={e => {
              setNewComment(e.target.value);
              setError('');
            }}
            rows={3}
            maxLength={500}
            disabled={submitting}
          />
          <div className="composer-footer">
            <span className="char-count">
              {newComment.length}/500
            </span>
            <button
              onClick={handleSubmitComment}
              disabled={submitting || !newComment.trim()}
              className="submit-btn"
            >
              {submitting ? 'Posting...' : 'Post Comment'}
            </button>
          </div>
          {error && <p className="error-message">{error}</p>}
        </div>
      )}

      {!user && (
        <div className="login-prompt">
          <p>Sign in to comment on this post</p>
        </div>
      )}

      <div className="comments-list">
        {loading ? (
          <div className="loading-comments">
            <div className="loading" />
          </div>
        ) : comments.length === 0 ? (
          <p className="no-comments">No comments yet. Be the first to comment!</p>
        ) : (
          comments.map(comment => (
            <CommentItem
              key={comment.id}
              comment={comment}
              onUpdate={handleUpdateComment}
              onDelete={handleDeleteComment}
            />
          ))
        )}
      </div>
    </div>
  );
};

export default Comments;
