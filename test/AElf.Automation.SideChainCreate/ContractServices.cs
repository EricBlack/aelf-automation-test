using AElfChain.Common.Contracts;
using AElfChain.Common.Managers;
using AElf.Types;

namespace AElf.Automation.SideChainCreate
{
    public class ContractServices
    {
        public readonly INodeManager NodeManager;
        public GenesisContract GenesisService { get; set; }
        public TokenContract TokenService { get; set; }
        public ConsensusContract ConsensusService { get; set; }
        public CrossChainContract CrossChainService { get; set; }
        public ParliamentAuthContract ParliamentService { get; set; }

        public string CallAddress { get; set; }
        public Address CallAccount { get; set; }

        public ContractServices(string url, string callAddress,string password)
        {
            NodeManager = new NodeManager(url);
            CallAddress = callAddress;
            CallAccount = AddressHelper.Base58StringToAddress(callAddress);
            
            NodeManager.UnlockAccount(CallAddress, password);
            GetContractServices();
        }

        private void GetContractServices()
        {
            GenesisService = GenesisContract.GetGenesisContract(NodeManager, CallAddress);

            //TokenService contract
            TokenService = GenesisService.GetTokenContract();

            //Consensus contract
            ConsensusService = GenesisService.GetConsensusContract();

            //CrossChain contract
            CrossChainService = GenesisService.GetCrossChainContract();
            
            //ParliamentAuth contract
            ParliamentService = GenesisService.GetParliamentAuthContract();
        }
    }
}