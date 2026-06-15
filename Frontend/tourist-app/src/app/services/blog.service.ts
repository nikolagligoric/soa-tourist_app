import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BlogPost {
  id: string;
  title: string;
  description: string;
  createdAt: string;
  tourId?: number;
  authorUsername: string;
  imageUrls: string[];
  likes?: number;
  hasLiked?: boolean;
  comments?: BlogComment[];
  showComments?: boolean;
  newCommentText?: string;
}

export interface BlogComment {
  id: string;
  authorUsername: string;
  text: string;
  createdAt: string;
  lastModifiedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class BlogService {
  private apiUrl = '/api/blog/api/blogs';
  private imageBaseUrl = '/api/blog';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('userToken');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  getAll(): Observable<BlogPost[]> {
    return this.http.get<BlogPost[]>(this.apiUrl);
  }

  getFromFollowing(): Observable<BlogPost[]> {
    return this.http.get<BlogPost[]>(`${this.apiUrl}/following`, {
      headers: this.getAuthHeaders()
    });
  }

  create(title: string, description: string, images: File[]): Observable<BlogPost> {
    const formData = new FormData();
    formData.append('Title', title);
    formData.append('Description', description);
    images.forEach(image => formData.append('Images', image));

    return this.http.post<BlogPost>(this.apiUrl, formData, {
      headers: this.getAuthHeaders()
    });
  }

  getComments(blogId: string): Observable<BlogComment[]> {
    return this.http.get<BlogComment[]>(`${this.apiUrl}/${blogId}/comments`);
  }

  addComment(blogId: string, text: string): Observable<BlogComment> {
    return this.http.post<BlogComment>(
      `${this.apiUrl}/${blogId}/comments`,
      { text },
      { headers: this.getAuthHeaders() }
    );
  }

  getLikes(blogId: string): Observable<{ likes: number }> {
    return this.http.get<{ likes: number }>(`${this.apiUrl}/${blogId}/likes`);
  }

  hasLiked(blogId: string): Observable<{ hasLiked: boolean }> {
    return this.http.get<{ hasLiked: boolean }>(`${this.apiUrl}/${blogId}/has-liked`, {
      headers: this.getAuthHeaders()
    });
  }

  like(blogId: string): Observable<{ likes: number }> {
    return this.http.post<{ likes: number }>(
      `${this.apiUrl}/${blogId}/like`,
      {},
      { headers: this.getAuthHeaders() }
    );
  }

  unlike(blogId: string): Observable<{ likes: number }> {
    return this.http.delete<{ likes: number }>(`${this.apiUrl}/${blogId}/like`, {
      headers: this.getAuthHeaders()
    });
  }

  resolveImageUrl(imageUrl: string): string {
    if (!imageUrl) return '';
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) return imageUrl;
    return `${this.imageBaseUrl}${imageUrl}`;
  }
}
