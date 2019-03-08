using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

using MongoDB.Bson.Serialization.Attributes;

namespace MS3toMongo
{
    class MS3Data
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("MS3date")]
        public string MS3Date { get; set; }
        [BsonElement("jobname")]
        public string JobName { get; set; }
        [BsonElement("count")]
        public Int32 Count { get; set; }
        [BsonElement("camname")]
        public string camName { get; set; }
        [BsonElement("data")]
        public List<BsonDocument> Data { get; set; }
 
        [BsonElement("date")]
        public DateTime Date { get; set; }

        //{ "date", "jobName", "count", "camName", "Data" }

        public  MS3Data(string MS3date, string jobname, Int32 count, string camname, List<BsonDocument> data, DateTime date)
        {
           
            JobName = jobname;
            MS3Date = MS3date;
            camName = camname; 
            Data = data;
            Count = count;
            Date = date;
        }


    }
}
