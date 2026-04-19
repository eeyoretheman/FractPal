import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { commentApi } from '../services/api';
import type { CommentDto } from '../services/types';
import './CommentItem.css';

interface CommentItemProps {
  comment: CommentDto;
  onUpdate: (updated: CommentDto) => void;
  onDelete: (id: string) => void;
}

const CommentItem: React.FC<CommentItemProps> = ({ comment, onUpdate, onDelete }) => {
  const { user } = useAuth();
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(comment.content);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState('');

  const isAuthor = user?.id === comment.authorId;
  const createdDate = new Date(comment.createdAt);
  const isEdited = comment.updatedAt && new Date(comment.updatedAt) > createdDate;

  const handleUpdate = async () => {
    if (!editContent.trim()) {
      setError('Comment cannot be empty');
      return;
    }

    try {
      setError('');
      const updated = await commentApi.updateComment(comment.id, { content: editContent.trim() });
      onUpdate(updated);
      setIsEditing(false);
    } catch (err: any) {
      setError(err?.message ?? 'Failed to update comment');
    }
  };

  const handleDelete = async () => {
    try {
      setIsDeleting(true);
      await commentApi.deleteComment(comment.id);
      onDelete(comment.id);
    } catch (err: any) {
      setError(err?.message ?? 'Failed to delete comment');
      setIsDeleting(false);
    }
  };

  const handleCancel = () => {
    setEditContent(comment.content);
    setIsEditing(false);
    setError('');
  };

  const formatDate = (date: Date): string => {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
  };

  return (
    <div className="comment-item">
      <div className="comment-header">
        <div className="comment-meta">
          <span className="comment-author">{comment.username}</span>
          <span className="comment-time">{formatDate(createdDate)}</span>
          {isEdited && <span className="comment-edited">(edited)</span>}
        </div>
        {isAuthor && (
          <div className="comment-actions">
            <button
              className="comment-action-btn edit"
              onClick={() => setIsEditing(!isEditing)}
              title={isEditing ? 'Cancel' : 'Edit'}
            >
              {isEditing ? '✕' : '✎'}
            </button>
            <button
              className="comment-action-btn delete"
              onClick={handleDelete}
              disabled={isDeleting}
              title="Delete"
            >
              🗑
            </button>
          </div>
        )}
      </div>

      {isEditing ? (
        <div className="comment-edit">
          <textarea
            value={editContent}
            onChange={e => {
              setEditContent(e.target.value);
              setError('');
            }}
            rows={3}
            placeholder="Edit your comment..."
            maxLength={500}
          />
          <div className="edit-actions">
            <button className="cancel-btn" onClick={handleCancel}>
              Cancel
            </button>
            <button
              className="save-btn"
              onClick={handleUpdate}
              disabled={editContent === comment.content}
            >
              Save
            </button>
          </div>
          {error && <p className="error-message">{error}</p>}
        </div>
      ) : (
        <p className="comment-content">{comment.content}</p>
      )}
    </div>
  );
};

export default CommentItem;
