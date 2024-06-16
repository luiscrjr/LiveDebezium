using System;
using System.Runtime.Serialization;

namespace Entities.Enums
{
    [Serializable]
    public enum EnumOperacao
    {
        [EnumMember(Value = "Nenhum")]
        Nenhum = 0,
        [EnumMember(Value = "Insert")]
        Inclusao = 1,
        [EnumMember(Value = "Update")]
        Alteracao = 2,
        [EnumMember(Value = "Delete")]
        Exclusao = 3
    }
}