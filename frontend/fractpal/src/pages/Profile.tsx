import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { userApi, fractalApi } from '../services/api';
import FractalCard from '../components/FractalCard';
import './Profile.css';

interface UserProfile {
  id: string;
  username: string;
  email: string;
  joinedDate: string;
  bio?: string;
  followerCount: number;
  followingCount: number;
  fractalCount: number;
  isFollowedByCurrentUser: boolean;
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
}

interface UserSearchResult {
  id: string;
  username: string;
  followerCount: number;
  isFollowedByCurrentUser: boolean;
}

const Profile: React.FC = () => {
  const { userId } = useParams<{ userId: string }>();
  const { user: currentUser } = useAuth();
  const navigate = useNavigate();

  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [fractals, setFractals] = useState<Fractal[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [bio, setBio] = useState('');

  // Search state
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<UserSearchResult[]>([]);
  const [searching, setSearching] = useState(false);

  const isOwnProfile = !userId || userId === currentUser?.id;

  useEffect(() => {
    loadProfile();
  }, [userId]);

  useEffect(() => {
    if (searchQuery.length >= 2) {
      const debounce = setTimeout(() => {
        searchUsers();
      }, 300);
      return () => clearTimeout(debounce);
    } else {
      setSearchResults([]);
    }
  }, [searchQuery]);

  const loadProfile = async () => {
    try {
      setLoading(true);
      const profileData = userId
        ? await userApi.getUserProfile(userId)
        : await userApi.getProfile();

      setProfile(profileData);
      setBio(profileData.bio || '');

      const fractalsData = isOwnProfile
        ? await fractalApi.getMyFractals()
        : await fractalApi.getUserFractals(profileData.id);

      setFractals(fractalsData);
    } catch (error) {
      console.error('Failed to load profile:', error);
    } finally {
      setLoading(false);
    }
  };

  const searchUsers = async () => {
    if (!searchQuery) return;

    try {
      setSearching(true);
      const results = await userApi.searchUsers(searchQuery);
      setSearchResults(results);
    } catch (error) {
      console.error('Search failed:', error);
    } finally {
      setSearching(false);
    }
  };

  const handleSaveBio = async () => {
    try {
      await userApi.updateProfile({ bio });
      setProfile(prev => (prev ? { ...prev, bio } : null));
      setEditing(false);
    } catch (error) {
      console.error('Failed to update bio:', error);
    }
  };

  const handleFollow = async () => {
    if (!profile) return;

    try {
      const result = await userApi.toggleFollow(profile.id);
      setProfile(prev =>
        prev
          ? {
              ...prev,
              isFollowedByCurrentUser: result.isFollowing,
              followerCount: result.isFollowing ? prev.followerCount + 1 : prev.followerCount - 1,
            }
          : null
      );
    } catch (error) {
      console.error('Failed to toggle follow:', error);
    }
  };

  const handleFollowUser = async (searchUserId: string) => {
    try {
      const result = await userApi.toggleFollow(searchUserId);
      setSearchResults(prev =>
        prev.map(u =>
          u.id === searchUserId
            ? {
                ...u,
                isFollowedByCurrentUser: result.isFollowing,
                followerCount: result.isFollowing ? u.followerCount + 1 : u.followerCount - 1,
              }
            : u
        )
      );
    } catch (error) {
      console.error('Failed to toggle follow:', error);
    }
  };

  const handleLike = async (id: string) => {
    try {
      const result = await fractalApi.toggleLike(id);
      setFractals(prev =>
        prev.map(f =>
          f.id === id
            ? {
                ...f,
                isLikedByCurrentUser: result.isLiked,
                likeCount: result.isLiked ? f.likeCount + 1 : f.likeCount - 1,
              }
            : f
        )
      );
    } catch (error) {
      console.error('Failed to toggle like:', error);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading loading-lg"></div>
      </div>
    );
  }

  if (!profile) {
    return <div className="empty-state">Profile not found</div>;
  }

  return (
    <div className="profile-page">
      {/* Search Bar */}
      <div className="search-bar-container">
        <div className="search-bar">
          <span className="search-icon">🔍</span>
          <input
            type="text"
            placeholder="Search users..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="search-input"
          />
          {searching && <span className="loading small"></span>}
        </div>

        {searchResults.length > 0 && (
          <div className="search-results">
            {searchResults.map((user) => (
              <div key={user.id} className="search-result-item">
                <div
                  className="search-result-info"
                  onClick={() => {
                    navigate(`/profile/${user.id}`);
                    setSearchQuery('');
                    setSearchResults([]);
                  }}
                >
                  <span className="search-result-name">{user.username}</span>
                  <span className="search-result-followers">
                    {user.followerCount} followers
                  </span>
                </div>
                <button
                  onClick={() => handleFollowUser(user.id)}
                  className={user.isFollowedByCurrentUser ? 'secondary small' : 'primary small'}
                >
                  {user.isFollowedByCurrentUser ? 'Unfollow' : 'Follow'}
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Profile Header */}
      <div className="profile-banner card">
        <div className="profile-info">
          <div className="profile-avatar">
            {profile.username.charAt(0).toUpperCase()}
          </div>

          <div className="profile-details">
            <h1>{profile.username}</h1>
            <p className="text-muted">Joined {formatDate(profile.joinedDate)}</p>

            <div className="profile-stats">
              <div className="stat">
                <strong>{profile.followerCount}</strong>
                <span>Followers</span>
              </div>
              <div className="stat">
                <strong>{profile.followingCount}</strong>
                <span>Following</span>
              </div>
              <div className="stat">
                <strong>{profile.fractalCount}</strong>
                <span>Fractals</span>
              </div>
            </div>

            {editing ? (
              <div className="bio-edit">
                <textarea
                  value={bio}
                  onChange={(e) => setBio(e.target.value)}
                  placeholder="Tell us about yourself..."
                  maxLength={500}
                />
                <div className="bio-actions">
                  <button onClick={() => setEditing(false)} className="secondary">
                    Cancel
                  </button>
                  <button onClick={handleSaveBio} className="primary">
                    Save
                  </button>
                </div>
              </div>
            ) : (
              <div className="bio-section">
                <p className="bio-text">{profile.bio || 'No bio yet.'}</p>
                {isOwnProfile && (
                  <button onClick={() => setEditing(true)} className="secondary small">
                    Edit Bio
                  </button>
                )}
              </div>
            )}
          </div>
        </div>

        {!isOwnProfile && (
          <div className="profile-actions">
            <button
              onClick={handleFollow}
              className={profile.isFollowedByCurrentUser ? 'secondary' : 'primary'}
            >
              {profile.isFollowedByCurrentUser ? 'Unfollow' : 'Follow'}
            </button>
          </div>
        )}
      </div>

      {/* Fractals Grid */}
      <section className="profile-fractals">
        <h2>{isOwnProfile ? 'Your' : `${profile.username}'s`} Fractals</h2>
        {fractals.length === 0 ? (
          <div className="empty-state">
            <p>No published fractals yet.</p>
          </div>
        ) : (
          <div className="fractals-grid">
            {fractals.map(fractal => (
              <FractalCard key={fractal.id} fractal={fractal} onLike={handleLike} />
            ))}
          </div>
        )}
      </section>
    </div>
  );
};

export default Profile;
