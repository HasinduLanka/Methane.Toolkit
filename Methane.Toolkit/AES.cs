using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;


namespace Methane.Toolkit
{
    public class AesRij
    {
        public RijndaelManaged rijndael = new RijndaelManaged();
        private readonly System.Text.UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

        private const int CHUNK_SIZE = 128;

        private void InitializeRijndael()
        {
            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.PKCS7;
        }

        public AesRij()
        {
            InitializeRijndael();

            rijndael.KeySize = CHUNK_SIZE;
            rijndael.BlockSize = CHUNK_SIZE;

            rijndael.GenerateKey();
            rijndael.GenerateIV();
        }



        public AesRij(String base64key, String base64iv)
        {
            InitializeRijndael();


            rijndael.Key = Convert.FromBase64String(base64key);
            rijndael.IV = Convert.FromBase64String(base64iv);
        }

        public AesRij(byte[] key, byte[] iv)
        {
            InitializeRijndael();

            rijndael.Key = key;
            rijndael.IV = iv;
        }

        public byte[] Decrypt(byte[] cipher)
        {
            ICryptoTransform transform = rijndael.CreateDecryptor();
            return transform.TransformFinalBlock(cipher, 0, cipher.Length);

        }



        public string DecryptFromBase64String(string base64cipher)
        {
            return Convert.ToBase64String(Decrypt(Convert.FromBase64String(base64cipher)));
        }

        public byte[] EncryptToByte(string plain)
        {
            ICryptoTransform encryptor = rijndael.CreateEncryptor();
            byte[] cipher = unicodeEncoding.GetBytes(plain);
            byte[] encryptedValue = encryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return encryptedValue;
        }
        public byte[] Encrypt(byte[] cipher)
        {
            ICryptoTransform encryptor = rijndael.CreateEncryptor();
            return encryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        }

        public string EncryptToBase64String(string plain)
        {
            return Convert.ToBase64String(EncryptToByte(plain));
        }

        public string GetKey()
        {
            return Convert.ToBase64String(rijndael.Key);
        }

        public string GetIV()
        {
            return Convert.ToBase64String(rijndael.IV);
        }

        public override string ToString()
        {
            return "KEY:" + GetKey() + Environment.NewLine + "IV:" + GetIV();
        }
    }
}
