using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

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
            return $"{_options.Iv} {_options.Key}";
        }

        public string Encrypt(string text)
        {
            using (Aes myRijndael = Aes.Create())
            {
                // ブロックサイズ（何文字単位で処理するか）
                myRijndael.BlockSize = 128;
                // 暗号化方式はAES-256を採用
                myRijndael.KeySize = 256;
                // 暗号利用モード
                myRijndael.Mode = CipherMode.CBC;
                // パディング
                myRijndael.Padding = PaddingMode.PKCS7;

                myRijndael.IV = Encoding.UTF8.GetBytes(_options.Iv);
                myRijndael.Key = Encoding.UTF8.GetBytes(_options.Key);

                // 暗号化
                ICryptoTransform encryptor = myRijndael.CreateEncryptor(myRijndael.Key, myRijndael.IV);

                byte[] encrypted;
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream ctStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(ctStream))
                        {
                            sw.Write(text);
                        }
                        encrypted = mStream.ToArray();
                    }
                }
                // Base64形式（64種類の英数字で表現）で返す
                return (System.Convert.ToBase64String(encrypted));
            }
        }

        public string Decrypt(string cipher)
        {
            using (Aes rijndael = Aes.Create())
            {
                // ブロックサイズ（何文字単位で処理するか）
                rijndael.BlockSize = 128;
                // 暗号化方式はAES-256を採用
                rijndael.KeySize = 256;
                // 暗号利用モード
                rijndael.Mode = CipherMode.CBC;
                // パディング
                rijndael.Padding = PaddingMode.PKCS7;

                rijndael.IV = Encoding.UTF8.GetBytes(_options.Iv);
                rijndael.Key = Encoding.UTF8.GetBytes(_options.Key);

                ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);

                string plain = string.Empty;
                using (MemoryStream mStream = new MemoryStream(System.Convert.FromBase64String(cipher)))
                {
                    using (CryptoStream ctStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(ctStream))
                        {
                            plain = sr!.ReadLine();
                        }
                    }
                }
                return plain;
            }
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
        public string Iv { get; set; }

        /// <summary>
        /// 対称アルゴリズムの共有鍵
        /// </summary>
        public string Key { get; set; }
    }

}
