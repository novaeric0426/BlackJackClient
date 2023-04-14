using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour {

    private static GameManager _instance;
    public SocketHandle SocketHandler;

    public Button hitBtn;
    public Button standBtn;
    public Button betBtn;
    public Button betDoneBtn;

    public SpriteRenderer[] CheckIcon;

    // Access the player and dealer's script
    public PlayerScript myPlayer;
    public PlayerScript dealerScript;

    private int standClicks = 0;
    public TMP_Text standBtnText;
    public TMP_Text scoreText;
    public TMP_Text dealerScoreText;
    public TMP_Text betsText;
    public TMP_Text cashText;
    public TMP_Text mainText;
    public TMP_Text[] clientNameList;

    // Card Hiding dealer's 2nd card
    public GameObject hideCard;
    //How much is bet
    int pot = 0;
    //내가 담당하고 있는 클라이언트 번호
    private int clientIndex = 0;

    public List<PlayerScript> playerList;
    private int lastNumber = 54;
    public static GameManager Instance {
        get {
            // 인스턴스가 없는 경우에 접근하려 하면 인스턴스를 할당해준다.
            if (!_instance) {
                _instance = FindObjectOfType (typeof (GameManager)) as GameManager;

                if (_instance == null)
                    Debug.Log ("no Singleton obj");
            }
            return _instance;
        }
    }

    private void Awake () {
        if (_instance == null) {
            _instance = this;
        }
        // 인스턴스가 존재하는 경우 새로생기는 인스턴스를 삭제한다.
        else if (_instance != this) {
            Destroy (gameObject);
        }
        // 아래의 함수를 사용하여 씬이 전환되더라도 선언되었던 인스턴스가 파괴되지 않는다.
        DontDestroyOnLoad (gameObject);
        SocketHandler = GameObject.Find ("SocketHandle").GetComponent<SocketHandle> ();
        playerList.Add (GameObject.Find ("Player1").GetComponent<PlayerScript> ());
        playerList.Add (GameObject.Find ("Player2").GetComponent<PlayerScript> ());
        playerList.Add (GameObject.Find ("Player3").GetComponent<PlayerScript> ());
    }

    // Start is called before the first frame update
    void Start () {
        BtnStart ();
        clientIndex = SocketHandler.clientIndex;
        myPlayer = playerList[clientIndex];

        var pkt = SocketHandler.MakePacket ("", 0, 0, "GetJoinedClientNickname");
        SocketHandler.Send (pkt);
    }

    public void BtnStart () {
        hitBtn.onClick.AddListener (() => {
            var pkt = SocketHandler.MakePacket ("", 0, 0, "HitCard");
            SocketHandler.Send (pkt);
        });
        standBtn.onClick.AddListener (() => StandClicked ());
        betBtn.onClick.AddListener (() => BetClicked ());
        betDoneBtn.onClick.AddListener (() => { BetDoneClicked (); });
    }

    private IEnumerator ResetGame () {
        yield return new WaitForSeconds (2);
        //Resset Round, hide text, prep for new hand
        foreach (PlayerScript player in playerList) {
            player.ResetHand ();
        }
        dealerScript.ResetHand ();

        mainText.gameObject.SetActive (false);
        dealerScoreText.gameObject.SetActive (false);

        scoreText.text = "Hand : " + myPlayer.handValue.ToString ();

        foreach (SpriteRenderer checkIcon in CheckIcon) {
            checkIcon.enabled = false;
        }
        hideCard.GetComponent<Renderer> ().enabled = true;
    }

    public void DealStart () {
        Debug.Log ("Deal Clicked!");

        scoreText.text = "Hand : " + myPlayer.handValue.ToString ();

        hitBtn.gameObject.SetActive (true);
        standBtn.gameObject.SetActive (true);
        betBtn.gameObject.SetActive (false);
        betDoneBtn.gameObject.SetActive (false);
        standBtnText.text = "Stand";

        cashText.text = "$" + myPlayer.GetMoney ().ToString ();
    }
    public void HitClicked (int cardnum_) {
        if (myPlayer.cardIndex <= 6) {
            myPlayer.GetCard (cardnum_);
            scoreText.text = "Hand : " + myPlayer.handValue.ToString ();
            if (myPlayer.handValue > 21) MidBust ();
        }
    }
    private void StandClicked () {
        standClicks++;
        hitBtn.gameObject.SetActive (false);
        if (standClicks > 1) {
            standBtn.gameObject.SetActive (false);
            var pkt = SocketHandler.MakePacket ("", 0, 0, "CurrentHandDone");
            SocketHandler.Send (pkt);
            var pkt1 = SocketHandler.MakePacket ("", (short) clientIndex, 0, "ChoiceDone");
            SocketHandler.Send (pkt1);
        }

        //RoundOver ();

        standBtnText.text = "Done";
    }
    private void BetDoneClicked () {
        betDoneBtn.gameObject.SetActive (false);
        betBtn.gameObject.SetActive (false);
        var pkt = SocketHandler.MakePacket ("", (short) clientIndex, 0, "BetDone");
        SocketHandler.Send (pkt);
    }

    public void HitDealer () {
        while (dealerScript.handValue < 16 && dealerScript.cardIndex < 6) {
            dealerScript.GetCard (generateRandomNum ());
            dealerScoreText.text = "Hand : " + dealerScript.handValue.ToString ();
        }
        RoundOver ();
        mainText.gameObject.SetActive (true);
    }

    //Check for winner and loser, hand is over
    public void RoundOver () {
        bool playerBust = myPlayer.handValue > 21;
        bool dealerBust = dealerScript.handValue > 21;
        bool player21 = myPlayer.handValue == 21;
        bool dealer21 = dealerScript.handValue == 21;

        if (standClicks < 2 && !playerBust && dealerBust && !player21 && !dealer21) return;
        bool roundOver = true;
        // if player busts, dealer didn't, or if dealer has more points, deal wins
        if (playerBust || ((dealerScript.handValue > myPlayer.handValue) && !dealerBust)) {
            mainText.text = "Dealer wins!";
        }
        //player wins
        else if (dealerBust || myPlayer.handValue > dealerScript.handValue) {
            mainText.text = "Player wins!";
            myPlayer.AdjustMoney (pot);
        }
        //Check for tie, return bets
        else if (myPlayer.handValue == dealerScript.handValue) {
            mainText.text = "Push : Bets returned";
            myPlayer.AdjustMoney (pot / 2);
        } else {
            roundOver = false;
        }
        // Set ui up for next move / hand / turn
        if (roundOver) {
            hitBtn.gameObject.SetActive (false);
            standBtn.gameObject.SetActive (false);
            mainText.gameObject.SetActive (true);
            betBtn.gameObject.SetActive (true);
            betDoneBtn.gameObject.SetActive (true);
            dealerScoreText.gameObject.SetActive (true);
            hideCard.GetComponent<Renderer> ().enabled = false;
            cashText.text = "$" + myPlayer.GetMoney ().ToString ();
            standClicks = 0;
            pot = 0;
            betsText.text = pot.ToString ();
            StartCoroutine (ResetGame ());
        }
    }

    private void BetClicked () {
        TMP_Text newBet = betBtn.GetComponentInChildren (typeof (TMP_Text)) as TMP_Text;
        int intBet = int.Parse (newBet.text.ToString ().Remove (0, 1));
        myPlayer.AdjustMoney (-intBet);
        cashText.text = "$" + myPlayer.GetMoney ().ToString ();
        pot += (intBet * 2);
        betsText.text = "Bets: $" + pot.ToString ();
    }

    public void ShowOtherCard (int clientindex_, int cardnum_) {
        playerList[clientindex_].hand[playerList[clientindex_].cardIndex].GetComponent<SpriteRenderer> ().enabled = true;
        playerList[clientindex_].hand[playerList[clientindex_].cardIndex].GetComponent<CardScript> ().SetSprite (playerList[clientindex_].deckScript.cardSprites[cardnum_]);
        playerList[clientindex_].cardIndex++;
    }
    private void BlackJack () {

    }
    private void MidBust () {
        var pkt = SocketHandler.MakePacket ("", 0, 0, "CurrentHandDone");
        SocketHandler.Send (pkt);
        var pkt1 = SocketHandler.MakePacket ("", (short) clientIndex, 0, "ChoiceDone");
        SocketHandler.Send (pkt1);
    }
    private int generateRandomNum () {
        int randomNumber = Random.Range (1, 53);
        for (int i = 0; randomNumber == lastNumber && i < 5; i++) {
            randomNumber = Random.Range (1, 53);
        }
        lastNumber = randomNumber;
        return lastNumber;
    }
}