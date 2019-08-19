using Acs0;
using Acs3;
using Acs7;
using AElf.Automation.Common.Contracts;
using AElf.Automation.Common.Helpers;
using AElf.Contracts.CrossChain;
using AElf.Contracts.MultiToken;
using AElf.Kernel;
using AElf.Sdk.CSharp;
using AElf.Types;
using AElfChain.SDK.Models;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ApproveInput = Acs3.ApproveInput;

namespace AElf.Automation.SideChainTests
{
    public class ContractTester
    {
        public readonly IApiHelper ApiHelper;
        public readonly ContractServices ContractServices;
        public readonly TokenContract TokenService;
        public readonly ConsensusContract ConsensusService;
        public readonly CrossChainContract CrossChainService;
        public readonly ParliamentAuthContract ParliamentService;

        public ContractTester(ContractServices contractServices)
        {
            ApiHelper = contractServices.ApiHelper;
            ContractServices = contractServices;

            TokenService = ContractServices.TokenService;
            ConsensusService = ContractServices.ConsensusService;
            CrossChainService = ContractServices.CrossChainService;
            ParliamentService = ContractServices.ParliamentService;
        }

        #region cross chain transfer

        public string ValidateTokenAddress()
        {
            var validateTransaction = ApiHelper.GenerateTransactionRawTx(
                ContractServices.CallAddress, ContractServices.GenesisService.ContractAddress,
                GenesisMethod.ValidateSystemContractAddress.ToString(), new ValidateSystemContractAddressInput
                {
                    Address = AddressHelper.Base58StringToAddress(TokenService.ContractAddress),
                    SystemContractHashName = Hash.FromString("AElf.ContractNames.Token")
                });
            return validateTransaction;
        }

        #endregion

        #region side chain create method

        public CommandInfo RequestSideChain(string account, long lockToken)
        {
            ByteString code = ByteString.FromBase64("4d5a90000300");

            CrossChainService.SetAccount(account);
            var result = CrossChainService.ExecuteMethodWithResult(CrossChainContractMethod.RequestChainCreation,
                new SideChainCreationRequest
                {
                    LockedTokenAmount = lockToken,
                    IndexingPrice = 1,
                    ContractCode = code,
                });
            return result;
        }


        public Address GetOrganizationAddress(string account)
        {
            ParliamentService.SetAccount(account);
            var address =
                ParliamentService.CallViewMethod<Address>(ParliamentMethod.GetGenesisOwnerAddress, new Empty());

            return address;
        }

        public CommandInfo CreateSideChainProposal(Address organizationAddress, string account, int indexingPrice,
            long lockedTokenAmount, bool isPrivilegePreserved)
        {
            ByteString code = ByteString.FromBase64("4d5a90000300");
            var createProposalInput = new SideChainCreationRequest
            {
                ContractCode = code,
                IndexingPrice = indexingPrice,
                LockedTokenAmount = lockedTokenAmount,
//                IsPrivilegePreserved = isPrivilegePreserved
            };
            ParliamentService.SetAccount(account);
            var result =
                ParliamentService.ExecuteMethodWithResult(ParliamentMethod.CreateProposal,
                    new CreateProposalInput
                    {
                        ContractMethodName = nameof(CrossChainContractMethod.CreateSideChain),
                        ExpiredTime = TimestampHelper.GetUtcNow().AddDays(1),
                        Params = createProposalInput.ToByteString(),
                        ToAddress = AddressHelper.Base58StringToAddress(CrossChainService.ContractAddress),
                        OrganizationAddress = organizationAddress
                    });

            return result;
        }


        public CommandInfo RequestChainDisposal(string account, int chainId)
        {
            CrossChainService.SetAccount(account);
            var result = CrossChainService.ExecuteMethodWithResult(CrossChainContractMethod.RequestChainDisposal,
                new SInt32Value
                {
                    Value = chainId
                });

            return result;
        }

        public CommandInfo Recharge(string account, int chainId, long amount)
        {
            CrossChainService.SetAccount(account);
            var result =
                CrossChainService.ExecuteMethodWithResult(CrossChainContractMethod.Recharge, new RechargeInput
                {
                    ChainId = chainId,
                    Amount = amount
                });
            return result;
        }

        public SInt32Value GetChainStatus(int chainId)
        {
            var result =
                CrossChainService.CallViewMethod<SInt32Value>(CrossChainContractMethod.GetChainStatus,
                    new SInt32Value {Value = chainId});
            return result;
        }

        public ProposalOutput GetProposal(string proposalId)
        {
            var result =
                ParliamentService.CallViewMethod<ProposalOutput>(ParliamentMethod.GetProposal,
                    HashHelper.HexStringToHash(proposalId));
            return result;
        }

        #endregion

        #region cross chain verify 

        public CommandInfo VerifyTransaction(VerifyTransactionInput input, string account)
        {
            CrossChainService.SetAccount(account);
            var result = CrossChainService.ExecuteMethodWithResult(CrossChainContractMethod.VerifyTransaction, input);
            return result;
        }

        public CrossChainMerkleProofContext GetBoundParentChainHeightAndMerklePathByHeight(string account,
            long blockNumber)
        {
            CrossChainService.SetAccount(account);
            var result = CrossChainService.CallViewMethod<CrossChainMerkleProofContext>(
                CrossChainContractMethod.GetBoundParentChainHeightAndMerklePathByHeight, new SInt64Value
                {
                    Value = blockNumber
                });
            return result;
        }

        #endregion

        #region Parliament Method

        public CommandInfo Approve(string account, string proposalId)
        {
            ParliamentService.SetAccount(account);
            var result = ParliamentService.ExecuteMethodWithResult(ParliamentMethod.Approve, new ApproveInput
            {
                ProposalId = HashHelper.HexStringToHash(proposalId)
            });

            return result;
        }

        public CommandInfo Release(string account, string proposalId)
        {
            ParliamentService.SetAccount(account);
            var transactionResult =
                ParliamentService.ExecuteMethodWithResult(ParliamentMethod.Release,
                    HashHelper.HexStringToHash(proposalId));
            return transactionResult;
        }

        #endregion

        #region Token Method

        //action
        public CommandInfo TransferToken(string owner, string spender, long amount, string symbol)
        {
            TokenService.SetAccount(owner);
            var transfer = TokenService.ExecuteMethodWithResult(TokenMethod.Transfer, new TransferInput
            {
                Symbol = symbol,
                To = AddressHelper.Base58StringToAddress(spender),
                Amount = amount,
                Memo = "Transfer Token"
            });
            return transfer;
        }

        public CommandInfo CreateToken(string issuer, string symbol, string tokenName)
        {
            TokenService.SetAccount(issuer);
            var create = TokenService.ExecuteMethodWithResult(TokenMethod.Create, new CreateInput
            {
                Symbol = symbol,
                Decimals = 2,
                IsBurnable = true,
                Issuer = AddressHelper.Base58StringToAddress(issuer),
                TokenName = tokenName,
                TotalSupply = 100_0000
            });
            return create;
        }

        public CommandInfo IssueToken(string issuer, string symbol, string toAddress)
        {
            TokenService.SetAccount(issuer);
            var issue = TokenService.ExecuteMethodWithResult(TokenMethod.Issue, new IssueInput
            {
                Symbol = symbol,
                Amount = 100_0000,
                Memo = "Issue",
                To = AddressHelper.Base58StringToAddress(toAddress)
            });

            return issue;
        }

        public CommandInfo TokenApprove(string owner, long amount)
        {
            TokenService.SetAccount(owner);

            var result = TokenService.ExecuteMethodWithResult(TokenMethod.Approve,
                new Contracts.MultiToken.ApproveInput
                {
                    Symbol = "ELF",
                    Spender = AddressHelper.Base58StringToAddress(CrossChainService.ContractAddress),
                    Amount = amount,
                });

            return result;
        }

        public CommandInfo CrossChainTransfer(string fromAccount, string toAccount, int toChainId,
            long amount)
        {
            TokenService.SetAccount(fromAccount);
            var result = TokenService.ExecuteMethodWithResult(TokenMethod.CrossChainTransfer,
                new CrossChainTransferInput
                {
                    Amount = amount,
                    Memo = "transfer to side chain",
                    To = AddressHelper.Base58StringToAddress(toAccount),
                    ToChainId = toChainId,
                });

            return result;
        }

        public CommandInfo CrossChainReceive(string account, CrossChainReceiveTokenInput input)
        {
            TokenService.SetAccount(account);
            var result = TokenService.ExecuteMethodWithResult(TokenMethod.CrossChainReceiveToken, input);
            return result;
        }

        //view
        public GetBalanceOutput GetBalance(string account, string symbol)
        {
            var balance = TokenService.CallViewMethod<GetBalanceOutput>(TokenMethod.GetBalance, new GetBalanceInput
            {
                Owner = AddressHelper.Base58StringToAddress(account),
                Symbol = symbol
            });
            return balance;
        }

        public TokenInfo GetTokenInfo(string symbol)
        {
            var tokenInfo = TokenService.CallViewMethod<TokenInfo>(TokenMethod.GetTokenInfo, new GetTokenInfoInput
            {
                Symbol = symbol
            });

            return tokenInfo;
        }

        #endregion

        #region Other Method

        public string ExecuteMethodWithTxId(string rawTx)
        {
            var ci = new CommandInfo(ApiMethods.SendTransaction)
            {
                Parameter = rawTx
            };
            ApiHelper.BroadcastWithRawTx(ci);
            if (ci.Result)
            {
                var transactionOutput = ci.InfoMsg as SendTransactionOutput;

                return transactionOutput?.TransactionId;
            }

            Assert.IsTrue(ci.Result, $"Execute contract failed. Reason: {ci.GetErrorMessage()}");

            return string.Empty;
        }

        #endregion
    }
}