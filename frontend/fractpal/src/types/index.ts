// src/types/index.ts

export interface User {
  id: string;
  email: string;
  roles: string[];
}

export interface AuthResponse {
  jwt: string;
  refreshToken: string;
  user?: User; // Depending on if your API returns the user obj with the token
}

export interface Fractal {
  id: string;
  name: string;
  imageUrl?: string;
  isPosted: boolean;
  likes: number;
  createdAt: string;
}

export interface UserProfile {
  username: string;
  joinedDate: string;
  followerCount: string;
  followingCount: number;
  bio: string;
  favoriteMathematician: string;
}
