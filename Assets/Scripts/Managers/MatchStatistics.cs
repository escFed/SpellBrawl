using System;
using System.Collections.Generic;
using UnityEngine;


// Match-wide values. GameManager owns this instance; character components report events to it.
public sealed class MatchStatistics
{
    public sealed class PlayerRecord
    {
        public int Eliminations { get; internal set; }
        public int TimesEliminated { get; internal set; }
        public int DamageDealt { get; internal set; }
        public int DamageReceived { get; internal set; }
        public int DamageBlocked { get; internal set; }
        public int SuccessfulParries { get; internal set; }
        public int OffensiveCardsUsed { get; internal set; }
        public int OffensiveCardsHit { get; internal set; }
        public float SurvivalSeconds { get; internal set; }
        public bool WasBehind { get; internal set; }
        public string MostUsedCard { get; internal set; } = "—";
        public Sprite MostUsedCardIcon { get; internal set; }
        public int MostUsedCardCount { get; internal set; }
        public int CardAccuracy => OffensiveCardsUsed == 0 ? 0 :
            Mathf.RoundToInt(100f * OffensiveCardsHit / OffensiveCardsUsed);

        internal readonly Dictionary<string, int> CardCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        internal readonly Dictionary<string, Sprite> CardIcons = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        internal readonly HashSet<int> HitCardUses = new HashSet<int>();
        internal bool LifeActive;
        internal float LifeStartedAt;
    }

    private readonly PlayerRecord[] players = { new PlayerRecord(), new PlayerRecord() };
    private int nextCardUseId;

    public PlayerRecord Get(int playerIndex) => IsValid(playerIndex) ? players[playerIndex] : null;

    public void StartLife(int playerIndex, float now)
    {
        PlayerRecord player = Get(playerIndex);
        if (player == null || player.LifeActive) return;
        player.LifeActive = true;
        player.LifeStartedAt = now;
    }

    public void EndLife(int playerIndex, float now)
    {
        PlayerRecord player = Get(playerIndex);
        if (player == null || !player.LifeActive) return;
        player.SurvivalSeconds += Mathf.Max(0f, now - player.LifeStartedAt);
        player.LifeActive = false;
    }

    public float SurvivalTime(int playerIndex, float now)  
    {
        PlayerRecord player = Get(playerIndex);
        return player == null ? 0f : player.SurvivalSeconds +
            (player.LifeActive ? Mathf.Max(0f, now - player.LifeStartedAt) : 0f);
    }

    public void RecordStockLost(int playerIndex, int creditedOpponent, float now)
    {
        PlayerRecord player = Get(playerIndex);
        if (player == null) return;
        EndLife(playerIndex, now);
        player.TimesEliminated++;
        if (IsValid(creditedOpponent) && creditedOpponent != playerIndex)
            players[creditedOpponent].Eliminations++;
    }

    public void RecordDamage(int defenderIndex, int attackerIndex, int damage, int blocked)
    {
        PlayerRecord defender = Get(defenderIndex);
        if (defender == null) return;
        defender.DamageReceived += Mathf.Max(0, damage);
        defender.DamageBlocked += Mathf.Max(0, blocked);
        if (IsValid(attackerIndex) && attackerIndex != defenderIndex)
            players[attackerIndex].DamageDealt += Mathf.Max(0, damage);
    }

    public void RecordParry(int playerIndex)
    {
        PlayerRecord player = Get(playerIndex);
        if (player != null) player.SuccessfulParries++;
    }

    public int RecordCardUsed(int playerIndex, ICardable card)
    {
        PlayerRecord player = Get(playerIndex);
        if (player == null || card == null) return -1;

        string name = string.IsNullOrWhiteSpace(card.CardName) ? "Unnamed card" : card.CardName;
        int count = player.CardCounts.TryGetValue(name, out int previous) ? previous + 1 : 1;
        player.CardCounts[name] = count;
        player.CardIcons[name] = card.CardVisual;
        if (count > player.MostUsedCardCount)
        {
            player.MostUsedCard = name;
            player.MostUsedCardIcon = card.CardVisual;
            player.MostUsedCardCount = count;
        }

        if (card.Type != CardType.Offensive) return -1;
        player.OffensiveCardsUsed++;
        return ++nextCardUseId;
    }

    public void RecordCardHit(int playerIndex, int useId)
    {
        PlayerRecord player = Get(playerIndex);
        if (player != null && useId > 0 && player.HitCardUses.Add(useId))
            player.OffensiveCardsHit++;
    }

    public void RecordRoundScore(int playerOneRounds, int playerTwoRounds)
    {
        if (playerOneRounds < playerTwoRounds) players[0].WasBehind = true;
        if (playerTwoRounds < playerOneRounds) players[1].WasBehind = true;
    }

    public string GetTitle(int playerIndex, bool isWinner)
    {
        PlayerRecord player = Get(playerIndex);
        if (player == null) return "Fighter";
        PlayerRecord other = players[1 - playerIndex];
        if (isWinner && player.WasBehind) return "Comeback";
        if (player.SuccessfulParries >= 2 && player.SuccessfulParries > other.SuccessfulParries)
            return "Perfect Reflexes";
        if (player.DamageBlocked >= 30 && player.DamageBlocked > other.DamageBlocked)
            return "Arcane Wall";
        if (player.OffensiveCardsHit >= 2 && player.CardAccuracy >= 70)
            return "Card Master";
        if (player.Eliminations >= 2 && player.Eliminations > other.Eliminations)
            return "Unstoppable";
        if (player.SurvivalSeconds >= 60f && player.TimesEliminated == 0)
            return "Survivor";
        return "Fighter";
    }

    private static bool IsValid(int playerIndex) => playerIndex >= 0 && playerIndex < 2;
}
