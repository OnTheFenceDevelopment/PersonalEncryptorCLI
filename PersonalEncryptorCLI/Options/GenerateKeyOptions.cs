using CommandLine;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI.Options
{
    [Verb("generatekeys", HelpText = "Generates a Private/Public key pair")]
    public class GenerateKeyOptions
    {
        [Option('k', "keylength", HelpText = "Key Length in bits", Default = 2048)]
        public int KeyLength { get; set; }

        [Option('n',"name", HelpText = "Key name prefix, e.g. <name>PrivateKey.xml and <name>PublicKey.xml")]
        public string Name { get; set; }

        [Option('o', "output", HelpText = "Output path for generated key pair files")]
        public string OutputPath { get; set; }
    }
}
