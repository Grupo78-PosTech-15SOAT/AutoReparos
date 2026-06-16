using AutoReparos.Application.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace AutoReparos.Infra.Services
{
    public class AprovacaoTokenService : IAprovacaoTokenService
    {
        private readonly byte[] _secretKey;

        public AprovacaoTokenService(IConfiguration configuration)
        {
            var secret = configuration["AprovacaoToken:Secret"]
                ?? throw new InvalidOperationException("AprovacaoToken:Secret não configurado.");
            _secretKey = Encoding.UTF8.GetBytes(secret);
        }

        public string GerarToken(Guid ordemServicoId)
        {
            var guidBytes = ordemServicoId.ToByteArray();
            var hmac = ComputeHmac(guidBytes);

            var tokenBytes = new byte[guidBytes.Length + hmac.Length];
            Buffer.BlockCopy(guidBytes, 0, tokenBytes, 0, guidBytes.Length);
            Buffer.BlockCopy(hmac, 0, tokenBytes, guidBytes.Length, hmac.Length);

            return Base64UrlEncode(tokenBytes);
        }

        public Guid ValidarToken(string token)
        {
            byte[] tokenBytes;

            try
            {
                tokenBytes = Base64UrlDecode(token);
            }
            catch
            {
                throw new InvalidOperationException("Token de aprovação inválido.");
            }

            if (tokenBytes.Length != 48)
                throw new InvalidOperationException("Token de aprovação inválido.");

            var guidBytes = tokenBytes[..16];
            var hmacRecebido = tokenBytes[16..];
            var hmacEsperado = ComputeHmac(guidBytes);

            if (!CryptographicOperations.FixedTimeEquals(hmacRecebido, hmacEsperado))
                throw new InvalidOperationException("Token de aprovação inválido ou adulterado.");

            return new Guid(guidBytes);
        }

        private byte[] ComputeHmac(byte[] data)
        {
            using var hmac = new HMACSHA256(_secretKey);
            return hmac.ComputeHash(data);
        }

        private static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        private static byte[] Base64UrlDecode(string value)
        {
            var base64 = value 
                .Replace('-', '+')
                .Replace('_', '/');

            var padding = (base64.Length % 4) switch
            {
                2 => "==",
                3 => "=",
                _ => ""
            };

            return Convert.FromBase64String(base64 + padding);
        }
    }
}
