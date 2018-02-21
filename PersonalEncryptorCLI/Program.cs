using CommandLine;
using Newtonsoft.Json;
using OnTheFenceDevelopment.PersonalEncryptorCLI.Options;
using System;
using System.Collections.Generic;
using System.IO;

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
                WriteMessageToConsole($"Generating Keys: Key Length = {opts.KeyLength}, Key Name = {opts.Name}, Output Path = {opts.OutputPath}", ConsoleColor.Green);

                if (FolderExistsOrWasCreated(opts.OutputPath) == false)
                {
                    WriteMessageToConsole("Output Path Invalid - Key Generation Aborted", ConsoleColor.Red);
                    return 1;
                }

                if (KeysExist(opts.Name, opts.OutputPath))
                {
                    bool? overwrite = null;

                    do
                    {
                        WriteMessageToConsole("One or more keys of the same name already exist in the specified location, Overwrite? (Y/N)", ConsoleColor.Red);

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

                WriteMessageToConsole($"Success: Key Pair generated in {opts.OutputPath}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                WriteMessageToConsole($"Exception: {ex.Message}", ConsoleColor.Red);
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

                try
                {
                    File.WriteAllText(opts.OutputPath, serializedPacket);
                    WriteMessageToConsole($"File successfully encrypted to {opts.OutputPath}", ConsoleColor.Green);
                    return 0;
                }
                catch (Exception ex)
                {
                    WriteMessageToConsole(ex.Message, ConsoleColor.Red);
                    return 1;
                }
            }
            else
            {
                WriteMessageToConsole("One or more of the specified files were not found - see above", ConsoleColor.Red);
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

                try
                {
                    // TODO: Currently need to know the file type, e.g. pdf, png or txt, but this is not ideal. Could add a meta-data property to the packet to hold the original filename (encrypted of course) and use that
                    File.WriteAllBytes(opts.OutputPath, decryptedData);
                }
                catch (Exception ex)
                {
                    WriteMessageToConsole(ex.Message, ConsoleColor.Red);
                    return 1;
                }

                WriteMessageToConsole($"File successfully decrypted to {opts.OutputPath}", ConsoleColor.Green);

                return 0;
            }
            else
            {
                WriteMessageToConsole("One or more of the specified files were not found - see above", ConsoleColor.Red);
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

        private static bool FolderExistsOrWasCreated(string folderPath)
        {
            var outcome = false;

            if (Directory.Exists(folderPath))
                return true;

            WriteMessageToConsole($"The specified folder [{folderPath}] does not exist - create it (Y/N)?", ConsoleColor.Red);

            var response = Console.ReadKey();

            while (response.Key != ConsoleKey.Y && response.Key != ConsoleKey.N)
            {
                WriteMessageToConsole("Please enter Y or N", ConsoleColor.Red);
                response = Console.ReadKey();
            }

            if (response.Key == ConsoleKey.N)
                return false;

            try
            {
                var directory = Directory.CreateDirectory(folderPath);
                outcome = true;
            }
            catch (Exception ex)
            {
                WriteMessageToConsole($"Unable to create folder [{folderPath}] - {ex.Message}", ConsoleColor.Red);
                return false;
            }

            return outcome;
        }

        private static void WriteMessageToConsole(string message, ConsoleColor textColour)
        {
            Console.ForegroundColor = textColour;
            Console.WriteLine();
            Console.Write(message);
            Console.WriteLine();
            Console.ResetColor();
        }

        private static bool FilesExist(List<string> filePaths)
        {
            var outcome = true;

            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath) == false)
                {
                    WriteMessageToConsole($"File Not Found: {filePath}", ConsoleColor.Red);
                    outcome = false;
                }
            }

            return outcome;
        }

        #endregion
    }
}
