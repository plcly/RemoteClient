using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RemoteClient.Core
{
    public class EncryptUtils
    {
        #region AES        
        private static readonly string IV = "0123456789abcdef"; //IV偏移量
        /// <summary>
        /// 根据密钥长度自动生成128，192，256位Key
        /// </summary>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        private static byte[] GenerateKey(string key)
        {
            var keyByteArray = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes;
            //根据密钥长度自动设置位数，不足补0，超出32位则截断
            if (keyByteArray.Length <= 16)
            {
                keyBytes = new byte[16];
            }
            else if (keyByteArray.Length > 16 && keyByteArray.Length <= 24)
            {
                keyBytes = new byte[24];
            }
            else
            {
                keyBytes = new byte[32];
            }
            int len = keyByteArray.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            System.Array.Copy(keyByteArray, keyBytes, len);
            return keyBytes;
        }

        private static byte[] GenerateIV(string iv)
        {
            var ivByteArray = Encoding.UTF8.GetBytes(iv);
            byte[] ivBytes = new byte[16];
            int len = ivByteArray.Length;
            if (len > ivBytes.Length)
            {
                len = ivBytes.Length;
            }
            System.Array.Copy(ivByteArray, ivBytes, len);
            return ivBytes;
        }

        /// <summary>
        /// 二进制转16进制
        /// </summary>
        /// <param name="bytes">二进制编码</param>
        /// <returns></returns>
        public static string ByteToHex(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 16进制转二进制
        /// </summary>
        /// <param name="text">16进制字符串</param>
        /// <returns></returns>
        public static byte[] HexToByte(string text)
        {
            int numberChars = text.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(text.Substring(i, 2), 16);
            }
            return bytes;
        }
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainText">待加密字符串</param>
        /// <param name="key">密钥</param>
        /// <param name="mode">AES模式，默认ECB</param>
        /// <param name="iv">偏移量</param>
        /// <param name="padding">填充模式</param>
        /// <returns></returns>
        public static string EncryptStringAES(string plainText, string key, CipherMode mode = CipherMode.ECB, string iv = null, PaddingMode padding = PaddingMode.PKCS7)
        {
            if (string.IsNullOrEmpty(iv))
            {
                iv = IV;
            }
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = mode;
                aes.BlockSize = 128;
                aes.Padding = padding;
                aes.Key = GenerateKey(key);
                aes.IV = GenerateIV(iv);//ECB模式不需要IV
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return ByteToHex(encrypted);
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="encryptedString">待解密16进制字符串</param>
        /// <param name="key">密钥</param>
        /// <param name="mode">AES模式，默认ECB</param>
        /// <param name="iv">偏移量</param>
        /// <param name="padding">填充模式</param>
        /// <returns></returns>
        public static string DecryptStringAES(string encryptedString, string key, CipherMode mode = CipherMode.ECB, string iv = null, PaddingMode padding = PaddingMode.PKCS7)
        {
            if (string.IsNullOrEmpty(iv))
            {
                iv = IV;
            }
            if (encryptedString == null || encryptedString.Length <= 0)
                return String.Empty;
            string plaintext = null;
            var cipherText = HexToByte(encryptedString);
            using (Aes aes = Aes.Create())
            {
                aes.Mode = mode;
                aes.BlockSize = 128;
                aes.Padding = padding;
                aes.Key = GenerateKey(key);
                aes.IV = GenerateIV(iv);//ECB模式不需要IV
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
        #endregion

        #region MD5
        /// <summary>
        /// MD5加密，小写32位
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static string MD5To32(string strs)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                return GetMd5Hash(md5Hash, strs);
            }
        }
        private static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        #endregion
    }
}

