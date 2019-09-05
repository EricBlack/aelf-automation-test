using AElf.Automation.Common.Contracts;
using AElf.Automation.Common.Helpers;
using AElf.Automation.Common.Managers;
using AElf.Types;

namespace AElf.Automation.SideChainCreate
{
    public class ContractServices
    {
        public readonly INodeManager NodeManager;
        public readonly string Url;
        public GenesisContract GenesisService { get; set; }
        public TokenContract TokenService { get; set; }
        public ConsensusContract ConsensusService { get; set; }
        public CrossChainContract CrossChainService { get; set; }
        public ParliamentAuthContract ParliamentService { get; set; }
        

        public string CallAddress { get; set; }
        public Address CallAccount { get; set; }

        public ContractServices(string url, string callAddress,string password)
        {
            var keyStorePath = CommonHelper.GetCurrentDataDir();
            Url = url;
            NodeManager = new NodeManager(url,keyStorePath);
            CallAddress = callAddress;
            CallAccount = AddressHelper.Base58StringToAddress(callAddress);
            
            NodeManager.UnlockAccount(CallAddress, password);
            GetContractServices();
        }

        public void GetContractServices()
        {
            GenesisService = GenesisContract.GetGenesisContract(NodeManager, CallAddress);

            //TokenService contract
            var tokenAddress = GenesisService.GetContractAddressByName(NameProvider.TokenName);
            TokenService = new TokenContract(NodeManager, CallAddress, tokenAddress.GetFormatted());

            //Consensus contract
            var consensusAddress = GenesisService.GetContractAddressByName(NameProvider.ConsensusName);
            ConsensusService = new ConsensusContract(NodeManager, CallAddress, consensusAddress.GetFormatted());

            //CrossChain contract
            var crossChainAddress = GenesisService.GetContractAddressByName(NameProvider.CrossChainName);
            CrossChainService = new CrossChainContract(NodeManager, CallAddress, crossChainAddress.GetFormatted());
            
            //ParliamentAuth contract
            var parliamentAuthAddress = GenesisService.GetContractAddressByName(NameProvider.ParliamentName);
            ParliamentService =
                new ParliamentAuthContract(NodeManager, CallAddress, parliamentAuthAddress.GetFormatted());
        }

        private void ConnectionChain()
        {
            var ci = new CommandInfo(ApiMethods.GetChainInformation);
            NodeManager.GetChainInformation(ci);
        }
    }
}