using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.SharedCode
{
    /// <summary>
    /// 出牌的类型
    /// </summary>
    public enum CardSetType
    {
        /// <summary>
        /// 不合法的牌型
        /// </summary>
        Invalid = -1,
        ///<summary>无牌型</summary>
        ///<remarks>没人要牌或者开始时会出现</remarks>
        None,
        ///<summary>王炸</summary>
        JokerBomb,
        ///<summary>炸</summary>
        Bomb,
        ///<summary>单张</summary>
        Single,
        ///<summary>对子</summary>
        Pair,
        ///<summary>三张</summary>
        Triple,
        ///<summary>三带一</summary>
        TripleWithOne,
        ///<summary>三带二</summary>
        TripleWithPair,
        ///<summary>顺子</summary>
        Straight,
        ///<summary>双顺（连对）</summary>
        DoubleStraight,
        ///<summary>三顺</summary>
        TripleStraight,
        ///<summary>三顺带1（飞机）</summary>
        TripleStraightWithOne,
        ///<summary>三顺带2（飞机带对子）</summary>
        TripleStraightWithPair,
        ///<summary>四带二</summary>
        QuadraWithTwo,
        ///<summary>四带两对</summary>
        QuadraWithTwoPairs,
    }

    /// <summary>
    /// 牌型，又可以说是一手牌，每次出牌使用这个类
    /// </summary>
    [Serializable]
    public class CardSet : IComparable<CardSet>
    {
        public CardSetType Type { get; private set; }

        /// <summary>
        /// 权值，表示这种牌的大小
        /// </summary>
        /// <remarks>这里使用牌型的第一比较关键字里最大的牌，其实就是最多的一种牌中最大的一种</remarks>
        /// <example>顺子34567的权值是7，三带二33399的权值是3</example>
        public int KeyNumber { get; private set; }


        ///// <summary>
        ///// 副权值，第二比较关键字，暂时用不上
        ///// </summary>
        //int subKeyNumber;

        /// <summary>
        /// 该牌型最小基础单元重复的次数，如果是定长牌型设置为1。
        /// 对于定长牌型，此属性无效。
        /// 不同重复次数的牌型无法比较。
        /// </summary>
        /// <example>
        /// 顺子34567的次数是5，连对55667788的次数是4，飞机JJJQQQ8844的次数是2，
        /// 一对22的次数是1（像一对这样的牌型长度无法变化，所以规定为1）
        /// </example>
        public int RepeatCount { get; private set; }

        /// <summary>
        /// 包含的牌
        /// </summary>
        public Card[] Cards { get; private set; }

        public CardSet() { }

        public CardSet(CardSetType setType, int keyNumber, int repeatCount, Card[] cards)
        {
            this.Type = setType;
            this.KeyNumber = keyNumber;
            this.RepeatCount = repeatCount;
            this.Cards = cards;
        }

        private int GetCardSetTypeWeight(CardSetType setType)
        {
            switch (setType)
            {
                case CardSetType.Invalid:
                case CardSetType.None:
                default:
                    //无效牌型，最小
                    return -1;
                case CardSetType.JokerBomb:
                    //王炸，最大
                    return 2;
                case CardSetType.Bomb:
                    //普通炸弹
                    return 1;
                case CardSetType.Single:
                case CardSetType.Pair:
                case CardSetType.Triple:
                case CardSetType.TripleWithOne:
                case CardSetType.TripleWithPair:
                case CardSetType.Straight:
                case CardSetType.DoubleStraight:
                case CardSetType.TripleStraight:
                case CardSetType.TripleStraightWithOne:
                case CardSetType.TripleStraightWithPair:
                case CardSetType.QuadraWithTwo:
                case CardSetType.QuadraWithTwoPairs:
                    //普通牌型
                    return 0;
            }
        }

        /// <summary>
        /// 这个牌型能否压住另一个牌型？
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsGreaterThan(CardSet other)
        {
            if (Type == other.Type)
            {
                return KeyNumber > other.KeyNumber;
            }

            int thisWeight = GetCardSetTypeWeight(Type);
            int otherWeight = GetCardSetTypeWeight(other.Type);
            return thisWeight > otherWeight;
        }

        #region 重写和实现接口
        /// <summary>
        /// 排序比较，在类型相同时可以用于大小比较
        /// </summary>
        public int CompareTo(CardSet other)
        {
            return Type == other.Type ? KeyNumber.CompareTo(other.KeyNumber) : Type.CompareTo(other.Type);
        }

        public override string ToString()
        {
            return string.Format("{0}<repeat{1},key{2}>:{3}",
                                 Type.ToString(),
                                 RepeatCount.ToString(),
                                 KeyNumber.ToString(),
                                 IterToString(Cards));
        }
        #endregion
        #region Card[]转换
        public static CardSetType GetCardSetType(Card[] cards)
        {
            int differentCards;
            List<(int cnt, int weight)> handData = GetHandDataCW(cards);
            differentCards = handData.Count;


            Card[] cardsSorted = cards.Clone() as Card[];
            Array.Sort(cardsSorted);

            if (IsSingle())
                return CardSetType.Single;
            else if (IsPair())
                return CardSetType.Pair;
            else if (IsTriple())
                return CardSetType.Triple;
            else if (IsBomb())
                return CardSetType.Bomb;
            else if (IsJokerBomb())
                return CardSetType.JokerBomb;
            else if (IsTripleWithOne())
                return CardSetType.TripleWithOne;
            else if (IsTripleWithPair())
                return CardSetType.TripleWithPair;
            else if (IsQuadraWithTwo())
                return CardSetType.QuadraWithTwo;
            else if (IsQuadraWithTwoPairs())
                return CardSetType.QuadraWithTwoPairs;
            else if (IsStraight())
                return CardSetType.Straight;
            else if (IsDoubleStraight())
                return CardSetType.DoubleStraight;
            else if (IsTripleStraight())
                return CardSetType.TripleStraight;
            else if (IsTripleStraightWithOne())
                return CardSetType.TripleStraightWithOne;
            else if (IsTripleStraightWithPair())
                return CardSetType.TripleStraightWithPair;

            return CardSetType.Invalid;

            bool IsSingle()
            {
                return cardsSorted.Length == 1;
            }

            bool IsPair()
            {
                return cardsSorted.Length == 2 && differentCards == 1;
            }

            bool IsTriple()
            {
                return cardsSorted.Length == 3 && differentCards == 1;
            }

            bool IsBomb()
            {
                return cardsSorted.Length == 4 && differentCards == 1;
            }

            bool IsJokerBomb()
            {
                return cardsSorted.Length == 2
                    && cardsSorted[0].type == CardType.JokerBlack
                    && cardsSorted[1].type == CardType.JokerRed;
            }

            bool IsTripleWithOne()
            {
                return handData.Count == 2 && handData[0].cnt == 1 && handData[1].cnt == 3;
            }

            bool IsTripleWithPair()
            {
                return handData.Count == 2 && handData[0].cnt == 2 && handData[1].cnt == 3;
            }

            bool IsQuadraWithTwo()
            {
                return handData.Count == 3 && handData[0].cnt == 1 && handData[1].cnt == 1 && handData[2].cnt == 4;
            }

            bool IsQuadraWithTwoPairs()
            {
                return handData.Count == 3 && handData[0].cnt == 2 && handData[1].cnt == 2 && handData[2].cnt == 4;
            }

            bool IsStraight()
            {
                //顺子至少5张牌
                if (handData.Count < 5)
                    return false;

                //顺子只能包含3~A
                if (cardsSorted[0].weight < CardWeight.w3 || cardsSorted[cardsSorted.Length - 1].weight > CardWeight.wA)
                    return false;

                //顺子每张应该连续
                for (int i = 1; i < handData.Count; i++)
                {
                    if (handData[i].weight != handData[i - 1].weight + 1)
                        return false;
                }

                //顺子每样只有一张
                //handData按张数排序
                if (handData[0].cnt != 1 || handData[handData.Count - 1].cnt != 1)
                    return false;

                return true;
            }

            //和上面的顺子基本一样，代码是复制改的
            bool IsDoubleStraight()
            {
                if (handData.Count < 3)
                    return false;

                if (cardsSorted[0].weight < CardWeight.w3 || cardsSorted[cardsSorted.Length - 1].weight > CardWeight.wA)
                    return false;

                for (int i = 1; i < handData.Count; i++)
                {
                    if (handData[i].weight != handData[i - 1].weight + 1)
                        return false;
                }

                if (handData[0].cnt != 2 || handData[handData.Count - 1].cnt != 2)
                    return false;

                return true;
            }

            //和上面的顺子基本一样，代码是复制改的
            bool IsTripleStraight()
            {
                if (handData.Count < 2)
                    return false;

                if (cardsSorted[0].weight < CardWeight.w3 || cardsSorted[cardsSorted.Length - 1].weight > CardWeight.wA)
                    return false;

                for (int i = 1; i < handData.Count; i++)
                {
                    if (handData[i].weight != handData[i - 1].weight + 1)
                        return false;
                }

                if (handData[0].cnt != 3 || handData[handData.Count - 1].cnt != 3)
                    return false;

                return true;
            }

            bool IsTripleStraightWithOne()
            {
                //飞机两个起飞
                if (handData.Count < 4)
                    return false;

                //种类数要是偶数
                if (handData.Count % 2 != 0)
                    return false;

                int halfCount = handData.Count / 2;

                //前半，是被带的
                for (int i = 0; i < halfCount; i++)
                {
                    if (handData[i].cnt != 1)
                        return false;
                }

                //后半，是带的，需要3个一组
                for (int i = halfCount; i < handData.Count; i++)
                {
                    if (handData[i].cnt != 3)
                        return false;

                    //三顺部分需要连续
                    if (i > halfCount && handData[i - 1].weight + 1 != handData[i].weight)
                        return false;
                }

                return true;
            }

            //和上面的飞机基本一样，代码是复制改的
            bool IsTripleStraightWithPair()
            {
                //飞机两个起飞
                if (handData.Count < 4)
                    return false;

                //种类数要是偶数
                if (handData.Count % 2 != 0)
                    return false;

                int halfCount = handData.Count / 2;

                //前半，是被带的
                for (int i = 0; i < halfCount; i++)
                {
                    if (handData[i].cnt != 2)
                        return false;
                }

                //后半，是带的，需要3个一组
                for (int i = halfCount; i < handData.Count; i++)
                {
                    if (handData[i].cnt != 3)
                        return false;

                    //三顺部分需要连续
                    if (i > halfCount && handData[i - 1].weight + 1 != handData[i].weight)
                        return false;
                }

                return true;
            }

        }

        /// <summary>
        /// 这些牌能组成哪一种牌型？
        /// </summary>
        /// <param name="cards"></param>
        /// <returns>能组成的牌型，无效返回invalid牌型</returns>
        public static CardSet GetCardSet(Card[] cards)
        {
            CardSetType setType = GetCardSetType(cards);
            List<(int cnt, int weight)> handData = GetHandDataCW(cards);
            CardSet result = new CardSet();
            result.Type = setType;
            result.Cards = cards;
            result.KeyNumber = handData[handData.Count - 1].weight; //所有牌型的key都是最多的一种牌中最大的一种

            //分情况可以不用写for暴力，但是代码就短多了，其实可读性差不多
            switch (setType)
            {
                case CardSetType.Invalid:
                case CardSetType.None:
                default:
                    result.KeyNumber = (int)CardWeight.wMin; //这些是无效的牌型
                    result.RepeatCount = 0;
                    break;
                case CardSetType.JokerBomb:
                    result.KeyNumber = (int)CardWeight.wJokerRed; //王炸的key是大王
                    result.RepeatCount = 1;
                    break;
                case CardSetType.Bomb:
                case CardSetType.Single:
                case CardSetType.Pair:
                case CardSetType.Triple:
                case CardSetType.TripleWithOne:
                case CardSetType.TripleWithPair:
                case CardSetType.QuadraWithTwo:
                case CardSetType.QuadraWithTwoPairs:
                    result.RepeatCount = 1; //这些牌型无重复部分，因为它们的长度是固定的
                    break;
                case CardSetType.Straight:
                case CardSetType.DoubleStraight:
                case CardSetType.TripleStraight:
                    result.RepeatCount = handData.Count; //这些牌型的重复数等于种类数（最小单元为1种牌）
                    break;
                case CardSetType.TripleStraightWithOne:
                case CardSetType.TripleStraightWithPair:
                    result.RepeatCount = handData.Count / 2; //这些牌型的重复数等于种类数的一半（最小单元为2种牌）
                    break;
            }

            return result;
        }

        #endregion
        #region 工具函数
        //to be improve GetHandDataCW GetHandDataWC GetCntDict这三个函数已过时
        //但是CardSet大量用到了（历史原因，它在CardHand之前写完的），重构时请使用CardHand类的相关函数

        /// <summary>
        /// 返回处理过的一组卡牌数据，wc指元组中顺序为Weight，Count
        /// </summary>
        /// <param name="cards"></param>
        /// <returns>一个升序排列的列表，排序关键字为(cnt,然后weight)，cnt:这种牌有几张，weight:这种牌的Card.weight，</returns>
        private static List<(int cnt, int weight)> GetHandDataCW(Card[] cards)
        {
            Dictionary<int, int> cntDict = GetCntDict(cards);

            List<(int cnt, int weight)> handData = new List<(int cnt, int weight)>();
            foreach (var item in cntDict)
            {
                handData.Add((cnt: item.Value, weight: item.Key));
            }
            handData.Sort();
            return handData;
        }


        //同上
        private static List<(int weight, int cnt)> GetHandDataWC(Card[] cards)
        {
            Dictionary<int, int> cntDict = GetCntDict(cards);

            List<(int weight, int cnt)> handData = new List<(int weight, int cnt)>();
            foreach (var item in cntDict)
            {
                handData.Add((weight: item.Key, cnt: item.Value));
            }
            handData.Sort();
            return handData;
        }

        private static Dictionary<int, int> GetCntDict(Card[] cards)
        {
            Dictionary<int, int> cntDict = new Dictionary<int, int>();
            foreach (Card card in cards)
            {
                if (cntDict.ContainsKey((int)card.weight))
                    cntDict[(int)card.weight] += 1;
                else
                    cntDict.Add((int)card.weight, 1);
            }

            return cntDict;
        }

        /// <summary>
        /// 获取一种牌型的最小合法重复数
        /// </summary>
        /// to be improve 因为历史原因，这个方法没有在CardSet里面用到，
        /// 但是实际上应该用它来代替硬编码，来提高可修改性（降低耦合，是这个意思不）
        public static int GetMinRepeat(CardSetType setType)
        {
            int repeat = 1;
            switch (setType)
            {
                case CardSetType.Invalid:
                case CardSetType.None:
                    repeat = 0;
                    break;
                case CardSetType.JokerBomb:
                case CardSetType.Bomb:
                case CardSetType.Single:
                case CardSetType.Pair:
                case CardSetType.Triple:
                case CardSetType.TripleWithOne:
                case CardSetType.TripleWithPair:
                case CardSetType.QuadraWithTwo:
                case CardSetType.QuadraWithTwoPairs:
                default:
                    repeat = 1;
                    break;
                case CardSetType.Straight:
                    repeat = 5;
                    break;
                case CardSetType.DoubleStraight:
                    repeat = 3;
                    break;
                case CardSetType.TripleStraight:
                case CardSetType.TripleStraightWithOne:
                case CardSetType.TripleStraightWithPair:
                    repeat = 2;
                    break;
            }
            return repeat;
        }

        private static string IterToString<T>(IEnumerable<T> container)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            bool first = true;
            foreach (var item in container)
            {
                if (!first)
                    builder.Append(",");

                first = false;
                builder.Append(item.ToString());
            }
            builder.Append("]");
            return builder.ToString();
        }
        #endregion
        #region 预设类实例
        /// <summary>
        /// 预设的王炸牌型
        /// </summary>
        public static CardSet JokerBomb
        {
            get
            {
                return new CardSet(
                    setType: CardSetType.JokerBomb,
                    keyNumber: (int)CardWeight.wJokerRed,
                    repeatCount: 1,
                    cards: new Card[] { Card.JokerBlack, Card.JokerRed }
                );
            }
        }

        public static CardSet Invalid
        {
            get
            {
                return new CardSet(
                    setType: CardSetType.Invalid,
                    keyNumber: -1,
                    repeatCount: 1,
                    cards: new Card[] { }
                );
            }
        }

        public static CardSet None
        {
            get
            {
                return new CardSet(
                    setType: CardSetType.None,
                    keyNumber: 0,
                    repeatCount: 1,
                    cards: new Card[] { }
                );
            }
        }


        #endregion

    }


}