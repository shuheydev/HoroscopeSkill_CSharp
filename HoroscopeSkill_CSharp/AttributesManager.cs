using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexa.NET.Request;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace HoroscopeSkill_CSharp
{
    /// <summary>
    /// DynamoDBを扱うクラスをシングルトンとして実装
    /// </summary>
    class AttributesManager
    {
        private static AttributesManager _thisInstance = new AttributesManager();
        //public static AttributesManager Current => _thisInstance;

        private static IAmazonDynamoDB _client = new AmazonDynamoDBClient();
        private static Table _table;
        private static Document _attributes = new Document();
        private static string _userId;
        private static string _tableName;

        private AttributesManager()
        {

        }

        public static AttributesManager Current(string userId)
        {
            _userId = userId;

            return _thisInstance;
        }



        /// <summary>
        /// 同名のテーブルが存在するかをチェックします。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool IsTableExist(string tableName)
        {
            //テーブル一覧を取得
            var tableList = _client.ListTablesAsync().Result;
            //TableNamesプロパティをチェック
            return tableList.TableNames.Exists(s => s.Equals(tableName));
        }

        public void SetPersistentAttributes(string attrName, DynamoDBEntry value)
        {
            _attributes[attrName] = value;
        }

        /// <summary>
        /// データをテーブルに追加します。
        /// </summary>
        public void SavePersistentAttributes()
        {
            var item = new Document();
            item["id"] = _userId;
            item["attributes"] = _attributes;

            var result = _table.PutItemAsync(item).Result;//Wait()じゃだめ？
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Document GetPersistentAttributes()
        {
            var result = _table.GetItemAsync(_userId).Result;

            var attributes = result?["attributes"].AsDocument();

            return attributes;
        }


        /// <summary>
        /// Alexaスキル用のテーブルを作成。
        /// </summary>
        public void CreateTable(string tableName)
        {
            //テーブル存在チェック
            if (!this.IsTableExist(tableName))
            {
                //テーブル作成リクエストの作成
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "id",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "id",
                            KeyType = KeyType.HASH //Partition key
                        },
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 5,
                        WriteCapacityUnits = 5
                    }
                };

                //テーブル作成
                var result = _client.CreateTableAsync(request).Result;
            }


            //テーブル接続
            this.ConnectTable(tableName);
        }

        /// <summary>
        /// テーブルに接続する。
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool ConnectTable(string tableName)
        {
            bool result = true;

            try
            {
                _table = Table.LoadTable(_client, tableName);
                _tableName = tableName;
            }
            catch (Exception e)
            {
                result = false;
                _tableName = "";
            }

            return result;
        }


    }
}
