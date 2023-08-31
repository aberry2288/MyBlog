using MyBlog.Enums;
using MyBlog.Services.Interfaces;

namespace MyBlog.Services
{
    public class ImageService : IImageService 
    {
        private readonly string? _defaultBlogImage = "/img/Sky Image Blog.jpg";
        private readonly string? _defaultUserImage = "/img/DefaultContactImage.png";
        private readonly string? _defaultCatagoryImage = "/img/General Category.jpg";
        private readonly string? _blogAuthorImage = "/img/DefaultContactImage.png";
        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage)
        {
            try
            {
                if (fileData == null || fileData.Length == 0)
                {
                    switch (defaultImage)
                    {
                        case DefaultImage.AuthorImage: return _blogAuthorImage;
                        case DefaultImage.BlogPostImage: return _defaultBlogImage;
                        case DefaultImage.CategoryImage: return _defaultCatagoryImage;
                        case DefaultImage.BlogUserImage: return _defaultUserImage;
                    }


                }

                string? imageBase64Data = Convert.ToBase64String(fileData!);
                imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageBase64Data;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file!.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
