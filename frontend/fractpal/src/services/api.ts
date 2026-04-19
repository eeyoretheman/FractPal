import type {
  CommentDto,
  CreateCommentRequest,
  CreateFractalRequest,
  CreatePostRequest,
  FractalDto,
  FractalFeedResponse,
  PostDto,
  PostFeedResponse,
  ToggleFollowResponse,
  ToggleLikeResponse,
  UpdateCommentRequest,
  UpdateFractalRequest,
  UpdatePostRequest,
  UpdateProfileRequest,
  UserProfileDto,
  UserSearchDto,
} from './types';

const API_BASE_URL = 'http://localhost:8042/api';

const getAuthHeader = (): Record<string, string> => {
  const token = localStorage.getItem('token');
  return token ? { Authorization: `Bearer ${token}` } : {};
};

async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const isFormData = options.body instanceof FormData;

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      ...(!isFormData ? { 'Content-Type': 'application/json' } : {}),
      ...getAuthHeader(),
      ...(options.headers ?? {}),
    },
  });

  if (response.status === 204) {
    return undefined as T;
  }

  if (!response.ok) {
    const err = await response.json().catch(() => ({ message: response.statusText }));
    throw new Error(err.message ?? 'Request failed');
  }

  return response.json();
}

export const fractalApi = {
  getFeed: (page = 1, pageSize = 20): Promise<FractalFeedResponse> =>
    apiFetch(`/fractal/feed?page=${page}&pageSize=${pageSize}`),

  getMyFractals: (): Promise<FractalDto[]> => apiFetch('/fractal/mine'),

  getUserFractals: (userId: string): Promise<FractalDto[]> =>
    apiFetch(`/fractal/user/${userId}`),

  getFractalById: (id: string): Promise<FractalDto> =>
    apiFetch(`/fractal/${id}`),

  createFractal: (data: CreateFractalRequest): Promise<FractalDto> =>
    apiFetch('/fractal', { method: 'POST', body: JSON.stringify(data) }),

  updateFractal: (id: string, data: UpdateFractalRequest): Promise<FractalDto> =>
    apiFetch(`/fractal/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  deleteFractal: (id: string): Promise<void> =>
    apiFetch(`/fractal/${id}`, { method: 'DELETE' }),

  forkFractal: (id: string): Promise<FractalDto> =>
    apiFetch(`/fractal/${id}/fork`, { method: 'POST' }),

  toggleLike: (id: string): Promise<ToggleLikeResponse> =>
    apiFetch(`/fractal/${id}/like`, { method: 'POST' }),
};

export const postApi = {
  getFeed: (page = 1, pageSize = 20): Promise<PostFeedResponse> =>
    apiFetch(`/posts/feed?page=${page}&pageSize=${pageSize}`),

  getPostById: (id: string): Promise<PostDto> =>
    apiFetch(`/posts/${id}`),

  getMyPosts: (): Promise<PostDto[]> => apiFetch('/posts/my'),

  getUserPosts: (userId: string): Promise<PostDto[]> =>
    apiFetch(`/posts/user/${userId}`),

  publishFractal: (fractalId: string, data: CreatePostRequest): Promise<PostDto> =>
    apiFetch(`/posts/publish/${fractalId}`, { method: 'POST', body: JSON.stringify(data) }),

  unpublishFractal: (fractalId: string): Promise<void> =>
    apiFetch(`/posts/unpublish/${fractalId}`, { method: 'POST' }),

  updatePost: (id: string, data: UpdatePostRequest): Promise<PostDto> =>
    apiFetch(`/posts/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  deletePost: (id: string): Promise<void> =>
    apiFetch(`/posts/${id}`, { method: 'DELETE' }),

  toggleLike: (id: string): Promise<ToggleLikeResponse> =>
    apiFetch(`/posts/${id}/like`, { method: 'POST' }),
};

export const userApi = {
  getProfile: (): Promise<UserProfileDto> => apiFetch('/user/profile'),

  getUserProfile: (id: string): Promise<UserProfileDto> =>
    apiFetch(`/user/${id}`),

  updateProfile: (data: UpdateProfileRequest): Promise<UserProfileDto> =>
    apiFetch('/user/profile', { method: 'PUT', body: JSON.stringify(data) }),

  toggleFollow: (id: string): Promise<ToggleFollowResponse> =>
    apiFetch(`/user/${id}/follow`, { method: 'POST' }),

  searchUsers: (query: string): Promise<UserSearchDto[]> =>
    apiFetch(`/user/search?query=${encodeURIComponent(query)}`),
};

export const commentApi = {
  getCommentById: (id: string): Promise<CommentDto> =>
    apiFetch(`/comments/${id}`),

  getPostComments: (postId: string): Promise<CommentDto[]> =>
    apiFetch(`/comments/post/${postId}`),

  createComment: (postId: string, data: CreateCommentRequest): Promise<CommentDto> =>
    apiFetch(`/comments/${postId}`, { method: 'POST', body: JSON.stringify(data) }),

  updateComment: (id: string, data: UpdateCommentRequest): Promise<CommentDto> =>
    apiFetch(`/comments/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  deleteComment: (id: string): Promise<void> =>
    apiFetch(`/comments/${id}`, { method: 'DELETE' }),
};

