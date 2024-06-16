using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Entities.Debezium
{

    public class DebeziumJsonModel
    {
        public DebeziumPayload payload;
        public DebeziumSchema schema;
    }
    public class DebeziumSchema
    {
        public List<DebeziumSchemaFields> fields;
    }

    public class DebeziumSchemaFields
    {
        public string field;
        public List<DebeziumSchemaFieldFields> fields;
    }

    public class DebeziumSchemaFieldFields
    {
        public string field;
        public bool optional;
        public string type;
        public string name;
        public JObject? parameters;
    }

    public class DebeziumPayload
    {
        public string op;
        public JObject? after;
        public JObject? before;
        public DebeziumPayloadSource source;
    }

    public class DebeziumPayloadSource
    {
        public string db;
        public string schema;
        public string table;
        /// <summary>
        /// Nome do Servidor
        /// </summary>
        public string name;
    }

    public class DebeziumKey
    {
        public JObject? payload;
    }
}
