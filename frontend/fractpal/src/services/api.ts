import type {
  FractalDto,
  FractalFeedResponse,
  CreateFractalRequest,
  UpdateFractalRequest,
  PostDto,
  PostFeedResponse,
  CreatePostRequest,
  UpdatePostRequest,
  UserProfileDto,
  UpdateProfileRequest,
  UserSearchDto,
  ToggleLikeResponse,
  ToggleFollowResponse,
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

  if (response.status === 204) return undefined as unknown as T;
  if (!response.ok) {
    const err = await response.json().catch(() => ({ message: response.statusText }));
    throw new Error(err.message ?? 'Request failed');
  }
  return response.json();
}

//  Fractal API
// Controller: FractalController  Route prefix: api/fractal

export const fractalApi = {
  /** GET api/fractal/feed */
  getFeed: (page = 1, pageSize = 20): Promise<FractalFeedResponse> =>
    apiFetch(`/fractal/feed?page=${page}&pageSize=${pageSize}`),

  /** GET api/fractal/mine */
  getMyFractals: (): Promise<FractalDto[]> =>
    apiFetch('/fractal/mine'),

  /** GET api/fractal/user/{userId} */
  getUserFractals: (userId: string): Promise<FractalDto[]> =>
    apiFetch(`/fractal/user/${userId}`),

  /** GET api/fractal/{id} */
  getFractalById: (id: string): Promise<FractalDto> =>
    apiFetch(`/fractal/${id}`),

  /** POST api/fractal */
  createFractal: (data: CreateFractalRequest): Promise<FractalDto> =>
    apiFetch('/fractal', { method: 'POST', body: JSON.stringify(data) }),

  /** PUT api/fractal/{id} */
  updateFractal: (id: string, data: UpdateFractalRequest): Promise<FractalDto> =>
    apiFetch(`/fractal/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  /** DELETE api/fractal/{id} */
  deleteFractal: (id: string): Promise<void> =>
    apiFetch(`/fractal/${id}`, { method: 'DELETE' }),

  /** POST api/fractal/{id}/fork */
  forkFractal: (id: string): Promise<FractalDto> =>
    apiFetch(`/fractal/${id}/fork`, { method: 'POST' }),

  /** POST api/fractal/{id}/like */
  toggleLike: (id: string): Promise<ToggleLikeResponse> =>
    apiFetch(`/fractal/${id}/like`, { method: 'POST' }),
};

//  Post API
// Controller: PostsController  Route prefix: api/posts

export const postApi = {
  /** GET api/posts/feed */
  getFeed: (page = 1, pageSize = 20): Promise<PostFeedResponse> =>
    apiFetch(`/posts/feed?page=${page}&pageSize=${pageSize}`),

  /** GET api/posts/{id} */
  getPostById: (id: string): Promise<PostDto> =>
    apiFetch(`/posts/${id}`),

  /** GET api/posts/my */
  getMyPosts: (): Promise<PostDto[]> =>
    apiFetch('/posts/my'),

  /** GET api/posts/user/{userId} */
  getUserPosts: (userId: string): Promise<PostDto[]> =>
    apiFetch(`/posts/user/${userId}`),

  /** POST api/posts/publish/{fractalId} */
  publishFractal: (fractalId: string, data: CreatePostRequest): Promise<PostDto> =>
    apiFetch(`/posts/publish/${fractalId}`, { method: 'POST', body: JSON.stringify(data) }),

  /** POST api/posts/unpublish/{fractalId} */
  unpublishFractal: (fractalId: string): Promise<void> =>
    apiFetch(`/posts/unpublish/${fractalId}`, { method: 'POST' }),

  /** PUT api/posts/{id} */
  updatePost: (id: string, data: UpdatePostRequest): Promise<PostDto> =>
    apiFetch(`/posts/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  /** DELETE api/posts/{id} */
  deletePost: (id: string): Promise<void> =>
    apiFetch(`/posts/${id}`, { method: 'DELETE' }),

  /** POST api/posts/{id}/like */
  toggleLike: (id: string): Promise<ToggleLikeResponse> =>
    apiFetch(`/posts/${id}/like`, { method: 'POST' }),
};

//  User API
// Controller: UserController  Route prefix: api/user

export const userApi = {
  /** GET api/user/profile */
  getProfile: (): Promise<UserProfileDto> =>
    apiFetch('/user/profile'),

  /** GET api/user/{id} */
  getUserProfile: (id: string): Promise<UserProfileDto> =>
    apiFetch(`/user/${id}`),

  /** PUT api/user/profile */
  updateProfile: (data: UpdateProfileRequest): Promise<UserProfileDto> =>
    apiFetch('/user/profile', { method: 'PUT', body: JSON.stringify(data) }),

  /** POST api/user/{id}/follow */
  toggleFollow: (id: string): Promise<ToggleFollowResponse> =>
    apiFetch(`/user/${id}/follow`, { method: 'POST' }),

  /** GET api/user/search?query=... */
  searchUsers: (query: string): Promise<UserSearchDto[]> =>
    apiFetch(`/user/search?query=${encodeURIComponent(query)}`),
};
