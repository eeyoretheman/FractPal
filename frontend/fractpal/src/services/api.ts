const API_BASE_URL = 'http://localhost:8042/api';

const getAuthHeader = (): Record<string, string> => {
  const token = localStorage.getItem('token');
  return token ? { Authorization: `Bearer ${token}` } : {};
};

// Fractal API
export const fractalApi = {
  getFeed: async (page = 1, pageSize = 20) => {
    const response = await fetch(
      `${API_BASE_URL}/fractal/feed?page=${page}&pageSize=${pageSize}`,
      {
        headers: {
          ...getAuthHeader(),
        },
      }
    );
    if (!response.ok) throw new Error('Failed to fetch feed');
    return response.json();
  },

  getMyFractals: async () => {
    const response = await fetch(`${API_BASE_URL}/fractal/mine`, {
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to fetch fractals');
    return response.json();
  },

  getUserFractals: async (userId: string) => {
    const response = await fetch(`${API_BASE_URL}/fractal/user/${userId}`, {
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to fetch user fractals');
    return response.json();
  },

  getFractalById: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/fractal/${id}`, {
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to fetch fractal');
    return response.json();
  },

  createFractal: async (data: any) => {
    const response = await fetch(`${API_BASE_URL}/fractal`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeader(),
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to create fractal');
    return response.json();
  },

  updateFractal: async (id: string, data: any) => {
    const response = await fetch(`${API_BASE_URL}/fractal/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeader(),
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to update fractal');
    return response.json();
  },

  deleteFractal: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/fractal/${id}`, {
      method: 'DELETE',
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to delete fractal');
  },

  publishFractal: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/fractal/${id}/publish`, {
      method: 'POST',
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to publish fractal');
    return response.json();
  },

  unpublishFractal: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/fractal/${id}/unpublish`, {
      method: 'POST',
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to unpublish fractal');
    return response.json();
  },

  toggleLike: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/fractal/${id}/like`, {
      method: 'POST',
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to toggle like');
    return response.json();
  },

  forkFractal: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/fractal/${id}/fork`, {
      method: 'POST',
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to fork fractal');
    return response.json();
  },
};

// User API
export const userApi = {
  getProfile: async () => {
    const response = await fetch(`${API_BASE_URL}/user/profile`, {
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to fetch profile');
    return response.json();
  },

  getUserProfile: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/user/${id}`, {
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to fetch user profile');
    return response.json();
  },

  updateProfile: async (data: { bio?: string }) => {
    const response = await fetch(`${API_BASE_URL}/user/profile`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeader(),
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to update profile');
    return response.json();
  },

  toggleFollow: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/user/${id}/follow`, {
      method: 'POST',
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to toggle follow');
    return response.json();
  },

  searchUsers: async (query: string) => {
    const response = await fetch(`${API_BASE_URL}/user/search?query=${encodeURIComponent(query)}`, {
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to search users');
    return response.json();
  },
};

// Comment API
export const commentApi = {
  getCommentsByFractal: async (fractalId: string) => {
    const response = await fetch(`${API_BASE_URL}/comment/fractal/${fractalId}`, {
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to fetch comments');
    return response.json();
  },

  getCommentById: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/comment/${id}`, {
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to fetch comment');
    return response.json();
  },

  createComment: async (fractalId: string, content: string) => {
    const response = await fetch(`${API_BASE_URL}/comment/${fractalId}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeader(),
      },
      body: JSON.stringify({ content }),
    });
    if (!response.ok) throw new Error('Failed to create comment');
    return response.json();
  },

  updateComment: async (id: string, content: string) => {
    const response = await fetch(`${API_BASE_URL}/comment/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeader(),
      },
      body: JSON.stringify({ content }),
    });
    if (!response.ok) throw new Error('Failed to update comment');
    return response.json();
  },

  deleteComment: async (id: string) => {
    const response = await fetch(`${API_BASE_URL}/comment/${id}`, {
      method: 'DELETE',
      headers: {
        ...getAuthHeader(),
      },
    });
    if (!response.ok) throw new Error('Failed to delete comment');
  },
};
