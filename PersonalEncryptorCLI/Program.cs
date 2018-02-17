using System;
using OnTheFenceDevelopment.PersonalEncryptorCLI.Options;
using CommandLine;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<GenerateKeyOptions, EncryptFileOptions, DecryptFileOptions>(args)
                .MapResult(
                    (GenerateKeyOptions opts) => GenerateKeyPair(opts),
                    (EncryptFileOptions opts) => EncryptFile(opts),
                    (DecryptFileOptions opts) => DecryptFileOptions(opts),
                    errs => 1);
        }

        #region Option Methods

        static int GenerateKeyPair(GenerateKeyOptions opts)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Generating Keys: Key Length = {opts.KeyLength}, Key Name = {opts.Name}, Output Path = {opts.OutputPath}");
                Console.WriteLine();
                Console.ResetColor();

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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("One or more keys of the same name already exist in the specified location, Overwrite? (Y/N)");
                        Console.ResetColor();

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

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Success: Key Pair generated in {opts.OutputPath}");
                Console.WriteLine();
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Exception: {ex.Message}");
                Console.ResetColor();

                return 1;
            }

            return 0;
        }

        static int EncryptFile(EncryptFileOptions opts)
        {
            if (FilesExist(new List<string> { opts.FilePath, opts.RecipientKeyPath, opts.SenderKeyPath }))
            {

                var fileContents = File.ReadAllBytes(opts.FilePath);
                var encryptedPacket = Encrypt(fileContents, opts.KeyBitLength, opts.RecipientKeyPath, opts.SenderKeyPath);

                var serializedPacket = JsonConvert.SerializeObject(encryptedPacket);

                File.WriteAllText(opts.OutputPath, serializedPacket);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine($"File successfully encrypted to {opts.OutputPath}");
                Console.WriteLine();
                Console.ResetColor();

                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("One or more of the specified files could not be found");
                Console.WriteLine();
                Console.ResetColor();

                return 1;
            }
        }

        static int DecryptFileOptions(DecryptFileOptions opts)
        {
            if (FilesExist(new List<string> { opts.EncryptedPacketPath, opts.RecipientKeyPath, opts.SenderKeyPath }))
            {
                var encryptedPacketText = File.ReadAllText(opts.EncryptedPacketPath);
                var encryptedPacket = JsonConvert.DeserializeObject<EncryptedPacket>(encryptedPacketText);

                var decryptedData = Decrypt(encryptedPacket, opts.KeyBitLength, opts.SenderKeyPath, opts.RecipientKeyPath);

                // TODO: Currently need to know the file type, e.g. pdf, png or txt, but this is not ideal. Could add a meta-data property to the packet to hold the original filename (encrypted of course) and use that
                File.WriteAllBytes(opts.OutputPath, decryptedData);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine($"File successfully decrypted to {opts.OutputPath}");
                Console.WriteLine();
                Console.ResetColor();

                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("One or more of the specified files could not be found");
                Console.WriteLine();
                Console.ResetColor();

                return 1;
            }
        }        

        #endregion
        
        #region Action Methods

        private static EncryptedPacket Encrypt(byte[] dataToEncrypt, int keyLength, string recipientPublicKeyPath, string sendersPrivateKeyPath)
        {
            var encryptor = new RSAEncryption();
            var digitalSigner = new DigitalSignature();

            var hybrid = new HybridEncryption();

            var encryptedPacket = hybrid.EncryptData(dataToEncrypt, keyLength, encryptor, digitalSigner, recipientPublicKeyPath, sendersPrivateKeyPath);

            return encryptedPacket;
        }

        private static byte[] Decrypt(EncryptedPacket encryptedPacket, int keyLength, string sendersPublicKeyPath, string recipientsPrivateKeyPath)
        {
            var encryptor = new RSAEncryption();
            var digitalSigner = new DigitalSignature();

            var hybrid = new HybridEncryption();

            var decryptedData = hybrid.DecryptData(encryptedPacket, keyLength, encryptor, digitalSigner, recipientsPrivateKeyPath, sendersPublicKeyPath);

            return decryptedData;
        }

        #endregion

        #region Helper Methods

        private static bool KeysExist(string keyName, string outputPath)
        {
            var keysExist = false;
            if (File.Exists(Path.Combine(outputPath, $"{keyName}PublicKey.xml")))
                keysExist = true;

            if (File.Exists(Path.Combine(outputPath, $"{keyName}PrivateKey.xml")))
                keysExist = true;

            return keysExist;
        }

        private static bool FilesExist(List<string> filePaths)
        {
            var outcome = true;

            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath) == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine($"File Not Found: {filePath}");
                    Console.WriteLine();
                    Console.ResetColor();
                    outcome = false;
                }
            }

            return outcome;
        }

        #endregion
    }
}
