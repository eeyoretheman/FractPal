//  Fractal DTOs
// Matches FractalDto.cs exactly

export interface FractalDto {
  id: string;
  name: string;
  username: string;
  userId: string;
  createdAt: string;
  publishedAt?: string | null;
  isPublished: boolean;
  thumbnail?: string | null;       // maps to FractalThumbnailPath on the entity
  likeCount: number;
  isLikedByCurrentUser: boolean;
  // L-System
  axiom: string;
  rules: string;
  instructions: string;
  generations: number;
  xTranslation: number;
  yTranslation: number;
  zoom: number;
}

export interface CreateFractalRequest {
  name: string;
  axiom: string;
  rules: string;
  instructions: string;
  generations: number;
  xTranslation: number;
  yTranslation: number;
  zoom: number;
  thumbnail?: string;
}

export interface UpdateFractalRequest {
  name: string;
  axiom: string;
  rules: string;
  instructions: string;
  generations: number;
  xTranslation: number;
  yTranslation: number;
  zoom: number;
  thumbnail?: string;
}

export interface FractalFeedResponse {
  fractals: FractalDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

//  Post DTOs
// Matches PostDto.cs exactly - note: no embedded fractal, has username

export interface PostDto {
  id: string;           // Guid serialised as string
  fractalId: string;
  authorId: string;
  username: string;     // post.Author.UserName from MapToDto
  name: string;
  description?: string | null;
  thumbnail?: string | null;   // post.Fractal.FractalThumbnailPath from MapToDto
  likeCount: number;
  isLikedByCurrentUser: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreatePostRequest {
  name: string;
  description?: string;
}

export interface UpdatePostRequest {
  name: string;
  description?: string;
}

export interface PostFeedResponse {
  posts: PostDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

//  User DTOs
// Matches UserProfileDto.cs exactly

export interface UserProfileDto {
  id: string;
  username: string;
  email: string;
  joinedDate: string;
  bio?: string | null;
  profileImageData?: string | null;
  followerCount: number;
  followingCount: number;
  fractalCount: number;
  isFollowedByCurrentUser: boolean;
}

export interface UpdateProfileRequest {
  bio?: string;
}

export interface UserSearchDto {
  id: string;
  username: string;
  profileImageData?: string | null;
  followerCount: number;
  isFollowedByCurrentUser: boolean;
}

//  Shared response types

export interface ToggleLikeResponse {
  isLiked: boolean;
}

export interface ToggleFollowResponse {
  isFollowing: boolean;
}
