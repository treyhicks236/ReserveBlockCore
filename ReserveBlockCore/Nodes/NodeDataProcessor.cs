﻿using Newtonsoft.Json;
using ReserveBlockCore.Data;
using ReserveBlockCore.Models;
using ReserveBlockCore.P2P;
using ReserveBlockCore.Services;

namespace ReserveBlockCore.Nodes
{
    public class NodeDataProcessor
    {
        public static async Task ProcessData(string message, string data)
        {
            if(Program.PeersConnecting == false)
            {
                if (message == null || message == "")
                {
                    return;
                }
                else
                {
                    if (Program.StopAllTimers == false && Program.BlockCrafting == false) //this will prevent new blocks from coming in if flag. Normally only flagged when syncing chain.
                    {
                        if (message == "tx")
                        {
                            var transaction = JsonConvert.DeserializeObject<Transaction>(data);
                            if (transaction != null)
                            {
                                var isTxStale = await TransactionData.IsTxTimestampStale(transaction);
                                if(!isTxStale)
                                {
                                    var mempool = TransactionData.GetPool();
                                    if (mempool.Count() != 0)
                                    {
                                        var txFound = mempool.FindOne(x => x.Hash == transaction.Hash);
                                        if (txFound == null)
                                        {

                                            var txResult = await TransactionValidatorService.VerifyTX(transaction);
                                            if (txResult == true)
                                            {
                                                var dblspndChk = await TransactionData.DoubleSpendCheck(transaction);
                                                var isCraftedIntoBlock = await TransactionData.HasTxBeenCraftedIntoBlock(transaction);

                                                if (dblspndChk == false && isCraftedIntoBlock == false)
                                                {
                                                    mempool.Insert(transaction);
                                                    P2PClient.SendTXMempool(transaction);
                                                }
                                            }

                                        }
                                        else
                                        {

                                            var isCraftedIntoBlock = await TransactionData.HasTxBeenCraftedIntoBlock(transaction);
                                            if (!isCraftedIntoBlock)
                                            {
                                                //P2PClient.SendTXMempool(transaction); // send to everyone I am connected too (out connects)
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    mempool.DeleteMany(x => x.Hash == transaction.Hash);// tx has been crafted into block. Remove.
                                                }
                                                catch (Exception ex)
                                                {
                                                    //delete failed
                                                }
                                            }                                            
                                        }
                                    }
                                    else
                                    {

                                        var txResult = await TransactionValidatorService.VerifyTX(transaction);
                                        if (txResult == true)
                                        {
                                            var dblspndChk = await TransactionData.DoubleSpendCheck(transaction);
                                            var isCraftedIntoBlock = await TransactionData.HasTxBeenCraftedIntoBlock(transaction);

                                            if (dblspndChk == false && isCraftedIntoBlock == false)
                                            {
                                                mempool.Insert(transaction);
                                                P2PClient.SendTXMempool(transaction);
                                            }
                                        }
                                    }
                                }
                                
                            }

                        }

                        if (message == "val")
                        {
                            await P2PClient.GetMasternodes();
                        }

                        if (message == "blk")
                        {
                            await BlockQueueService.ProcessBlockQueue();

                            var nextBlock = JsonConvert.DeserializeObject<Block>(data);
                            var nextHeight = BlockchainData.GetHeight() + 1;
                            var currentHeight = nextBlock.Height;

                            if (currentHeight < nextHeight)
                            {
                                //already have block
                                var checkBlock = BlockchainData.GetBlockByHeight(currentHeight);

                                if (checkBlock != null)
                                {
                                    var localHash = checkBlock.Hash;
                                    var remoteHash = nextBlock.Hash;

                                    if (localHash != remoteHash)
                                    {
                                        Console.WriteLine("Possible block differ");
                                    }
                                }
                            }
                            else
                            {
                                if (nextHeight == currentHeight)
                                {
                                    var broadcast = await BlockQueueService.AddBlock(nextBlock);

                                    if (broadcast == true)
                                    {
                                        Console.WriteLine("Block was added from: " + nextBlock.Validator);
                                    }
                                }
                                if (nextHeight < currentHeight)
                                {
                                    // means we need to download some blocks
                                    //Check to make sure blocks aren't already being downloaded, so we don't downloaded them multiple times
                                    if (Program.BlocksDownloading == false)
                                    {
                                        Program.BlocksDownloading = true; 
                                        var setDownload = await BlockDownloadService.GetAllBlocks(currentHeight);
                                        Program.BlocksDownloading = setDownload;
                                    }

                                }
                            }

                        }
                    }

                }
            }
            
        }
    }
}
