using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ServerProtocol.SharedCode
{
    /// <summary>
    /// 用于管理和简化手牌信息的类
    /// </summary>
    public class CardHand
    {
        #region 成员变量与访问器
        private List<Card> cards;

        /// <summary>
        /// 获取全部手牌
        /// </summary>
        public Card[] GetCards() => cards.ToArray();
        public int CardCount => cards.Count;

        public SortedDictionary<int, int> weightCountDict
        {
            get
            {
                if (!cacheVaild)
                {
                    _weightCountDict = GetWeightCountDict();
                    cacheVaild = true;
                }
                return _weightCountDict;
            }
        }
        private SortedDictionary<int, int> _weightCountDict = new SortedDictionary<int, int>();

        /// <summary>weightCountDict更新之前，手牌是否变化过</summary>
        /// <remarks>避免每次重新计算weightCountDict</remarks>
        private bool cacheVaild;

        private SortedDictionary<int, int> GetWeightCountDict()
        {
            SortedDictionary<int, int> dict = new SortedDictionary<int, int>();
            foreach (Card card in cards)
            {
                if (dict.ContainsKey((int)card.weight))
                    dict[(int)card.weight] += 1;
                else
                    dict.Add((int)card.weight, 1);
            }

            return dict;
        }
        #endregion
        #region 作为容器操作
        public CardHand(Card[] cards)
        {
            this.cards = new List<Card>(cards);
            ResetHandIds();
        }

        public CardHand()
        {
            this.cards = new List<Card>();
        }

        public void AddCards(Card[] cards)
        {
            this.cards.AddRange(cards);
            ResetHandIds();
        }

        public void RemoveCards(Card[] cards)
        {
            Card[] cardsClone = cards.Clone() as Card[];
            Array.Sort(cardsClone, (a, b) => a.handId.CompareTo(b.handId));//handid必须升序才能使用以下算法。
            for (int i = 0; i < cardsClone.Length; i++)
            {
                this.cards.RemoveAt(cardsClone[i].handId - i);
            }
            ResetHandIds();
        }

        public void RemoveCards(int[] cardIndexs)
        {
            for (int i = 0; i < cardIndexs.Length; i++)
            {
                this.cards.RemoveAt(cardIndexs[i] - i);
            }
            ResetHandIds();
        }

        public void Clear()
        {
            this.cards.Clear();
        }

        /// <summary>
        /// 重新设置每张卡的HandId，手牌张数不会超过20，
        /// 故没有对删除添加做优化，直接重置即可
        /// </summary>
        private void ResetHandIds()
        {
            cacheVaild = false; //调用这个方法时，手牌发生了变化，所以标记
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].handId = i;
            }
        }

        /// <summary>
        /// 以打牌最常见的方式排序，即降序排列
        /// </summary>
        public void Sort()
        {
            cards.Sort((a, b) => -a.CompareTo(b));
            ResetHandIds();
        }

        public void Sort(IComparer<Card> comparer)
        {
            cards.Sort(comparer);
            ResetHandIds();
        }

        public int GetCardCount(CardWeight weight)
        {
            if (weightCountDict.TryGetValue((int)(weight), out int count))
                return count;
            else
                return 0;
        }
        #endregion
        #region 获取牌型
        /// <summary>
        /// 手牌中是否存在一张牌
        /// </summary>
        /// <param name="weight">牌的权值</param>
        /// <param name="minCount">最少需要几张</param>
        /// <returns></returns>
        public bool ExistCard(int weight, int minCount = 1)
        {
            if (weightCountDict.TryGetValue(weight, out int count))
                return count >= minCount;
            else
                return false;
        }

        /// <summary>
        /// 手牌中是否存在一个顺子
        /// </summary>
        /// <param name="weight">顺子最大牌的权值</param>
        /// <param name="minCount">每张牌最少需要几张</param>
        /// <returns></returns>
        public bool ExistStraight(int weight, int length, int minCount = 1)
        {
            for (int i = weight; i > weight - length; i--)
            {
                if (i < (int)CardWeight.w3 || i > (int)CardWeight.wA)//顺子只能包含3~A
                    return false;
                if (!ExistCard(i, minCount))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 从手牌中获取指定卡牌count张，无验证
        /// </summary>
        private Card[] GetCards(int weight, int count = 1)
        {
            List<Card> result = new List<Card>();
            int nowCount = 0;
            foreach (Card card in cards)
            {
                if (nowCount >= count)
                    break;
                if ((int)card.weight == weight)
                {
                    result.Add(card);
                    nowCount += 1;
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 尝试从手牌中获取指定卡牌count张
        /// </summary>
        /// <param name="weight">指定的大小</param>
        /// <param name="count">至少多少张</param>
        /// <returns></returns>
        public bool TryGetCards(out Card[] cards, int weight, int count = 1)
        {
            cards = null;
            if (weightCountDict.TryGetValue(weight, out int hasCount))
            {
                if (hasCount >= count)
                {
                    cards = GetCards(weight, count);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试获取一个精确的顺子，支持非单顺
        /// </summary>
        /// <param name="weight">顺子的keyNumber（最大的一张）</param>
        /// <param name="length">顺子长度</param>
        /// <param name="straightRepeat">顺子重复数，比如双顺是2</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetStraight(out Card[] cards, int weight, int length, int straightRepeat = 1)
        {
            cards = null;
            List<Card> result = new List<Card>();
            for (int i = weight; i > weight - length; i--)
            {
                if (i < (int)CardWeight.w3 || i > (int)CardWeight.wA)//顺子只能包含3~A
                    return false;
                bool exist = TryGetCards(out Card[] arr, i, straightRepeat);
                if (!exist)
                    return false;
                result.AddRange(arr);
            }

            cards = result.ToArray();
            return true;
        }

        /// <summary>
        /// 尝试获取一个精确的带其他卡牌的顺子，支持非单顺，不检查顺子长度
        /// </summary>
        /// <param name="weight">顺子的keyNumber（最大的一张）</param>
        /// <param name="length">顺子长度</param>
        /// <param name="straightRepeat">顺子重复数，比如双顺是2</param>
        /// <returns>是否获取成功</returns>
        /// <remarks>可以用来获取三带一等牌型，这些牌型可以看成长度为1的带牌顺子</remarks>
        private bool TryGetStraightWithSubCards(out Card[] cards, int weight, int length, int[] subCardCnts, int straightRepeat = 1)
        {
            if (subCardCnts.Length > 1 && length > 1)
                throw new NotSupportedException("不支持带多个的多顺");

            cards = null;
            List<Card> result = new List<Card>();

            //数字比较少，就不用树形数据结构了，
            //不写成(w>weight||w<weight-length)是因为用contains可读性好
            List<int> selectedWeights = new List<int>();

            //顺子部分，这里和上面的一样
            for (int i = weight; i > weight - length; i--)
            {
                if (i < (int)CardWeight.w3 || i > (int)CardWeight.wA)//顺子只能包含3~A
                    return false;

                bool exist = TryGetCards(out Card[] arr, i, straightRepeat);
                selectedWeights.Add(i);

                if (!exist)
                    return false;
                result.AddRange(arr);
            }

            //处理被带的卡牌，这些卡牌没有大小要求
            Array.Sort(subCardCnts);
            List<(int cnt, int weight)> cntWeightList = new List<(int cnt, int weight)>();
            foreach (var item in weightCountDict)
            {
                cntWeightList.Add((cnt: item.Value, weight: item.Key));
            }
            //cnt升序，然后weight升序排，这样得出的结果是较优的（优先带少的小牌）
            cntWeightList.Sort();

            //都是排好序的，所以直接扫一遍就可以了
            int index = 0;
            foreach (int requiredCnt in subCardCnts)
            {
                bool ok = false;
                for (; index < cntWeightList.Count; index++)
                {
                    var item = cntWeightList[index];
                    if (item.cnt > requiredCnt && !selectedWeights.Contains(item.weight))//满足需求并且没有拿过，可以拿这种卡牌
                    {
                        //这一句实际上不需要，因为顺序扫，并且接下来不会用到selectedWeights
                        //但是加上保证加代码不出错
                        selectedWeights.Add(item.weight);
                        result.AddRange(GetCards(item.weight, requiredCnt));
                        index++;
                        ok = true;
                        break;
                    }
                }

                //没有全部匹配上，ok用于处理边界条件，即最后一个和最后一个刚好匹配不能return false
                if (index >= cntWeightList.Count && !ok)
                    return false;
            }

            cards = result.ToArray();
            return true;
        }

        /// <summary>
        /// 尝试获取一个任意的带其他卡牌的最长的顺子，支持非单顺，仅检测3~A，不检查顺子长度
        /// </summary>
        /// <param name="cards">输出获取的卡牌结果</param>
        /// <param name="length">输出顺子的长度</param>
        /// <param name="subCardCnt">带几张，不能超过straightRepeat</param>
        /// <param name="straightRepeat">顺子重复数，比如双顺是2</param>
        /// <returns>是否获取成功</returns>
        /// <remarks>
        /// 这个和TryGetStraightWithSubCards不一样，只能拿顺子（3~A那种的）类型
        /// 3个out参数因为要构造CardSet，缺少类型
        /// </remarks>
        private bool TryGetAnyStraightWithSubCards(out Card[] cards, out int length, out int keyNumber, int subCardCnt, int straightRepeat = 1)
        {
            if (straightRepeat <= subCardCnt)
                throw new ArgumentException("参数错误。subCardCnt应该小于straightRepeat");


            cards = null;
            List<Card> result = new List<Card>();
            List<int> selectedWeights = new List<int>();

            //寻找顺子部分
            List<Card> resultTemp = new List<Card>();
            for (int weight = (int)CardWeight.wA; weight >= (int)CardWeight.w3; weight--)
            {
                for (int i = weight; i >= (int)CardWeight.w3; i--)
                {
                    bool exist = TryGetCards(out Card[] arr, i, straightRepeat);
                    if (!exist)
                        break;
                    resultTemp.AddRange(arr);
                }

                if (resultTemp.Count > result.Count)
                    result = resultTemp;
                else
                    resultTemp.Clear();
            }

            foreach (Card card in result)
            {
                selectedWeights.Add((int)card.weight);
            }
            length = selectedWeights.Count;

            if (length == 0)//找不到了
            {
                keyNumber = -1;
                return false;
            }

            keyNumber = selectedWeights[0];

            //处理被带的卡牌，这些卡牌没有大小要求
            if (subCardCnt <= 0)//这种是没有带牌的纯顺子
            {
                return true;
            }

            List<(int cnt, int weight)> cntWeightList = new List<(int cnt, int weight)>();
            foreach (var item in weightCountDict)
            {
                cntWeightList.Add((cnt: item.Value, weight: item.Key));
            }
            //cnt升序，然后weight升序排，这样得出的结果是较优的（优先带少的小牌）
            cntWeightList.Sort();

            //都是排好序的，所以直接扫一遍就可以了
            //顺子有多长，就要带几张
            for (int i = 0; i < length; i++)
            {
                bool ok = false;
                for (int index = 0; index < cntWeightList.Count; index++)
                {
                    var item = cntWeightList[index];
                    if (item.cnt > subCardCnt && !selectedWeights.Contains(item.weight))//满足需求并且没有拿过，可以拿这种卡牌
                    {
                        //这一句实际上不需要，因为顺序扫，并且接下来不会用到selectedWeights
                        //但是加上保证加代码不出错
                        selectedWeights.Add(item.weight);
                        result.AddRange(GetCards(item.weight, subCardCnt));
                        ok = true;
                        break;
                    }
                }
                if (!ok)
                    return false;
            }
            return true;
        }


        /// <summary>
        /// 尝试获取一个任意的x带y的牌型，y可以为0，优先最小的
        /// </summary>
        /// <param name="cards">输出获取的卡牌结果</param>
        /// <param name="keyNumber">输出获取的关键字（构造CardSet）</param>
        /// <param name="mainCardCnt">主牌x需要几张</param>
        /// <param name="subCardCnt">副牌y需要几张</param>
        /// <returns>是否获取成功</returns>
        private bool TryGetAnyCardsWithSubCards(out Card[] cards, out int keyNumber, int mainCardCnt, int subCardCnt)
        {
            List<Card> cardsResult = new List<Card>();
            Card[] tempCards;
            int mainCardWeight = (int)CardWeight.wMin;
            int subCardWeight = (int)CardWeight.wMin;

            foreach (var pair in weightCountDict)
            {
                if (pair.Value >= mainCardCnt)
                {
                    if (TryGetCards(out tempCards, pair.Key, mainCardCnt))
                    {
                        cardsResult.AddRange(tempCards);
                        mainCardWeight = pair.Key;
                        break;
                    }
                }
            }


            if (mainCardWeight == (int)CardWeight.wMin) // 找不到主卡
            {
                cards = null;
                keyNumber = -1;
                return false;
            }

            keyNumber = mainCardWeight; // 至此，主卡就找到了
            if (subCardCnt <= 0) // 只判断主卡就行了
            {
                cards = cardsResult.ToArray();
                return true;
            }

            //同样的办法找副卡
            foreach (var pair in weightCountDict)
            {
                if (pair.Value >= subCardCnt && pair.Key != mainCardWeight)//但是不能和主卡一样
                {
                    if (TryGetCards(out tempCards, pair.Key, subCardCnt))
                    {
                        cardsResult.AddRange(tempCards);
                        subCardWeight = pair.Key;
                        break;
                    }
                }
            }

            if (subCardWeight == (int)CardWeight.wMin) // 找不到副卡
            {
                cards = null;
                keyNumber = -1;
                return false;
            }

            cards = cardsResult.ToArray();
            return true;
        }


        /// <summary>
        /// 尝试获取一个精确的牌型（精确到第一关键字）
        /// </summary>
        /// <returns>是否获取成功</returns>
        private bool TryGetExactCardSet(out CardSet cardSet, CardSetType setType, int keyNumber, int repeat)
        {
            cardSet = null;
            Card[] cardsResult = null;
            switch (setType)
            {
                case CardSetType.Invalid:
                    cardSet = CardSet.Invalid;
                    return true;
                case CardSetType.None:
                    cardSet = CardSet.None;
                    return true;
                case CardSetType.JokerBomb:
                    return TryGetCardSetJokerBomb(out cardSet);
                case CardSetType.Single:
                case CardSetType.Pair:
                case CardSetType.Triple:
                case CardSetType.Bomb:
                    if (!TryGetExactCardSetPart1(out cardsResult))
                    {
                        return false;
                    }
                    break;
                case CardSetType.Straight:
                case CardSetType.DoubleStraight:
                case CardSetType.TripleStraight:
                    if (!TryGetExactCardSetPart2(out cardsResult))
                    {
                        return false;
                    }
                    break;
                case CardSetType.TripleWithOne:
                case CardSetType.TripleWithPair:
                case CardSetType.TripleStraightWithOne:
                case CardSetType.TripleStraightWithPair:
                case CardSetType.QuadraWithTwo:
                case CardSetType.QuadraWithTwoPairs:
                    if (!TryGetExactCardSetPart3(out cardsResult))
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
            cardSet = new CardSet(setType, keyNumber, repeat, cardsResult);
            return true;

            bool TryGetExactCardSetPart1(out Card[] cardsResult1)//加1避免名称冲突（vs可以通过编译，unity不行）
            {
                int repeatCount = 1;
                switch (setType)
                {
                    case CardSetType.Single:
                        repeatCount = 1;
                        break;
                    case CardSetType.Pair:
                        repeatCount = 2;
                        break;
                    case CardSetType.Triple:
                        repeatCount = 3;
                        break;
                    case CardSetType.Bomb:
                        repeatCount = 4;
                        break;
                }

                return TryGetCards(out cardsResult1, keyNumber, repeatCount);
            }

            bool TryGetExactCardSetPart2(out Card[] cardsResult1)
            {
                int straightRepeat = 1;//这是几顺，是单顺，双顺还是三顺
                switch (setType)
                {
                    case CardSetType.Straight:
                        straightRepeat = 1;
                        break;
                    case CardSetType.DoubleStraight:
                        straightRepeat = 2;
                        break;
                    case CardSetType.TripleStraight:
                        straightRepeat = 3;
                        break;
                }
                return TryGetStraight(out cardsResult1, keyNumber, repeat, straightRepeat);
            }

            bool TryGetExactCardSetPart3(out Card[] cardsResult1)
            {
                int length = 1;
                int straightRepeat = 1;
                int[] subCardCnts = null;
                switch (setType)
                {
                    case CardSetType.TripleWithOne:
                        length = 1;
                        straightRepeat = 3;
                        subCardCnts = new int[] { 1 };
                        break;
                    case CardSetType.TripleWithPair:
                        length = 1;
                        straightRepeat = 3;
                        subCardCnts = new int[] { 2 };
                        break;
                    case CardSetType.TripleStraightWithOne:
                        length = repeat;
                        straightRepeat = 3;
                        subCardCnts = new int[] { 1 };
                        break;
                    case CardSetType.TripleStraightWithPair:
                        length = repeat;
                        straightRepeat = 3;
                        subCardCnts = new int[] { 2 };
                        break;
                    case CardSetType.QuadraWithTwo:
                        length = 1;
                        straightRepeat = 4;
                        subCardCnts = new int[] { 1, 1 };
                        break;
                    case CardSetType.QuadraWithTwoPairs:
                        length = 1;
                        straightRepeat = 4;
                        subCardCnts = new int[] { 2, 2 };
                        break;
                }
                return TryGetStraightWithSubCards(out cardsResult1, keyNumber, length, subCardCnts, straightRepeat);
            }
        }

        /// <summary>
        /// 尝试获取一个可以压住指定牌型的相同牌型
        /// </summary>
        /// <returns>是否成功</returns>
        private bool TryGetCardSetSameTypeGreater(out CardSet result, CardSet prevSet)
        {
            result = null;
            for (int key = prevSet.KeyNumber + 1; key < (int)CardWeight.wMax; key++)
            {
                if (TryGetExactCardSet(out result, prevSet.Type, key, prevSet.RepeatCount))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试从手牌中获取一个点数至少为minWeight的炸弹
        /// </summary>
        /// <returns>是否成功</returns>
        private bool TryGetCardSetBomb(out CardSet result, int minWeight = (int)CardWeight.w3)
        {
            result = null;
            for (int key = minWeight; key < (int)CardWeight.wMax; key++)
            {
                if (TryGetExactCardSet(out result, CardSetType.Bomb, key, 1))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试从手牌中获取一个王炸
        /// </summary>
        /// <returns>是否成功</returns>
        private bool TryGetCardSetJokerBomb(out CardSet result)
        {
            if (ExistCard((int)CardWeight.wJokerBlack) && ExistCard((int)CardWeight.wJokerRed))
            {
                //注意不能直接用CardSet.jokerBomb，因为Card中添加了索引信息，需要从手牌里拿才行
                Card[] resultCards = new Card[] { GetCards((int)CardWeight.wJokerBlack)[0], GetCards((int)CardWeight.wJokerRed)[0] };
                result = new CardSet(CardSetType.JokerBomb, (int)CardWeight.wJokerRed, 1, resultCards);
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// 从手牌中获取一个任意牌型
        /// 使用情况为先手可出任意牌型
        /// </summary>
        /// <remarks>获取牌型的顺序为：顺带类（比如飞机）>顺子类（比如连对）>单带类（比如三带一）>单出类（比如一对）>炸弹>王炸 ，
        /// 优先获取最小的，顺子优先最长的，非最后一手不出四带二，四带两对。 </remarks>
        private CardSet GetAnyCardSet()
        {
            //如果手里没牌，则无法出牌
            if (cards.Count <= 0)
                return CardSet.Invalid;

            //如果一手可出完，则直接返回这个牌型
            CardSet result = CardSet.GetCardSet(cards.ToArray());
            if (result.Type != CardSetType.Invalid)
                return result;

            //否则挨个检测
            Card[] resultCards;
            int length;
            int keyNumber;


            #region 顺带类（比如飞机）
            if (TryGetAnyStraightWithSubCards(out resultCards, out length, out keyNumber, 2, 3))
                if (length >= CardSet.GetMinRepeat(CardSetType.TripleStraightWithPair)) //不用&&，写成两行清楚些
                    return new CardSet(CardSetType.TripleStraightWithPair, keyNumber, length, resultCards);

            if (TryGetAnyStraightWithSubCards(out resultCards, out length, out keyNumber, 1, 3))
                if (length >= CardSet.GetMinRepeat(CardSetType.TripleStraightWithOne))
                    return new CardSet(CardSetType.TripleStraightWithOne, keyNumber, length, resultCards);
            #endregion
            #region 顺子类（比如连对）
            if (TryGetAnyStraightWithSubCards(out resultCards, out length, out keyNumber, 0, 3))
                if (length >= CardSet.GetMinRepeat(CardSetType.TripleStraight))
                    return new CardSet(CardSetType.TripleStraight, keyNumber, length, resultCards);

            if (TryGetAnyStraightWithSubCards(out resultCards, out length, out keyNumber, 0, 2))
                if (length >= CardSet.GetMinRepeat(CardSetType.DoubleStraight))
                    return new CardSet(CardSetType.DoubleStraight, keyNumber, length, resultCards);

            if (TryGetAnyStraightWithSubCards(out resultCards, out length, out keyNumber, 0, 1))
                if (length >= CardSet.GetMinRepeat(CardSetType.Straight))
                    return new CardSet(CardSetType.Straight, keyNumber, length, resultCards);
            #endregion
            #region 单带类（比如三带一）
            if (TryGetAnyCardsWithSubCards(out resultCards, out keyNumber, 3, 2))
                return new CardSet(CardSetType.TripleWithPair, keyNumber, 1, resultCards);

            if (TryGetAnyCardsWithSubCards(out resultCards, out keyNumber, 3, 1))
                return new CardSet(CardSetType.TripleWithOne, keyNumber, 1, resultCards);
            #endregion
            #region 单出类（比如一对）
            if (TryGetAnyCardsWithSubCards(out resultCards, out keyNumber, 3, 0))
                return new CardSet(CardSetType.Triple, keyNumber, 1, resultCards);

            if (TryGetAnyCardsWithSubCards(out resultCards, out keyNumber, 2, 0))
                return new CardSet(CardSetType.Pair, keyNumber, 1, resultCards);

            if (TryGetAnyCardsWithSubCards(out resultCards, out keyNumber, 1, 0))
                return new CardSet(CardSetType.Single, keyNumber, 1, resultCards);
            #endregion
            #region 炸弹
            if (TryGetAnyCardsWithSubCards(out resultCards, out keyNumber, 4, 0))
                return new CardSet(CardSetType.Bomb, keyNumber, 1, resultCards);
            #endregion
            #region 王炸
            if (TryGetCardSetJokerBomb(out result))
                return result;
            #endregion

            return CardSet.Invalid;
        }

        /// <summary>
        /// 获取一个可以压住指定牌型的牌型，如果没有则返回牌型Invalid
        /// 这个牌型为较优策略（不会整最优策略）
        /// </summary>
        /// <param name="prevSet"></param>
        /// <returns></returns>
        public CardSet GetCardSetGreater(CardSet prevSet)
        {
            CardSet result;
            if (prevSet.Type == CardSetType.JokerBomb) //没有比王炸大的牌
                return CardSet.Invalid;
            else if (prevSet.Type == CardSetType.Bomb) //只有炸弹和王炸比炸弹大
            {
                if (TryGetCardSetBomb(out result, prevSet.KeyNumber + 1))
                    return result;

                if (TryGetCardSetJokerBomb(out result))
                    return result;
            }
            else if (prevSet.Type == CardSetType.None) //此时可以任意出牌
            {
                return GetAnyCardSet();
            }
            else //普通牌型，从小往大判断
            {
                if (TryGetCardSetSameTypeGreater(out result, prevSet))
                    return result;

                if (TryGetCardSetBomb(out result))
                    return result;

                if (TryGetExactCardSet(out result, CardSetType.JokerBomb, 0, 0))//王炸不需要后两个参数
                    return result;
            }

            return CardSet.Invalid;
        }

        /// <summary>
        /// 计算手牌评分，用于抢地主
        /// </summary>
        /// <remarks>目前是最简单方法，只看双王和2</remarks>
        public int GetCardHandScore()
        {
            int jokerB = GetCardCount(CardWeight.wJokerBlack);
            int jokerR = GetCardCount(CardWeight.wJokerRed);
            int card2 = GetCardCount(CardWeight.w2);
            return jokerB * 3 + jokerR * 4 + card2 * 2;
        }

        #endregion
    }

}