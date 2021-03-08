using System;
using System.Collections.Generic;

namespace ServerProtocol.SharedCode
{
    /// <summary>
    /// 卡牌的花色类型，包括桃心梅方和大小王
    /// </summary>
    public enum CardType
    {
        Spade,//黑桃♠
        Heart,//红心♥
        Club,//梅花♣
        Diamond,//方块♦
        JokerBlack,//小王
        JokerRed,//大王
    }

    /// <summary>
    /// 卡牌的权值，值越小表示它在比较关系中越小
    /// </summary>
    public enum CardWeight
    {
        /// <summary>最小权值，没有对应的牌，作为边界使用</summary>
        wMin,
        w3,
        w4,
        w5,
        w6,
        w7,
        w8,
        w9,
        w10,
        wJ,
        wQ,
        wK,
        wA,
        w2,
        wJokerBlack,
        wJokerRed,
        /// <summary>最大权值，没有对应的牌，作为边界使用</summary>
        wMax,
    }

    /// <summary>
    /// 卡牌的颜色类型，可以通过花色决定
    /// </summary>
    public enum CardColor
    {
        NoColor,
        Black,
        Red,
    }

    /// <summary>
    /// 卡牌类，表示一张卡牌
    /// </summary>
    [Serializable]
    public class Card : IComparable<Card>
    {
        public CardType type { get; private set; }
        public int number { get; private set; }

        /// <summary>
        /// 用于标识这是手牌的第几张
        /// </summary>
        public int handId;

        /// <summary>
        /// 大小，即比较权重
        /// </summary>
        public CardWeight weight
        {
            get
            {
                if (number > 15)
                    return CardWeight.wMax;
                return sortOrder[number];
            }
        }

        private readonly CardWeight[] sortOrder = new CardWeight[16] {
        CardWeight.wMin,
        CardWeight.wA,
        CardWeight.w2,
        CardWeight.w3,
        CardWeight.w4,
        CardWeight.w5,
        CardWeight.w6,
        CardWeight.w7,
        CardWeight.w8,
        CardWeight.w9,
        CardWeight.w10,
        CardWeight.wJ,
        CardWeight.wQ,
        CardWeight.wK,
        CardWeight.wJokerBlack,
        CardWeight.wJokerRed,
    };

        public CardColor color
        {
            get
            {
                switch (this.type)
                {
                    case CardType.Spade:
                    case CardType.Club:
                    case CardType.JokerBlack:
                        return CardColor.Black;
                    case CardType.Heart:
                    case CardType.Diamond:
                    case CardType.JokerRed:
                        return CardColor.Red;
                    default:
                        return CardColor.NoColor;
                }
            }
        }

        public Card(CardType type, int number)
        {
            this.type = type;
            this.number = number;
            if (type == CardType.JokerBlack)
                this.number = 14;
            else if (type == CardType.JokerRed)
                this.number = 15;
            else if (number < 1 || number > 13)
                throw new ArgumentOutOfRangeException("非大小王的卡牌数字不在1~13范围内");
        }

        /// <summary>
        /// 通过ID生成一张卡牌
        /// </summary>
        /// <remarks>
        /// 卡牌id：小王52，大王53，
        /// 黑桃A~K 0~12
        /// 红心A~K 13~25
        /// 梅花A~K 26~38
        /// 方片A~K 39~51
        /// </remarks>
        public Card(int id)
        {

            if (id == 52)
            {
                this.type = CardType.JokerBlack;
                this.number = 14;
            }
            else if (id == 53)
            {
                this.type = CardType.JokerRed;
                this.number = 15;
            }
            else
            {
                this.type = (CardType)(id / 13);
                this.number = id % 13 + 1;
            }
        }

        public const int totalCardCount = 54;

        /// <summary>
        /// 大王
        /// </summary>
        public static Card JokerRed => new Card(53);

        /// <summary>
        /// 小王
        /// </summary>
        public static Card JokerBlack => new Card(52);

        //因为大王小王的数字已经定义了是15和14，
        //所以这里可以直接用number比较
        //这个比较函数用于排序
        public int CompareTo(Card other)
        {
            return number == other.number ? type.CompareTo(other.type) : weight.CompareTo(other.weight);
        }

        public override string ToString()
        {
            if (type == CardType.JokerBlack || type == CardType.JokerRed)
                return type.ToString();
            else
            {
                string numberString;
                switch (weight)
                {
                    case CardWeight.wJ:
                        numberString = "J";
                        break;
                    case CardWeight.wQ:
                        numberString = "Q";
                        break;
                    case CardWeight.wK:
                        numberString = "K";
                        break;
                    case CardWeight.wA:
                        numberString = "A";
                        break;
                    default:
                        numberString = number.ToString();
                        break;
                }
                return type.ToString() + numberString;

            }
        }

        public static List<Card> AllCardList
        {
            get
            {
                List<Card> cardList = new List<Card>();
                for (int i = 0; i < 54; i++)
                {
                    cardList.Add(new Card(i));
                }
                return cardList;
            }
        }
    }

}
