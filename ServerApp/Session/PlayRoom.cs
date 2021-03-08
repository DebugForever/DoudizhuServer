using DoudizhuServer;
using ServerProtocol.Code;
using ServerProtocol.Dto;
using ServerProtocol.SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApp.Session
{
    /// <summary>
    /// 游戏阶段，分为抢地主阶段和出牌阶段
    /// </summary>
    internal enum GameTurnPhase
    {
        GrabLandlord,
        PlayCard
    }

    /// <summary>
    /// 回合阶段
    /// </summary>
    internal enum GameTurnState
    {
        /// <summary>出牌阶段</summary>
        PlayCard,
        /// <summary>结束阶段</summary>
        End,
    }

    /// <summary>
    /// 回合管理类
    /// </summary>
    internal class TurnManager
    {
        /// <summary>
        /// 房间容量，默认是3
        /// </summary>
        readonly int roomCapacity;

        /// <summary>
        /// 当前回合的类型
        /// </summary>
        public GameTurnPhase TurnPhase { get; private set; }

        /// <summary>
        /// 当前进行回合的玩家序号
        /// </summary>
        private int currentPlayer;

        /// <summary>
        /// 当前回合状态
        /// </summary>
        private GameTurnState turnState;

        /// <summary>
        /// 回合开始时触发这个事件，传入的参数为当前玩家编号。
        /// </summary>
        /// <remarks>
        /// 因为Reset调用顺序的原因，初始化/构造时并不会触发这个事件。
        /// 即只在上一个玩家结束回合时触发。
        /// </remarks>
        public event Action<int> TurnStarted;

        /// <summary>
        /// 回合开始时触发这个事件，传入的参数为当前玩家编号。
        /// </summary>
        public event Action<int> TurnEnded;

        public TurnManager(int roomCapacity)
        {
            this.roomCapacity = roomCapacity;
            Reset();
        }

        public int GetCurrentPlayer()
        {
            return currentPlayer;
        }

        /// <summary>
        /// 重置至游戏开始状态
        /// </summary>
        public void Reset()
        {
            currentPlayer = 0;
            turnState = GameTurnState.End; //只有这个状态才能开始回合
            TurnPhase = GameTurnPhase.GrabLandlord;
        }

        /// <summary>
        /// 当前是否是指定玩家的回合
        /// </summary>
        public bool IsMyTurn(int playerIndex)
        {
            return playerIndex == currentPlayer && turnState != GameTurnState.End;
        }

        /// <summary>
        /// 结束指定玩家的回合
        /// </summary>
        /// <param name="playerIndex">指定玩家下标</param>
        /// <returns>结束回合是否成功</returns>
        /// <remarks>回合是自动开始的</remarks>
        public bool EndMyTurn(int playerIndex)
        {
            if (currentPlayer != playerIndex || turnState != GameTurnState.PlayCard)
                return false;
            turnState = GameTurnState.End;
            TurnEnded(playerIndex);
            NextPlayerTurn();
            return true;
        }

        /// <summary>
        /// 结束当前玩家的回合
        /// </summary>
        public void EndCurrentPlayerTurn()
        {
            turnState = GameTurnState.End;
            TurnEnded(currentPlayer);
            NextPlayerTurn();
        }

        /// <summary>
        /// 结束抢地主阶段，并由地主开始出牌
        /// </summary>
        public void EndGrabLandlordPhase(int landlordIndex)
        {
            TurnEnded(currentPlayer); //抢地主阶段结束，最后操作玩家的回合还没有结束
            TurnPhase = GameTurnPhase.PlayCard;
            SwitchPlayer(landlordIndex);
        }

        /// <summary>
        /// 强制跳到指定玩家的回合
        /// </summary>
        /// <param name="playerIndex"></param>
        public void SwitchPlayer(int playerIndex)
        {
            currentPlayer = playerIndex;
            turnState = GameTurnState.PlayCard;
            TurnStarted(currentPlayer);
        }

        public void GameStart(int playerindex)
        {
            SwitchPlayer(playerindex);
        }

        private void NextPlayerTurn()
        {
            currentPlayer += 1;
            if (currentPlayer == roomCapacity)
                currentPlayer = 0;
            turnState = GameTurnState.PlayCard;
            TurnStarted(currentPlayer);
        }
    }

    internal class GrabLandlordManager
    {
        private readonly int roomCapacity;
        private int totalGrabCount;
        private int totalNoGrabCount;

        /// <summary>上一个抢地主的人</summary>
        private int lastGrab;

        /// <summary>每人抢地主的次数</summary>
        private List<int> grabCount;

        /// <summary>抢地主结束时间，参数为成为地主的玩家编号</summary>
        public event Action<int> BecomeLandlord;

        public GrabLandlordManager(int roomCapacity)
        {
            this.roomCapacity = roomCapacity;
            grabCount = new List<int>();
            for (int i = 0; i < roomCapacity; i++)
            {
                grabCount.Add(0);
            }
        }

        public void Reset()
        {
            totalGrabCount = 0;
            totalNoGrabCount = 0;
            lastGrab = 0;
            for (int i = 0; i < grabCount.Count; i++)
            {
                grabCount[i] = 0;
            }
        }

        public void GrabLandlord(int playerIndex)
        {
            grabCount[playerIndex] += 1;
            totalGrabCount += 1;
            lastGrab = playerIndex;
            CheckBecomeLandlord(playerIndex);
        }

        public void NoGrabLandlord(int playerIndex)
        {
            totalNoGrabCount += 1;
            CheckBecomeLandlord(playerIndex);
        }

        private void CheckBecomeLandlord(int playerIndex)
        {
            if (grabCount[playerIndex] >= 2) // 第二次抢地主的人（这个人只能是最先行动的人）当地主
                BecomeLandlord(playerIndex);

            if (totalNoGrabCount == roomCapacity - 1 && totalGrabCount > 0) // 只有一个人抢地主，其他人不抢
                BecomeLandlord(lastGrab);

            if (totalNoGrabCount == roomCapacity) // 全都不抢，直接重开
                BecomeLandlord(-1);
        }
    }

    /// <summary>
    /// 游戏房间类
    /// </summary>
    public class PlayRoom : RoomBase
    {
        private List<PlayerInfo> playerList;
        private CardSet lastHand;
        private TurnManager turnManager;
        private int landlordPlayer;
        private GrabLandlordManager landlordManager;
        /// <summary>
        /// 当前回合玩家的Timer，只会存在一个。
        /// </summary>
        private Timer currentTimer;

        /// <summary>底牌</summary>
        private Card[] underCards;

        public PlayRoom()
        {
            playerList = new List<PlayerInfo>();
            turnManager = new TurnManager(capacity);
            turnManager.TurnStarted += OnTurnStarted;
            turnManager.TurnEnded += OnTurnEnded;
            landlordManager = new GrabLandlordManager(capacity);
            landlordManager.BecomeLandlord += OnBecomeLandlord;
        }

        public PlayRoom(List<ClientPeer> clientList) : this()
        {
            AddPlayers(clientList);
        }

        public void Reset()
        {
            foreach (var item in playerList)
            {
                item.cardHand.Clear();
                item.isLandlord = false;
            }
            turnManager.Reset();
            landlordManager.Reset();
            underCards = null;
        }

        public int GetClientIndex(ClientPeer client)
        {
            return playerList.FindIndex((player) => player.client == client);
        }

        private void OnBecomeLandlord(int landlordIndex)
        {
            if (landlordIndex == -1)
            {
                Console.WriteLine("重开还没写");
                return;
            }
            playerList[landlordIndex].isLandlord = true;
            playerList[landlordIndex].cardHand.AddCards(underCards);
            playerList[landlordIndex].cardHand.Sort();
            Broadcast(OpCode.play, PlayCode.GrabLandlordEndBrd, new GrabLandlordEndDto(landlordIndex, underCards));
            turnManager.EndGrabLandlordPhase(landlordIndex);
        }

        private void OnTurnStarted(int playerIndex)
        {
            Broadcast(OpCode.play, PlayCode.StartTurnBrd, playerIndex);
            currentTimer = new Timer(timerCallback, null, SharedConstants.TurnDuration, Timeout.Infinite);

            void timerCallback(object state)
            {
                // 计时器涉及到了多线程，应该对turnManager加锁
                // PlayerPassTurn也锁上了turnManager。同一个线程可以共享这个锁，因此不会死锁
                lock (turnManager)
                {
                    if (turnManager.IsMyTurn(playerIndex))
                    {
                        if (turnManager.TurnPhase == GameTurnPhase.PlayCard)
                            PlayerPassTurn(playerIndex);
                        else if (turnManager.TurnPhase == GameTurnPhase.GrabLandlord)
                            PlayerGrabLandlord(playerIndex, false);
                        turnManager.EndCurrentPlayerTurn();
                    }
                }
            }
        }

        private void OnTurnEnded(int playerIndex)
        {
            if (currentTimer != null) // 回合结束时应销毁计时器
                currentTimer.Dispose();
            Broadcast(OpCode.play, PlayCode.EndTurnBrd, playerIndex);
        }


        public void AddPlayers(List<ClientPeer> clientList)
        {
            foreach (ClientPeer client in clientList)
            {
                playerList.Add(new PlayerInfo { client = client, cardHand = new CardHand(), isLandlord = false });
            }
        }

        /// <summary>
        /// 出牌，暂时没有对出牌进行合法性校验
        /// </summary>
        public void PlayerPlayCard(int playerIndex, CardSet cardSet)
        {
            // 计时器涉及到了多线程，这个方法可能会与其竞争，所以对turnManager加锁
            lock (turnManager)
            {
                if (turnManager.IsMyTurn(playerIndex) && turnManager.TurnPhase == GameTurnPhase.PlayCard)
                {
                    playerList[playerIndex].cardHand.RemoveCards(cardSet.Cards);
                    lastHand = cardSet;
                    Broadcast(OpCode.play, PlayCode.PlayCardBrd, new PlayCardDto(cardSet, playerIndex));
                    CheckGameEnd(playerIndex);
                    turnManager.EndMyTurn(playerIndex);
                }
            }
        }

        /// <summary>
        /// 不出
        /// </summary>
        public void PlayerPassTurn(int playerIndex)
        {
            // 计时器涉及到了多线程，这个方法可能会与其竞争，所以对turnManager加锁
            lock (turnManager)
            {
                if (turnManager.IsMyTurn(playerIndex) && turnManager.TurnPhase == GameTurnPhase.PlayCard)
                {
                    Broadcast(OpCode.play, PlayCode.PassPlayCardBrd, playerIndex);
                    turnManager.EndMyTurn(playerIndex);
                }
            }
        }

        public void PlayerGrabLandlord(int playerIndex, bool isGrab)
        {
            // 计时器涉及到了多线程，这个方法可能会与其竞争，所以对turnManager加锁
            lock (turnManager)
            {
                if (!turnManager.IsMyTurn(playerIndex) || turnManager.TurnPhase != GameTurnPhase.GrabLandlord)
                {
                    //暂时不发回消息
                    return;
                }

                if (isGrab)
                {
                    Broadcast(OpCode.play, PlayCode.GrabLandlordBrd, playerIndex);
                    landlordManager.GrabLandlord(playerIndex);
                }
                else
                {
                    Broadcast(OpCode.play, PlayCode.NoGrabLandlordBrd, playerIndex);
                    landlordManager.NoGrabLandlord(playerIndex);
                }

                //防止在抢地主结束后，自己是地主的情况下，会立即结束出牌
                if (turnManager.TurnPhase == GameTurnPhase.GrabLandlord)
                    turnManager.EndMyTurn(playerIndex);
            }
        }

        private void CheckGameEnd(int playerIndex)
        {
            if (playerList[playerIndex].cardHand.CardCount == 0)
            {
                GameEnd();
            }
        }

        private void GameEnd()
        {
            Broadcast(OpCode.play, PlayCode.GameEndBrd, null/* todo */);
        }

        public override void Broadcast(int opCode, int subOpCode, object message, ClientPeer ignoreClient = null)
        {
            foreach (PlayerInfo item in playerList)
            {
                ClientPeer client = item.client;
                if (client == ignoreClient)
                    continue;
                client.SendNetMsg(opCode, subOpCode, message);
            }
        }

        /// <summary>
        /// 开始游戏并发牌，设置三张底牌
        /// </summary>
        public void GameStart()
        {
            DealCard();
            turnManager.GameStart(0);
        }

        private void DealCard()
        {
            List<Card> cardList = Card.AllCardList;
            Shuffle(cardList);

            underCards = cardList.GetRange(0, 3).ToArray();
            const int takeCardCnt = (Card.totalCardCount - 3) / capacity;
            for (int i = 0; i < capacity; i++)
            {
                Card[] cards = cardList.GetRange(3 + i * takeCardCnt, takeCardCnt).ToArray();
                Array.Sort(cards, (a, b) => b.CompareTo(a));
                playerList[i].cardHand.AddCards(cards);
                playerList[i].client.SendNetMsg(OpCode.play, PlayCode.DealCardBrd, new DealCardDto(cards));
            }

            void Shuffle<T>(List<T> list)
            {
                Random random = new Random();
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = random.Next(0, i + 1);
                    //swap
                    T temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }
        }
    }

    internal class PlayerInfo
    {
        public ClientPeer client;
        public CardHand cardHand;
        public bool isLandlord;
    }
}
