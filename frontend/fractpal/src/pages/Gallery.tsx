import React, { useState, useEffect } from 'react';
import { fractalApi, postApi } from '../services/api';
import type { FractalDto, PostDto } from '../services/types';
import FractalCard from '../components/FractalCard';
import Modal from '../components/Modal';
import './Gallery.css';

const Gallery: React.FC = () => {
  const [fractals, setFractals] = useState<FractalDto[]>([]);
  // fractalId -> postId - source of truth for whether a fractal has an active post
  const [postedMap, setPostedMap] = useState<Map<string, string>>(new Map());
  const [loading, setLoading] = useState(true);

  const [deleteModal, setDeleteModal] = useState<{ show: boolean; id: string | null }>({ show: false, id: null });
  const [publishModal, setPublishModal] = useState<{ show: boolean; fractalId: string | null }>({ show: false, fractalId: null });
  const [publishName, setPublishName] = useState('');
  const [publishDesc, setPublishDesc] = useState('');
  const [publishError, setPublishError] = useState('');
  const [publishLoading, setPublishLoading] = useState(false);

  useEffect(() => { loadGallery(); }, []);

  const loadGallery = async () => {
    try {
      setLoading(true);
      const [fractalsData, postsData] = await Promise.all([
        fractalApi.getMyFractals(),
        postApi.getMyPosts(),
      ]);
      setFractals(fractalsData);
      setPostedMap(new Map((postsData as PostDto[]).map(p => [p.fractalId, p.id])));
    } catch (error) {
      console.error('Failed to load gallery:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteModal.id) return;
    try {
      await fractalApi.deleteFractal(deleteModal.id);
      setFractals(prev => prev.filter(f => f.id !== deleteModal.id));
      setPostedMap(prev => { const m = new Map(prev); m.delete(deleteModal.id!); return m; });
      setDeleteModal({ show: false, id: null });
    } catch (error) {
      console.error('Failed to delete fractal:', error);
    }
  };

  const openPublishModal = (fractalId: string) => {
    const f = fractals.find(x => x.id === fractalId);
    setPublishName(f?.name ?? '');
    setPublishDesc('');
    setPublishError('');
    setPublishModal({ show: true, fractalId });
  };

  const handlePublish = async () => {
    if (!publishModal.fractalId) return;
    if (!publishName.trim()) { setPublishError('Post title is required.'); return; }
    try {
      setPublishLoading(true);
      setPublishError('');
      const post = await postApi.publishFractal(publishModal.fractalId, {
        name: publishName.trim(),
        description: publishDesc.trim() || undefined,
      });
      setPostedMap(prev => new Map(prev).set(post.fractalId, post.id));
      setPublishModal({ show: false, fractalId: null });
    } catch (err: any) {
      setPublishError(err?.message ?? 'Failed to publish.');
    } finally {
      setPublishLoading(false);
    }
  };

  const handleUnpublish = async (fractalId: string) => {
    try {
      await postApi.unpublishFractal(fractalId);
      setPostedMap(prev => { const m = new Map(prev); m.delete(fractalId); return m; });
    } catch (error) {
      console.error('Failed to unpublish:', error);
    }
  };

  const drafts = fractals.filter(f => !postedMap.has(f.id));
  const posted = fractals.filter(f => postedMap.has(f.id));

  return (
    <div className="gallery-page">
      <header className="page-header">
        <h1>Your Fractals</h1>
        <p className="text-muted">Manage your fractal collection</p>
      </header>

      {loading ? (
        <div className="loading-container"><div className="loading" /></div>
      ) : (
        <>
          {drafts.length > 0 && (
            <section className="gallery-section">
              <h2>Drafts</h2>
              <div className="fractals-grid">
                {drafts.map(f => (
                  <FractalCard
                    key={f.id}
                    fractal={{ id: f.id, name: f.name, username: f.username, userId: f.userId, imageUrl: f.thumbnail, likeCount: f.likeCount, isLikedByCurrentUser: f.isLikedByCurrentUser, publishedAt: undefined }}
                    showActions
                    onDelete={id => setDeleteModal({ show: true, id })}
                    onPublish={openPublishModal}
                  />
                ))}
              </div>
            </section>
          )}

          {posted.length > 0 && (
            <section className="gallery-section">
              <h2>Posted</h2>
              <div className="fractals-grid">
                {posted.map(f => (
                  <FractalCard
                    key={f.id}
                    fractal={{ id: f.id, name: f.name, username: f.username, userId: f.userId, imageUrl: f.thumbnail, likeCount: f.likeCount, isLikedByCurrentUser: f.isLikedByCurrentUser, publishedAt: f.createdAt }}
                    showActions
                    onDelete={id => setDeleteModal({ show: true, id })}
                    onUnpublish={handleUnpublish}
                  />
                ))}
              </div>
            </section>
          )}

          {fractals.length === 0 && (
            <div className="empty-state"><p>No fractals yet. Create one in the Workbench!</p></div>
          )}
        </>
      )}

      <Modal isOpen={deleteModal.show} onClose={() => setDeleteModal({ show: false, id: null })} title="Delete Fractal">
        <p>Are you sure you want to delete this fractal? This cannot be undone.</p>
        <div className="modal-actions">
          <button onClick={() => setDeleteModal({ show: false, id: null })} className="secondary">Cancel</button>
          <button onClick={handleDelete} className="danger">Delete</button>
        </div>
      </Modal>

      <Modal isOpen={publishModal.show} onClose={() => setPublishModal({ show: false, fractalId: null })} title="Post Fractal">
        <p className="text-muted" style={{ marginBottom: '1rem' }}>Give your post a title and optional description.</p>
        <div className="form-group" style={{ marginBottom: '0.75rem' }}>
          <label htmlFor="pub-name" style={{ display: 'block', marginBottom: '0.25rem', fontWeight: 500 }}>
            Title <span style={{ color: 'red' }}>*</span>
          </label>
          <input id="pub-name" type="text" value={publishName} onChange={e => setPublishName(e.target.value)} maxLength={100} style={{ width: '100%' }} />
        </div>
        <div className="form-group" style={{ marginBottom: '1rem' }}>
          <label htmlFor="pub-desc" style={{ display: 'block', marginBottom: '0.25rem', fontWeight: 500 }}>Description</label>
          <textarea id="pub-desc" value={publishDesc} onChange={e => setPublishDesc(e.target.value)} rows={3} maxLength={250} style={{ width: '100%' }} />
        </div>
        {publishError && <p style={{ color: 'red', marginBottom: '0.75rem', fontSize: '0.875rem' }}>{publishError}</p>}
        <div className="modal-actions">
          <button onClick={() => setPublishModal({ show: false, fractalId: null })} className="secondary" disabled={publishLoading}>Cancel</button>
          <button onClick={handlePublish} className="primary" disabled={publishLoading}>
            {publishLoading ? <span className="loading" /> : 'Post'}
          </button>
        </div>
      </Modal>
    </div>
  );
};

export default Gallery;
