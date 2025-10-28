using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingCard.GamePlay.Model.Message
{
    public enum TableStateType
    {
        Start,
        End,
        Exit
    }

    public struct TableStateMessage
    {
        public TableStateType Type;

        public TableStateMessage(TableStateType type)
        {
            Type = type;
        }
    }
}
