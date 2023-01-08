using Dropbox.Api;
using Dropbox.Api.Files;

namespace MyNetwork.Services
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
            foreach (var img in file.Split(' '))
            {
                if (img != "")
                    await dbx.Files.DeleteV2Async("/" + folder + "/" + img);
            }
        }

        public static async Task<string> GetImageName(IFormFile[] images)
        {
            string imgName = "";
            if (images.Length > 0)
            {
                int index = 0;
                foreach (var image in images)
                {
                    if (image.ContentType.Contains("image"))
                    {
                        string curImgName = index.ToString() + '_' + DateTime.Now.ToString().Replace('.', '-').Replace(' ', '-').Replace(':', '-').Replace('/', '-') + '.' + image.FileName.Split('.').Last();
                        imgName += curImgName + ' ';
                        using (var fileStream = image.OpenReadStream())
                        {
                            byte[] bytes = new byte[image.Length];
                            fileStream.Read(bytes, 0, (int)image.Length);
                            await Upload(curImgName, bytes);
                        }
                        index++;
                    }
                }
            }
            return imgName;
        }
    }
}
