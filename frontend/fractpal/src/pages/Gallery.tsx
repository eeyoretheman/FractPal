import React, { useState, useEffect } from 'react';
import { fractalApi } from '../services/api';
import FractalCard from '../components/FractalCard';
import Modal from '../components/Modal';
import './Gallery.css';

interface Fractal {
  id: string;
  name: string;
  username: string;
  userId: string;
  isPublished: boolean;
  publishedAt?: string;
  imageUrl?: string;
  likeCount: number;
  isLikedByCurrentUser: boolean;
}

const Gallery: React.FC = () => {
  const [fractals, setFractals] = useState<Fractal[]>([]);
  const [loading, setLoading] = useState(true);
  const [deleteModal, setDeleteModal] = useState<{ show: boolean; id: string | null }>({
    show: false,
    id: null,
  });
  const [publishModal, setPublishModal] = useState<{ show: boolean; id: string | null }>({
    show: false,
    id: null,
  });

  useEffect(() => {
    loadGallery();
  }, []);

  const loadGallery = async () => {
    try {
      setLoading(true);
      const data = await fractalApi.getMyFractals();
      setFractals(data);
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
      setDeleteModal({ show: false, id: null });
    } catch (error) {
      console.error('Failed to delete fractal:', error);
    }
  };

  const handlePublish = async () => {
    if (!publishModal.id) return;

    try {
      await fractalApi.publishFractal(publishModal.id);
      setFractals(prev =>
        prev.map(f =>
          f.id === publishModal.id
            ? { ...f, isPublished: true, publishedAt: new Date().toISOString() }
            : f
        )
      );
      setPublishModal({ show: false, id: null });
    } catch (error) {
      console.error('Failed to publish fractal:', error);
    }
  };

  const handleUnpublish = async (id: string) => {
    try {
      await fractalApi.unpublishFractal(id);
      setFractals(prev =>
        prev.map(f => (f.id === id ? { ...f, isPublished: false, publishedAt: undefined } : f))
      );
    } catch (error) {
      console.error('Failed to unpublish fractal:', error);
    }
  };

  const published = fractals.filter(f => f.isPublished);
  const unpublished = fractals.filter(f => !f.isPublished);

  return (
    <div className="gallery-page">
      <header className="page-header">
        <h1>Your Fractals</h1>
        <p className="text-muted">Manage your fractal collection</p>
      </header>

      {loading ? (
        <div className="loading-container">
          <div className="loading"></div>
        </div>
      ) : (
        <>
          {unpublished.length > 0 && (
            <section className="gallery-section">
              <h2>Drafts</h2>
              <div className="fractals-grid">
                {unpublished.map(fractal => (
                  <FractalCard
                    key={fractal.id}
                    fractal={fractal}
                    showActions
                    onDelete={(id: string) => setDeleteModal({ show: true, id })}
                    onPublish={(id: string) => setPublishModal({ show: true, id })}
                  />
                ))}
              </div>
            </section>
          )}

          {published.length > 0 && (
            <section className="gallery-section">
              <h2>Published</h2>
              <div className="fractals-grid">
                {published.map(fractal => (
                  <FractalCard
                    key={fractal.id}
                    fractal={fractal}
                    showActions
                    onDelete={(id: string) => setDeleteModal({ show: true, id })}
                    onUnpublish={handleUnpublish}
                  />
                ))}
              </div>
            </section>
          )}

          {fractals.length === 0 && (
            <div className="empty-state">
              <p>No fractals yet. Create one in the Workbench!</p>
            </div>
          )}
        </>
      )}

      <Modal
        isOpen={deleteModal.show}
        onClose={() => setDeleteModal({ show: false, id: null })}
        title="Delete Fractal"
      >
        <p>Are you sure you want to delete this fractal? This action cannot be undone.</p>
        <div className="modal-actions">
          <button onClick={() => setDeleteModal({ show: false, id: null })} className="secondary">
            Cancel
          </button>
          <button onClick={handleDelete} className="danger">
            Delete
          </button>
        </div>
      </Modal>

      <Modal
        isOpen={publishModal.show}
        onClose={() => setPublishModal({ show: false, id: null })}
        title="Publish Fractal"
      >
        <p>Are you sure you want to publish this fractal? It will be visible to everyone.</p>
        <div className="modal-actions">
          <button onClick={() => setPublishModal({ show: false, id: null })} className="secondary">
            Cancel
          </button>
          <button onClick={handlePublish} className="primary">
            Publish
          </button>
        </div>
      </Modal>
    </div>
  );
};

export default Gallery;
