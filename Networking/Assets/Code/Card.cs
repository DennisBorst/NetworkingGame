using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    public CardTypes cardType;
    [HideInInspector] public Card cardRef;
    
    [SerializeField] private Card selectedCard;
    [SerializeField] private TextMeshProUGUI cardTypeText;


    public void GetRandomCardType()
    {
        int randomInt = Random.Range(0, (int)CardTypes.Count);
        cardType = (CardTypes)randomInt;
        cardTypeText.text = cardType.ToString();
    }

    public void UpdateCard()
    {
        selectedCard.ReceiveCardType(this);
    }

    public void ReceiveCardType(Card card)
    {
        cardRef = card;
        cardType = card.cardType;
        cardTypeText.text = card.cardType.ToString();
    }

    public void ResetSelectedCard()
    {
        cardRef = null;
        cardTypeText.text = "";
    }

    private void Awake()
    {
        //cardTypeText.text = cardType.ToString();
    }
}

public enum CardTypes
{
    Rock,
    Paper,
    Scissors,
    Count
}
