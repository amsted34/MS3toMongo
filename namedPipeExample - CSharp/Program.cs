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
        //Mongo Initializing - two strings one for Local and the other for remote. 
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

        //Insert Method to MongoDb called by OnUDP line 80 via task.factory.startNew()
        public static async Task CreateOne(MS3Data mS3LiveData)
        {
            var task = Task.CurrentId; 
            var st2 = new Stopwatch();
            st2.Start();
           await MS3Collection.InsertOneAsync(mS3LiveData);
            Console.WriteLine($"---------------------------------------------------------------------------");
            Console.WriteLine($"Insert Method : Inserted callback in {st2.Elapsed.TotalMilliseconds}ms \nusing taskNum: {task} \nreturned: \n {mS3LiveData.ToJson()}");
            Console.WriteLine("**********************************************************************");
            Console.WriteLine("**********************************************************************");
            Console.WriteLine("**********************************************************************");

        }

        static void OnUdpData(IAsyncResult result)
        {

            //Mongo Write defs + StopWatch for some diagnostics
            WriteConcern WCValue = new WriteConcern(0);
            client.WithWriteConcern(WCValue);
            var st1 = new Stopwatch();
            st1.Start();

            //UDP Init
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = socket.EndReceive(result, ref source);

            //get Message in String
            var sMessage = Encoding.ASCII.GetString(message);
           
            //split the message before creating our document to be sent to Mongo
            var splitMessage = sMessage.Split(',');
            var data = splitMessage[4].Split(';');

            //Create a new list for camRead Data
            List<BsonDocument> camRead = new List<BsonDocument>(); 
            
            //formating the split data and adding to the Document List list<bsonDocument>
                foreach (var read in data)
            {
                
                var newData = read.TrimEnd().Split('-');
                var bdocument = new BsonDocument(newData[0], newData[1]);
                camRead.Add(bdocument);
            };

            //create a new instance of the MS3Data object with the relevant information that will make our bson Object
            var LISData = new MS3Data(splitMessage[0], splitMessage[1], Convert.ToInt32(splitMessage[2]), splitMessage[3], camRead);

               // create a task to call createOne method which hold the insert statement to Mongo
          Task.Factory.StartNew( () =>CreateOne(LISData));
            Console.ForegroundColor = ConsoleColor.Green; 
            Console.WriteLine($"-----------------------------------{splitMessage[2]}-------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"UDP EVENT : UDP + Mongo Insert Elapsed: {st1.Elapsed.TotalMilliseconds}ms");
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            
        }



    }





}

