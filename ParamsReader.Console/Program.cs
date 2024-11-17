using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ParamsReader;
using System.Text;

BenchmarkRunner.Run<Compilation>();

[MemoryDiagnoser]
public class Compilation
{

    [Params("subUserName=18700000000", 
        "subUserName=18700000000&tradeNo=167769&transferNo=BR915RFD&transferStatus=0&transferType=0",
        "amount=50&bankName=工商银行&bankSubbranchName=工商银行深圳支行&charge=20&chargeMode=1&currencyID=CNY&payeeBankcard=6217003380000522687&payeeIdNo=110101200303073279&payeeName=陈珍珍&payeeTel=13163701234&subUserName=18700000000&tradeNo=167769&transferNo=BR915RFD&transferStatus=1&transferType=0",
        "amount=50&bankName=工商银行&bankSubbranchName=工商银行深圳支行&charge=20&chargeMode=1&currencyID=CNY&payeeBankcard=6217003380000522687&payeeIdNo=110101200303073279&payeeName=陈珍珍&payeeTel=13163701234&subUserName=18700000000&tradeNo=167769&transferNo=BR915RFD&transferStatus=1&transferType=0amount=50&bankName=工商银行&bankSubbranchName=工商银行深圳支行&charge=20&chargeMode=1&currencyID=CNY&payeeBankcard=6217003380000522687&payeeIdNo=110101200303073279&payeeName=陈珍珍&payeeTel=13163701234&subUserName=18700000000&tradeNo=167769&transferNo=BR915RFD&transferStatus=1&transferType=0")]
    public string Data { get; set; }


    [Benchmark]
    public string ParseAdvance()
    {
       using var reader = new Reader();

        return reader.ParseToString(Data);
    }

    [Benchmark]
    public string ParseSimpleNoSpan()
    {
        var builder = new StringBuilder();

        var dataArray = Data.Split('&');

        for (int i = 0; i < dataArray.Length; i++)
        {
            var value = dataArray[i].Split('=');

            builder.Append(value[0])
                .Append(':')
                .Append(value[1]);
        }

        return builder.ToString();
    }
}