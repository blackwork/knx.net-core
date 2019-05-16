
namespace KnxCli
{
    public class KnxSettings
    {
        public KnxSettings()
        {
            KnxGatewayPort = 3671;
            KnxLocalPort = 3761;
        }

        public string KnxGatewayIP { get; set; }
        public int KnxGatewayPort { get; set; }

        public int KnxLocalPort { get; set; }
        
    }
}