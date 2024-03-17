using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models.Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;

namespace Rocky.Controllers
{
    public class BlogController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILikeRepository _likeRepository;
        public BlogController(IPostRepository postRepository, UserManager<IdentityUser> userManager,
                              IWebHostEnvironment webHostEnvironment, ILikeRepository likeRepository) 
        {
            _postRepository = postRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _likeRepository = likeRepository;
        }


        public IActionResult Index(string searchTerm)
        {
            var posts = _postRepository.GetAll(includeProperties: "ApplicationUser")
            .Select(post => new Post
            {
                Id = post.Id,
                Title = post.Title,
                Text = post.Text,
                ShortText = post.ShortText,
                Type = post.Type,
                Image = post.Image,
                CreatedDate = post.CreatedDate,
                ApplicationUser = post.ApplicationUser,            
                Count = _likeRepository.GetAll(x => x.PostId == post.Id).Count(),
            }).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                posts = posts.Where(p => p.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                         p.Type.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                             .ToList();
            }

            return View(posts);
        }

        [Authorize(Roles = WC.AdminRole)]
        public IActionResult Create()
        {
            var postVM = new PostVM();

            return View(postVM);
        }

        [Authorize(Roles = WC.AdminRole)]
        [HttpPost]
        public IActionResult Create(PostVM postVM)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;
            string upload = webRootPath + WC.ImagePathPosts;
            string fileName = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(files[0].FileName);

            using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
            {
                files[0].CopyTo(fileStream);
            }

            postVM.Post.Image = fileName + extension;
            postVM.Post.CreatedDate = DateTime.Now;
            postVM.Post.ApplicationUserId = _userManager.GetUserId(User)!;

            _postRepository.Add(postVM.Post);
            _postRepository.Save();


            TempData[WC.Success] = "Action completed successfully";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = WC.CastomerRole + "," + WC.AdminRole)]
        public IActionResult Like(int Id)
        {
            var userId = _userManager.GetUserId(User)!;
            var existing = _likeRepository.FirstOrDefault(x => x.ApplicationUserId == userId && x.PostId == Id);

            if (existing != null)
            {
                 _likeRepository.Remove(existing);
            }
            else
            {
                 var like = new Like()
                 {
                     ApplicationUserId = userId,
                     PostId = Id
                 };

                 _likeRepository.Update(like);
            }

            _likeRepository.Save();
            return Redirect(Request.Headers["Referer"].ToString());

        }

        public IActionResult Details(int Id)
        {
            var post = _postRepository.FirstOrDefault(x => x.Id == Id, "ApplicationUser");

            post = new Post
            {
                Id = post.Id,
                Title = post.Title,
                Text = post.Text,
                ShortText = post.ShortText,
                Type = post.Type,
                Image = post.Image,
                CreatedDate = post.CreatedDate,
                ApplicationUser = post.ApplicationUser,
                Count = _likeRepository.GetAll(x => x.PostId == post.Id).Count(),
            };

            return View(post);
        }

        [Authorize(Roles = WC.AdminRole)]
        public IActionResult Edit(int Id)
        {
            PostVM postVM = new PostVM();

            postVM.Post = _postRepository.Find(Id);
            if (postVM == null)
                return NotFound();

            return View(postVM);

        }

        [Authorize(Roles = WC.AdminRole)]
        [HttpPost]
        public IActionResult Edit(PostVM postVM)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;
            var objFromDb = _postRepository.FirstOrDefault(u => u.Id == postVM.Post.Id, isTracking: false);

            if (files.Count > 0)
            {
                string upload = webRootPath + WC.ImagePathPosts;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);

                var oldFile = Path.Combine(upload, objFromDb.Image);

                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                postVM.Post.Image = fileName + extension;
            }
            else
            {
                postVM.Post.Image = objFromDb.Image;
            }

            postVM.Post.ApplicationUserId = objFromDb.ApplicationUserId;
            postVM.Post.CreatedDate = objFromDb.CreatedDate;

            _postRepository.Update(postVM.Post);
            _postRepository.Save();

            TempData[WC.Success] = "Action completed successfully";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = WC.AdminRole)]
        public IActionResult Delete(int Id)
        {
            if (Id <= 0)
                return NotFound();

            var post = _postRepository.Find(Id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int Id)
        {
            var post = _postRepository.Find(Id);
            if (post == null)
            {
                NotFound();
            }

            _postRepository.Remove(post!);
            _postRepository.Save();

            TempData[WC.Success] = "Action completed successfully";
            return RedirectToAction("Index");
        }
    }
}
