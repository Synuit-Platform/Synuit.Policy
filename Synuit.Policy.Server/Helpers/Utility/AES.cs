using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Synuit.Policy.Helpers.Utility
{
   /// <summary>
   /// AES Encryption Helper Class
   /// ---------------------------
   ///
   /// </summary>
   ///
   /// <remarks>
   /// configuration setup:
   ///
   /// "AESConfig":
   /// {
   ///   "KeySize": 128,
   ///   "Key": "dS5PmpVuNIRwvEzn3Qt5CXT8",
   ///   "SlashReplacement": "!",
   ///   "PlusReplacement": "-"
   /// }
   /// </remarks>
   public static class AES
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="data"></param>
      /// <returns></returns>
      public static byte[] Encrypt(byte[] data)
      {
         using (MemoryStream ms = new MemoryStream())
         {
            using (AesManaged cryptor = new AesManaged())
            {
               cryptor.Mode = CipherMode.CBC;
               cryptor.Padding = PaddingMode.PKCS7;
               cryptor.KeySize = Convert.ToByte(Startup.Configuration["AESConfig:KeySize"]);
               cryptor.BlockSize = 128;

               //We use the random generated iv created by AesManaged
               byte[] key = Encoding.UTF8.GetBytes(Startup.Configuration["AESConfig:Key"]);
               byte[] iv = cryptor.IV;

               using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateEncryptor(key, iv), CryptoStreamMode.Write))
               {
                  cs.Write(data, 0, data.Length);
               }
               byte[] encryptedContent = ms.ToArray();

               //Create new byte array that should contain both unencrypted iv and encrypted data
               byte[] result = new byte[iv.Length + encryptedContent.Length];

               //copy our 2 array into one
               System.Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
               System.Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

               return result;
            }
         }
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="data"></param>
      /// <returns></returns>
      public static byte[] Decrypt(byte[] data)
      {
         byte[] key = Encoding.UTF8.GetBytes(Startup.Configuration["AESConfig:Key"]);
         byte[] iv = new byte[16]; //initial vector is 16 bytes
         byte[] encryptedContent = new byte[data.Length - 16]; //the rest should be encryptedcontent

         //Copy data to byte array
         System.Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
         System.Buffer.BlockCopy(data, iv.Length, encryptedContent, 0, encryptedContent.Length);

         using (MemoryStream ms = new MemoryStream())
         {
            using (AesManaged cryptor = new AesManaged())
            {
               cryptor.Mode = CipherMode.CBC;
               cryptor.Padding = PaddingMode.PKCS7;
               cryptor.KeySize = Convert.ToByte(Startup.Configuration["AESConfig:KeySize"]);
               cryptor.BlockSize = 128;

               using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateDecryptor(key, iv), CryptoStreamMode.Write))
               {
                  cs.Write(encryptedContent, 0, encryptedContent.Length);
               }
               return ms.ToArray();
            }
         }
      }
   }
}