using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Driver;


namespace MS3toMongo
{
    class Program
    {

        static MongoClient client = new MongoClient("mongodb://amsted34:pinttw68@mongotest-shard-00-00-fxdpg.azure.mongodb.net:27017,mongotest-shard-00-01-fxdpg.azure.mongodb.net:27017,mongotest-shard-00-02-fxdpg.azure.mongodb.net:27017/test?ssl=true&replicaSet=Mongotest-shard-0&authSource=admin&retryWrites=true");
        static IMongoDatabase db = client.GetDatabase("MS3DataDB");
        static IMongoCollection<MS3Data> MS3Collection = db.GetCollection<MS3Data>("DATA");

        //Entry Point
        static void Main(string[] args)
        {

            UdpClient socket = new UdpClient(27000);
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            Console.ReadKey();

        }

        public static async Task<string> CreateOne(MS3Data mS3LiveDataCollection)
        {
            await MS3Collection.InsertOneAsync(mS3LiveDataCollection);
            return "inserted";
        }

        static void OnUdpData(IAsyncResult result)
        {
            WriteConcern WCValue = new WriteConcern(1);
            client.WithWriteConcern(WCValue);

            var st1 = new Stopwatch();
            st1.Start();
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = socket.EndReceive(result, ref source);

            var sMessage = Encoding.ASCII.GetString(message);
           

            var splitMessage = sMessage.Split(',');
            var data = splitMessage[4].Split(';');

            List<BsonDocument> dataList = new List<BsonDocument>(); 
            
                foreach (var read in data)
            {
                var newData = read.Split('-');
                var bdocument = new BsonDocument(newData[0], newData[1]);
                dataList.Add(bdocument);
            };


            var LISData = new MS3Data(splitMessage[0], splitMessage[1], Convert.ToInt32(splitMessage[2]), splitMessage[3], dataList, DateTime.Now);

            CreateOne(LISData);

            Console.WriteLine(st1.Elapsed.TotalMilliseconds);
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            
        }



    }





}

