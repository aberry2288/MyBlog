﻿using MyBlog.Models;

namespace MyBlog.Services.Interfaces
{
    public interface IBlogService
    {
        public Task AddBlogPostAsync(BlogPost? blogPost);

        public Task<BlogPost> GetBlogPostAsync(int? id);

        public Task<BlogPost> GetBlogPostAsync(string? slug);

        public Task<IEnumerable<BlogPost>> GetBlogPostsAsync();

        public Task UpdateBlogPostAsync(BlogPost? blogPost);

        public Task<IEnumerable<Category>> GetCategoriesAsync();

        public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int? count);

        public Task<IEnumerable<Tag>> GetTagsAsync();

        public Task AddTagsToBlogPostAsync(IEnumerable<string>? tags, int? blogPostId);

        public Task<bool> IsTagOnBlogPostAsync(int? tagId, int? blogPostId);

        public Task RemoveAllBlogPostTagsAsync(int? blogPostId);

        public IEnumerable<BlogPost> SearchBlogPosts(string? searchString);

        public Task<bool> ValidSlugAsync(string? title, int? blogPostId);

        public Task<IEnumerable<BlogPost>> GetBlogPostsByCategory(int? categoryId);

        public Task<IEnumerable<BlogPost>> GetAllBlogPostsAsync();
    }
}
