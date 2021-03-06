﻿using AElfChain.Common.Managers;

namespace AElfChain.Common.Contracts
{
    public enum ResourceMethod
    {
        //View
        GetElfTokenAddress,
        GetFeeAddress,
        GetResourceControllerAddress,
        GetConverter,
        GetUserBalance,
        GetExchangeBalance,
        GetElfBalance,

        //Action
        Initialize,
        IssueResource,
        BuyResource,
        SellResource,
        LockResource,
        WithdrawResource
    }

    public class ResourceContract : BaseContract<ResourceMethod>
    {
        public ResourceContract(INodeManager nodeManager, string callAddress)
            : base(nodeManager, "AElf.Contracts.Resource", callAddress)
        {
        }

        public ResourceContract(INodeManager nodeManager, string callAddress, string contractAddress) :
            base(nodeManager, contractAddress)
        {
            SetAccount(callAddress);
        }
    }
}