using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyBlog.Data;
using MyBlog.Helpers;
using MyBlog.Models;
using MyBlog.Services.Interfaces;
using X.PagedList;

namespace MyBlog.Controllers
{

    public class BlogPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IBlogService _blogService;

        public BlogPostsController(ApplicationDbContext context, UserManager<BlogUser> userManager, IImageService imageService, IBlogService blogService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _blogService = blogService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuthorArea(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;


            IPagedList<BlogPost> blogPosts = await (await _blogService.GetAllBlogPostsAsync()).ToPagedListAsync(page, pageSize);

            return View(blogPosts);

        }

        [AllowAnonymous]
        // GET: BlogPosts
        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;


            IPagedList<BlogPost> blogPosts = await (await _blogService.GetBlogPostsAsync()).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(Index);

            return View(blogPosts);
        }

        //GET: Search Bars
        public async Task<IActionResult> SearchIndex(string? searchString, int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;


            IPagedList<BlogPost> blogPosts = await _blogService.SearchBlogPosts(searchString).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(SearchIndex);

            ViewData["SearchString"] = searchString;

            return View(nameof(Index), blogPosts);
        }

        public async Task<IActionResult> Popular(int? pageNum)
        {

            int pageSize = 3;
            int page = pageNum ?? 1;

            ViewData["ActionName"] = nameof(Popular);

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetPopularBlogPostsAsync()).ToPagedListAsync(page, pageSize);

            return View(nameof(Index), blogPosts);

        }

        //public async Task<IActionResult> BlogPostByCategory(int? categoryId)
        //{
        //    IEnumerable<BlogPost> blogPosts = await _blogService.GetBlogPostsByCategory(categoryId);

        //    return View(nameof(Index), blogPosts);
        //}

        [AllowAnonymous]
        // GET: BlogPosts/Details/5
        public async Task<IActionResult> Details(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(slug);

            if (blogPost == null)
            {
                return NotFound();
            }



            return View(blogPost);
        }



        [Authorize(Roles = "Admin")]
        // GET: BlogPosts/Create
        public IActionResult Create()
        {
            //BlogPost blogPost = new BlogPost();

            //string userId = _userManager.GetUserId(User)!;

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Abstract,Content,IsPublished,ImageFile,CategoryId")] BlogPost blogPost, string? stringTags)
        {
            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                string? newSlug = StringHelper.BlogPostSlug(blogPost.Title);

                if (!await _blogService.ValidSlugAsync(newSlug, blogPost.Id))
                {
                    ModelState.AddModelError("Title", "A similar Title/Slug is already in use.");

                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

                    return View(blogPost);

                }

                blogPost.Slug = newSlug;

                //Set created date
                blogPost.Created = DateTime.Now;

                //Set the image data if one has been chosen
                if (blogPost.ImageFile != null)
                {
                    //create image service, convert file to byte array and assign it to the imagedata
                    blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);

                    //Assign the imagetype based on the chosen file
                    blogPost.ImageType = blogPost.ImageFile.ContentType;
                }


                await _blogService.AddBlogPostAsync(blogPost);


                if (string.IsNullOrEmpty(stringTags) == false)
                {
                    IEnumerable<string> tags = stringTags.Split(',');

                await _blogService.AddTagsToBlogPostAsync(tags, blogPost.Id);

                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name", blogPost.CategoryId);

            return View(blogPost);
        }

        [Authorize(Roles = "Admin, Moderator")]
        // GET: BlogPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Abstract,Content,Created,Slug,IsPublished,IsDeleted,ImageData,ImageType,ImageFile,CategoryId")] BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Set created date
                    blogPost.Updated = DateTime.Now;

                    //Set the image data if one has been chosen
                    if (blogPost.ImageFile != null)
                    {
                        //create image service, convert file to byte array and assign it to the imagedata
                        blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);

                        //Assign the imagetype based on the chosen file
                        blogPost.ImageType = blogPost.ImageFile.ContentType;
                    }

                    await _blogService.UpdateBlogPostAsync(blogPost);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        [Authorize(Roles = "Admin")]
        // GET: BlogPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.IsDeleted = true;

            await _blogService.UpdateBlogPostAsync(blogPost);

            return RedirectToAction(nameof(AuthorArea));
        }

        [Authorize(Roles = "Admin")]
        // GET: BlogPosts/Delete/5
        public async Task<IActionResult> Undelete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.IsDeleted = false;

            await _blogService.UpdateBlogPostAsync(blogPost);

            return RedirectToAction(nameof(AuthorArea));
        }


        private bool BlogPostExists(int id)
        {
            return (_context.BlogPosts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.IsPublished = true;

            await _blogService.UpdateBlogPostAsync(blogPost);

            return RedirectToAction(nameof(AuthorArea));

        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unpublish(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.IsPublished = false;

            await _blogService.UpdateBlogPostAsync(blogPost);

            return RedirectToAction(nameof(AuthorArea));
        }
    }
}
