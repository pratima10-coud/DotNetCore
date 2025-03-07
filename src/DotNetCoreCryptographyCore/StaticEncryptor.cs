﻿using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DotNetCoreCryptographyCore
{
    public static class StaticEncryptor
    {
        public static async Task EncryptAsync(Stream sourceStream, Stream destinationStream, EncryptionKey key)
        {
            using var encryptor = key.CreateEncryptor(destinationStream);
            using CryptoStream csEncrypt = new(destinationStream, encryptor, CryptoStreamMode.Write);
            await sourceStream.CopyToAsync(csEncrypt).ConfigureAwait(false);
        }

        public static async Task DecryptAsync(Stream encryptedStream, Stream destinationStream, EncryptionKey key)
        {
            using var decryptor = key.CreateDecryptor(encryptedStream);
            using CryptoStream csDecrypt = new(encryptedStream, decryptor, CryptoStreamMode.Read);
            await csDecrypt.CopyToAsync(destinationStream).ConfigureAwait(false);
        }

        public static async Task AesEncryptWithPasswordAsync(Stream sourceStream, Stream destinationStream, string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);
            var aes = Aes.Create();
            using var encryptor = aes.GetEncryptorFromPassword(password, salt);
            //need to write salt unencrypted in final stream
            destinationStream.Write(salt, 0, salt.Length);
            using CryptoStream csEncrypt = new(destinationStream, encryptor, CryptoStreamMode.Write);
            await sourceStream.CopyToAsync(csEncrypt).ConfigureAwait(false);
        }

        public static async Task<byte[]> AesEncryptWithPasswordAsync(byte[] data, string password)
        {
            using var sourceStream = new MemoryStream(data);
            using var destinationStream = new MemoryStream(data.Length);
            await AesEncryptWithPasswordAsync(sourceStream, destinationStream, password).ConfigureAwait(false);
            return destinationStream.ToArray();
        }

        public static async Task AesDecryptWithPasswordAsync(Stream encryptedStream, Stream destinationStream, string password)
        {
            var salt = new byte[16];
            encryptedStream.Read(salt, 0, salt.Length);
            var aes = Aes.Create();
            using var decryptor = aes.GetDecryptorFromPassword(password, salt);
            using CryptoStream csDecrypt = new(encryptedStream, decryptor, CryptoStreamMode.Read);
            await csDecrypt.CopyToAsync(destinationStream).ConfigureAwait(false);
        }

        public static async Task<byte[]> AesDecryptWithPasswordAsync(byte[] encryptedData, string password)
        {
            using var sourceStream = new MemoryStream(encryptedData);
            using var destinationStream = new MemoryStream(encryptedData.Length);
            await AesDecryptWithPasswordAsync(sourceStream, destinationStream, password).ConfigureAwait(false);
            return destinationStream.ToArray();
        }
    }
}
