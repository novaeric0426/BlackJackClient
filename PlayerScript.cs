using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    // --- This script is for BOTH player and dealer

    public CardScript cardScript;
    public DeckScript deckScript;

    public int handValue = 0;
    private int money = 1000;

    //Array of card objects on table
    public GameObject[] hand;

    // Index of next card to be turned over
    public int cardIndex = 0;

    //Tracking aces for 1 to 11 conversions
    List<CardScript> aceList = new List<CardScript>();

    public int GetCard(int cardNum)
    {
        //Get a card
        int cardValue = deckScript.DealCard(hand[cardIndex].GetComponent<CardScript>(), cardNum);
        //Show card on game screen
        hand[cardIndex].GetComponent<SpriteRenderer>().enabled = true;
        handValue += cardValue;
        //If value is 1, it is an ace
        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndex].GetComponent<CardScript>());
        }

        //Check if we should use an 11 instead of a 1
        AceCheck();
        cardIndex++;
        return handValue;
    }

    public void AceCheck()
    {
        foreach (CardScript ace in aceList)
        {
            if (handValue + 10 < 22 && ace.GetValueOfCard() == 1)
            {
                // if converting, adjust card object value and hand
                ace.SetValue(11);
                handValue += 10;
            }
            else if (handValue > 21 && ace.GetValueOfCard() == 11)
            {
                // if converting, adjust gamobject value and hand value
                ace.SetValue(1);
                handValue -= 10;
            }
        }
    }

    public void AdjustMoney(int amount)
    {
        money += amount;
    }

    public int GetMoney()
    {
        return money;
    }
    public void ResetHand()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
        cardIndex = 0;
        handValue = 0;
        aceList = new List<CardScript>();
    }

}