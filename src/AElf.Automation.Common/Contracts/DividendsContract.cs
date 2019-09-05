﻿using AElf.Automation.Common.Helpers;
using AElf.Automation.Common.Managers;

namespace AElf.Automation.Common.Contracts
{
    public enum DividendsMethod
    {
        GetTermDividends,
        GetTermTotalWeights,
        GetAvailableDividends,
        GetAvailableDividendsByVotingInformation,
        CheckStandardDividends,
        CheckStandardDividendsOfPreviousTerm,
        CheckDividends,
        CheckDividendsOfPreviousTerm
    }

    public class DividendsContract : BaseContract<DividendsMethod>
    {
        public DividendsContract(INodeManager nodeManager, string callAddress, string dividendsAddress)
            : base(nodeManager, dividendsAddress)
        {
            SetAccount(callAddress);
        }

        public DividendsContract(INodeManager nodeManager, string callAddress)
            : base(nodeManager, ContractFileName, callAddress)
        {
        }

        public static string ContractFileName => "AElf.Contracts.Dividends";
    }
}