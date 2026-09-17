import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { marked } from 'marked';
import { AuthService } from '../services/auth.service';
import { BlogComment, BlogPost, BlogService } from '../services/blog.service';

declare const require: any;
const DOMPurify = require('dompurify');

@Component({
  selector: 'app-blog',
  templateUrl: './blog.component.html',
  styleUrls: ['./blog.component.css']
})
export class BlogComponent implements OnInit {
  blogs: BlogPost[] = [];
  title = '';
  description = '';
  selectedImages: File[] = [];
  selectedImageNames: string[] = [];
  currentUser: any = null;
  isLoading = false;
  isCreating = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private blogService: BlogService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getUserDetails();
    if (!this.currentUser) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadBlogs();
  }

  loadBlogs(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.blogService.getFromFollowing().subscribe({
      next: blogs => {
        this.blogs = blogs.map(blog => ({
          ...blog,
          likes: 0,
          hasLiked: false,
          comments: [],
          showComments: false,
          newCommentText: ''
        }));
        this.blogs.forEach(blog => {
          this.loadLikes(blog);
          this.loadHasLiked(blog);
        });
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Nije moguce ucitati blogove korisnika koje pratis.';
        this.isLoading = false;
      }
    });
  }

  onImagesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedImages = Array.from(input.files || []);
    this.selectedImageNames = this.selectedImages.map(file => file.name);
  }

  createBlog(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.title.trim() || !this.description.trim()) {
      this.errorMessage = 'Naslov i opis su obavezni.';
      return;
    }

    this.isCreating = true;
    this.blogService.create(this.title.trim(), this.description.trim(), this.selectedImages).subscribe({
      next: createdBlog => {
        this.blogs = [{
          ...createdBlog,
          likes: 0,
          hasLiked: false,
          comments: [],
          showComments: false,
          newCommentText: ''
        }, ...this.blogs];
        this.title = '';
        this.description = '';
        this.selectedImages = [];
        this.selectedImageNames = [];
        this.successMessage = 'Blog je uspesno kreiran.';
        this.isCreating = false;
      },
      error: err => {
        this.errorMessage = err.error || 'Greska pri kreiranju bloga.';
        this.isCreating = false;
      }
    });
  }

  toggleComments(blog: BlogPost): void {
    blog.showComments = !blog.showComments;
    if (blog.showComments && (!blog.comments || blog.comments.length === 0)) {
      this.loadComments(blog);
    }
  }

  loadComments(blog: BlogPost): void {
    this.blogService.getComments(blog.id).subscribe({
      next: comments => blog.comments = comments,
      error: () => this.errorMessage = 'Nije moguce ucitati komentare.'
    });
  }

  addComment(blog: BlogPost): void {
    const text = (blog.newCommentText || '').trim();
    if (!text) return;

    this.blogService.addComment(blog.id, text).subscribe({
      next: comment => {
        blog.comments = [comment, ...(blog.comments || [])];
        blog.newCommentText = '';
        blog.showComments = true;
      },
      error: err => this.errorMessage = err.error || 'Greska pri dodavanju komentara.'
    });
  }

  toggleLike(blog: BlogPost): void {
    const action = blog.hasLiked ? this.blogService.unlike(blog.id) : this.blogService.like(blog.id);

    action.subscribe({
      next: response => {
        blog.likes = response.likes;
        blog.hasLiked = !blog.hasLiked;
      },
      error: err => this.errorMessage = err.error || 'Greska pri izmeni lajka.'
    });
  }

  renderMarkdown(markdown: string): string {
    const parsedHtml = marked.parse(markdown || '', {
      breaks: true,
      gfm: true
    });

    return DOMPurify.sanitize(parsedHtml as string);
  }

  getImageUrl(imageUrl: string): string {
    return this.blogService.resolveImageUrl(imageUrl);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString('sr-RS');
  }

  private loadLikes(blog: BlogPost): void {
    this.blogService.getLikes(blog.id).subscribe({
      next: response => blog.likes = response.likes,
      error: () => blog.likes = 0
    });
  }

  private loadHasLiked(blog: BlogPost): void {
    this.blogService.hasLiked(blog.id).subscribe({
      next: response => blog.hasLiked = response.hasLiked,
      error: () => blog.hasLiked = false
    });
  }

}
