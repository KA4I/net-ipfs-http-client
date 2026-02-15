namespace Ipfs.Http.Client.Tests.CoreApi;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TestClass]
public class BitswapApiTest
{
    [TestMethod]
    [Ignore("API removed")]
    public async Task Wants()
    {
        // var block = new DagNode(Encoding.UTF8.GetBytes("BitswapApiTest unknown block"));
        // Task.Run(() => TestFixture.IpfsContext.Bitswap.GetAsync(block.Id).Wait());

        // var endTime = DateTime.Now.AddSeconds(10);
        // while (DateTime.Now < endTime)
        // {
        //     await Task.Delay(100);
        //     var wants = await TestFixture.IpfsContext.Bitswap.WantsAsync();
        //     if (wants.Contains(block.Id))
        //     {
        //         return;
        //     }
        // }

        // Assert.Fail("wanted block is missing");
        await Task.CompletedTask;
    }

    [TestMethod]
    [Ignore("API removed")]
    public async Task Unwant()
    {
        // var block = new DagNode(Encoding.UTF8.GetBytes("BitswapApiTest unknown block 2"));
        // Task.Run(() => TestFixture.IpfsContext.Bitswap.GetAsync(block.Id).Wait());

        // var endTime = DateTime.Now.AddSeconds(10);
        // while (true)
        // {
        //     if (DateTime.Now > endTime)
        //     {
        //         Assert.Fail("wanted block is missing");
        //     }

        //     await Task.Delay(100);
        //     var wants = await TestFixture.IpfsContext.Bitswap.WantsAsync();
        //     if (wants.Contains(block.Id))
        //     {
        //         break;
        //     }
        // }

        // await TestFixture.IpfsContext.Bitswap.UnwantAsync(block.Id);
        // endTime = DateTime.Now.AddSeconds(10);
        // while (true)
        // {
        //     if (DateTime.Now > endTime)
        //     {
        //         Assert.Fail("unwanted block is present");
        //     }

        //     await Task.Delay(100);
        //     var wants = await TestFixture.IpfsContext.Bitswap.WantsAsync();
        //     if (!wants.Contains(block.Id))
        //     {
        //         break;
        //     }
        // }
        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task Ledger()
    {
        var peer = new Peer { Id = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3" };
        var ledger = await TestFixture.IpfsContext.Bitswap.LedgerAsync(peer);
        Assert.IsNotNull(ledger);
        Assert.AreEqual(peer.Id, ledger.Peer.Id);
    }
}
