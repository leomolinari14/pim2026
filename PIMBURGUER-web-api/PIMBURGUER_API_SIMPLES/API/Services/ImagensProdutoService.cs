using System.Text.RegularExpressions;

namespace PimBurguer.Api.Services
{
    public class ImagensProdutoService
    {
        private readonly IConfiguration _configuracao;
        private readonly IWebHostEnvironment _ambiente;

        public ImagensProdutoService(IConfiguration configuracao, IWebHostEnvironment ambiente)
        {
            _configuracao = configuracao;
            _ambiente = ambiente;
        }

        public string Pasta
        {
            get
            {
                return Path.GetFullPath(_configuracao["Pim:ImagensPath"] ?? Path.Combine(_ambiente.WebRootPath, "Uploads"));
            }
        }

        public async Task<string> Salvar(IFormFile arquivo, CancellationToken ct)
        {
            if (arquivo.Length is <= 0 or > 5 * 1024 * 1024)
            {
                throw new RegraException("Escolha uma imagem de até 5 MB.");
            }

            await using var stream = arquivo.OpenReadStream();
            var inicio = new byte[12];
            var lidos = await stream.ReadAtLeastAsync(inicio, 12, false, ct);
            var png = new byte[]
            {
                137,
                80,
                78,
                71,
                13,
                10,
                26,
                10
            };
            string? ext = null;
            if (lidos >= 8 && inicio.AsSpan(0, 8).SequenceEqual(png))
            {
                ext = "png";
            }
            else if (lidos >= 3 && inicio[0] == 255 && inicio[1] == 216 && inicio[2] == 255)
            {
                ext = "jpg";
            }
            else if (lidos >= 12 && System.Text.Encoding.ASCII.GetString(inicio, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(inicio, 8, 4) == "WEBP")
            {
                ext = "webp";
            }

            if (ext is null)
            {
                throw new RegraException("Use uma imagem PNG, JPEG ou WebP válida. SVG e outros formatos não são aceitos.");
            }

            Directory.CreateDirectory(Pasta);
            var nome = Guid.NewGuid().ToString("N") + "." + ext;
            await using var destino = new FileStream(Path.Combine(Pasta, nome), FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await destino.WriteAsync(inicio.AsMemory(0, lidos), ct);
            await stream.CopyToAsync(destino, ct);
            return "/midia/produtos/" + nome;
        }

        public (string Caminho, string Tipo)? Buscar(string nome)
        {
            if (!Regex.IsMatch(nome, @"^[a-f0-9]{32}\.(png|jpg|webp)$"))
            {
                return null;
            }

            var caminho = Path.Combine(Pasta, nome);
            if (!File.Exists(caminho))
            {
                return null;
            }

            return (caminho, Path.GetExtension(nome) switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            });
        }
    }
}
