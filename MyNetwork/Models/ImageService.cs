using Dropbox.Api;
using Dropbox.Api.Files;

namespace MyNetwork.Models
{
    public static class ImageService
    {
        private static string token = "";
        private static string folder = "images";
        private static DropboxClient dbx = new DropboxClient(token);

        public static async Task Upload(string file, byte[] content)
        {
            using (var mem = new MemoryStream(content))
            {
                var updated = await dbx.Files.UploadAsync(
                    "/" + folder + "/" + file,
                    WriteMode.Overwrite.Instance,
                    body: mem);
            }
        }

        public static void setToken(string accessToken)
        {
            token = accessToken;
            dbx = new DropboxClient(token);
        }

        public static string getToken()
        { 
            return token; 
        }

        public static async Task<byte[]> Download(string file)
        {
            using (var response = await dbx.Files.DownloadAsync("/" + folder + "/" + file))
            {
                return await response.GetContentAsByteArrayAsync();
            }
        }

        public static async Task Remove(string file)
        {
            await dbx.Files.DeleteV2Async("/" + folder + "/" + file);
        }

        public static async Task<string> GetImageName(IFormFile image)
        {
            string imgName = "";
            if (image != null && image.ContentType.Contains("image"))
            {
                imgName = DateTime.Now.ToString().Replace('.', '-').Replace(' ', '-').Replace(':', '-').Replace('/', '-') + '.' + image.FileName.Split('.').Last();
                using (var fileStream = image.OpenReadStream())
                {
                    byte[] bytes = new byte[image.Length];
                    fileStream.Read(bytes, 0, (int)image.Length);
                    await Upload(imgName, bytes);
                }
            }
            return imgName;
        }
    }
}
