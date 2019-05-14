﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using AElf.Automation.Common.Helpers;
using McMaster.Extensions.CommandLineUtils;

namespace AElf.Automation.SideChain.Verification.Test
{
    [Command(Name = "Transaction Client", Description = "Monitor contract transaction testing client.")]
    [HelpOption("-?")]
    class Program
    {
        #region Parameter Option

        [Option("-ruM|--mainRpc.url", Description = "Rpc service url of node. It's required parameter.")]
        public string MainUrl { get; }
        
        [Option("-ruS1|--side1Rpc.url", Description = "Rpc service url of node. It's required parameter.")]
        public string SideUrl1 { get; }
        
        [Option("-ruS2|--side2Rpc.url", Description = "Rpc service url of node. It's required parameter.")]
        public string SideUrl2 { get; }
        
        [Option("-ac|--chain.account", Description = "Main Chain account, It's required parameter.")]
        public static string InitAccount { get; }
        
        [Option("-em|--execute.mode", Description =
            "Transaction execution mode include: \n0. Not set \n1. Verify main chain transaction \n2. Verify side chain transaction ")]
        public int ExecuteMode { get; } = 0;
        
        public int ThreadCount { get; } = 1;
        public int TransactionGroup { get; } = 10;

        #endregion
        
        
        public static List<string> SideUrls { get; set; }
        private static readonly ILogHelper Logger = LogHelper.GetLogHelper();

        public static int Main(string[] args)
        {
            if (args.Length != 4) return CommandLineApplication.Execute<Program>(args);
            
            var ruM = args[0];
            var ruS1 = args[1];
            var ruS2 = args[2];
            var ac = args[3];
            var em = args[4];
            args = new[] {"-ruM",ruM,"-ruS1",ruS1,"-ruS2",ruS2, "-ac",ac, "-em", "0"};

            return CommandLineApplication.Execute<Program>(args);
        }
        
        private void OnExecute(CommandLineApplication app)
        {
            if (MainUrl == null)
            {
                app.ShowHelp();
                return;
            }
            SideUrls = new List<string>();
            SideUrls.Add(SideUrl1);
            SideUrls.Add(SideUrl2);

            var operationSet = new OperationSet(ThreadCount,TransactionGroup,InitAccount,SideUrls,MainUrl);
            
            //Init Logger
            var logName = "RpcTh_" + operationSet.ThreadCount + "_Tx_" + operationSet.ExeTimes +"_"+ DateTime.Now.ToString("MMddHHmmss") + ".log";
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", logName);
            Logger.InitLogHelper(dir);
            
            //Execute transaction command
            try
            {
                operationSet.InitMainExecCommand();
                
                ExecuteOperation(operationSet,ExecuteMode);
            }
            catch (Exception e)
            {
                Logger.WriteError("Message: " + e.Message);
                Logger.WriteError("Source: " + e.Source);
                Logger.WriteError("StackTrace: " + e.StackTrace);
            }
            finally
            {
                //Delete accounts
                operationSet.DeleteAccounts();
            }

            //Result summary
            var set = new CategoryInfoSet(operationSet.MainChain.ApiHelper.CommandList);
            set.GetCategoryBasicInfo();
            set.GetCategorySummaryInfo();
            var xmlFile = set.SaveTestResultXml(operationSet.ThreadCount, operationSet.ExeTimes);
            Logger.WriteInfo("Log file: {0}", dir);
            Logger.WriteInfo("Xml file: {0}", xmlFile);
            Logger.WriteInfo("Complete performance testing.");
        }
        
        private static void ExecuteOperation(OperationSet operationSet, int execMode = 0)
        {
            if (execMode == 0)
            {
                Logger.WriteInfo("Select execution type:");
                Console.WriteLine("1. Verify main chain transaction");
                Console.WriteLine("2. Verify side chain transaction");
                Console.WriteLine("3. Cross Chain Transfer");
                Console.Write("Input selection:");

                var runType = Console.ReadLine();
                var check = int.TryParse(runType, out execMode);
                if (!check)
                {
                    Logger.WriteInfo("Wrong input, please input again.");
                    ExecuteOperation(operationSet);
                }
            }

            var tm = (TestMode) execMode;
            switch (tm)
            {
                case TestMode.VerifyMainTx:
                    Logger.WriteInfo($"Run with verify main chain transaction: {tm.ToString()}.");
                    operationSet.MainChainTransactionVerifyOnSideChains();
                    break;
                case TestMode.VerifySideTx:
                    Logger.WriteInfo($"Run with verify side chain transaction: {tm.ToString()}.");
                    Console.Write("Input the side number: ");
                    var sideChainNum = Console.ReadLine();
                    var num = int.Parse(sideChainNum);
                    if (num > SideUrls.Count+1)
                    {
                        Logger.WriteInfo("Wrong input, please input again.");
                        ExecuteOperation(operationSet);
                    }
                    operationSet.SideChainTransactionVerifyOnMainChain(num-1);
                    break;
                case TestMode.CrossChainTransfer:
                    Logger.WriteInfo($"Run with cross chain transfer: {tm.ToString()}."); 
                    operationSet.CrossChainTransferToInitAccount();
                    operationSet.MultiCrossChainTransferFromMainChain();
//                    operationSet.MultiSideChainCrossChainTransfer();
                    break;
                case TestMode.NotSet:
                    break;
                default:
                    Logger.WriteInfo("Wrong input, please input again.");
                    ExecuteOperation(operationSet);
                    break;
            }
        }
    }
}