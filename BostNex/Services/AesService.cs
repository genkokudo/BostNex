using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace BostNex.Services
{
    public interface IAesService
    {
        /// <summary>
        /// 対称鍵暗号を使って文字列を暗号化する
        /// </summary>
        /// <param name="text">暗号化する文字列</param>
        /// <returns>暗号化された文字列</returns>
        public string Encrypt(string text);

        /// <summary>
        /// 対称鍵暗号を使って暗号文を復号する
        /// </summary>
        /// <param name="cipher">暗号化された文字列</param>
        /// <returns>復号された文字列</returns>
        public string Decrypt(string cipher);

        public string Test();
    }

    public class AesService : IAesService
    {
         private readonly AesOption _options;

        public AesService(IOptions<AesOption> options)
        {
            _options = options.Value;
        }

        public string Test()
        {
            using var myRijndael = Aes.Create();
            // ブロックサイズ（何文字単位で処理するか）
            myRijndael.BlockSize = 128;
            // 暗号化方式はAES-256を採用
            myRijndael.KeySize = 256;
            // 暗号利用モード
            myRijndael.Mode = CipherMode.CBC;
            // パディング
            myRijndael.Padding = PaddingMode.PKCS7;

            myRijndael.GenerateIV();
            myRijndael.GenerateKey();

            string IV = Convert.ToBase64String(myRijndael.IV);
            string Key = Convert.ToBase64String(myRijndael.Key);

            return $"{IV} {Key}";
        }

        public string Encrypt(string text)
        {
            using var rijndael = Aes.Create();
            // ブロックサイズ（何文字単位で処理するか）
            rijndael.BlockSize = 128;
            // 暗号化方式はAES-256を採用
            rijndael.KeySize = 256;
            // 暗号利用モード
            rijndael.Mode = CipherMode.CBC;
            // パディング
            rijndael.Padding = PaddingMode.PKCS7;

            rijndael.IV = Convert.FromBase64String(_options.Iv);
            rijndael.Key = Convert.FromBase64String(_options.Key);

            var encryptor = rijndael.CreateEncryptor();

            byte[] encrypted;
            using (var mStream = new MemoryStream())
            {
                using var ctStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write);
                using (var sw = new StreamWriter(ctStream))
                {
                    sw.Write(text);
                }
                encrypted = mStream.ToArray();
            }
            var encryptedString = Convert.ToBase64String(encrypted);
            return encryptedString;
        }

        public string Decrypt(string cipher)
        {
            using var rijndael = Aes.Create();
            rijndael.BlockSize = 128;
            rijndael.KeySize = 256;
            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.PKCS7;

            rijndael.IV = Convert.FromBase64String(_options.Iv);
            rijndael.Key = Convert.FromBase64String(_options.Key);

            var decryptor = rijndael.CreateDecryptor();

            string plain = string.Empty;
            using (var mStream = new MemoryStream(Convert.FromBase64String(cipher)))
            {
                using var ctStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(ctStream);
                plain = sr.ReadLine()!;
            }
            return plain;
        }



    }



    /// <summary>
    /// 設定項目
    /// </summary>
    public class AesOption
    {
        /// <summary>
        /// 対称アルゴリズムの初期ベクター
        /// </summary>
        public string Iv { get; set; } = string.Empty;

        /// <summary>
        /// 対称アルゴリズムの共有鍵
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }

}
