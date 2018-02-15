﻿using System;
using OnTheFenceDevelopment.PersonalEncryptorCLI.Options;
using CommandLine;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<GenerateKeyOptions, EncryptTextOptions, DecryptTextOptions, EncryptFileOptions, DecryptFileOptions>(args)
                .MapResult(
                    (GenerateKeyOptions opts) => GenerateKeyPair(opts),
                    (EncryptTextOptions opts) => EncryptText(opts),
                    (DecryptTextOptions opts) => DecryptText(opts),
                    (EncryptFileOptions opts) => EncryptFile(opts),
                    (DecryptFileOptions opts) => DecryptFileOptions(opts),
                    errs => 1);
        }

        static int EncryptFile(EncryptFileOptions opts)
        {
            // TODO: Need to validate Paths before proceeding
            var encryptor = new RSAEncryption();
            var digitalSigner = new DigitalSignature();

            var hybrid = new HybridEncryption();

            var fileContents = File.ReadAllBytes(opts.FilePath);

            var encryptedPacket = hybrid.EncryptData(fileContents, opts.KeyBitLength, encryptor, digitalSigner, opts.RecipientKeyPath, opts.SenderKeyPath);

            var serializedPacket = JsonConvert.SerializeObject(encryptedPacket);

            File.WriteAllText(opts.OutputPath, serializedPacket);

            return 0;
        }
        static int DecryptFileOptions(DecryptFileOptions opts)
        {
            // TODO: Need to validate Paths before proceeding
            var encryptor = new RSAEncryption();
            var digitalSigner = new DigitalSignature();

            var hybrid = new HybridEncryption();

            var encryptedPacketText = File.ReadAllText(opts.EncryptedPacketPath);
            var encryptedPacket = JsonConvert.DeserializeObject<EncryptedPacket>(encryptedPacketText);

            var decryptedData = hybrid.DecryptData(encryptedPacket, opts.KeyBitLength, encryptor, digitalSigner, opts.RecipientKeyPath, opts.SenderKeyPath);

            // TODO: Currently need to know the file type, e.g. pdf, png or txt, but this is not ideal. Could add a meta-data property to the packet to hold the original filename (encrypted of course) and use that
            File.WriteAllBytes(opts.OutputPath, decryptedData);

            return 0;
        }

        static int EncryptText(EncryptTextOptions opts)
        {
            // TODO: Need to validate Paths before proceeding

            var encryptor = new RSAEncryption();
            var digitalSigner = new DigitalSignature();

            var hybrid = new HybridEncryption();

            var encryptedPacket = hybrid.EncryptData(Encoding.ASCII.GetBytes(opts.Text), opts.KeyBitLength, encryptor, digitalSigner, opts.RecipientKeyPath, opts.SenderKeyPath);

            var serializedPacket = JsonConvert.SerializeObject(encryptedPacket);

            File.WriteAllText(opts.OutputPath, serializedPacket);

            return 0;
        }

        static int DecryptText(DecryptTextOptions opts)
        {
            // TODO: Need to validate Paths before proceeding
            var encryptor = new RSAEncryption();
            var digitalSigner = new DigitalSignature();

            var hybrid = new HybridEncryption();

            var encryptedPacketText = File.ReadAllText(opts.EncryptedPacketPath);
            var encryptedPacket = JsonConvert.DeserializeObject<EncryptedPacket>(encryptedPacketText);

            var decryptedData = hybrid.DecryptData(encryptedPacket, opts.KeyBitLength, encryptor, digitalSigner, opts.RecipientKeyPath, opts.SenderKeyPath);

            var serialisedPlainText = JsonConvert.SerializeObject(Encoding.UTF8.GetString(decryptedData));

            File.WriteAllText(opts.OutputPath, serialisedPlainText);

            return 0;
        }

        static int GenerateKeyPair(GenerateKeyOptions opts)
        {
            try
            {
                Console.WriteLine($"Generating Keys: Key Length = {opts.KeyLength}, Key Name = {opts.Name}, Output Path = {opts.OutputPath}");

                if (string.IsNullOrEmpty(opts.OutputPath) == false)
                {
                    if (Directory.Exists(opts.OutputPath) == false)
                        Directory.CreateDirectory(opts.OutputPath);
                }

                if (KeysExist(opts.Name, opts.OutputPath))
                {
                    bool? overwrite = null;

                    do
                    {
                        Console.WriteLine("One or more keys of the same name already exist in the specified location, Overwrite? (Y/N)");

                        var shouldOverwrite = Console.ReadLine();

                        switch (shouldOverwrite)
                        {
                            case "Y":
                            case "y":
                                overwrite = true;
                                break;
                            case "N":
                            case "n":
                                overwrite = false;
                                break;
                        }

                    } while (overwrite.HasValue == false);

                    if (overwrite.Value == false)
                        return 0;
                }


                var rsa = new RSAEncryption();

                rsa.GenerateKeyPair(opts.KeyLength, opts.Name, opts.OutputPath);

                Console.WriteLine($"Success: Key Pair generated in {opts.OutputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                return 1;
            }

            return 0;
        }

        private static bool KeysExist(string keyName, string outputPath)
        {
            var keysExist = false;
            if (File.Exists(Path.Combine(outputPath, $"{keyName}PublicKey.xml")))
                keysExist = true;

            if (File.Exists(Path.Combine(outputPath, $"{keyName}PrivateKey.xml")))
                keysExist = true;

            return keysExist;
        }
    }
}
