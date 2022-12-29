using Dropbox.Api;
using Dropbox.Api.Files;
using System.Text;

namespace MyNetwork.Models
{
    public static class ImageService
    {
        private static string folder = "images";
        private static DropboxClient dbx = new DropboxClient("sl.BV4vGXaoBGtjts1Ynga2BhuadlA7u0n6rTGg80iKc_wWSm_UsGUq47igvsDLp5Dw4j9Q3JWZ-9pO-qw-pnrgFA1Jq8DBilQUwoJj8XhQwzVU_86Atn0TwEw1TTJFFP7e6RX__aLi1nh7");

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
