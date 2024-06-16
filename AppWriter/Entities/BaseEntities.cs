using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Entities
{
    [BsonIgnoreExtraElements]
    [Serializable]
    public class BaseEntities
    {
        private string _id;

        public BaseEntities()
        {
        }

        public virtual string Id
        {
            get
            {
                _id = string.IsNullOrEmpty(_id) ? Guid.NewGuid().ToString() : _id;
                return _id;
            }
            set { _id = value; }
        }

        public void GerarNovoCodigo()
        {
            _id = Guid.NewGuid().ToString();
        }
    }
}