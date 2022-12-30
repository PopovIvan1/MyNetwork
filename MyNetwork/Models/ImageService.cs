using Dropbox.Api;
using Dropbox.Api.Files;
using System.Text;

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

        public static async Task<byte[]> Download(string file)
        {
            using (var response = await dbx.Files.DownloadAsync("/" + folder + "/" + file))
            {
                return await response.GetContentAsByteArrayAsync();
            }
        }

        public static async Task Remove(string file)
        {
            await dbx.Files.DeleteAsync("/" + folder + "/" + file);
        }
    }
}
