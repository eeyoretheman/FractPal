import React, { useState, useEffect, useCallback } from 'react';
import { postApi } from '../services/api';
import type { PostDto } from '../services/types';
import PostCard from '../components/PostCard';
import './Home.css';

const Home: React.FC = () => {
  const [posts, setPosts] = useState<PostDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [totalCount, setTotalCount] = useState(0);

  const loadFeed = useCallback(async (pageNum: number, append: boolean) => {
    try {
      append ? setLoadingMore(true) : setLoading(true);
      const response = await postApi.getFeed(pageNum, 20);
      setPosts(prev => append ? [...prev, ...response.posts] : response.posts);
      setTotalCount(response.totalCount);
      setHasMore(pageNum * 20 < response.totalCount);
    } catch (error) {
      console.error('Failed to load feed:', error);
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, []);

  useEffect(() => {
    loadFeed(1, false);
  }, [loadFeed]);

  const handleLike = async (postId: string) => {
    try {
      const result = await postApi.toggleLike(postId);
      setPosts(prev => prev.map(p =>
        p.id === postId
          ? { ...p, isLikedByCurrentUser: result.isLiked, likeCount: result.isLiked ? p.likeCount + 1 : p.likeCount - 1 }
          : p
      ));
    } catch (error) {
      console.error('Failed to toggle like:', error);
    }
  };

  const loadMore = () => {
    if (!loadingMore && hasMore) {
      const next = page + 1;
      setPage(next);
      loadFeed(next, true);
    }
  };

  if (loading) return <div className="loading-container"><div className="loading" /></div>;

  return (
    <div className="home-page">
      <header className="page-header">
        <h1>Home</h1>
        <p className="text-muted">
          {totalCount > 0
            ? `${totalCount} posts from the community`
            : 'Discover posts from the community'}
        </p>
      </header>

      {posts.length === 0 ? (
        <div className="empty-state">
          <p>No posts yet. Be the first to share a fractal!</p>
        </div>
      ) : (
        <>
          <div className="posts-grid">
            {posts.map((post, index) => (
              <div key={post.id} className="fade-in" style={{ animationDelay: `${index * 0.05}s` }}>
                <PostCard
                  post={post}
                  onLike={handleLike}
                />
              </div>
            ))}
          </div>

          {hasMore && (
            <div className="load-more-container">
              <button onClick={loadMore} disabled={loadingMore} className="secondary">
                {loadingMore ? <span className="loading" /> : 'Load More'}
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default Home;
