using System;
using OnTheFenceDevelopment.PersonalEncryptorCLI.Options;
using CommandLine;
using System.IO;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<GenerateKeyOptions>(args)
                .MapResult(
                    (GenerateKeyOptions opts) => GenerateKeyPair(opts),
                    errs => 1);
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
                
                // TODO: Check for existing Keys in Output location = prompt for overwrite!

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
    }
}
