using System;
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
            return Parser.Default.ParseArguments<GenerateKeyOptions, EncryptTextOptions>(args)
                .MapResult(
                    (GenerateKeyOptions opts) => GenerateKeyPair(opts),
                    (EncryptTextOptions opts) => EncryptText(opts),
                    errs => 1);
        }

        static int EncryptText(EncryptTextOptions opts)
        {
            var encryptor = new RSAWithRSAParameterKey();
            var digitalSigner = new DigitalSignature();

            var hybrid = new HybridEncryption();

            var foo = hybrid.EncryptData(Encoding.ASCII.GetBytes(opts.Text), opts.KeyBitLength, encryptor, digitalSigner, opts.RecipientKeyPath, opts.SenderKeyPath);

            var serializedPacket = JsonConvert.SerializeObject(foo);

            File.WriteAllText(opts.OutputPath, serializedPacket);

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


                var rsa = new RSAWithRSAParameterKey();

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
