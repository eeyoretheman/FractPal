import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { userApi, fractalApi, postApi } from '../services/api';
import type { UserProfileDto, FractalDto, PostDto } from '../services/types';
import FractalCard from '../components/FractalCard';
import './Profile.css';

type Tab = 'posts' | 'fractals';

const Profile: React.FC = () => {
  const { userId } = useParams<{ userId: string }>();
  const { user: currentUser } = useAuth();

  const [profile, setProfile] = useState<UserProfileDto | null>(null);
  const [posts, setPosts] = useState<PostDto[]>([]);
  const [fractals, setFractals] = useState<FractalDto[]>([]);
  const [activeTab, setActiveTab] = useState<Tab>('posts');
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [bio, setBio] = useState('');

  const isOwnProfile = !userId || userId === currentUser?.id;
  const targetId = userId ?? currentUser?.id ?? '';

  useEffect(() => { loadAll(); }, [userId]);

  const loadAll = async () => {
    try {
      setLoading(true);
      const [profileData, postsData, fractalsData] = await Promise.all([
        isOwnProfile ? userApi.getProfile() : userApi.getUserProfile(targetId),
        isOwnProfile ? postApi.getMyPosts() : postApi.getUserPosts(targetId),
        isOwnProfile ? fractalApi.getMyFractals() : fractalApi.getUserFractals(targetId),
      ]);
      setProfile(profileData);
      setBio(profileData.bio ?? '');
      setPosts(postsData);
      setFractals(fractalsData);
    } catch (error) {
      console.error('Failed to load profile:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSaveBio = async () => {
    try {
      const updated = await userApi.updateProfile({ bio });
      setProfile(updated);
      setEditing(false);
    } catch (error) {
      console.error('Failed to save bio:', error);
    }
  };

  const handleFollow = async () => {
    if (!profile) return;
    try {
      const result = await userApi.toggleFollow(profile.id);
      setProfile(prev => prev ? {
        ...prev,
        isFollowedByCurrentUser: result.isFollowing,
        followerCount: result.isFollowing ? prev.followerCount + 1 : prev.followerCount - 1,
      } : null);
    } catch (error) {
      console.error('Failed to toggle follow:', error);
    }
  };

  const handleLikePost = async (postId: string) => {
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

  const handleLikeFractal = async (fractalId: string) => {
    try {
      const result = await fractalApi.toggleLike(fractalId);
      setFractals(prev => prev.map(f =>
        f.id === fractalId
          ? { ...f, isLikedByCurrentUser: result.isLiked, likeCount: result.isLiked ? f.likeCount + 1 : f.likeCount - 1 }
          : f
      ));
    } catch (error) {
      console.error('Failed to toggle like:', error);
    }
  };

  const formatDate = (d: string) =>
    new Date(d).toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });

  if (loading) return <div className="loading-container"><div className="loading" /></div>;
  if (!profile) return <div className="empty-state">Profile not found</div>;

  return (
    <div className="profile-page">
      <div className="profile-banner card">
        <div className="profile-info">
          <h1>{profile.username}</h1>
          <p className="text-muted">Joined {formatDate(profile.joinedDate)}</p>

          <div className="profile-stats">
            <div className="stat"><strong>{profile.followerCount}</strong><span>Followers</span></div>
            <div className="stat"><strong>{profile.followingCount}</strong><span>Following</span></div>
            <div className="stat"><strong>{profile.fractalCount}</strong><span>Fractals</span></div>
          </div>

          {editing ? (
            <div className="bio-edit">
              <textarea value={bio} onChange={e => setBio(e.target.value)} placeholder="Tell us about yourself..." maxLength={500} />
              <div className="bio-actions">
                <button onClick={() => setEditing(false)} className="secondary">Cancel</button>
                <button onClick={handleSaveBio} className="primary">Save</button>
              </div>
            </div>
          ) : (
            <div className="bio-section">
              <p className="bio-text">{profile.bio || 'No bio yet.'}</p>
              {isOwnProfile && (
                <button onClick={() => setEditing(true)} className="secondary small">Edit Bio</button>
              )}
            </div>
          )}
        </div>

        {!isOwnProfile && (
          <div className="profile-actions">
            <button onClick={handleFollow} className={profile.isFollowedByCurrentUser ? 'secondary' : 'primary'}>
              {profile.isFollowedByCurrentUser ? 'Unfollow' : 'Follow'}
            </button>
          </div>
        )}
      </div>

      <div className="profile-tabs">
        <button className={`tab-btn${activeTab === 'posts' ? ' active' : ''}`} onClick={() => setActiveTab('posts')}>
          Posts ({posts.length})
        </button>
        <button className={`tab-btn${activeTab === 'fractals' ? ' active' : ''}`} onClick={() => setActiveTab('fractals')}>
          Fractals ({fractals.length})
        </button>
      </div>

      {activeTab === 'posts' && (
        <section className="profile-content">
          {posts.length === 0
            ? <div className="empty-state"><p>No posts yet.</p></div>
            : <div className="fractals-grid">
                {posts.map(p => (
                  <FractalCard
                    key={p.id}
                    fractal={{
                      id: p.fractalId,
                      postId: p.id,
                      name: p.name,
                      username: p.username,
                      userId: p.authorId,
                      imageUrl: p.imageUrl,
                      likeCount: p.likeCount,
                      isLikedByCurrentUser: p.isLikedByCurrentUser,
                      createdAt: p.createdAt,
                    }}
                    onLike={handleLikePost}
                  />
                ))}
              </div>
          }
        </section>
      )}

      {activeTab === 'fractals' && (
        <section className="profile-content">
          {fractals.length === 0
            ? <div className="empty-state"><p>No fractals yet.</p></div>
            : <div className="fractals-grid">
                {fractals.map(f => (
                  <FractalCard
                    key={f.id}
                    fractal={{
                      id: f.id,
                      name: f.name,
                      username: f.username,
                      userId: f.userId,
                      imageUrl: f.imageUrl,
                      likeCount: f.likeCount,
                      isLikedByCurrentUser: f.isLikedByCurrentUser,
                      createdAt: f.createdAt,
                    }}
                    onLike={handleLikeFractal}
                  />
                ))}
              </div>
          }
        </section>
      )}
    </div>
  );
};

export default Profile;
