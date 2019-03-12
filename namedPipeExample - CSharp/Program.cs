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

       // static MongoClient client = new MongoClient("mongodb://amsted34:pinttw68@mongotest-shard-00-00-fxdpg.azure.mongodb.net:27017,mongotest-shard-00-01-fxdpg.azure.mongodb.net:27017,mongotest-shard-00-02-fxdpg.azure.mongodb.net:27017/test?ssl=true&replicaSet=Mongotest-shard-0&authSource=admin&retryWrites=true");
       static MongoClient client = new MongoClient("mongodb://localhost:27017"); 
        static IMongoDatabase db = client.GetDatabase("MS3DataDB");
        static IMongoCollection<MS3Data> MS3Collection = db.GetCollection<MS3Data>("DATA");

        //Entry Point
        static  void Main(string[] args)
        {

            UdpClient socket = new UdpClient(27000);
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            Console.ReadKey();
           
        }

      

        public static async Task CreateOne(MS3Data mS3LiveData)
        {
            var task = Task.CurrentId; 
            var st2 = new Stopwatch();
            st2.Start();
           await MS3Collection.InsertOneAsync(mS3LiveData);
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine($"Insert Method : Inserted callback in {st2.Elapsed.TotalMilliseconds}ms \nusing taskNum: {task} \nreturned: \n {mS3LiveData.ToJson()}");
            Console.WriteLine("*******************************************");
            Console.WriteLine("*******************************************");
            Console.WriteLine("*******************************************");

        }

        static void OnUdpData(IAsyncResult result)
        {


            //Mongo Write defs + StopWatch for some diagnostics
            WriteConcern WCValue = new WriteConcern(0);
            client.WithWriteConcern(WCValue);
            var st1 = new Stopwatch();
            st1.Start();
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = socket.EndReceive(result, ref source);

            var sMessage = Encoding.ASCII.GetString(message);
           

            var splitMessage = sMessage.Split(',');
            var data = splitMessage[4].Split(';');

            List<BsonDocument> camRead = new List<BsonDocument>(); 
            
                foreach (var read in data)
            {
                
                var newData = read.TrimEnd().Split('-');
                var bdocument = new BsonDocument(newData[0], newData[1]);
                camRead.Add(bdocument);
            };


            var LISData = new MS3Data(splitMessage[0], splitMessage[1], Convert.ToInt32(splitMessage[2]), splitMessage[3], camRead);

          Task.Factory.StartNew( () =>CreateOne(LISData));

            Console.WriteLine($"UDP EVENT : UDP + Mongo Insert Elapsed: {st1.Elapsed.TotalMilliseconds}ms");
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            
        }



    }





}

