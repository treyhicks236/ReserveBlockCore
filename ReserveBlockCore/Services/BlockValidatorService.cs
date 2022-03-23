﻿using ReserveBlockCore.Data;
using ReserveBlockCore.Models;
using ReserveBlockCore.Utilities;

namespace ReserveBlockCore.Services
{
    public class BlockValidatorService
    {
        public static async Task<bool> ValidateBlock(Block block, bool blockDownloads = false)
        {
            bool result = false;

            var badBlocks = BadBlocksUtility.GetBadBlocks();

            if(badBlocks.ContainsKey(block.Height))
            {
                var badBlockHash = badBlocks[block.Height];
                if(badBlockHash == block.Hash)
                {
                    return result;//reject because its on our bad block list
                }
            }

            if (block == null) return result; //null block submitted. reject 

            if (block.Height == 0)
            {
                if (block.ChainRefId != BlockchainData.ChainRef)
                {
                    return result;//block rejected due to chainref difference
                }
                //Genesis Block
                result = true;
                BlockchainData.AddBlock(block);
                StateData.UpdateTreis(block);
                return result;
            }

            var verifyBlockSig = SignatureService.VerifySignature(block.Validator, block.Hash, block.ValidatorSignature);

            //validates the signature of the validator that crafted the block
            if(verifyBlockSig != true)
            {
                return result;//block rejected due to failed validator signature
            }
            //Validates that the block has same chain ref
            if(block.ChainRefId != BlockchainData.ChainRef)
            {
                return result;//block rejected due to chainref difference
            }

            var blockVersion = BlockVersionUtility.GetBlockVersion(block.Height);

            if(block.Version != blockVersion)
            {
                return result;
            }

            var newBlock = new Block {
                Height = block.Height,
                Timestamp = block.Timestamp,
                Transactions = block.Transactions,
                Validator = block.Validator,
                ChainRefId = block.ChainRefId,
                NextValidators = block.NextValidators
            };

            newBlock.Build();

            //This will also check that the prev hash matches too
            if (!newBlock.Hash.Equals(block.Hash))
            {
                return result;//block rejected
            }

            if(!newBlock.MerkleRoot.Equals(block.MerkleRoot))
            {
                return result;//block rejected
            }

            if(block.Height != 0)
            {
                var blockCoinBaseResult = BlockchainData.ValidateBlock(block); //this checks the coinbase tx

                //Need to check here the prev hash if it is correct!

                if (blockCoinBaseResult == false)
                    return result;//block rejected

                if (block.Transactions.Count() > 0)
                {
                    //validate transactions.
                    bool rejectBlock = false;
                    foreach (Transaction transaction in block.Transactions)
                    {
                        if(transaction.FromAddress != "Coinbase_TrxFees" && transaction.FromAddress != "Coinbase_BlkRwd")
                        {
                            var txResult = await TransactionValidatorService.VerifyTX(transaction, blockDownloads);
                            rejectBlock = txResult == false ? rejectBlock = true : false;
                        }
                        else
                        {
                            //do nothing as its the coinbase fee
                        }

                        if (rejectBlock)
                            break;
                    }
                    if (rejectBlock)
                        return result;//block rejected due to bad transaction(s)
                
                
                    result = true;
                    BlockchainData.AddBlock(block);//add block to chain.
                    BlockQueueService.UpdateMemBlocks();//update mem blocks
                    StateData.UpdateTreis(block); 

                    foreach (Transaction transaction in block.Transactions)
                    {
                        var mempool = TransactionData.GetPool();

                        var mempoolTx = mempool.FindAll().Where(x => x.Hash == transaction.Hash);
                        if(mempoolTx.Count() > 0)
                        {
                            mempool.DeleteMany(x => x.Hash == transaction.Hash);
                        }

                        var account = AccountData.GetAccounts().FindOne(x => x.Address == transaction.ToAddress);
                        if(account != null)
                        {
                            AccountData.UpdateLocalBalanceAdd(transaction.ToAddress, transaction.Amount);
                            var txdata = TransactionData.GetAll();
                            txdata.Insert(transaction);
                        }
                    }
                }

                return result;//block accepted
            }
            else
            {
                //Genesis Block
                result = true;
                BlockchainData.AddBlock(block);
                StateData.UpdateTreis(block);
                return result;
            }
            //Need to add validator validation method.
            
        }



    }
}
